using Attendance.ViewModels;

namespace Attendance.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
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
