using SQLite;

namespace Attendance.Models;

/// <summary>
/// Represents a user-defined attendance schedule: when the morning (clock-in)
/// and evening (clock-out) automation should run, and on which days.
/// </summary>
public class Schedule
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ScheduleType Type { get; set; } = ScheduleType.Daily;

    /// <summary>Stored as the underlying int of the <see cref="Weekday"/> flags enum.</summary>
    public Weekday Days { get; set; } = Weekday.Weekdays;

    /// <summary>Time of day the morning Login → Clock In → Logout flow should run.</summary>
    public TimeSpan MorningTime { get; set; } = new TimeSpan(9, 0, 0);

    /// <summary>Time of day the evening Login → Clock Out → Logout flow should run.</summary>
    public TimeSpan EveningTime { get; set; } = new TimeSpan(18, 0, 0);

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public string DaysSummary => Helpers.WeekdayFormatter.Summarize(Days);
}
