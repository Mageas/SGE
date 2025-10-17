using Microsoft.AspNetCore.Mvc;
using SGE.Application.DTOs.Attendances;
using SGE.Application.Interfaces.Services;
using SGE.Core.Entities;

namespace SGE.API.Controllers;

/// <summary>
///     Controller for managing employee attendance records.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AttendancesController(IAttendanceService attendanceService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var attendances = await attendanceService.GetAllAsync(cancellationToken);
        return Ok(attendances);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AttendanceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var attendance = await attendanceService.GetByIdAsync(id, cancellationToken);
        if (attendance == null) return NotFound();
        return Ok(attendance);
    }

    [HttpGet("employee/{employeeId:int}")]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetByEmployeeId(int employeeId,
        CancellationToken cancellationToken)
    {
        var attendances = await attendanceService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return Ok(attendances);
    }

    //todo: not working
    [HttpGet("employee/{employeeId:int}/date/{date:datetime}")]
    public async Task<ActionResult<AttendanceDto>> GetByEmployeeAndDate(int employeeId, DateTime date,
        CancellationToken cancellationToken)
    {
        date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        var attendance = await attendanceService.GetByEmployeeAndDateAsync(employeeId, date, cancellationToken);
        if (attendance == null) return NotFound();
        return Ok(attendance);
    }

    //todo: not working
    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var attendances = await attendanceService.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return Ok(attendances);
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceDto>> Create(AttendanceCreateDto dto, CancellationToken cancellationToken)
    {
        var created = await attendanceService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AttendanceUpdateDto dto, CancellationToken cancellationToken)
    {
        var ok = await attendanceService.UpdateAsync(id, dto, cancellationToken);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ok = await attendanceService.DeleteAsync(id, cancellationToken);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpPost("import")]
    public async Task<ActionResult> ImportFile([FromForm] FileUploadModel? fileUploadModel)
    {
        if (fileUploadModel == null) return BadRequest("No file uploaded");
        if (fileUploadModel.File.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            return BadRequest("File is not a valid Excel file");

        var createdDtos = await attendanceService.ImportFile(fileUploadModel);
        return Ok(createdDtos);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var excelData = await attendanceService.ExportToExcelAsync(cancellationToken);
        return File(
            excelData,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Attendances_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );
    }
}