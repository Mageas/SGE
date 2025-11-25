using SGE.Core.Exceptions;

namespace SGE.Application.DTOs.Departments;

public class DepartmentCreateDto
{
    /// <summary>
    /// Gets or sets the name of the department.
    /// </summary>
    private readonly string _name = String.Empty;

    public string Name
    {
        get => _name;
        init
        {
            if (value.Length <= 2)
            {
                throw new ValidationException(nameof(Name), "Name must be at least 2 characters long.");
            }

            _name = value;
        }
    }

    /// <summary>
    /// Gets or sets the code that uniquely identifies the department
    /// </summary>
    private readonly string _code = String.Empty;

    public string Code
    {
        get => _code;
        init
        {
            if (value.Length <= 2 || value.Length > 10)

            {
                throw new ValidationException(nameof(Code), "Code must be at least 2 characters long and max 10 characters long.");
            }

            _code = value;
        }
    }

    /// <summary>
    /// Gets or sets the description of the department.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}