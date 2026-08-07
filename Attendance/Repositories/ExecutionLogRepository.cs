using Attendance.Database;
using Attendance.Interfaces;
using Attendance.Models;

namespace Attendance.Repositories;

public class ExecutionLogRepository : IExecutionLogRepository
{
    private readonly AppDatabase _database;

    public ExecutionLogRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<List<ExecutionLog>> GetAllAsync()
    {
        await _database.EnsureInitializedAsync();
        return await _database.Connection.Table<ExecutionLog>()
            .OrderByDescending(l => l.Time)
            .ToListAsync();
    }

    public async Task<List<ExecutionLog>> GetRecentAsync(int count)
    {
        await _database.EnsureInitializedAsync();
        var logs = await _database.Connection.Table<ExecutionLog>()
            .OrderByDescending(l => l.Time)
            .ToListAsync();
        return logs.Take(count).ToList();
    }

    public async Task<int> SaveAsync(ExecutionLog log)
    {
        await _database.EnsureInitializedAsync();
        if (log.Id == 0)
            return await _database.Connection.InsertAsync(log);

        return await _database.Connection.UpdateAsync(log);
    }

    public async Task<int> DeleteAsync(ExecutionLog log)
    {
        await _database.EnsureInitializedAsync();
        return await _database.Connection.DeleteAsync(log);
    }
}
