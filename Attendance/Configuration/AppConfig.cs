namespace Attendance.Configuration;

public static class AppConfig
{
    public const string HttpClientName = "AttendanceApi";
    public const string DefaultApiBaseUrl = "https://api.example.com";
    public const int DefaultTimeoutSeconds = 30;
    public const int DefaultRetryCount = 3;
}
