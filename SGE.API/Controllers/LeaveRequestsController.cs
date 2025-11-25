using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGE.Application.DTOs.LeaveRequests;
using SGE.Application.Interfaces.Services;

namespace SGE.API.Controllers;

/// <summary>
///     Controller for managing employee leave requests.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetByEmployeeId(int employeeId,
        CancellationToken cancellationToken)
    {
        var leaveRequests = await leaveRequestService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return Ok(leaveRequests);
    }

    [HttpGet("status/{status:int}")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetByStatus(int status,
        CancellationToken cancellationToken)
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
    public async Task<ActionResult<LeaveRequestDto>> Create(LeaveRequestCreateDto dto,
        CancellationToken cancellationToken)
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
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApprovalDto dto, CancellationToken cancellationToken)
    {
        await leaveRequestService.ApproveAsync(id, dto.ApprovedBy, dto.Comments, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDto dto, CancellationToken cancellationToken)
    {
        await leaveRequestService.RejectAsync(id, dto.ApprovedBy, dto.Comments, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await leaveRequestService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

public class ApprovalDto
{
    public string ApprovedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
}