using EMS.API.Data;
using EMS.API.DTOs.Employees;
using EMS.API.Models;
using EMS.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EMS.Tests.Integration;

[TestFixture]
public class EmployeeRepositoryIntegrationTests
{
    [Test]
    public async Task SearchAsync_ShouldFilterSortAndPaginateEmployees()
    {
        await using var context = CreateContext();
        await SeedEmployeesAsync(context);
        var repository = new EmployeeRepository(context);

        var result = await repository.SearchAsync(new EmployeeQueryParametersDto
        {
            Search = "engineer",
            Department = "Engineering",
            Status = "Active",
            SortBy = "salary",
            SortDir = "desc",
            Page = 1,
            PageSize = 2
        });

        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.Items.First().Salary, Is.GreaterThanOrEqualTo(result.Items.Last().Salary));
    }

    [Test]
    public async Task DbSeeder_ShouldSeedUsersAndEmployees_WhenDatabaseIsEmpty()
    {
        await using var context = CreateContext();

        await DbSeeder.SeedAsync(context);

        Assert.That(await context.AppUsers.CountAsync(), Is.EqualTo(2));
        Assert.That(await context.Employees.CountAsync(), Is.EqualTo(15));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task SeedEmployeesAsync(ApplicationDbContext context)
    {
        var employees = new List<Employee>
        {
            CreateEmployee("Aarav", "Sharma", "Engineering", "Software Engineer", 85000m, EmployeeStatus.Active),
            CreateEmployee("Kabir", "Joshi", "Engineering", "Tech Lead", 120000m, EmployeeStatus.Active),
            CreateEmployee("Diya", "Patel", "Finance", "Analyst", 70000m, EmployeeStatus.Active),
            CreateEmployee("Rohan", "Mehta", "Engineering", "QA Engineer", 76000m, EmployeeStatus.Inactive)
        };

        await context.Employees.AddRangeAsync(employees);
        await context.SaveChangesAsync();
    }

    private static Employee CreateEmployee(
        string firstName,
        string lastName,
        string department,
        string designation,
        decimal salary,
        EmployeeStatus status)
        => new()
        {
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@ems.local",
            Phone = "9999999999",
            Department = department,
            Designation = designation,
            Salary = salary,
            JoinDate = new DateTime(2024, 1, 1),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
}
