using System.ComponentModel.DataAnnotations;

namespace EMS.API.DTOs.Employees;

public class CreateEmployeeDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Designation { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Salary { get; set; }

    public DateTime JoinDate { get; set; }

    [Required]
    [RegularExpression("Active|Inactive", ErrorMessage = "Status must be Active or Inactive.")]
    public string Status { get; set; } = "Active";
}
