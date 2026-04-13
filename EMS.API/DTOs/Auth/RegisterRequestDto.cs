using System.ComponentModel.DataAnnotations;
using EMS.API.Models;

namespace EMS.API.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("Admin|Viewer", ErrorMessage = "Role must be Admin or Viewer.")]
    public string Role { get; set; } = UserRole.Viewer;
}
