using Attendance.Models;

namespace Attendance.Helpers;

public static class WeekdayFormatter
{
    private static readonly (Weekday Flag, string Short)[] Order =
    {
        (Weekday.Sunday, "Sun"),
        (Weekday.Monday, "Mon"),
        (Weekday.Tuesday, "Tue"),
        (Weekday.Wednesday, "Wed"),
        (Weekday.Thursday, "Thu"),
        (Weekday.Friday, "Fri"),
        (Weekday.Saturday, "Sat"),
    };

    public static string Summarize(Weekday days)
    {
        if (days == Weekday.None)
            return "No days selected";

        if (days == Weekday.All)
            return "Every day";

        if (days == Weekday.Weekdays)
            return "Weekdays";

        var selected = Order.Where(o => days.HasFlag(o.Flag)).Select(o => o.Short);
        return string.Join(", ", selected);
    }
}
