namespace SGE.Application.DTOs.Departments;

public class DepartmentUpdateDto
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
                throw new ArgumentException("Name must be at least 2 characters long.");
            }

            _name = value;
        }
    }

    /// <summary>
    /// Gets or sets the description of the department.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}