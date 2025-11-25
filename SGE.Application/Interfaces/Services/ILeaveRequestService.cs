using SGE.Application.DTOs.LeaveRequests;

namespace SGE.Application.Interfaces.Services;

/// <summary>
///     Defines service operations for managing leave requests.
/// </summary>
public interface ILeaveRequestService
{
    Task<IEnumerable<LeaveRequestDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LeaveRequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<LeaveRequestDto>> GetByEmployeeIdAsync(int employeeId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<LeaveRequestDto>> GetByEmployeeEmailAsync(string email,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<LeaveRequestDto>> GetByStatusAsync(int status, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, LeaveRequestUpdateDto dto, CancellationToken cancellationToken = default);

    Task<bool> ApproveAsync(int id, string approvedBy, string? comments = null,
        CancellationToken cancellationToken = default);

    Task<bool> RejectAsync(int id, string rejectedBy, string? comments = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}