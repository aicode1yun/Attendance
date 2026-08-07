using System.Globalization;
using Attendance.Models;

namespace Attendance.Converters;

public class ExecutionResultToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is ExecutionResult result
            ? result switch
            {
                ExecutionResult.Success => Color.FromArgb("#22C55E"),
                ExecutionResult.Retrying => Color.FromArgb("#F59E0B"),
                _ => Color.FromArgb("#EF4444")
            }
            : Color.FromArgb("#9CA3AF");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
