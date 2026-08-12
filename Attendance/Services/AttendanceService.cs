using System.Diagnostics;
using System.Net.Http.Json;
using Attendance.Configuration;
using Attendance.DTOs;
using Attendance.Interfaces;

namespace Attendance.Services;

/// <summary>
/// Performs the individual clock-in / clock-out HTTP calls described in
/// API-CONTRACT.md. Never called directly by the scheduler —
/// IAttendanceOrchestrator orchestrates the Login → Clock In/Out → Logout
/// sequence and calls this service for the middle step only.
/// </summary>
public class AttendanceService : IAttendanceService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingsService _settingsService;

    public AttendanceService(IHttpClientFactory httpClientFactory, ISettingsService settingsService)
    {
        _httpClientFactory = httpClientFactory;
        _settingsService = settingsService;
    }

    public Task<AttendanceResult> ClockInAsync(CancellationToken cancellationToken = default) =>
        CallAsync(ApiEndpoints.ClockIn, new ClockInRequest { Timestamp = DateTime.UtcNow }, cancellationToken);

    public Task<AttendanceResult> ClockOutAsync(CancellationToken cancellationToken = default) =>
        CallAsync(ApiEndpoints.ClockOut, new ClockOutRequest { Timestamp = DateTime.UtcNow }, cancellationToken);

    private async Task<AttendanceResult> CallAsync<TRequest>(string path, TRequest body, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var client = _httpClientFactory.CreateClient(AppConfig.HttpClientName);
        client.BaseAddress = new Uri(_settingsService.ApiBaseUrl, UriKind.Absolute);
        client.Timeout = TimeSpan.FromSeconds(_settingsService.TimeoutSeconds);

        try
        {
            using var response = await client.PostAsJsonAsync(path, body, cancellationToken);
            stopwatch.Stop();

            var parsed = await response.Content.ReadFromJsonAsync<ClockResponse>(cancellationToken: cancellationToken);

            return new AttendanceResult
            {
                Success = response.IsSuccessStatusCode && (parsed?.Success ?? false),
                HttpStatus = (int)response.StatusCode,
                RequestId = parsed?.RequestId ?? string.Empty,
                ErrorMessage = response.IsSuccessStatusCode ? parsed?.ErrorMessage : $"HTTP {(int)response.StatusCode}: {parsed?.ErrorMessage ?? response.ReasonPhrase}",
                DurationMs = stopwatch.Elapsed.TotalMilliseconds
            };
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            return new AttendanceResult
            {
                Success = false,
                ErrorMessage = "Request timed out.",
                DurationMs = stopwatch.Elapsed.TotalMilliseconds
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return new AttendanceResult
            {
                Success = false,
                ErrorMessage = $"Network error: {ex.Message}",
                DurationMs = stopwatch.Elapsed.TotalMilliseconds
            };
        }
    }
}
