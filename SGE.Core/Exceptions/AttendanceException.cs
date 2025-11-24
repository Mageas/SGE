namespace SGE.Core.Exceptions;

public class AttendanceException : SgeException
{
    public AttendanceException(string message, string errorCode = "ATTENDANCE_ERROR")
        : base(message, errorCode, 400)
    {
    }
}