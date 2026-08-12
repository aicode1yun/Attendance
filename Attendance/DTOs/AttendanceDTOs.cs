namespace Attendance.DTOs;

public class ClockInRequest
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ClockOutRequest
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>Raw shape returned by /api/attendance/clock-in and /api/attendance/clock-out.</summary>
public class ClockResponse
{
    public bool Success { get; set; }
    public string? RequestId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>Logout has no request body; the token is sent via the Authorization header.</summary>
public class LogoutRequest
{
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
