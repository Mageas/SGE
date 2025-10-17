using SGE.Core.Entities;

namespace SGE.Application.Interfaces.Repositories;

/// <summary>
/// Provides an abstraction for attendance-specific repository operations.
/// </summary>
public interface IAttendanceRepository : IRepository<Attendance>
{
    /// <summary>
    /// Retrieves all attendance records for a specific employee.
    /// </summary>
    Task<IEnumerable<Attendance>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves attendance record for a specific employee on a specific date.
    /// </summary>
    Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves attendance records within a date range.
    /// </summary>
    Task<IEnumerable<Attendance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}