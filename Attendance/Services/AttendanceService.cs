using System.Diagnostics;
using Attendance.DTOs;
using Attendance.Interfaces;

namespace Attendance.Services;

/// <summary>
/// Performs the individual clock-in / clock-out operations. Never called
/// directly by the scheduler - <see cref="ISchedulerService"/> orchestrates
/// the Login → Clock In/Out → Logout sequence and calls this service for the
/// middle step only, per MASTER-SPEC's "never expose API calls directly
/// inside scheduler" rule.
/// Phase 1 simulates the call so logging, dashboard and history views can be
/// built and verified; Phase 2 replaces the body with a real HttpClient call.
/// </summary>
public class AttendanceService : IAttendanceService
{
    public async Task<AttendanceResult> ClockInAsync(CancellationToken cancellationToken = default)
    {
        return await SimulateCallAsync(cancellationToken);
    }

    public async Task<AttendanceResult> ClockOutAsync(CancellationToken cancellationToken = default)
    {
        return await SimulateCallAsync(cancellationToken);
    }

    private static async Task<AttendanceResult> SimulateCallAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(400, cancellationToken);
        stopwatch.Stop();

        return new AttendanceResult
        {
            Success = true,
            HttpStatus = 200,
            RequestId = Guid.NewGuid().ToString("N"),
            DurationMs = stopwatch.Elapsed.TotalMilliseconds
        };
    }
}
