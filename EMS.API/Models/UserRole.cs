namespace EMS.API.Models;

public static class UserRole
{
    public const string Admin = "Admin";
    public const string Viewer = "Viewer";

    public static readonly string[] All = [Admin, Viewer];
}
