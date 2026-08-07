using Attendance.DTOs;
using Attendance.Interfaces;

namespace Attendance.Services;

/// <summary>
/// Handles authentication and secure token storage.
/// Phase 1 validates credentials locally (mock) so the UI, navigation and
/// storage layers can be built and exercised end to end. Phase 2 swaps the
/// body of <see cref="LoginAsync"/> for a real HttpClient call against
/// ISettingsService.ApiBaseUrl without touching any caller of this service.
/// </summary>
public class LoginService : ILoginService
{
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string ExpiryKey = "auth_expiry";

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponse { Success = false, ErrorMessage = "Email and password are required." };
        }

        // Simulate network latency so loading/skeleton states can be validated.
        await Task.Delay(600, cancellationToken);

        if (request.Password.Length < 4)
        {
            return new LoginResponse { Success = false, ErrorMessage = "Invalid email or password." };
        }

        var expiresAt = DateTime.UtcNow.AddHours(8);
        var token = $"mock-{Guid.NewGuid():N}";
        var refreshToken = $"mock-refresh-{Guid.NewGuid():N}";

        await SecureStorage.Default.SetAsync(TokenKey, token);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
        await SecureStorage.Default.SetAsync(ExpiryKey, expiresAt.ToString("O"));

        return new LoginResponse
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(ExpiryKey);
        return Task.CompletedTask;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        var expiryRaw = await SecureStorage.Default.GetAsync(ExpiryKey);

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(expiryRaw))
            return false;

        if (!DateTime.TryParse(expiryRaw, out var expiry))
            return false;

        return expiry > DateTime.UtcNow;
    }
}
