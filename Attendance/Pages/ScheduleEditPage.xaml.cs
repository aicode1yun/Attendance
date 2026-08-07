using Attendance.ViewModels;

namespace Attendance.Pages;

public partial class ScheduleEditPage : ContentPage
{
    public ScheduleEditPage(ScheduleEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
