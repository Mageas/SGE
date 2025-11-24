namespace SGE.Core.Exceptions;

public class InvalidLeaveRequestDataException : SgeException
{
    public InvalidLeaveRequestDataException(string message)
        : base(message, "INVALID_LEAVE_REQUEST_DATA", 400)
    {
    }
}
