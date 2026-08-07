using System.Collections.ObjectModel;
using Attendance.Interfaces;
using Attendance.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Attendance.ViewModels;

public partial class ScheduleListViewModel : BaseViewModel
{
    private readonly IScheduleRepository _scheduleRepository;

    [ObservableProperty]
    private bool isEmpty;

    public ObservableCollection<Schedule> Schedules { get; } = new();

    public ScheduleListViewModel(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
        Title = "Schedules";
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
            Schedules.Clear();
            foreach (var schedule in schedules)
                Schedules.Add(schedule);

            IsEmpty = Schedules.Count == 0;
        }
        catch (Exception ex)
        {
            SetError($"Unable to load schedules: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task AddScheduleAsync() => await Shell.Current.GoToAsync("ScheduleEditPage");

    [RelayCommand]
    private static async Task EditScheduleAsync(Schedule schedule)
    {
        var parameters = new Dictionary<string, object> { ["ScheduleId"] = schedule.Id };
        await Shell.Current.GoToAsync("ScheduleEditPage", parameters);
    }

    [RelayCommand]
    private async Task PersistToggleAsync(Schedule schedule)
    {
        await _scheduleRepository.SaveAsync(schedule);
    }

    [RelayCommand]
    private async Task DeleteScheduleAsync(Schedule schedule)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is not null)
        {
            var confirmed = await page.DisplayAlert(
                "Delete Schedule",
                $"Delete \"{schedule.Name}\"? This cannot be undone.",
                "Delete", "Cancel");

            if (!confirmed)
                return;
        }

        await _scheduleRepository.DeleteAsync(schedule);
        Schedules.Remove(schedule);
        IsEmpty = Schedules.Count == 0;
    }
}
