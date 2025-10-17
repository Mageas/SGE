namespace SGE.Application.DTOs.Attendances;

public class AttendanceCreateDto
{
    public DateTime Date { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
}