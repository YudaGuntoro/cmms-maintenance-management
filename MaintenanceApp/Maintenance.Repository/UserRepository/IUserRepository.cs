using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.UserRepository;

public interface IUserRepository
{
    Task<List<AppUser>> GetUsersAsync(string? search, AppUserRole? role, AppUserStatus? status);
    Task<AppUser?> GetUserAsync(int id);
    Task<AppUser?> GetUserByUsernameAsync(string username);
    Task<AppUser?> GetUserByEmailAsync(string email);
    Task<AppUser> CreateUserAsync(AppUser user);
    Task<AppUser?> UpdateUserAsync(int id, AppUser user);
    Task<AppUser?> UpdatePasswordAsync(int id, string passwordHash, string passwordSalt);
    Task<bool> DeleteUserAsync(int id);
}
