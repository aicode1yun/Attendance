using Attendance.Database;
using Attendance.Interfaces;
using Attendance.Pages;
using Attendance.Repositories;
using Attendance.Services;
using Attendance.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Attendance;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        RegisterDatabase(builder.Services);
        RegisterServices(builder.Services);
        RegisterViewModels(builder.Services);
        RegisterPages(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegisterDatabase(IServiceCollection services)
    {
        services.AddSingleton<AppDatabase>();
        services.AddSingleton<IScheduleRepository, ScheduleRepository>();
        services.AddSingleton<IExecutionLogRepository, ExecutionLogRepository>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ILoginService, LoginService>();
        services.AddSingleton<IAttendanceService, AttendanceService>();
        services.AddSingleton<ISchedulerService, SchedulerService>();

        services.AddTransient<AuthTokenHandler>();
        services.AddHttpClient(Configuration.AppConfig.HttpClientName)
            .AddHttpMessageHandler<AuthTokenHandler>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ScheduleListViewModel>();
        services.AddTransient<ScheduleEditViewModel>();
        services.AddTransient<LogsViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    private static void RegisterPages(IServiceCollection services)
    {
        services.AddTransient<SplashPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<DashboardPage>();
        services.AddTransient<ScheduleListPage>();
        services.AddTransient<ScheduleEditPage>();
        services.AddTransient<LogsPage>();
        services.AddTransient<SettingsPage>();
    }
}
