using Attendance.Interfaces;
using Attendance.Models;
using Plugin.LocalNotification;

namespace Attendance.Services;

/// <summary>
/// Fires a local device notification summarizing a scheduled execution.
/// Only called when ISettingsService.NotificationsEnabled is true (checked
/// by the caller — AttendanceOrchestrator).
/// </summary>
public class NotificationService : INotificationService
{
    public async Task NotifyExecutionResultAsync(ExecutionLog log)
    {
        var sessionLabel = log.Session == ExecutionSession.Morning ? "Morning Clock-In" : "Evening Clock-Out";
        var isSuccess = log.Result == ExecutionResult.Success;

        var request = new NotificationRequest
        {
            NotificationId = (int)DateTime.UtcNow.Ticks % int.MaxValue,
            Title = isSuccess ? $"{sessionLabel} succeeded" : $"{sessionLabel} failed",
            Description = isSuccess
                ? $"Completed at {log.Time:h:mm tt} ({log.DurationMs:0} ms)."
                : log.ErrorMessage ?? "Please check the Logs page for details.",
            ReturningData = "execution_log",
        };

        try
        {
            await LocalNotificationCenter.Current.Show(request);
        }
        catch
        {
            // Notifications are a courtesy, not core functionality — never let a
            // notification failure affect the execution result or crash the run.
        }
    }
}
