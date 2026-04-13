using EMS.API.Models;

namespace EMS.API.Repositories;

public interface IAppUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
