using EMS.API.Data;
using EMS.API.DTOs.Employees;
using EMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EMS.API.Repositories;

public class EmployeeRepository(ApplicationDbContext context) : Repository<Employee>(context), IEmployeeRepository
{
    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await Context.Employees.FirstOrDefaultAsync(
            employee => employee.Email.ToLower() == email.ToLower(),
            cancellationToken);

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(
        EmployeeQueryParametersDto query,
        CancellationToken cancellationToken = default)
    {
        var employeeQuery = Context.Employees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            employeeQuery = employeeQuery.Where(employee =>
                employee.FirstName.ToLower().Contains(search) ||
                employee.LastName.ToLower().Contains(search) ||
                employee.Email.ToLower().Contains(search) ||
                employee.Designation.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var department = query.Department.Trim().ToLower();
            employeeQuery = employeeQuery.Where(employee => employee.Department.ToLower() == department);
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<EmployeeStatus>(query.Status, true, out var status))
        {
            employeeQuery = employeeQuery.Where(employee => employee.Status == status);
        }

        employeeQuery = ApplySorting(employeeQuery, query.SortBy, query.SortDir);

        var totalCount = await employeeQuery.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await employeeQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<DepartmentCountDto>> GetDepartmentBreakdownAsync(CancellationToken cancellationToken = default)
        => await Context.Employees.AsNoTracking()
            .GroupBy(employee => employee.Department)
            .Select(group => new DepartmentCountDto
            {
                Department = group.Key,
                Count = group.Count()
            })
            .OrderBy(item => item.Department)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Employee>> GetRecentEmployeesAsync(int count, CancellationToken cancellationToken = default)
        => await Context.Employees.AsNoTracking()
            .OrderByDescending(employee => employee.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortBy, string? sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "firstname" => descending ? query.OrderByDescending(e => e.FirstName) : query.OrderBy(e => e.FirstName),
            "lastname" => descending ? query.OrderByDescending(e => e.LastName) : query.OrderBy(e => e.LastName),
            "email" => descending ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
            "department" => descending ? query.OrderByDescending(e => e.Department) : query.OrderBy(e => e.Department),
            "designation" => descending ? query.OrderByDescending(e => e.Designation) : query.OrderBy(e => e.Designation),
            "salary" => descending ? query.OrderByDescending(e => e.Salary) : query.OrderBy(e => e.Salary),
            "joindate" => descending ? query.OrderByDescending(e => e.JoinDate) : query.OrderBy(e => e.JoinDate),
            "status" => descending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
            _ => descending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt)
        };
    }
}
