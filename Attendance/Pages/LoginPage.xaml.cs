using Attendance.ViewModels;

namespace Attendance.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        VisualStateManager.GoToState(FormLayout, width >= 600 ? "Wide" : "Normal");
    }
}
