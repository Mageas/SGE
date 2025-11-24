namespace SGE.Core.Exceptions;

public class InvalidDateRangeException : SgeException
{
    public InvalidDateRangeException(string message = "La plage de dates fournie est invalide.")
        : base(message, "INVALID_DATE_RANGE", 400)
    {
    }
}
