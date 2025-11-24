using AutoMapper;
using SGE.Application.DTOs.Attendances;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
using SGE.Application.Services.Readers;
using SGE.Application.Services.Writers;
using SGE.Core.Entities;
using SGE.Core.Exceptions;

namespace SGE.Application.Services;

public class AttendanceService(
    IAttendanceRepository attendanceRepository,
    IEmployeeRepository employeeRepository,
    IMapper mapper) : IAttendanceService
{
    public async Task<IEnumerable<AttendanceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await attendanceRepository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<AttendanceDto>>(list);
    }

    public async Task<AttendanceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var attendance = await attendanceRepository.GetByIdAsync(id, cancellationToken);
        return attendance == null ? null : mapper.Map<AttendanceDto>(attendance);
    }

    public async Task<IEnumerable<AttendanceDto>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var list = await attendanceRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return mapper.Map<IEnumerable<AttendanceDto>>(list);
    }

    public async Task<AttendanceDto?> GetByEmployeeAndDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken = default)
    {
        var attendance = await attendanceRepository.GetByEmployeeAndDateAsync(employeeId, date, cancellationToken);
        return attendance == null ? null : mapper.Map<AttendanceDto>(attendance);
    }

    public async Task<IEnumerable<AttendanceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var list = await attendanceRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return mapper.Map<IEnumerable<AttendanceDto>>(list);
    }

    public async Task<AttendanceDto> CreateAsync(AttendanceCreateDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(dto.EmployeeId, cancellationToken);
        if (employee == null)
            throw new EmployeeNotFoundException(dto.EmployeeId);

        var existing = await attendanceRepository.GetByEmployeeAndDateAsync(dto.EmployeeId, dto.Date, cancellationToken);
        if (existing != null)
            throw new DuplicateAttendanceException(dto.EmployeeId, dto.Date);

        // Validation des données
        if (dto.ClockIn.HasValue && dto.ClockOut.HasValue && dto.ClockOut <= dto.ClockIn)
            throw new InvalidAttendanceDataException("L'heure de sortie doit être postérieure à l'heure d'entrée.");

        if (dto.BreakDuration.HasValue && dto.BreakDuration.Value.TotalHours < 0)
            throw new InvalidAttendanceDataException("La durée de pause ne peut pas être négative.");

        var entity = mapper.Map<Attendance>(dto);
        
        // Calculate worked hours and overtime
        if (dto.ClockIn.HasValue && dto.ClockOut.HasValue)
        {
            var totalHours = (dto.ClockOut.Value - dto.ClockIn.Value).TotalHours;
            if (dto.BreakDuration.HasValue)
                totalHours -= dto.BreakDuration.Value.TotalHours;

            entity.WorkedHours = (decimal)Math.Max(0, totalHours);
            
            const decimal standardHours = 8;
            entity.OvertimeHours = entity.WorkedHours > standardHours 
                ? entity.WorkedHours - standardHours 
                : 0;
        }

        entity.CreatedBy = "System";
        entity.UpdatedBy = "System";

        await attendanceRepository.AddAsync(entity, cancellationToken);
        return mapper.Map<AttendanceDto>(entity);
    }

    public async Task<bool> UpdateAsync(int id, AttendanceUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await attendanceRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new AttendanceNotFoundException(id);

        mapper.Map(dto, entity);
        
        // Validation des données
        if (entity.ClockIn.HasValue && entity.ClockOut.HasValue && entity.ClockOut <= entity.ClockIn)
            throw new InvalidAttendanceDataException("L'heure de sortie doit être postérieure à l'heure d'entrée.");

        if (entity.BreakDuration.HasValue && entity.BreakDuration.Value.TotalHours < 0)
            throw new InvalidAttendanceDataException("La durée de pause ne peut pas être négative.");
        
        // Recalculate worked hours if times are updated
        if (entity.ClockIn.HasValue && entity.ClockOut.HasValue)
        {
            var totalHours = (entity.ClockOut.Value - entity.ClockIn.Value).TotalHours;
            if (entity.BreakDuration.HasValue)
                totalHours -= entity.BreakDuration.Value.TotalHours;

            entity.WorkedHours = (decimal)Math.Max(0, totalHours);
            
            const decimal standardHours = 8;
            entity.OvertimeHours = entity.WorkedHours > standardHours 
                ? entity.WorkedHours - standardHours 
                : 0;
        }

        entity.UpdatedBy = "System";
        entity.UpdatedAt = DateTime.UtcNow;

        await attendanceRepository.UpdateAsync(entity, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await attendanceRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new AttendanceNotFoundException(id);

        await attendanceRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<List<AttendanceDto>> ImportFile(FileUploadModel fileUploadModel)
    {
        var excelReader = new ExcelReader();
        var rows = excelReader.Read(fileUploadModel.File);

        var createdDtos = new List<AttendanceDto>();
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

                if (!DateTime.TryParse(row["date"], out var date))
                {
                    errors.Add($"Ligne {rowNumber}: Date invalide '{row["date"]}'");
                    continue;
                }

                date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

                TimeSpan? clockIn = null;
                if (!string.IsNullOrEmpty(row["clockin"]) && TimeSpan.TryParse(row["clockin"], out var ci))
                    clockIn = ci;

                TimeSpan? clockOut = null;
                if (!string.IsNullOrEmpty(row["clockout"]) && TimeSpan.TryParse(row["clockout"], out var co))
                    clockOut = co;

                TimeSpan? breakDuration = null;
                if (!string.IsNullOrEmpty(row["breakduration"]) && TimeSpan.TryParse(row["breakduration"], out var bd))
                    breakDuration = bd;

                var dto = new AttendanceCreateDto
                {
                    EmployeeId = employeeId,
                    Date = date,
                    ClockIn = clockIn,
                    ClockOut = clockOut,
                    BreakDuration = breakDuration,
                    Notes = row.ContainsKey("notes") ? row["notes"] : string.Empty
                };

                var created = await CreateAsync(dto);
                createdDtos.Add(created);
            }
            catch (SgeException ex)
            {
                errors.Add($"Ligne {rowNumber}: {ex.Message}");
            }
            catch (Exception ex)
            {
                errors.Add($"Ligne {rowNumber}: Erreur inattendue - {ex.Message}");
            }
        }

        if (errors.Any())
        {
            var validationErrors = new Dictionary<string, List<string>>
            {
                { "Import", errors }
            };
            throw new ValidationException(validationErrors);
        }

        return createdDtos;
    }

    public async Task<byte[]> ExportToExcelAsync(CancellationToken cancellationToken)
    {
        var excelWriter = new ExcelWriter();
        var attendances = await GetAllAsync(cancellationToken);
        return excelWriter.Write(attendances.ToList(), "Attendances");
    }
}