using Attendance.Interfaces;
using Attendance.Models;

namespace Attendance.Services;

/// <summary>
/// Owns run state (Running / Paused / Stopped), computes the next scheduled
/// execution, and — while the app is in the foreground — actually triggers
/// IAttendanceOrchestrator at the right time via a one-minute timer. For
/// execution while the app is backgrounded/killed, IBackgroundScheduler
/// schedules a platform wake-up (Android WorkManager / iOS BGTaskScheduler)
/// that calls the same orchestrator independently — see
/// Platforms/{Platform}/BackgroundScheduler.cs and AI-BRAIN.md.
/// </summary>
public class SchedulerService : ISchedulerService, IDisposable
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IAttendanceOrchestrator _orchestrator;
    private readonly IBackgroundScheduler _backgroundScheduler;

    private Timer? _timer;
    private readonly HashSet<string> _executedToday = new();
    private DateTime _executedTodayDate = DateTime.UtcNow.Date;
    private bool _isExecuting;

    public SchedulerService(
        IScheduleRepository scheduleRepository,
        IAttendanceOrchestrator orchestrator,
        IBackgroundScheduler backgroundScheduler)
    {
        _scheduleRepository = scheduleRepository;
        _orchestrator = orchestrator;
        _backgroundScheduler = backgroundScheduler;
    }

    public SchedulerState State { get; private set; } = SchedulerState.Stopped;

    public DateTime? NextExecution { get; private set; }

    public event EventHandler<SchedulerState>? StateChanged;
    public event EventHandler<ExecutionLog>? ExecutionCompleted;

    public void Start()
    {
        State = SchedulerState.Running;
        StateChanged?.Invoke(this, State);
        _timer ??= new Timer(async _ => await TickAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
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
        _timer?.Dispose();
        _timer = null;
        StateChanged?.Invoke(this, State);
        _ = _backgroundScheduler.CancelAsync();
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

        if (earliest is not null)
            await _backgroundScheduler.ScheduleAsync(earliest.Value);
    }

    /// <summary>
    /// Runs every 30s while the scheduler is Running: finds any enabled
    /// schedule whose morning/evening time has arrived and hasn't already run
    /// today, executes it via the orchestrator, and refreshes NextExecution.
    /// Public so platform background-wake handlers can also invoke a single
    /// pass on demand (see Platforms/Android/AttendanceWorker.cs).
    /// </summary>
    public async Task TickAsync()
    {
        if (_isExecuting || State != SchedulerState.Running)
            return;

        var today = DateTime.UtcNow.Date;
        if (today != _executedTodayDate)
        {
            _executedToday.Clear();
            _executedTodayDate = today;
        }

        _isExecuting = true;
        try
        {
            var schedules = await _scheduleRepository.GetAllAsync();
            var now = DateTime.Now;

            foreach (var schedule in schedules.Where(s => s.IsEnabled))
            {
                var weekday = ToWeekdayFlag(now.DayOfWeek);
                if (!schedule.Days.HasFlag(weekday))
                    continue;

                await TryRunIfDueAsync(schedule, ExecutionSession.Morning, schedule.MorningTime, now);
                await TryRunIfDueAsync(schedule, ExecutionSession.Evening, schedule.EveningTime, now);
            }

            await RefreshNextExecutionAsync();
        }
        finally
        {
            _isExecuting = false;
        }
    }

    private async Task TryRunIfDueAsync(Schedule schedule, ExecutionSession session, TimeSpan dueTime, DateTime now)
    {
        var key = $"{schedule.Id}-{now:yyyy-MM-dd}-{session}";
        if (_executedToday.Contains(key))
            return;

        var due = now.Date + dueTime;
        // Due window: from the exact time up to 5 minutes after, so a 30s
        // tick cadence never misses it, but we don't fire hours late either.
        if (now < due || now > due.AddMinutes(5))
            return;

        _executedToday.Add(key);

        var log = await _orchestrator.ExecuteAsync(schedule, session);
        ExecutionCompleted?.Invoke(this, log);
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

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
