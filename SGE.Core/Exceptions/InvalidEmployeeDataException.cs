namespace SGE.Core.Exceptions;

public class InvalidEmployeeDataException : SgeException
{
    public InvalidEmployeeDataException(string message)
        : base(message, "INVALID_EMPLOYEE_DATA", 400)
    {
    }
}