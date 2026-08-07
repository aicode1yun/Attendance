using Attendance.Interfaces;
using Attendance.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Attendance.ViewModels;

[QueryProperty(nameof(ScheduleId), "ScheduleId")]
public partial class ScheduleEditViewModel : BaseViewModel
{
    private readonly IScheduleRepository _scheduleRepository;
    private Schedule _schedule = new();

    [ObservableProperty]
    private int scheduleId;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool isWeekly = true;

    [ObservableProperty]
    private TimeSpan morningTime = new(9, 0, 0);

    [ObservableProperty]
    private TimeSpan eveningTime = new(18, 0, 0);

    [ObservableProperty]
    private bool isEnabled = true;

    [ObservableProperty]
    private bool isSunday;

    [ObservableProperty]
    private bool isMonday = true;

    [ObservableProperty]
    private bool isTuesday = true;

    [ObservableProperty]
    private bool isWednesday = true;

    [ObservableProperty]
    private bool isThursday = true;

    [ObservableProperty]
    private bool isFriday = true;

    [ObservableProperty]
    private bool isSaturday;

    public bool IsNew => _schedule.Id == 0;

    public ScheduleEditViewModel(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
        Title = "New Schedule";
    }

    async partial void OnScheduleIdChanged(int value)
    {
        if (value <= 0)
        {
            _schedule = new Schedule();
            Title = "New Schedule";
            return;
        }

        var existing = await _scheduleRepository.GetByIdAsync(value);
        if (existing is null)
            return;

        _schedule = existing;
        Title = "Edit Schedule";

        Name = existing.Name;
        IsWeekly = existing.Type == ScheduleType.Weekly;
        MorningTime = existing.MorningTime;
        EveningTime = existing.EveningTime;
        IsEnabled = existing.IsEnabled;

        IsSunday = existing.Days.HasFlag(Weekday.Sunday);
        IsMonday = existing.Days.HasFlag(Weekday.Monday);
        IsTuesday = existing.Days.HasFlag(Weekday.Tuesday);
        IsWednesday = existing.Days.HasFlag(Weekday.Wednesday);
        IsThursday = existing.Days.HasFlag(Weekday.Thursday);
        IsFriday = existing.Days.HasFlag(Weekday.Friday);
        IsSaturday = existing.Days.HasFlag(Weekday.Saturday);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
            return;

        ClearError();

        if (string.IsNullOrWhiteSpace(Name))
        {
            SetError("Please give this schedule a name.");
            return;
        }

        var days = BuildSelectedDays();
        if (days == Weekday.None)
        {
            SetError("Select at least one day.");
            return;
        }

        try
        {
            IsBusy = true;

            _schedule.Name = Name.Trim();
            _schedule.Type = IsWeekly ? ScheduleType.Weekly : ScheduleType.Daily;
            _schedule.Days = IsWeekly ? days : Weekday.All;
            _schedule.MorningTime = MorningTime;
            _schedule.EveningTime = EveningTime;
            _schedule.IsEnabled = IsEnabled;

            await _scheduleRepository.SaveAsync(_schedule);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            SetError($"Unable to save schedule: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task CancelAsync() => await Shell.Current.GoToAsync("..");

    private Weekday BuildSelectedDays()
    {
        var days = Weekday.None;
        if (IsSunday) days |= Weekday.Sunday;
        if (IsMonday) days |= Weekday.Monday;
        if (IsTuesday) days |= Weekday.Tuesday;
        if (IsWednesday) days |= Weekday.Wednesday;
        if (IsThursday) days |= Weekday.Thursday;
        if (IsFriday) days |= Weekday.Friday;
        if (IsSaturday) days |= Weekday.Saturday;
        return days;
    }
}
