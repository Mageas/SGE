using AutoMapper;
using Microsoft.AspNetCore.Http;
using SGE.Application.DTOs.Departments;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
using SGE.Application.Services.Readers;
using SGE.Application.Services.Writers;
using SGE.Core.Entities;

namespace SGE.Application.Services;

public class DepartmentService(IDepartmentRepository departmentRepository, IMapper mapper) : IDepartmentService
{
    /// <summary>
    /// Retrieves all department records asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of department data transfer objects.</returns>
    public async Task<IEnumerable<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await departmentRepository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<DepartmentDto>>(list);
    }

    /// <summary>
    /// Retrieves a department record by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the department.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A department data transfer object if found; otherwise, null.</returns>
    public async Task<DepartmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var dept = await departmentRepository.GetByIdAsync(id, cancellationToken);
        return dept == null ? null : mapper.Map<DepartmentDto>(dept);
    }

    /// <summary>
    /// Creates a new department record asynchronously.
    /// </summary>
    /// <param name="dto">The data transfer object containing the details of the department to be created.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The created department data transfer object.</returns>
    /// <exception cref="ApplicationException">Thrown if the department name or code already exists.</exception>
    public async Task<DepartmentDto> CreateAsync(DepartmentCreateDto dto, CancellationToken cancellationToken = default)
    {
        var existingName = await departmentRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (existingName != null)
        {
            throw new ApplicationException("Department name already exists");
        }

        var existingCode = await departmentRepository.GetByCodeAsync(dto.Code, cancellationToken);
        if (existingCode != null)
        {
            throw new ApplicationException("Department code already exists");
        }

        var entity = mapper.Map<Department>(dto);
        await departmentRepository.AddAsync(entity, cancellationToken);
        return mapper.Map<DepartmentDto>(entity);
    }

    /// <summary>
    /// Updates an existing department record asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the department to update.</param>
    /// <param name="dto">The data transfer object containing updated information for the department.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A boolean value indicating whether the update was successful.</returns>
    public async Task<bool> UpdateAsync(int id, DepartmentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await departmentRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        mapper.Map(dto, entity);
        await departmentRepository.UpdateAsync(entity, cancellationToken);
        return true;
    }

    /// <summary>
    /// Deletes a department record by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the department to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await departmentRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        await departmentRepository.DeleteAsync(entity.Id, cancellationToken);
        return true;
    }

    /// <summary>
    /// Import Department from Excel file
    /// </summary>
    /// <param name="fileUploadModel"></param>
    /// <returns></returns>
    public async Task<List<DepartmentDto>> ImportFile(FileUploadModel fileUploadModel)
    {
        var excelReader = new ExcelReader();
        var rows = excelReader.Read(fileUploadModel.File);

        var dtosList = rows.Select(row => new DepartmentCreateDto
        {
            Name = row["name"],
            Code = row["code"],
            Description = row["description"],
        });

        var createdDtos = new List<DepartmentDto>();
        foreach (DepartmentCreateDto dto in dtosList)
        {
            createdDtos.Add(await CreateAsync(dto));
        }

        return createdDtos;
    }
    
    /// <summary>
    /// Export Departments to Excel
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> ExportToExcelAsync(CancellationToken cancellationToken)
    {
        var excelWriter = new ExcelWriter();
        var departments = await GetAllAsync(cancellationToken);
        return excelWriter.Write(departments.ToList(), "Departments");
    }
}