using EMS.API.DTOs.Employees;
using EMS.API.Models;
using EMS.API.Repositories;
using EMS.API.Services.Implementations;
using Moq;

namespace EMS.Tests.Services;

[TestFixture]
public class EmployeeServiceTests
{
    private Mock<IEmployeeRepository> _employeeRepository = null!;
    private EmployeeService _employeeService = null!;

    [SetUp]
    public void SetUp()
    {
        _employeeRepository = new Mock<IEmployeeRepository>();
        _employeeService = new EmployeeService(_employeeRepository.Object);
    }

    [Test]
    public async Task GetEmployeesAsync_ShouldReturnPaginatedResults()
    {
        var employees = new List<Employee>
        {
            CreateEmployee(1, "Aarav", "Sharma", "Engineering", 82000m, EmployeeStatus.Active),
            CreateEmployee(2, "Diya", "Patel", "HR", 64000m, EmployeeStatus.Active)
        };

        _employeeRepository.Setup(repository => repository.SearchAsync(It.IsAny<EmployeeQueryParametersDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((employees, 12));

        var result = await _employeeService.GetEmployeesAsync(new EmployeeQueryParametersDto
        {
            Page = 1,
            PageSize = 2,
            SortBy = "firstName",
            SortDir = "asc"
        });

        Assert.That(result.TotalCount, Is.EqualTo(12));
        Assert.That(result.TotalPages, Is.EqualTo(6));
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.Items.First().FirstName, Is.EqualTo("Aarav"));
    }

    [Test]
    public async Task CreateAsync_ShouldPersistEmployee_WhenEmailIsUnique()
    {
        _employeeRepository.Setup(repository => repository.GetByEmailAsync("new.employee@ems.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        Employee? savedEmployee = null;
        _employeeRepository.Setup(repository => repository.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Callback<Employee, CancellationToken>((employee, _) =>
            {
                employee.Id = 7;
                savedEmployee = employee;
            })
            .Returns(Task.CompletedTask);

        var result = await _employeeService.CreateAsync(new CreateEmployeeDto
        {
            FirstName = "New",
            LastName = "Employee",
            Email = "new.employee@ems.local",
            Phone = "9999999999",
            Department = "Operations",
            Designation = "Coordinator",
            Salary = 56000m,
            JoinDate = new DateTime(2025, 1, 10),
            Status = "Active"
        });

        Assert.That(result.Id, Is.EqualTo(7));
        Assert.That(savedEmployee, Is.Not.Null);
        Assert.That(savedEmployee!.Department, Is.EqualTo("Operations"));
        _employeeRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetDashboardAsync_ShouldAggregateCountsAndRecentEmployees()
    {
        var employees = new List<Employee>
        {
            CreateEmployee(1, "Aarav", "Sharma", "Engineering", 82000m, EmployeeStatus.Active),
            CreateEmployee(2, "Diya", "Patel", "Finance", 64000m, EmployeeStatus.Inactive),
            CreateEmployee(3, "Rohan", "Mehta", "Engineering", 71000m, EmployeeStatus.Active)
        };

        _employeeRepository.Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);
        _employeeRepository.Setup(repository => repository.GetDepartmentBreakdownAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DepartmentCountDto>
            {
                new() { Department = "Engineering", Count = 2 },
                new() { Department = "Finance", Count = 1 }
            });
        _employeeRepository.Setup(repository => repository.GetRecentEmployeesAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees.OrderByDescending(employee => employee.CreatedAt).ToList());

        var result = await _employeeService.GetDashboardAsync();

        Assert.That(result.TotalEmployees, Is.EqualTo(3));
        Assert.That(result.ActiveCount, Is.EqualTo(2));
        Assert.That(result.InactiveCount, Is.EqualTo(1));
        Assert.That(result.DepartmentBreakdown.Count, Is.EqualTo(2));
        Assert.That(result.RecentEmployees.Count, Is.EqualTo(3));
    }

    private static Employee CreateEmployee(
        int id,
        string firstName,
        string lastName,
        string department,
        decimal salary,
        EmployeeStatus status)
        => new()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@ems.local",
            Phone = "9999999999",
            Department = department,
            Designation = "Engineer",
            Salary = salary,
            JoinDate = new DateTime(2024, 1, 1),
            Status = status,
            CreatedAt = DateTime.UtcNow.AddMinutes(-id),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-id)
        };
}
