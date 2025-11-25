using AutoMapper;
using SGE.Application.DTOs.Attendances;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
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

    public async Task<IEnumerable<AttendanceDto>> GetByEmployeeIdAsync(int employeeId,
        CancellationToken cancellationToken = default)
    {
        var list = await attendanceRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return mapper.Map<IEnumerable<AttendanceDto>>(list);
    }

    public async Task<AttendanceDto?> GetByEmployeeAndDateAsync(int employeeId, DateTime date,
        CancellationToken cancellationToken = default)
    {
        var attendance = await attendanceRepository.GetByEmployeeAndDateAsync(employeeId, date, cancellationToken);
        return attendance == null ? null : mapper.Map<AttendanceDto>(attendance);
    }

    public async Task<IEnumerable<AttendanceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var list = await attendanceRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return mapper.Map<IEnumerable<AttendanceDto>>(list);
    }

    public async Task<AttendanceDto> CreateAsync(AttendanceCreateDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(dto.EmployeeId, cancellationToken);
        if (employee == null)
            throw new EmployeeNotFoundException(dto.EmployeeId);

        var existing =
            await attendanceRepository.GetByEmployeeAndDateAsync(dto.EmployeeId, dto.Date, cancellationToken);
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
}