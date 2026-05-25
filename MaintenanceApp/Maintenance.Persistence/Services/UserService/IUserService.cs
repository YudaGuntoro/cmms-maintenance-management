using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.UserService;

public interface IUserService
{
    Task<List<UserResponse>> GetUsersAsync(string? search, AppUserRole? role, AppUserStatus? status);
    Task<UserResponse?> GetUserAsync(int id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<UserResponse?> ChangePasswordAsync(int id, ChangePasswordRequest request);
    Task<bool> DeleteUserAsync(int id);
}
