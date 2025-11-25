namespace SGE.Core.Exceptions;

public class EmployeeEmailNotFoundException : SgeException
{
    public EmployeeEmailNotFoundException(string employeeEmail)
        : base($"Employé avec l'email {employeeEmail} introuvable.", "EMPLOYEE_NOT_FOUND", 404)
    {
    }
}