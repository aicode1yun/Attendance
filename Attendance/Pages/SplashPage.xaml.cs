using Attendance.Interfaces;

namespace Attendance.Pages;

public partial class SplashPage : ContentPage
{
    private readonly ILoginService _loginService;

    public SplashPage(ILoginService loginService)
    {
        InitializeComponent();
        _loginService = loginService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.WhenAll(
            LogoBadge.FadeTo(1, 400),
            LogoBadge.ScaleTo(1, 400, Easing.SpringOut));

        await Task.WhenAll(
            TitleLabel.FadeTo(1, 300),
            SubtitleLabel.FadeTo(1, 300));

        var isAuthenticated = await _loginService.IsAuthenticatedAsync();

        await Task.Delay(400);

        await Shell.Current.GoToAsync(isAuthenticated ? "//DashboardPage" : "//LoginPage");
    }
}
