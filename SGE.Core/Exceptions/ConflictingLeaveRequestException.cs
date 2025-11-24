namespace SGE.Core.Exceptions;

public class ConflictingLeaveRequestException : SgeException
{
    public ConflictingLeaveRequestException(DateTime startDate, DateTime endDate)
        : base($"Conflit de congé détecté pour la période du {startDate:dd/MM/yyyy} au {endDate:dd/MM/yyyy}",
            "CONFLICTING_LEAVE_REQUEST", 409)
    {
    }
}