namespace Attendance.Models;

/// <summary>
/// Bit-flag representation of the days of the week so a schedule can target
/// any combination of days with a single stored integer value.
/// </summary>
[Flags]
public enum Weekday
{
    None = 0,
    Sunday = 1 << 0,
    Monday = 1 << 1,
    Tuesday = 1 << 2,
    Wednesday = 1 << 3,
    Thursday = 1 << 4,
    Friday = 1 << 5,
    Saturday = 1 << 6,
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
    All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday
}
