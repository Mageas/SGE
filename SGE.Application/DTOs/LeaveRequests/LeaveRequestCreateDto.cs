using System.Text.Json.Serialization;

namespace SGE.Application.DTOs.LeaveRequests;

public class LeaveRequestCreateDto
{
    [JsonPropertyName("leaveType")]
    public int LeaveType { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("employeeId")]
    public int EmployeeId { get; set; }
}