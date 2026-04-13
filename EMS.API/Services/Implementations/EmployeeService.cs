using EMS.API.DTOs.Employees;
using EMS.API.Models;
using EMS.API.Repositories;
using EMS.API.Services.Contracts;

namespace EMS.API.Services.Implementations;

public class EmployeeService(IEmployeeRepository employeeRepository) : IEmployeeService
{
    public async Task<EmployeeListResponseDto> GetEmployeesAsync(EmployeeQueryParametersDto query, CancellationToken cancellationToken = default)
    {
        var sanitizedQuery = new EmployeeQueryParametersDto
        {
            Search = query.Search,
            Department = query.Department,
            Status = query.Status,
            SortBy = query.SortBy,
            SortDir = query.SortDir,
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, 100)
        };

        var (items, totalCount) = await employeeRepository.SearchAsync(sanitizedQuery, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)sanitizedQuery.PageSize);

        return new EmployeeListResponseDto
        {
            Items = items.Select(MapListItem).ToList(),
            Page = sanitizedQuery.Page,
            PageSize = sanitizedQuery.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<EmployeeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with id {id} was not found.");

        return Map(employee);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto request, CancellationToken cancellationToken = default)
    {
        await EnsureEmailIsUniqueAsync(request.Email, cancellationToken);

        var now = DateTime.UtcNow;
        var employee = new Employee
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Department = request.Department.Trim(),
            Designation = request.Designation.Trim(),
            Salary = request.Salary,
            JoinDate = request.JoinDate,
            Status = ParseStatus(request.Status),
            CreatedAt = now,
            UpdatedAt = now
        };

        await employeeRepository.AddAsync(employee, cancellationToken);
        await employeeRepository.SaveChangesAsync(cancellationToken);

        return Map(employee);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto request, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with id {id} was not found.");

        var emailOwner = await employeeRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (emailOwner is not null && emailOwner.Id != id)
        {
            throw new InvalidOperationException("Employee email already exists.");
        }

        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = request.Email.Trim();
        employee.Phone = request.Phone.Trim();
        employee.Department = request.Department.Trim();
        employee.Designation = request.Designation.Trim();
        employee.Salary = request.Salary;
        employee.JoinDate = request.JoinDate;
        employee.Status = ParseStatus(request.Status);
        employee.UpdatedAt = DateTime.UtcNow;

        employeeRepository.Update(employee);
        await employeeRepository.SaveChangesAsync(cancellationToken);

        return Map(employee);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with id {id} was not found.");

        employeeRepository.Remove(employee);
        await employeeRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmployeeDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var allEmployees = await employeeRepository.GetAllAsync(cancellationToken);
        var departmentBreakdown = await employeeRepository.GetDepartmentBreakdownAsync(cancellationToken);
        var recentEmployees = await employeeRepository.GetRecentEmployeesAsync(5, cancellationToken);

        return new EmployeeDashboardDto
        {
            TotalEmployees = allEmployees.Count,
            ActiveCount = allEmployees.Count(employee => employee.Status == EmployeeStatus.Active),
            InactiveCount = allEmployees.Count(employee => employee.Status == EmployeeStatus.Inactive),
            DepartmentBreakdown = departmentBreakdown,
            RecentEmployees = recentEmployees.Select(Map).ToList()
        };
    }

    private async Task EnsureEmailIsUniqueAsync(string email, CancellationToken cancellationToken)
    {
        var existingEmployee = await employeeRepository.GetByEmailAsync(email, cancellationToken);
        if (existingEmployee is not null)
        {
            throw new InvalidOperationException("Employee email already exists.");
        }
    }

    private static EmployeeStatus ParseStatus(string status)
    {
        if (!Enum.TryParse<EmployeeStatus>(status, true, out var parsedStatus))
        {
            throw new InvalidOperationException("Invalid employee status.");
        }

        return parsedStatus;
    }

    private static EmployeeDto Map(Employee employee)
        => new()
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Department = employee.Department,
            Designation = employee.Designation,
            Salary = employee.Salary,
            JoinDate = employee.JoinDate,
            Status = employee.Status.ToString(),
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };

    private static EmployeeListItemDto MapListItem(Employee employee)
        => new()
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Department = employee.Department,
            Designation = employee.Designation,
            Salary = employee.Salary,
            JoinDate = employee.JoinDate,
            Status = employee.Status.ToString(),
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
}
