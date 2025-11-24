using SGE.Core.Enums;

namespace SGE.Core.Exceptions;

public class InvalidLeaveStatusTransitionException : SgeException
{
    public InvalidLeaveStatusTransitionException(LeaveStatus currentStatus, LeaveStatus newStatus)
        : base($"Transition de statut invalide de '{currentStatus.ToString()}' vers '{newStatus.ToString()}'",
            "INVALID_STATUS_TRANSITION",
            400)
    {
    }
}