using Attendance.Interfaces;
using Attendance.Models;

namespace Attendance.Services;

/// <summary>
/// Phase 1 foundation for the scheduler: owns run state (Running / Paused /
/// Stopped) and can compute the next scheduled execution from the schedules
/// stored in SQLite so the Dashboard has real data to display.
/// Actual unattended background execution (Android WorkManager / iOS
/// BGTaskScheduler, survives reboot) is implemented in Phase 2 - see
/// AI-BRAIN.md and ROADMAP.md.
/// </summary>
public class SchedulerService : ISchedulerService
{
    private readonly IScheduleRepository _scheduleRepository;

    public SchedulerService(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public SchedulerState State { get; private set; } = SchedulerState.Stopped;

    public DateTime? NextExecution { get; private set; }

    public event EventHandler<SchedulerState>? StateChanged;

    public void Start()
    {
        State = SchedulerState.Running;
        StateChanged?.Invoke(this, State);
    }

    public void Pause()
    {
        if (State != SchedulerState.Running)
            return;

        State = SchedulerState.Paused;
        StateChanged?.Invoke(this, State);
    }

    public void Resume()
    {
        if (State != SchedulerState.Paused)
            return;

        State = SchedulerState.Running;
        StateChanged?.Invoke(this, State);
    }

    public void Stop()
    {
        State = SchedulerState.Stopped;
        NextExecution = null;
        StateChanged?.Invoke(this, State);
    }

    public async Task RefreshNextExecutionAsync()
    {
        var schedules = await _scheduleRepository.GetAllAsync();
        var enabled = schedules.Where(s => s.IsEnabled).ToList();

        if (State != SchedulerState.Running || enabled.Count == 0)
        {
            NextExecution = null;
            return;
        }

        DateTime? earliest = null;
        var now = DateTime.Now;

        foreach (var schedule in enabled)
        {
            foreach (var candidate in NextOccurrences(schedule, now))
            {
                if (earliest is null || candidate < earliest)
                    earliest = candidate;
            }
        }

        NextExecution = earliest;
    }

    private static IEnumerable<DateTime> NextOccurrences(Schedule schedule, DateTime now)
    {
        for (var dayOffset = 0; dayOffset <= 7; dayOffset++)
        {
            var date = now.Date.AddDays(dayOffset);
            var weekday = ToWeekdayFlag(date.DayOfWeek);

            if (!schedule.Days.HasFlag(weekday))
                continue;

            var morning = date + schedule.MorningTime;
            var evening = date + schedule.EveningTime;

            if (morning > now)
                yield return morning;

            if (evening > now)
                yield return evening;
        }
    }

    private static Weekday ToWeekdayFlag(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Sunday => Weekday.Sunday,
        DayOfWeek.Monday => Weekday.Monday,
        DayOfWeek.Tuesday => Weekday.Tuesday,
        DayOfWeek.Wednesday => Weekday.Wednesday,
        DayOfWeek.Thursday => Weekday.Thursday,
        DayOfWeek.Friday => Weekday.Friday,
        DayOfWeek.Saturday => Weekday.Saturday,
        _ => Weekday.None
    };
}
