using System.Diagnostics;
using Attendance.DTOs;
using Attendance.Interfaces;
using Attendance.Models;

namespace Attendance.Services;

/// <summary>
/// The single place that runs a scheduled attendance execution end to end.
/// Called by SchedulerService (foreground timer) and by each platform's
/// IBackgroundScheduler wake-up handler, so behaviour is identical either
/// way: ensure a valid token (refreshing/logging in if needed) → clock
/// in/out with retry → write one ExecutionLog row → optionally notify.
/// </summary>
public class AttendanceOrchestrator : IAttendanceOrchestrator
{
    private readonly ILoginService _loginService;
    private readonly IAttendanceService _attendanceService;
    private readonly IExecutionLogRepository _logRepository;
    private readonly ISettingsService _settingsService;
    private readonly IRetryPolicy _retryPolicy;
    private readonly INotificationService _notificationService;

    public AttendanceOrchestrator(
        ILoginService loginService,
        IAttendanceService attendanceService,
        IExecutionLogRepository logRepository,
        ISettingsService settingsService,
        IRetryPolicy retryPolicy,
        INotificationService notificationService)
    {
        _loginService = loginService;
        _attendanceService = attendanceService;
        _logRepository = logRepository;
        _settingsService = settingsService;
        _retryPolicy = retryPolicy;
        _notificationService = notificationService;
    }

    public async Task<ExecutionLog> ExecuteAsync(Schedule schedule, ExecutionSession session, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var maxRetries = Math.Max(0, _settingsService.RetryCount);

        var log = new ExecutionLog
        {
            ScheduleId = schedule.Id,
            Date = DateTime.UtcNow.Date,
            Time = DateTime.UtcNow,
            Session = session,
        };

        try
        {
            var hasValidToken = await _loginService.EnsureValidTokenAsync(cancellationToken);
            if (!hasValidToken)
            {
                log.Result = ExecutionResult.Failed;
                log.ErrorMessage = "Not signed in and no refresh token available. Open the app and sign in.";
                log.RequestId = Guid.NewGuid().ToString("N");
                return log;
            }

            var (result, attempts) = await _retryPolicy.ExecuteAsync(
                action: () => session == ExecutionSession.Morning
                    ? _attendanceService.ClockInAsync(cancellationToken)
                    : _attendanceService.ClockOutAsync(cancellationToken),
                isSuccess: r => r.Success,
                maxRetries: maxRetries,
                cancellationToken: cancellationToken);

            log.Result = result.Success ? ExecutionResult.Success : ExecutionResult.Failed;
            log.HttpStatus = result.HttpStatus;
            log.RequestId = string.IsNullOrWhiteSpace(result.RequestId) ? Guid.NewGuid().ToString("N") : result.RequestId;
            log.ErrorMessage = result.Success ? null : result.ErrorMessage;
            log.RetryCount = attempts - 1;

            // Logout after every attempt (success or exhausted retries), matching
            // MASTER-SPEC's Login -> Clock -> Logout sequence. Best-effort.
            await _loginService.LogoutAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            log.Result = ExecutionResult.Failed;
            log.ErrorMessage = ex.Message;
            log.RequestId = Guid.NewGuid().ToString("N");
        }
        finally
        {
            stopwatch.Stop();
            log.DurationMs = stopwatch.Elapsed.TotalMilliseconds;
        }

        await _logRepository.SaveAsync(log);

        if (_settingsService.NotificationsEnabled)
            await _notificationService.NotifyExecutionResultAsync(log);

        return log;
    }
}
