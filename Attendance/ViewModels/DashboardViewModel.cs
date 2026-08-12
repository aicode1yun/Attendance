using System.Collections.ObjectModel;
using Attendance.Interfaces;
using Attendance.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Attendance.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ISchedulerService _schedulerService;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IExecutionLogRepository _logRepository;

    [ObservableProperty]
    private SchedulerState schedulerState;

    [ObservableProperty]
    private string nextExecutionText = "No upcoming execution";

    [ObservableProperty]
    private string todaysScheduleText = "No schedule configured";

    [ObservableProperty]
    private string todaysResultText = "Not run yet";

    [ObservableProperty]
    private int activeScheduleCount;

    [ObservableProperty]
    private bool isEmpty;

    public ObservableCollection<ExecutionLog> RecentLogs { get; } = new();

    public DashboardViewModel(
        ISchedulerService schedulerService,
        IScheduleRepository scheduleRepository,
        IExecutionLogRepository logRepository)
    {
        _schedulerService = schedulerService;
        _scheduleRepository = scheduleRepository;
        _logRepository = logRepository;
        Title = "Dashboard";

        _schedulerService.StateChanged += (_, state) => SchedulerState = state;
        _schedulerService.ExecutionCompleted += async (_, _) => await AppearingAsync();
        SchedulerState = _schedulerService.State;
    }

    [RelayCommand]
    private async Task AppearingAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var schedules = await _scheduleRepository.GetAllAsync();
            var enabled = schedules.Where(s => s.IsEnabled).ToList();
            ActiveScheduleCount = enabled.Count;
            IsEmpty = schedules.Count == 0;

            TodaysScheduleText = enabled.Count switch
            {
                0 => "No active schedule for today",
                1 => $"{enabled[0].Name} · {enabled[0].DaysSummary}",
                _ => $"{enabled.Count} active schedules"
            };

            await _schedulerService.RefreshNextExecutionAsync();
            NextExecutionText = _schedulerService.NextExecution is { } next
                ? next.ToString("ddd, MMM d 'at' h:mm tt")
                : "No upcoming execution";

            var recent = await _logRepository.GetRecentAsync(10);
            RecentLogs.Clear();
            foreach (var log in recent)
                RecentLogs.Add(log);

            TodaysResultText = recent.FirstOrDefault(l => l.Date.Date == DateTime.UtcNow.Date) is { } todayLog
                ? todayLog.Result.ToString()
                : "Not run yet";
        }
        catch (Exception ex)
        {
            SetError($"Unable to load dashboard: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EnableSchedulerAsync()
    {
        _schedulerService.Start();
        await _schedulerService.RefreshNextExecutionAsync();
        NextExecutionText = _schedulerService.NextExecution is { } next
            ? next.ToString("ddd, MMM d 'at' h:mm tt")
            : "No upcoming execution";
    }

    [RelayCommand]
    private void PauseScheduler() => _schedulerService.Pause();

    [RelayCommand]
    private void ResumeScheduler() => _schedulerService.Resume();

    [RelayCommand]
    private void StopScheduler()
    {
        _schedulerService.Stop();
        NextExecutionText = "No upcoming execution";
    }

    [RelayCommand]
    private static async Task GoToSchedulesAsync() => await Shell.Current.GoToAsync("//SchedulesPage");
}
