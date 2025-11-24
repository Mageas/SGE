namespace SGE.Core.Exceptions;

public class LeaveRequestNotFoundException : SgeException
{
    public LeaveRequestNotFoundException(int leaveRequestId)
        : base($"Demande de congé avec l'ID {leaveRequestId} introuvable.", "LEAVE_REQUEST_NOT_FOUND", 404)
    {
    }
}