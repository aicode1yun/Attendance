using BackgroundTasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Attendance.Interfaces;

namespace Attendance;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    public const string BackgroundTaskIdentifier = "com.companyname.attendance.scheduledcheck";

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIKit.UIApplication application, NSDictionary launchOptions)
    {
        var result = base.FinishedLaunching(application, launchOptions);

        // Registration must happen before FinishedLaunching returns, and only
        // once per process — see Platforms/iOS/BackgroundScheduler.cs for the
        // scheduling side (BGTaskScheduler.Shared.Submit).
        BGTaskScheduler.Shared.Register(BackgroundTaskIdentifier, null, HandleBackgroundTask);

        return result;
    }

    private static void HandleBackgroundTask(BGTask task)
    {
        var processingTask = (BGProcessingTask)task;
        var cancellationTokenSource = new CancellationTokenSource();

        processingTask.ExpirationHandler = () => cancellationTokenSource.Cancel();

        Task.Run(async () =>
        {
            try
            {
                var services = IPlatformApplication.Current?.Services;
                if (services is not null)
                {
                    var scheduler = services.GetRequiredService<ISchedulerService>();
                    await scheduler.TickAsync();

                    if (scheduler.NextExecution is { } next)
                    {
                        var backgroundScheduler = services.GetRequiredService<IBackgroundScheduler>();
                        await backgroundScheduler.ScheduleAsync(next);
                    }
                }

                processingTask.SetTaskCompleted(true);
            }
            catch
            {
                processingTask.SetTaskCompleted(false);
            }
        }, cancellationTokenSource.Token);
    }
}
