namespace SGE.Core.Exceptions;

public class AttendanceNotFoundException : SgeException
{
    public AttendanceNotFoundException(int attendanceId)
        : base($"Présence avec l'ID {attendanceId} introuvable.", "ATTENDANCE_NOT_FOUND", 404)
    {
    }
}
