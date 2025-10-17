using AutoMapper;
using SGE.Application.DTOs.Attendances;
using SGE.Application.DTOs.Departments;
using SGE.Application.DTOs.Employees;
using SGE.Application.DTOs.LeaveRequests;
using SGE.Core.Entities;

namespace SGE.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Department, DepartmentDto>();
        CreateMap<DepartmentCreateDto, Department>();
        CreateMap<DepartmentUpdateDto, Department>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember)
                => srcMember != null));

        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.DepartmentName, opt =>
                opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty));
        CreateMap<EmployeeCreateDto, Employee>();
        CreateMap<EmployeeUpdateDto, Employee>().ForAllMembers(opts =>
            opts.Condition((src, dest, srcMember) => srcMember != null)); // ignore nulls

        // Attendance mappings
        CreateMap<Attendance, AttendanceDto>()
            .ForMember(dest => dest.EmployeeName, opt =>
                opt.MapFrom(src =>
                    src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : string.Empty));
        CreateMap<AttendanceCreateDto, Attendance>();
        CreateMap<AttendanceUpdateDto, Attendance>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // LeaveRequest mappings
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(dest => dest.EmployeeName, opt =>
                opt.MapFrom(src =>
                    src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : string.Empty));
        CreateMap<LeaveRequestCreateDto, LeaveRequest>();
        CreateMap<LeaveRequestUpdateDto, LeaveRequest>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}