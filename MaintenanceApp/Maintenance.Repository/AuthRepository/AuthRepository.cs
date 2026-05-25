using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.AuthRepository;

public class AuthRepository : IAuthRepository
{
    private readonly MaintenanceDbContext Context;

    public AuthRepository(MaintenanceDbContext context) => Context = context;

    public Task<AppUser?> GetUserForLoginAsync(string username)
    {
        return Context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task UpdateLastLoginAsync(int userId, DateTime lastLoginAt)
    {
        var user = await Context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return;
        }

        user.LastLoginAt = lastLoginAt;
        user.UpdatedAt = DateTime.Now;
        await Context.SaveChangesAsync();
    }
}
