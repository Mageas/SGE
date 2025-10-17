using SGE.Core.Entities;

namespace SGE.Application.Interfaces.Repositories;

/// <summary>
/// Provides an abstraction for leave request-specific repository operations.
/// </summary>
public interface ILeaveRequestRepository : IRepository<LeaveRequest>
{
    /// <summary>
    /// Retrieves all leave requests for a specific employee.
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves leave requests by status.
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(int status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves pending leave requests.
    /// </summary>
    Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
}