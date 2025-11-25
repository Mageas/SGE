namespace SGE.Core.Exceptions;

public class DateTimeFormatException : SgeException
{
    public DateTimeFormatException(string dateStr)
        : base($"Format de date non valide : {dateStr}", "DATETIME_FORMAT", 409)
    {
    }
}