using Attendance.Database;
using Attendance.Interfaces;
using Attendance.Models;

namespace Attendance.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly AppDatabase _database;

    public ScheduleRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<List<Schedule>> GetAllAsync()
    {
        await _database.EnsureInitializedAsync();
        return await _database.Connection.Table<Schedule>()
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Schedule?> GetByIdAsync(int id)
    {
        await _database.EnsureInitializedAsync();
        return await _database.Connection.Table<Schedule>()
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveAsync(Schedule schedule)
    {
        await _database.EnsureInitializedAsync();
        schedule.UpdatedAt = DateTime.UtcNow;

        if (schedule.Id == 0)
        {
            schedule.CreatedAt = DateTime.UtcNow;
            return await _database.Connection.InsertAsync(schedule);
        }

        return await _database.Connection.UpdateAsync(schedule);
    }

    public async Task<int> DeleteAsync(Schedule schedule)
    {
        await _database.EnsureInitializedAsync();
        return await _database.Connection.DeleteAsync(schedule);
    }
}
