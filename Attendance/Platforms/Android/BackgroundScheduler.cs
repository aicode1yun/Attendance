using System.Diagnostics.CodeAnalysis;
using Android.Content;
using AndroidX.Work;
using Attendance.Interfaces;
using Java.Util.Concurrent;

namespace Attendance.Services;

/// <summary>
/// Schedules a one-time WorkManager job (via AttendanceWorker) to wake the
/// app at the next execution time even if it's backgrounded or killed.
/// AttendanceWorker re-schedules the following wake-up after it runs, so
/// this chains itself as long as the scheduler is Running.
/// Requires android:minSdkVersion 21+ (already set in the csproj) and the
/// app to be exempted from battery optimization for reliable delivery —
/// see SettingsPage's "Battery Optimization Guide".
/// </summary>
[SuppressMessage("Interoperability", "CA1416", Justification = "Android-only file, compiled only into the android TFM.")]
public class BackgroundScheduler : IBackgroundScheduler
{
    private const string UniqueWorkName = "attendance-scheduled-check";

    public Task ScheduleAsync(DateTime nextExecution)
    {
        var context = Android.App.Application.Context;
        var delay = nextExecution - DateTime.Now;
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        var workRequest = new OneTimeWorkRequest.Builder(typeof(AttendanceWorker))
            .SetInitialDelay((long)delay.TotalMilliseconds, TimeUnit.Milliseconds!)
            .Build();

        WorkManager.GetInstance(context)
            .EnqueueUniqueWork(UniqueWorkName, ExistingWorkPolicy.Replace!, workRequest);

        return Task.CompletedTask;
    }

    public Task CancelAsync()
    {
        var context = Android.App.Application.Context;
        WorkManager.GetInstance(context).CancelUniqueWork(UniqueWorkName);
        return Task.CompletedTask;
    }
}
