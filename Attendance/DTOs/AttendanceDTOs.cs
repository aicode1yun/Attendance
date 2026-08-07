namespace Attendance.DTOs;

public class ClockInRequest
{
    public string Token { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ClockOutRequest
{
    public string Token { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class LogoutRequest
{
    public string Token { get; set; } = string.Empty;
}

/// <summary>Generic result wrapper returned by attendance operations.</summary>
public class AttendanceResult
{
    public bool Success { get; set; }
    public int? HttpStatus { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public double DurationMs { get; set; }
}
