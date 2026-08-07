using System.Globalization;
using Attendance.Models;

namespace Attendance.Converters;

public class SchedulerStateToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is SchedulerState state
            ? state switch
            {
                SchedulerState.Running => Color.FromArgb("#22C55E"),
                SchedulerState.Paused => Color.FromArgb("#F59E0B"),
                _ => Color.FromArgb("#9CA3AF")
            }
            : Color.FromArgb("#9CA3AF");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
