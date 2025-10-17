using System.Text.RegularExpressions;
using SGE.Core.Enums;

namespace SGE.Application.DTOs;

public partial class EmployeeCreateDto
{
    /// <summary>
    /// Gets or sets the first name of the employee.
    /// </summary>
    private readonly string _firstName = String.Empty;

    public string FirstName
    {
        get => _firstName;
        init
        {
            if (value.Length <= 2)
            {
                throw new ArgumentException("First name must be at least 2 characters long.", nameof(value));
            }

            _firstName = value;
        }
    }

    /// <summary>
    /// Gets or sets the last name of the employee.
    /// </summary>
    private readonly string _lastName = String.Empty;

    public string LastName
    {
        get => _lastName;
        init
        {
            if (value.Length <= 2)
            {
                throw new ArgumentException("Last name must be at least 2 characters long.");
            }

            _lastName = value;
        }
    }

    /// <summary>
    /// Gets or sets the gender of the employee.
    /// </summary>
    private int _gender;

    public int Gender
    {
        get => _gender;
        init
        {
            if (!Enum.IsDefined(typeof(Gender), value))
            {
                throw new ArgumentOutOfRangeException("Gender value must be a valid Gender enum value.");
            }

            _gender = value;
        }
    }

    /// <summary>
    /// Gets or sets the email address of the employee.
    /// </summary>
    private readonly string _email = String.Empty;

    public string Email
    {
        get => _email;
        init
        {
            var regex = EmailRegex();
            if (!regex.IsMatch(value))
            {
                throw new ArgumentException("Email must be a valid email.");
            }

            _lastName = value;
        }
    }

    /// <summary>
    /// Gets or sets the phone number associated with the employee.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address of the employee.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position or job title of the employee.
    /// </summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the salary of the employee.
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// Gets or sets the name of the department associated with the employee.
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the date the employee was hired.
    /// </summary>
    public DateTime HireDate { get; set; }

    [GeneratedRegex(
        "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])")]
    private static partial Regex EmailRegex();
}