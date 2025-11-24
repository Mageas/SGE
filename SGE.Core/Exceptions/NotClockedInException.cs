namespace SGE.Core.Exceptions;

public class NotClockedInException : SgeException
{
    public NotClockedInException(int employeeId)
        : base($"L'employé {employeeId} n'a pas pointé à l'arrivée.", "NOT_CLOCKED_IN", 400)
    {
    }
}