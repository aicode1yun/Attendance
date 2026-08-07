using System.Text.RegularExpressions;

namespace Attendance.Behaviors;

/// <summary>Restricts an Entry to digits only, used on numeric settings fields.</summary>
public class NumericValidationBehavior : Behavior<Entry>
{
    private static readonly Regex NumericRegex = new("^[0-9]*$", RegexOptions.Compiled);

    protected override void OnAttachedTo(Entry entry)
    {
        entry.TextChanged += OnTextChanged;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnTextChanged;
        base.OnDetachingFrom(entry);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue))
            return;

        if (!NumericRegex.IsMatch(e.NewTextValue) && sender is Entry entry)
        {
            entry.Text = e.OldTextValue;
        }
    }
}
