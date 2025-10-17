namespace SGE.Application.DTOs.LeaveRequests;

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysRequested { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? ManagerComments { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
}