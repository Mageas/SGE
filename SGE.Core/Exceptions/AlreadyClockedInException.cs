namespace SGE.Core.Exceptions;

public class AlreadyClockedInException : SgeException
{
    public AlreadyClockedInException(int employeeId)
        : base($"L'employé {employeeId} est déjà pointé.", "ALREADY_CLOCKED_IN", 409)
    {
    }
}