using Attendance.ViewModels;

namespace Attendance.Pages;

public partial class LogsPage : ContentPage
{
    private readonly LogsViewModel _viewModel;

    public LogsPage(LogsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.AppearingCommand.CanExecute(null))
            await _viewModel.AppearingCommand.ExecuteAsync(null);
    }
}
