using Attendance.Models;

namespace Attendance.Interfaces;

public interface IScheduleRepository
{
    Task<List<Schedule>> GetAllAsync();
    Task<Schedule?> GetByIdAsync(int id);
    Task<int> SaveAsync(Schedule schedule);
    Task<int> DeleteAsync(Schedule schedule);
}

public interface IExecutionLogRepository
{
    Task<List<ExecutionLog>> GetAllAsync();
    Task<List<ExecutionLog>> GetRecentAsync(int count);
    Task<int> SaveAsync(ExecutionLog log);
    Task<int> DeleteAsync(ExecutionLog log);
}
