using Attendance.Models;
using Attendance.ViewModels;

namespace Attendance.Pages;

public partial class ScheduleListPage : ContentPage
{
    private readonly ScheduleListViewModel _viewModel;

    public ScheduleListPage(ScheduleListViewModel viewModel)
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

    private async void OnScheduleEnabledToggled(object? sender, ToggledEventArgs e)
    {
        if (sender is Switch { BindingContext: Schedule schedule } &&
            _viewModel.PersistToggleCommand.CanExecute(schedule))
        {
            await _viewModel.PersistToggleCommand.ExecuteAsync(schedule);
        }
    }
}
