using EMS.API.DTOs.Auth;
using EMS.API.Models;
using EMS.API.Repositories;
using EMS.API.Services.Contracts;

namespace EMS.API.Services.Implementations;

public class AuthService(IAppUserRepository userRepository, IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<UserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedRole = NormalizeRole(request.Role);
        var existingUser = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var user = new AppUser
        {
            Username = request.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = normalizedRole,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        return jwtTokenService.GenerateToken(user);
    }

    private static string NormalizeRole(string role)
    {
        if (string.Equals(role, UserRole.Admin, StringComparison.OrdinalIgnoreCase))
        {
            return UserRole.Admin;
        }

        if (string.Equals(role, UserRole.Viewer, StringComparison.OrdinalIgnoreCase))
        {
            return UserRole.Viewer;
        }

        throw new InvalidOperationException("Only Admin and Viewer roles are allowed.");
    }
}
