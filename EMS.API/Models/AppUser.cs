using System.ComponentModel.DataAnnotations;

namespace EMS.API.Models;

public class AppUser
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Role { get; set; } = UserRole.Viewer;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
