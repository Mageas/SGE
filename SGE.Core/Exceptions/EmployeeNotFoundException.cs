namespace SGE.Core.Exceptions;

public class EmployeeNotFoundException : SgeException
{
    public EmployeeNotFoundException(int employeeId)
        : base($"Employé avec l'ID {employeeId} introuvable.", "EMPLOYEE_NOT_FOUND", 404)
    {
    }
}