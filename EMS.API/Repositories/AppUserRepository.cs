using EMS.API.Data;
using EMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EMS.API.Repositories;

public class AppUserRepository(ApplicationDbContext context) : Repository<AppUser>(context), IAppUserRepository
{
    public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => await Context.AppUsers.FirstOrDefaultAsync(
            user => user.Username.ToLower() == username.ToLower(),
            cancellationToken);
}
