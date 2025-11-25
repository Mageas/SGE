using Microsoft.EntityFrameworkCore;
using SGE.Application.Interfaces.Repositories;
using SGE.Core.Entities;
using SGE.Infrastructure.Data;

namespace SGE.Infrastructure.Repositories;

/// <summary>
///     Repository implementation for Attendance entity operations.
/// </summary>
public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new async Task<IEnumerable<Attendance>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(a => a.Employee)
            .ToListAsync(cancellationToken);
    }

    public new async Task<Attendance?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Attendance>> GetByEmployeeIdAsync(int employeeId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId)
            .Include(a => a.Employee)
            .OrderByDescending(a => a.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateTime date,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == date.Date, cancellationToken);
    }

    public async Task<IEnumerable<Attendance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(a => a.Date >= startDate && a.Date <= endDate)
            .Include(a => a.Employee)
            .OrderBy(a => a.Date)
            .ToListAsync(cancellationToken);
    }
}