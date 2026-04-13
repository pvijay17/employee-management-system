using EMS.API.DTOs.Employees;
using EMS.API.Models;

namespace EMS.API.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(EmployeeQueryParametersDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DepartmentCountDto>> GetDepartmentBreakdownAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetRecentEmployeesAsync(int count, CancellationToken cancellationToken = default);
}
