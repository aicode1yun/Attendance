using Attendance.DTOs;
using Attendance.Models;

namespace Attendance.Interfaces;

public interface ILoginService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Ensures a valid (non-expired) access token is available, refreshing it
    /// via the stored refresh token if needed. Returns false if the caller
    /// must sign in again (no refresh token, or the refresh call failed).
    /// </summary>
    Task<bool> EnsureValidTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Securely stores credentials for unattended re-login, used only when the
    /// user opts in via "Remember Me". Required because the orchestrator logs
    /// out completely after every run (per MASTER-SPEC's Login → Clock →
    /// Logout sequence), so the *next* run needs a fresh login, not just a
    /// refreshed token.
    /// </summary>
    Task SaveCredentialsAsync(string email, string password);

    Task ClearStoredCredentialsAsync();

    Task<bool> HasStoredCredentialsAsync();

    /// <summary>Logs in using previously saved credentials. Fails if none are stored.</summary>
    Task<LoginResponse> LoginWithStoredCredentialsAsync(CancellationToken cancellationToken = default);
}

public interface IAttendanceService
{
    Task<AttendanceResult> ClockInAsync(CancellationToken cancellationToken = default);
    Task<AttendanceResult> ClockOutAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Executes the full Login → Clock In/Out → Logout sequence for a single
/// schedule + session, applying retry policy and writing the ExecutionLog.
/// This is the only thing SchedulerService (foreground) and the platform
/// background schedulers (Android WorkManager / iOS BGTaskScheduler) call.
/// </summary>
public interface IAttendanceOrchestrator
{
    Task<ExecutionLog> ExecuteAsync(Schedule schedule, ExecutionSession session, CancellationToken cancellationToken = default);
}

public interface IRetryPolicy
{
    /// <summary>
    /// Runs <paramref name="action"/>, retrying up to <paramref name="maxRetries"/>
    /// additional times with exponential backoff when it throws or returns a
    /// result <paramref name="isSuccess"/> deems unsuccessful. Returns the last
    /// result/attempt count regardless of outcome.
    /// </summary>
    Task<(T Result, int AttemptsUsed)> ExecuteAsync<T>(
        Func<Task<T>> action,
        Func<T, bool> isSuccess,
        int maxRetries,
        CancellationToken cancellationToken = default);
}

public interface ISchedulerService
{
    SchedulerState State { get; }
    DateTime? NextExecution { get; }

    event EventHandler<SchedulerState>? StateChanged;
    event EventHandler<ExecutionLog>? ExecutionCompleted;

    void Start();
    void Pause();
    void Resume();
    void Stop();
    Task RefreshNextExecutionAsync();

    /// <summary>
    /// Checks all enabled schedules once and runs any that are due. Called on
    /// the internal 30s foreground timer, and directly by each platform's
    /// background wake-up handler (Android WorkManager Worker, iOS
    /// BGTaskScheduler handler) so behaviour is identical either way.
    /// </summary>
    Task TickAsync();
}

/// <summary>
/// Schedules the app's process to be woken up (Android WorkManager / iOS
/// BGTaskScheduler) so attendance can run even when the app isn't in the
/// foreground. Each platform provides its own implementation under
/// Platforms/{Platform}/BackgroundScheduler.cs; MAUI's multi-targeting
/// compiles only the matching one into each build.
/// </summary>
public interface IBackgroundScheduler
{
    /// <summary>Schedules (or reschedules) background wake-ups for the next known executions.</summary>
    Task ScheduleAsync(DateTime nextExecution);

    /// <summary>Cancels any pending background wake-ups.</summary>
    Task CancelAsync();
}

public interface INotificationService
{
    Task NotifyExecutionResultAsync(ExecutionLog log);
}

public interface ISettingsService
{
    string ApiBaseUrl { get; set; }
    int TimeoutSeconds { get; set; }
    int RetryCount { get; set; }
    bool IsDarkMode { get; set; }
    bool NotificationsEnabled { get; set; }
    bool RememberMe { get; set; }
}
