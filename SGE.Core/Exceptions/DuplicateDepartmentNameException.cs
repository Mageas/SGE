namespace SGE.Core.Exceptions;

public class DuplicateDepartmentNameException : SgeException
{
    public DuplicateDepartmentNameException(string departmentName)
        : base($"Le nom du département '{departmentName}' existe déjà.", "DEPARTMENT_NAME_EXISTS", 409)
    {
    }
}