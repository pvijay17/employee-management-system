using EMS.API.DTOs.Auth;
using EMS.API.Models;

namespace EMS.API.Services.Contracts;

public interface IJwtTokenService
{
    AuthResponseDto GenerateToken(AppUser user);
}
