namespace SGE.Application.DTOs.Employees;

public class EmployeeDto
{
    /// <summary>
    ///     Gets or sets the unique identifier for the employee.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the unique ID of the employee.
    /// </summary>
    public string UniqueId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the full name of the employee.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the gender of the employee.
    /// </summary>
    public int Gender { get; set; }

    /// <summary>
    ///     Gets or sets the email address of the employee.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the phone number associated with the employee.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the address of the employee.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the position or job title of the employee.
    /// </summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the salary of the employee.
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    ///     Gets or sets the name of the department associated with the employee.
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date the employee was hired.
    /// </summary>
    public DateTime HireDate { get; set; }
}