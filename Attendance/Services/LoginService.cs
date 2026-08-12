using System.Net.Http.Json;
using Attendance.Configuration;
using Attendance.DTOs;
using Attendance.Interfaces;

namespace Attendance.Services;

/// <summary>
/// Handles authentication against the real backend described in
/// API-CONTRACT.md, and secure token storage/refresh. See that file for the
/// exact request/response shapes this implementation expects.
/// </summary>
public class LoginService : ILoginService
{
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string ExpiryKey = "auth_expiry";
    private const string StoredEmailKey = "stored_email";
    private const string StoredPasswordKey = "stored_password";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingsService _settingsService;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public LoginService(IHttpClientFactory httpClientFactory, ISettingsService settingsService)
    {
        _httpClientFactory = httpClientFactory;
        _settingsService = settingsService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponse { Success = false, ErrorMessage = "Email and password are required." };
        }

        try
        {
            var client = CreateClient();
            using var response = await client.PostAsJsonAsync(ApiEndpoints.Login, request, cancellationToken);

            var body = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken)
                       ?? new LoginResponse { Success = false, ErrorMessage = "Empty response from server." };

            if (!response.IsSuccessStatusCode || !body.Success || string.IsNullOrWhiteSpace(body.Token))
            {
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = body.ErrorMessage ?? $"Sign in failed ({(int)response.StatusCode})."
                };
            }

            await PersistTokensAsync(body.Token, body.RefreshToken, body.ExpiresAt);
            return body;
        }
        catch (TaskCanceledException)
        {
            return new LoginResponse { Success = false, ErrorMessage = "The request timed out. Check the API base URL and try again." };
        }
        catch (HttpRequestException ex)
        {
            return new LoginResponse { Success = false, ErrorMessage = $"Unable to reach the server: {ex.Message}" };
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient();
            using var response = await client.PostAsync(ApiEndpoints.Logout, content: null, cancellationToken);
            // Best-effort: server-side logout failing shouldn't block clearing local state.
            _ = response;
        }
        catch
        {
            // Ignored intentionally — local tokens are cleared below regardless.
        }
        finally
        {
            SecureStorage.Default.Remove(TokenKey);
            SecureStorage.Default.Remove(RefreshTokenKey);
            SecureStorage.Default.Remove(ExpiryKey);
        }
    }

    public Task<bool> IsAuthenticatedAsync() => EnsureValidTokenAsync();

    public async Task<bool> EnsureValidTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        var expiryRaw = await SecureStorage.Default.GetAsync(ExpiryKey);

        if (!string.IsNullOrWhiteSpace(token) &&
            DateTime.TryParse(expiryRaw, out var expiry) &&
            expiry > DateTime.UtcNow.AddMinutes(1))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(token) && await TryRefreshAsync(cancellationToken))
            return true;

        // No usable token/refresh token (e.g. the orchestrator logged out after
        // the previous run). Fall back to a fresh login with stored credentials
        // if the user opted in via "Remember Me" — this is what makes the next
        // scheduled run possible without the user reopening the app.
        if (await HasStoredCredentialsAsync())
        {
            var loginResult = await LoginWithStoredCredentialsAsync(cancellationToken);
            return loginResult.Success;
        }

        return false;
    }

    public async Task SaveCredentialsAsync(string email, string password)
    {
        await SecureStorage.Default.SetAsync(StoredEmailKey, email);
        await SecureStorage.Default.SetAsync(StoredPasswordKey, password);
    }

    public Task ClearStoredCredentialsAsync()
    {
        SecureStorage.Default.Remove(StoredEmailKey);
        SecureStorage.Default.Remove(StoredPasswordKey);
        return Task.CompletedTask;
    }

    public async Task<bool> HasStoredCredentialsAsync()
    {
        var email = await SecureStorage.Default.GetAsync(StoredEmailKey);
        var password = await SecureStorage.Default.GetAsync(StoredPasswordKey);
        return !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);
    }

    public async Task<LoginResponse> LoginWithStoredCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var email = await SecureStorage.Default.GetAsync(StoredEmailKey);
        var password = await SecureStorage.Default.GetAsync(StoredPasswordKey);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new LoginResponse { Success = false, ErrorMessage = "No stored credentials." };
        }

        return await LoginAsync(new LoginRequest { Email = email, Password = password }, cancellationToken);
    }

    private async Task<bool> TryRefreshAsync(CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            // Another caller may have already refreshed while we waited.
            var expiryRaw = await SecureStorage.Default.GetAsync(ExpiryKey);
            if (DateTime.TryParse(expiryRaw, out var expiry) && expiry > DateTime.UtcNow.AddMinutes(1))
                return true;

            var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var client = CreateClient();
            using var response = await client.PostAsJsonAsync(
                ApiEndpoints.Refresh,
                new RefreshTokenRequest { RefreshToken = refreshToken },
                cancellationToken);

            var body = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode || body is null || !body.Success || string.IsNullOrWhiteSpace(body.Token))
            {
                SecureStorage.Default.Remove(TokenKey);
                SecureStorage.Default.Remove(RefreshTokenKey);
                SecureStorage.Default.Remove(ExpiryKey);
                return false;
            }

            await PersistTokensAsync(body.Token, body.RefreshToken, body.ExpiresAt);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task PersistTokensAsync(string token, string? refreshToken, DateTime? expiresAt)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);

        if (!string.IsNullOrWhiteSpace(refreshToken))
            await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);

        await SecureStorage.Default.SetAsync(ExpiryKey, (expiresAt ?? DateTime.UtcNow.AddHours(8)).ToString("O"));
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(AppConfig.HttpClientName);
        client.BaseAddress = new Uri(_settingsService.ApiBaseUrl, UriKind.Absolute);
        client.Timeout = TimeSpan.FromSeconds(_settingsService.TimeoutSeconds);
        return client;
    }
}
