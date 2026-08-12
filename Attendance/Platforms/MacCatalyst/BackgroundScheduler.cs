using Attendance.Interfaces;

namespace Attendance.Services;

/// <summary>
/// Mac Catalyst apps are typically long-running desktop processes, so this
/// intentionally does not register OS-level background wake-ups: while the
/// app is running, SchedulerService's own 30s foreground timer already
/// covers execution timing. If truly execution-while-fully-quit is required
/// on this platform, BackgroundTasks/BGTaskScheduler is available on Mac
/// Catalyst (mirroring Platforms/iOS/BackgroundScheduler.cs) but needs its
/// own AppDelegate registration + entitlement — left as a follow-up rather
/// than shipped unverified.
/// </summary>
public class BackgroundScheduler : IBackgroundScheduler
{
    public Task ScheduleAsync(DateTime nextExecution) => Task.CompletedTask;

    public Task CancelAsync() => Task.CompletedTask;
}
