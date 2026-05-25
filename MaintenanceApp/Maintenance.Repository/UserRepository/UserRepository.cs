using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly MaintenanceDbContext Context;

    public UserRepository(MaintenanceDbContext context) => Context = context;

    public async Task<List<AppUser>> GetUsersAsync(string? search, AppUserRole? role, AppUserStatus? status)
    {
        var query = Context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Username.Contains(search) ||
                x.FullName.Contains(search) ||
                (x.Email != null && x.Email.Contains(search)));
        }

        if (role.HasValue)
        {
            query = query.Where(x => x.Role == role.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query.OrderBy(x => x.Username).ToListAsync();
    }

    public Task<AppUser?> GetUserAsync(int id)
    {
        return Context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return Context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username);
    }

    public Task<AppUser?> GetUserByEmailAsync(string email)
    {
        return Context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<AppUser> CreateUserAsync(AppUser user)
    {
        user.Id = 0;
        user.Username = user.Username.Trim();
        user.FullName = user.FullName.Trim();
        user.Email = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim();
        user.Phone = string.IsNullOrWhiteSpace(user.Phone) ? null : user.Phone.Trim();
        user.CreatedAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;

        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return user;
    }

    public async Task<AppUser?> UpdateUserAsync(int id, AppUser user)
    {
        var existing = await Context.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.Username = user.Username.Trim();
        existing.FullName = user.FullName.Trim();
        existing.Email = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim();
        existing.Phone = string.IsNullOrWhiteSpace(user.Phone) ? null : user.Phone.Trim();
        existing.Role = user.Role;
        existing.Status = user.Status;
        existing.UpdatedAt = DateTime.Now;

        await Context.SaveChangesAsync();
        return existing;
    }

    public async Task<AppUser?> UpdatePasswordAsync(int id, string passwordHash, string passwordSalt)
    {
        var existing = await Context.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.PasswordHash = passwordHash;
        existing.PasswordSalt = passwordSalt;
        existing.UpdatedAt = DateTime.Now;

        await Context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var existing = await Context.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        Context.Users.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }
}
