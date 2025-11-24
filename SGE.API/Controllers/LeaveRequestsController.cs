using Microsoft.AspNetCore.Mvc;
using SGE.Application.DTOs.LeaveRequests;
using SGE.Application.Interfaces.Services;
using SGE.Core.Entities;

namespace SGE.API.Controllers;

/// <summary>
/// Controller for managing employee leave requests.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LeaveRequestsController(ILeaveRequestService leaveRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetAll(CancellationToken cancellationToken)
    {
        var leaveRequests = await leaveRequestService.GetAllAsync(cancellationToken);
        return Ok(leaveRequests);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeaveRequestDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestService.GetByIdAsync(id, cancellationToken);
        if (leaveRequest == null) return NotFound();
        return Ok(leaveRequest);
    }

    [HttpGet("employee/{employeeId:int}")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetByEmployeeId(int employeeId, CancellationToken cancellationToken)
    {
        var leaveRequests = await leaveRequestService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return Ok(leaveRequests);
    }

    [HttpGet("status/{status:int}")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetByStatus(int status, CancellationToken cancellationToken)
    {
        var leaveRequests = await leaveRequestService.GetByStatusAsync(status, cancellationToken);
        return Ok(leaveRequests);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetPending(CancellationToken cancellationToken)
    {
        var leaveRequests = await leaveRequestService.GetPendingRequestsAsync(cancellationToken);
        return Ok(leaveRequests);
    }

    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> Create(LeaveRequestCreateDto dto, CancellationToken cancellationToken)
    {
        var created = await leaveRequestService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LeaveRequestUpdateDto dto, CancellationToken cancellationToken)
    {
        await leaveRequestService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApprovalDto dto, CancellationToken cancellationToken)
    {
        await leaveRequestService.ApproveAsync(id, dto.ApprovedBy, dto.Comments, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDto dto, CancellationToken cancellationToken)
    {
        await leaveRequestService.RejectAsync(id, dto.ApprovedBy, dto.Comments, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await leaveRequestService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("import")]
    public async Task<ActionResult> ImportFile([FromForm] FileUploadModel? fileUploadModel)
    {
        if (fileUploadModel == null) return BadRequest("No file uploaded");
        if (fileUploadModel.File.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            return BadRequest("File is not a valid Excel file");

        var createdDtos = await leaveRequestService.ImportFile(fileUploadModel);
        return Ok(createdDtos);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var excelData = await leaveRequestService.ExportToExcelAsync(cancellationToken);
        return File(
            excelData,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"LeaveRequests_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );
    }
}

public class ApprovalDto
{
    public string ApprovedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
}