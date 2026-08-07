using System.Reflection;
using Attendance.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Attendance.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly ILoginService _loginService;
    private readonly ISchedulerService _schedulerService;

    [ObservableProperty]
    private string apiBaseUrl = string.Empty;

    [ObservableProperty]
    private string timeoutSeconds = "30";

    [ObservableProperty]
    private string retryCount = "3";

    [ObservableProperty]
    private bool isDarkMode;

    [ObservableProperty]
    private bool notificationsEnabled;

    [ObservableProperty]
    private string backgroundServiceStatus = "Stopped";

    [ObservableProperty]
    private string appVersion = "1.0";

    public SettingsViewModel(
        ISettingsService settingsService,
        ILoginService loginService,
        ISchedulerService schedulerService)
    {
        _settingsService = settingsService;
        _loginService = loginService;
        _schedulerService = schedulerService;
        Title = "Settings";

        ApiBaseUrl = _settingsService.ApiBaseUrl;
        TimeoutSeconds = _settingsService.TimeoutSeconds.ToString();
        RetryCount = _settingsService.RetryCount.ToString();
        IsDarkMode = _settingsService.IsDarkMode;
        NotificationsEnabled = _settingsService.NotificationsEnabled;
        BackgroundServiceStatus = _schedulerService.State.ToString();
        AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "1.0";
    }

    partial void OnApiBaseUrlChanged(string value) => _settingsService.ApiBaseUrl = value;

    partial void OnIsDarkModeChanged(bool value)
    {
        _settingsService.IsDarkMode = value;
        Application.Current!.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
    }

    partial void OnNotificationsEnabledChanged(bool value) => _settingsService.NotificationsEnabled = value;

    [RelayCommand]
    private void SaveNumericSettings()
    {
        if (int.TryParse(TimeoutSeconds, out var timeout) && timeout > 0)
            _settingsService.TimeoutSeconds = timeout;

        if (int.TryParse(RetryCount, out var retries) && retries >= 0)
            _settingsService.RetryCount = retries;
    }

    [RelayCommand]
    private static async Task ShowBatteryGuideAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        await page.DisplayAlert(
            "Battery Optimization",
            "For the scheduler to run reliably in the background, disable battery optimization for this app in your device's system settings.",
            "OK");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _schedulerService.Stop();
        await _loginService.LogoutAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
