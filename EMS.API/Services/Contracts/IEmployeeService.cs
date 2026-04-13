using EMS.API.DTOs.Employees;

namespace EMS.API.Services.Contracts;

public interface IEmployeeService
{
    Task<EmployeeListResponseDto> GetEmployeesAsync(EmployeeQueryParametersDto query, CancellationToken cancellationToken = default);
    Task<EmployeeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto request, CancellationToken cancellationToken = default);
    Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
