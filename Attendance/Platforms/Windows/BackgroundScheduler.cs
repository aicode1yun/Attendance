using Attendance.Interfaces;

namespace Attendance.Services;

/// <summary>
/// Windows apps are typically kept running on desktop (or in the system
/// tray); this intentionally does not register a Windows Background Task
/// (which needs its own manifest capability + a separate Runtime Component
/// project) since it can't be verified in this environment. While the app
/// process is alive, SchedulerService's own 30s foreground timer covers
/// execution timing. Revisit with a real Background Task Builder
/// registration if execution-while-fully-closed is required on Windows.
/// </summary>
public class BackgroundScheduler : IBackgroundScheduler
{
    public Task ScheduleAsync(DateTime nextExecution) => Task.CompletedTask;

    public Task CancelAsync() => Task.CompletedTask;
}
