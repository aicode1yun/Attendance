using Attendance.DTOs;
using Attendance.Models;

namespace Attendance.Interfaces;

public interface ILoginService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<bool> IsAuthenticatedAsync();
}

public interface IAttendanceService
{
    Task<AttendanceResult> ClockInAsync(CancellationToken cancellationToken = default);
    Task<AttendanceResult> ClockOutAsync(CancellationToken cancellationToken = default);
}

public interface ISchedulerService
{
    SchedulerState State { get; }
    DateTime? NextExecution { get; }

    event EventHandler<SchedulerState>? StateChanged;

    void Start();
    void Pause();
    void Resume();
    void Stop();
    Task RefreshNextExecutionAsync();
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
