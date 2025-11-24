namespace SGE.Core.Exceptions;

public class DuplicateAttendanceException : SgeException
{
    public DuplicateAttendanceException(int employeeId, DateTime date)
        : base($"Une présence existe déjà pour l'employé {employeeId} à la date {date:yyyy-MM-dd}.", "DUPLICATE_ATTENDANCE", 409)
    {
    }
}
