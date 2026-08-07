using Attendance.Interfaces;

namespace Attendance.Services;

/// <summary>
/// Wraps <see cref="Preferences"/> for non-sensitive app configuration.
/// Sensitive values (tokens, credentials) live in <see cref="SecureStorage"/>
/// and are handled by <see cref="ILoginService"/> instead.
/// </summary>
public class SettingsService : ISettingsService
{
    private const string ApiBaseUrlKey = "settings.api_base_url";
    private const string TimeoutSecondsKey = "settings.timeout_seconds";
    private const string RetryCountKey = "settings.retry_count";
    private const string DarkModeKey = "settings.dark_mode";
    private const string NotificationsKey = "settings.notifications_enabled";
    private const string RememberMeKey = "settings.remember_me";

    public string ApiBaseUrl
    {
        get => Preferences.Get(ApiBaseUrlKey, "https://api.example.com");
        set => Preferences.Set(ApiBaseUrlKey, value);
    }

    public int TimeoutSeconds
    {
        get => Preferences.Get(TimeoutSecondsKey, 30);
        set => Preferences.Set(TimeoutSecondsKey, value);
    }

    public int RetryCount
    {
        get => Preferences.Get(RetryCountKey, 3);
        set => Preferences.Set(RetryCountKey, value);
    }

    public bool IsDarkMode
    {
        get => Preferences.Get(DarkModeKey, Application.Current?.RequestedTheme == AppTheme.Dark);
        set => Preferences.Set(DarkModeKey, value);
    }

    public bool NotificationsEnabled
    {
        get => Preferences.Get(NotificationsKey, true);
        set => Preferences.Set(NotificationsKey, value);
    }

    public bool RememberMe
    {
        get => Preferences.Get(RememberMeKey, false);
        set => Preferences.Set(RememberMeKey, value);
    }
}
