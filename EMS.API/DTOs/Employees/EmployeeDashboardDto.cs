namespace EMS.API.DTOs.Employees;

public class EmployeeDashboardDto
{
    public int TotalEmployees { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public IReadOnlyCollection<DepartmentCountDto> DepartmentBreakdown { get; set; } = [];
    public IReadOnlyCollection<EmployeeDto> RecentEmployees { get; set; } = [];
}
