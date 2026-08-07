using Attendance.Models;
using SQLite;

namespace Attendance.Database;

/// <summary>
/// Owns the single SQLite connection used by the app and guarantees the
/// schema exists before any repository touches it.
/// </summary>
public class AppDatabase
{
    private readonly SQLiteAsyncConnection _connection;
    private readonly Task _initialization;

    public AppDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "attendance.db3");
        _connection = new SQLiteAsyncConnection(dbPath);
        _initialization = InitializeAsync();
    }

    public SQLiteAsyncConnection Connection => _connection;

    private async Task InitializeAsync()
    {
        await _connection.CreateTableAsync<Schedule>();
        await _connection.CreateTableAsync<ExecutionLog>();
    }

    public Task EnsureInitializedAsync() => _initialization;
}
