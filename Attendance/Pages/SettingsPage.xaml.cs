using Attendance.ViewModels;

namespace Attendance.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnNumericFieldUnfocused(object sender, FocusEventArgs e)
    {
        if (_viewModel.SaveNumericSettingsCommand.CanExecute(null))
            _viewModel.SaveNumericSettingsCommand.Execute(null);
    }
}
