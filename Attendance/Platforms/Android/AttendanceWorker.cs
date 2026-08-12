using Android.Content;
using AndroidX.Work;
using Attendance.Interfaces;
using Java.Util.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Attendance.Services;

/// <summary>
/// Runs on a WorkManager background thread when the OS wakes the app at a
/// scheduled attendance time. Resolves the same DI container the running
/// app uses (MauiApplication keeps it alive as long as the process exists;
/// MAUI's SingleProject app also re-initializes it on a cold background
/// start) so the exact same ISchedulerService/IAttendanceOrchestrator code
/// path runs whether the app is foregrounded or not.
/// </summary>
public class AttendanceWorker : Worker
{
    public AttendanceWorker(Context context, WorkerParameters workerParameters)
        : base(context, workerParameters)
    {
    }

    public override Result DoWork()
    {
        try
        {
            var services = IPlatformApplication.Current?.Services;
            if (services is null)
                return Result.InvokeRetry()!;

            var scheduler = services.GetRequiredService<ISchedulerService>();

            // Worker.DoWork runs on a background thread and must be synchronous;
            // blocking here is the documented WorkManager pattern for async work.
            scheduler.TickAsync().GetAwaiter().GetResult();

            return Result.InvokeSuccess()!;
        }
        catch
        {
            return Result.InvokeRetry()!;
        }
    }
}
