using BCrypt.Net;
using EMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EMS.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!await context.AppUsers.AnyAsync())
        {
            var users = new List<AppUser>
            {
                new()
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Username = "viewer",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("viewer123"),
                    Role = UserRole.Viewer,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.AppUsers.AddRangeAsync(users);
        }

        if (!await context.Employees.AnyAsync())
        {
            var baseDate = DateTime.UtcNow.Date;
            var employees = new List<Employee>
            {
                CreateEmployee("Aarav", "Sharma", "aarav.sharma@ems.local", "9876543210", "Engineering", "Software Engineer", 82000m, baseDate.AddDays(-920), EmployeeStatus.Active, 1),
                CreateEmployee("Diya", "Patel", "diya.patel@ems.local", "9876543211", "Human Resources", "HR Manager", 64000m, baseDate.AddDays(-860), EmployeeStatus.Active, 2),
                CreateEmployee("Rohan", "Mehta", "rohan.mehta@ems.local", "9876543212", "Finance", "Financial Analyst", 71000m, baseDate.AddDays(-790), EmployeeStatus.Active, 3),
                CreateEmployee("Anaya", "Reddy", "anaya.reddy@ems.local", "9876543213", "Engineering", "QA Engineer", 68000m, baseDate.AddDays(-720), EmployeeStatus.Active, 4),
                CreateEmployee("Vivaan", "Gupta", "vivaan.gupta@ems.local", "9876543214", "Sales", "Sales Executive", 59000m, baseDate.AddDays(-650), EmployeeStatus.Inactive, 5),
                CreateEmployee("Isha", "Nair", "isha.nair@ems.local", "9876543215", "Marketing", "Marketing Specialist", 61000m, baseDate.AddDays(-590), EmployeeStatus.Active, 6),
                CreateEmployee("Kabir", "Joshi", "kabir.joshi@ems.local", "9876543216", "Engineering", "Tech Lead", 118000m, baseDate.AddDays(-540), EmployeeStatus.Active, 7),
                CreateEmployee("Myra", "Singh", "myra.singh@ems.local", "9876543217", "Support", "Support Engineer", 52000m, baseDate.AddDays(-480), EmployeeStatus.Active, 8),
                CreateEmployee("Arjun", "Verma", "arjun.verma@ems.local", "9876543218", "Operations", "Operations Analyst", 67000m, baseDate.AddDays(-420), EmployeeStatus.Inactive, 9),
                CreateEmployee("Sara", "Kapoor", "sara.kapoor@ems.local", "9876543219", "Engineering", "Product Engineer", 93000m, baseDate.AddDays(-360), EmployeeStatus.Active, 10),
                CreateEmployee("Advik", "Bose", "advik.bose@ems.local", "9876543220", "Finance", "Accountant", 56000m, baseDate.AddDays(-300), EmployeeStatus.Active, 11),
                CreateEmployee("Kiara", "Das", "kiara.das@ems.local", "9876543221", "Marketing", "Content Strategist", 60000m, baseDate.AddDays(-240), EmployeeStatus.Active, 12),
                CreateEmployee("Reyansh", "Iyer", "reyansh.iyer@ems.local", "9876543222", "Sales", "Account Manager", 76000m, baseDate.AddDays(-180), EmployeeStatus.Active, 13),
                CreateEmployee("Aisha", "Mishra", "aisha.mishra@ems.local", "9876543223", "Support", "Customer Success Manager", 69000m, baseDate.AddDays(-120), EmployeeStatus.Inactive, 14),
                CreateEmployee("Ivaan", "Kulkarni", "ivaan.kulkarni@ems.local", "9876543224", "Engineering", "DevOps Engineer", 98000m, baseDate.AddDays(-60), EmployeeStatus.Active, 15)
            };

            await context.Employees.AddRangeAsync(employees);
        }

        await context.SaveChangesAsync();
    }

    private static Employee CreateEmployee(
        string firstName,
        string lastName,
        string email,
        string phone,
        string department,
        string designation,
        decimal salary,
        DateTime joinDate,
        EmployeeStatus status,
        int order)
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-order * 10);
        return new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Department = department,
            Designation = designation,
            Salary = salary,
            JoinDate = joinDate,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
}
