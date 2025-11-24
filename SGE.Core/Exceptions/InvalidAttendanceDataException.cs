namespace SGE.Core.Exceptions;

public class InvalidAttendanceDataException : SgeException
{
    public InvalidAttendanceDataException(string message)
        : base(message, "INVALID_ATTENDANCE_DATA", 400)
    {
    }
}
