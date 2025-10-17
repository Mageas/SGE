using AutoMapper;
using SGE.Application.DTOs.LeaveRequests;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
using SGE.Application.Services.Readers;
using SGE.Application.Services.Writers;
using SGE.Core.Entities;
using SGE.Core.Enums;

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

    public async Task<IEnumerable<LeaveRequestDto>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var list = await leaveRequestRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return mapper.Map<IEnumerable<LeaveRequestDto>>(list);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetByStatusAsync(int status, CancellationToken cancellationToken = default)
    {
        var list = await leaveRequestRepository.GetByStatusAsync(status, cancellationToken);
        return mapper.Map<IEnumerable<LeaveRequestDto>>(list);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        var list = await leaveRequestRepository.GetPendingRequestsAsync(cancellationToken);
        return mapper.Map<IEnumerable<LeaveRequestDto>>(list);
    }

    public async Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(dto.EmployeeId, cancellationToken);
        if (employee == null)
            throw new ApplicationException("Employee not found");

        var entity = mapper.Map<LeaveRequest>(dto);
        
        // Calculate days requested
        entity.DaysRequested = (dto.EndDate.Date - dto.StartDate.Date).Days + 1;
        entity.Status = LeaveStatus.Pending;
        entity.CreatedBy = "System";
        entity.UpdatedBy = "System";

        await leaveRequestRepository.AddAsync(entity, cancellationToken);
        return mapper.Map<LeaveRequestDto>(entity);
    }

    public async Task<bool> UpdateAsync(int id, LeaveRequestUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        if (dto.Status.HasValue)
            entity.Status = (LeaveStatus)dto.Status.Value;

        if (dto.ManagerComments != null)
            entity.ManagerComments = dto.ManagerComments;

        entity.UpdatedBy = "System";
        entity.UpdatedAt = DateTime.UtcNow;

        await leaveRequestRepository.UpdateAsync(entity, cancellationToken);
        return true;
    }

    public async Task<bool> ApproveAsync(int id, string approvedBy, string? comments = null, CancellationToken cancellationToken = default)
    {
        var entity = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        if (entity.Status != LeaveStatus.Pending)
            throw new ApplicationException("Only pending leave requests can be approved");

        entity.Status = LeaveStatus.Approved;
        entity.ReviewedBy = approvedBy;
        entity.ReviewedAt = DateTime.UtcNow;
        entity.ManagerComments = comments;
        entity.UpdatedBy = approvedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await leaveRequestRepository.UpdateAsync(entity, cancellationToken);
        return true;
    }

    public async Task<bool> RejectAsync(int id, string rejectedBy, string? comments = null, CancellationToken cancellationToken = default)
    {
        var entity = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        if (entity.Status != LeaveStatus.Pending)
            throw new ApplicationException("Only pending leave requests can be rejected");

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
        if (entity == null) return false;

        await leaveRequestRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<List<LeaveRequestDto>> ImportFile(FileUploadModel fileUploadModel)
    {
        var excelReader = new ExcelReader();
        var rows = excelReader.Read(fileUploadModel.File);

        var createdDtos = new List<LeaveRequestDto>();
        var errors = new List<string>();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNumber = i + 2;

            try
            {
                if (!int.TryParse(row["employeeid"], out var employeeId))
                {
                    errors.Add($"Ligne {rowNumber}: EmployeeId invalide '{row["employeeid"]}'");
                    continue;
                }

                if (!int.TryParse(row["leavetype"], out var leaveType))
                {
                    errors.Add($"Ligne {rowNumber}: LeaveType invalide '{row["leavetype"]}'");
                    continue;
                }

                if (!DateTime.TryParse(row["startdate"], out var startDate))
                {
                    errors.Add($"Ligne {rowNumber}: StartDate invalide '{row["startdate"]}'");
                    continue;
                }

                if (!DateTime.TryParse(row["enddate"], out var endDate))
                {
                    errors.Add($"Ligne {rowNumber}: EndDate invalide '{row["enddate"]}'");
                    continue;
                }

                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
                endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

                var dto = new LeaveRequestCreateDto
                {
                    EmployeeId = employeeId,
                    LeaveType = leaveType,
                    StartDate = startDate,
                    EndDate = endDate,
                    Reason = row.ContainsKey("reason") ? row["reason"] : string.Empty
                };

                var created = await CreateAsync(dto);
                createdDtos.Add(created);
            }
            catch (ApplicationException ex)
            {
                errors.Add($"Ligne {rowNumber}: {ex.Message}");
            }
            catch (Exception ex)
            {
                errors.Add($"Ligne {rowNumber}: Erreur inattendue - {ex.Message}");
            }
        }

        return errors.Any()
            ? throw new ApplicationException($"Erreurs lors de l'import:\n{string.Join("\n", errors)}")
            : createdDtos;
    }

    public async Task<byte[]> ExportToExcelAsync(CancellationToken cancellationToken)
    {
        var excelWriter = new ExcelWriter();
        var leaveRequests = await GetAllAsync(cancellationToken);
        return excelWriter.Write(leaveRequests.ToList(), "LeaveRequests");
    }
}