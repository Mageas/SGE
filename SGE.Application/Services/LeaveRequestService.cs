using AutoMapper;
using SGE.Application.DTOs.LeaveRequests;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
using SGE.Core.Entities;
using SGE.Core.Enums;
using SGE.Core.Exceptions;

namespace SGE.Application.Services;

public class LeaveRequestService(
    ILeaveRequestRepository leaveRequestRepository,
    IEmployeeRepository employeeRepository,
    IMapper mapper) : ILeaveRequestService
{
    public async Task<IEnumerable<LeaveRequestDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await leaveRequestRepository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<LeaveRequestDto>>(list);
    }

    public async Task<LeaveRequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        return leaveRequest == null ? null : mapper.Map<LeaveRequestDto>(leaveRequest);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetByEmployeeIdAsync(int employeeId,
        CancellationToken cancellationToken = default)
    {
        var list = await leaveRequestRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return mapper.Map<IEnumerable<LeaveRequestDto>>(list);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetByStatusAsync(int status,
        CancellationToken cancellationToken = default)
    {
        var list = await leaveRequestRepository.GetByStatusAsync(status, cancellationToken);
        return mapper.Map<IEnumerable<LeaveRequestDto>>(list);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await leaveRequestRepository.GetPendingRequestsAsync(cancellationToken);
        return mapper.Map<IEnumerable<LeaveRequestDto>>(list);
    }

    public async Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        // Validation du type de congé
        if (!Enum.IsDefined(typeof(LeaveType), dto.LeaveType))
            throw new InvalidLeaveRequestDataException($"Le type de congé '{dto.LeaveType}' n'est pas valide.");

        var employee = await employeeRepository.GetByIdAsync(dto.EmployeeId, cancellationToken);
        if (employee == null)
            throw new EmployeeNotFoundException(dto.EmployeeId);

        // Normaliser les dates (ignorer l'heure)
        var startDate = dto.StartDate.Date;
        var endDate = dto.EndDate.Date;

        // Validation des dates
        if (startDate < DateTime.UtcNow.Date)
            throw new InvalidLeaveRequestDataException(
                $"La date de début ({startDate:yyyy-MM-dd}) ne peut pas être dans le passé.");

        if (endDate < startDate)
            throw new InvalidLeaveRequestDataException(
                $"La date de fin ({endDate:yyyy-MM-dd}) doit être postérieure ou égale à la date de début ({startDate:yyyy-MM-dd}).");

        var entity = mapper.Map<LeaveRequest>(dto);

        // Utiliser les dates normalisées
        entity.StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        entity.EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        // Calculate days requested
        entity.DaysRequested = (endDate - startDate).Days + 1;
        entity.Status = LeaveStatus.Pending;
        entity.CreatedBy = "System";
        entity.UpdatedBy = "System";

        await leaveRequestRepository.AddAsync(entity, cancellationToken);
        return mapper.Map<LeaveRequestDto>(entity);
    }

    public async Task<bool> UpdateAsync(int id, LeaveRequestUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new LeaveRequestNotFoundException(id);

        if (dto.Status.HasValue)
            entity.Status = (LeaveStatus)dto.Status.Value;

        if (dto.ManagerComments != null)
            entity.ManagerComments = dto.ManagerComments;

        entity.UpdatedBy = "System";
        entity.UpdatedAt = DateTime.UtcNow;

        await leaveRequestRepository.UpdateAsync(entity, cancellationToken);
        return true;
    }

    public async Task<bool> ApproveAsync(int id, string approvedBy, string? comments = null,
        CancellationToken cancellationToken = default)
    {
        var entity = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new LeaveRequestNotFoundException(id);

        if (entity.Status != LeaveStatus.Pending)
            throw new InvalidLeaveStatusTransitionException(entity.Status, LeaveStatus.Approved);

        entity.Status = LeaveStatus.Approved;
        entity.ReviewedBy = approvedBy;
        entity.ReviewedAt = DateTime.UtcNow;
        entity.ManagerComments = comments;
        entity.UpdatedBy = approvedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await leaveRequestRepository.UpdateAsync(entity, cancellationToken);
        return true;
    }

    public async Task<bool> RejectAsync(int id, string rejectedBy, string? comments = null,
        CancellationToken cancellationToken = default)
    {
        var entity = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new LeaveRequestNotFoundException(id);

        if (entity.Status != LeaveStatus.Pending)
            throw new InvalidLeaveStatusTransitionException(entity.Status, LeaveStatus.Rejected);

        entity.Status = LeaveStatus.Rejected;
        entity.ReviewedBy = rejectedBy;
        entity.ReviewedAt = DateTime.UtcNow;
        entity.ManagerComments = comments ?? "Request rejected";
        entity.UpdatedBy = rejectedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await leaveRequestRepository.UpdateAsync(entity, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new LeaveRequestNotFoundException(id);

        await leaveRequestRepository.DeleteAsync(id, cancellationToken);
        return true;
    }
}