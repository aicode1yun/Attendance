using BackgroundTasks;
using Foundation;
using Attendance.Interfaces;

namespace Attendance.Services;

/// <summary>
/// Submits a BGProcessingTaskRequest so iOS wakes the app near the next
/// execution time. iOS does not guarantee exact timing for background tasks
/// (it batches wake-ups for battery reasons) — this is a best-effort
/// mechanism, same as on Android. The identifier here must match
/// AppDelegate.BackgroundTaskIdentifier and the
/// BGTaskSchedulerPermittedIdentifiers entry in Info.plist.
/// </summary>
public class BackgroundScheduler : IBackgroundScheduler
{
    public Task ScheduleAsync(DateTime nextExecution)
    {
        BGTaskScheduler.Shared.CancelAllTaskRequests();

        var request = new BGProcessingTaskRequest(AppDelegate.BackgroundTaskIdentifier)
        {
            EarliestBeginDate = (NSDate)nextExecution.ToUniversalTime(),
            RequiresNetworkConnectivity = true,
            RequiresExternalPower = false,
        };

        BGTaskScheduler.Shared.Submit(request, out _);

        return Task.CompletedTask;
    }

    public Task CancelAsync()
    {
        BGTaskScheduler.Shared.CancelAllTaskRequests();
        return Task.CompletedTask;
    }
}
