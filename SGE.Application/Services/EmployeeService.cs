using AutoMapper;
using SGE.Application.DTOs.Employees;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
using SGE.Application.Services.Readers;
using SGE.Application.Services.Writers;
using SGE.Core.Entities;
using SGE.Core.Exceptions;
using SGE.Core.Helpers;

namespace SGE.Application.Services;

public class EmployeeService(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository,
    IMapper mapper) : IEmployeeService
{
    /// <summary>
    ///     Asynchronously retrieves all employees from the repository and maps them to a collection of EmployeeDto.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a collection of EmployeeDto
    ///     objects.
    /// </returns>
    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await employeeRepository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<EmployeeDto>>(list);
    }

    /// <summary>
    ///     Asynchronously retrieves an employee by their unique identifier and maps it to an EmployeeDto.
    /// </summary>
    /// <param name="id">The unique identifier of the employee.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an EmployeeDto object if found;
    ///     otherwise, null.
    /// </returns>
    public async Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var emp = await employeeRepository.GetByIdAsync(id, cancellationToken);
        return emp == null ? null : mapper.Map<EmployeeDto>(emp);
    }

    /// <summary>
    ///     Asynchronously retrieves an employee by their email address and maps it to an EmployeeDto.
    /// </summary>
    /// <param name="email">The email address of the employee to retrieve.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the EmployeeDto if found;
    ///     otherwise, null.
    /// </returns>
    public async Task<EmployeeDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emp = await employeeRepository.GetByEmailAsync(email, cancellationToken);
        return emp == null ? null : mapper.Map<EmployeeDto>(emp);
    }

    /// <summary>
    ///     Asynchronously retrieves employees belonging to a specific department and maps them to a collection of EmployeeDto.
    /// </summary>
    /// <param name="departmentId">The unique identifier of the department whose employees should be retrieved.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a collection of EmployeeDto
    ///     objects associated with the specified department.
    /// </returns>
    public async Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(int departmentId,
        CancellationToken cancellationToken = default)
    {
        var list = await employeeRepository.GetByDepartmentAsync(departmentId, cancellationToken);
        return mapper.Map<IEnumerable<EmployeeDto>>(list);
    }

    /// <summary>
    ///     Asynchronously creates a new employee in the repository based on the provided data transfer object.
    /// </summary>
    /// <param name="dto">The data transfer object containing details of the employee to be created.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created EmployeeDto object.</returns>
    /// <exception cref="ApplicationException">
    ///     Thrown if the specified department does not exist or if the email is already
    ///     associated with another employee.
    /// </exception>
    public async Task<EmployeeDto> CreateAsync(EmployeeCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.GetByIdAsync(dto.DepartmentId, cancellationToken);
        if (department == null)
            throw new DepartmentNotFoundException(dto.DepartmentId);

        var existingEmployee = await employeeRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingEmployee != null)
            throw new InvalidEmployeeDataException(
                $"L'adresse email '{dto.Email}' est déjà utilisée par un autre employé.");

        var entity = mapper.Map<Employee>(dto);

        entity.UniqueId = await GenerateUniqueId(dto.FirstName, dto.LastName, dto.DepartmentId);

        await employeeRepository.AddAsync(entity, cancellationToken);
        return mapper.Map<EmployeeDto>(entity);
    }

    /// <summary>
    ///     Asynchronously updates an employee's information in the repository using the provided data.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to update.</param>
    /// <param name="dto">An object containing the updated details of the employee.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result indicates whether the update operation was
    ///     successful.
    /// </returns>
    public async Task<bool> UpdateAsync(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new EmployeeNotFoundException(id);

        if (entity.DepartmentId != dto.DepartmentId)
        {
            var department = await departmentRepository.GetByIdAsync(dto.DepartmentId, cancellationToken);
            if (department == null)
                throw new DepartmentNotFoundException(dto.DepartmentId);
        }

        mapper.Map(dto, entity);
        await employeeRepository.UpdateAsync(entity, cancellationToken);
        return true;
    }

    /// <summary>
    ///     Asynchronously deletes an employee by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to be deleted.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the deletion was successful.
    /// </returns>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new EmployeeNotFoundException(id);

        await employeeRepository.DeleteAsync(entity.Id, cancellationToken);
        return true;
    }

    /// <summary>
    ///     Import Department from Excel file
    /// </summary>
    /// <param name="fileUploadModel"></param>
    /// <returns></returns>
    public async Task<List<EmployeeDto>> ImportFile(FileUploadModel fileUploadModel)
    {
        var excelReader = new ExcelReader();
        var rows = excelReader.Read(fileUploadModel.File);

        if (rows.Count == 0)
            return new List<EmployeeDto>();

        // Validation des colonnes requises
        var requiredColumns = new[]
        {
            "firstname", "lastname", "email", "departmentid", "hiredate", "salary", "gender"
        };

        var missingColumns = requiredColumns.Where(c => !rows[0].ContainsKey(c)).ToList();

        if (missingColumns.Any())
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "General", new List<string> { $"Colonnes manquantes : {string.Join(", ", missingColumns)}" } }
            };
            throw new ImportException(errors);
        }

        var createdDtos = new List<EmployeeDto>();
        var errorsByLine = new Dictionary<string, List<string>>();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNumber = i + 2;
            var lineErrors = new List<string>();

            // Validation des champs obligatoires et types
            if (string.IsNullOrWhiteSpace(row["firstname"]))
                lineErrors.Add("Le prénom est requis.");

            if (string.IsNullOrWhiteSpace(row["lastname"]))
                lineErrors.Add("Le nom est requis.");

            if (string.IsNullOrWhiteSpace(row["email"]))
                lineErrors.Add("L'email est requis.");

            if (!int.TryParse(row["gender"], out var gender))
                lineErrors.Add($"Genre invalide : '{row["gender"]}'.");

            if (!decimal.TryParse(row["salary"], out var salary))
                lineErrors.Add($"Salaire invalide : '{row["salary"]}'.");

            if (!int.TryParse(row["departmentid"], out var departmentId))
                lineErrors.Add($"ID Département invalide : '{row["departmentid"]}'.");

            var hireDate = DateHelper.ParseDate(row["hiredate"]);
            if (!hireDate.HasValue)
                lineErrors.Add(
                    $"Date d'embauche invalide : '{row["hiredate"]}'. Formats acceptés : dd/MM/yyyy, yyyy-MM-dd.");

            if (lineErrors.Any())
            {
                errorsByLine.Add($"Ligne {rowNumber}", lineErrors);
                continue;
            }

            try
            {
                var dto = new EmployeeCreateDto
                {
                    FirstName = row["firstname"],
                    LastName = row["lastname"],
                    Gender = gender,
                    Email = row["email"],
                    PhoneNumber = row.TryGetValue("phonenumber", out var value) ? value : null,
                    Address = row.ContainsKey("address") ? row["address"] : null,
                    Position = row.ContainsKey("position") ? row["position"] : null,
                    Salary = salary,
                    DepartmentId = departmentId,
                    HireDate = hireDate!.Value
                };

                var created = await CreateAsync(dto);
                createdDtos.Add(created);
            }
            catch (SgeException ex)
            {
                errorsByLine.Add($"Ligne {rowNumber}", new List<string> { ex.Message });
            }
            catch (Exception ex)
            {
                errorsByLine.Add($"Ligne {rowNumber}", new List<string> { $"Erreur inattendue : {ex.Message}" });
            }
        }

        if (errorsByLine.Any()) throw new ImportException(errorsByLine);

        return createdDtos;
    }

    /// <summary>
    ///     Export Employee to Excel
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> ExportToExcelAsync(CancellationToken cancellationToken)
    {
        var excelWriter = new ExcelWriter();
        var departments = await GetAllAsync(cancellationToken);
        return excelWriter.Write(departments.ToList(), "Employees");
    }


    /// <summary>
    ///     Tries to generate a unique id until it finds one that is not already in use.
    ///     If it fails to find a unique id after 10 tries, it throws an exception.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <param name="departmentId"></param>
    /// <returns>A unique id for the employee.</returns>
    /// <exception cref="ApplicationException"></exception>
    private async Task<string> GenerateUniqueId(string firstName, string lastName, int departmentId)
    {
        var chars = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var rand = new Random();
        var num = rand.Next(0, chars.Length);

        string FormatUniqueId()
        {
            return (firstName.Substring(0, int.Clamp(firstName.Length, 0, 2)) +
                    lastName.Substring(0, int.Clamp(lastName.Length, 0, 2)) + chars[num] + departmentId).ToUpper();
        }

        var uniqueId = FormatUniqueId();

        var retry = 0;
        while (await employeeRepository.GetByUniqueIdAsync(uniqueId) != null)
        {
            num = rand.Next(0, chars.Length);
            uniqueId = FormatUniqueId();

            if (retry >= 10)
                throw new InvalidEmployeeDataException("Impossible de générer un identifiant unique pour cet employé.");

            retry++;
        }

        return uniqueId;
    }
}