namespace SGE.Application.DTOs.Attendances;

public class AttendanceUpdateDto
{
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public string Notes { get; set; } = string.Empty;
}