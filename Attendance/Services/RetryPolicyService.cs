using Attendance.Interfaces;

namespace Attendance.Services;

/// <summary>
/// Simple exponential-backoff retry helper: 1s, 2s, 4s, 8s... between
/// attempts, capped at 30s. Used by AttendanceOrchestrator so every network
/// call (login, refresh, clock-in/out, logout) shares one retry behaviour.
/// </summary>
public class RetryPolicyService : IRetryPolicy
{
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(30);

    public async Task<(T Result, int AttemptsUsed)> ExecuteAsync<T>(
        Func<Task<T>> action,
        Func<T, bool> isSuccess,
        int maxRetries,
        CancellationToken cancellationToken = default)
    {
        var attempts = 0;
        T? lastResult = default;

        while (true)
        {
            attempts++;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                lastResult = await action();

                if (isSuccess(lastResult) || attempts > maxRetries)
                    return (lastResult, attempts);
            }
            catch when (attempts <= maxRetries)
            {
                // Swallow and retry below; the final attempt's exception propagates.
            }

            if (attempts > maxRetries)
            {
                if (lastResult is not null)
                    return (lastResult, attempts);

                throw new InvalidOperationException("Retry policy exhausted without a result.");
            }

            var delaySeconds = Math.Pow(2, attempts - 1);
            var delay = TimeSpan.FromSeconds(Math.Min(delaySeconds, MaxDelay.TotalSeconds));
            await Task.Delay(delay, cancellationToken);
        }
    }
}
