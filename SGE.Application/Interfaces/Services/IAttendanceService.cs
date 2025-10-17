using SGE.Application.DTOs.Attendances;
using SGE.Core.Entities;

namespace SGE.Application.Interfaces.Services;

/// <summary>
/// Defines service operations for managing attendance records.
/// </summary>
public interface IAttendanceService : IExportData
{
    Task<IEnumerable<AttendanceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AttendanceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceDto>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<AttendanceDto?> GetByEmployeeAndDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<AttendanceDto> CreateAsync(AttendanceCreateDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, AttendanceUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<List<AttendanceDto>> ImportFile(FileUploadModel fileUploadModel);
}