namespace Attendance.Configuration;

/// <summary>Relative endpoint paths, appended to ISettingsService.ApiBaseUrl. See API-CONTRACT.md.</summary>
public static class ApiEndpoints
{
    public const string Login = "/api/auth/login";
    public const string Refresh = "/api/auth/refresh";
    public const string Logout = "/api/auth/logout";
    public const string ClockIn = "/api/attendance/clock-in";
    public const string ClockOut = "/api/attendance/clock-out";
}
