using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.AuthRepository;

public interface IAuthRepository
{
    Task<AppUser?> GetUserForLoginAsync(string username);
    Task UpdateLastLoginAsync(int userId, DateTime lastLoginAt);
}
