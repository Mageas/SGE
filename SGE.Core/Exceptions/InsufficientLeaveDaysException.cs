namespace SGE.Core.Exceptions;

public class InsufficientLeaveDaysException : SgeException
{
    public InsufficientLeaveDaysException(int requiredDays, int availableDays)
        : base($"Jours de congé insuffisants. Demandé: {requiredDays}, Disponible: {availableDays}",
            "INSUFFICIENT_LEAVE_DAYS", 400)
    {
    }
}