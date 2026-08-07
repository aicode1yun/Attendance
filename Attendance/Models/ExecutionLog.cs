using SQLite;

namespace Attendance.Models;

/// <summary>
/// A single recorded execution attempt (morning or evening run) with full
/// diagnostic detail, as required by MASTER-SPEC's logging section.
/// </summary>
public class ExecutionLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    public DateTime Time { get; set; } = DateTime.UtcNow;

    public ExecutionSession Session { get; set; }

    public ExecutionResult Result { get; set; }

    public int? HttpStatus { get; set; }

    public double DurationMs { get; set; }

    public string RequestId { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }
}
