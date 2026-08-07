namespace Attendance.Models;

/// <summary>Identifies whether an execution belongs to the morning (clock-in) or evening (clock-out) run.</summary>
public enum ExecutionSession
{
    Morning = 0,
    Evening = 1
}

/// <summary>Outcome of a scheduled execution.</summary>
public enum ExecutionResult
{
    Success = 0,
    Failed = 1,
    Retrying = 2
}
