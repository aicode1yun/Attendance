using System.Globalization;

namespace Attendance.Converters;

public class BoolToShowHideConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool hidden && hidden ? "Show" : "Hide";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
