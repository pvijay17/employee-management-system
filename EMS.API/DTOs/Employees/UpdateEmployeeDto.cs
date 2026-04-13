using System.ComponentModel.DataAnnotations;

namespace EMS.API.DTOs.Employees;

public class UpdateEmployeeDto : CreateEmployeeDto
{
    [Required]
    public int Id { get; set; }
}
