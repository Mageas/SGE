namespace SGE.Core.Exceptions;

public class DepartmentNotFoundException : SgeException
{
    public DepartmentNotFoundException(int departmentId)
        : base($"Département avec l'ID {departmentId} introuvable.", "DEPARTMENT_NOT_FOUND", 404)
    {
    }
}