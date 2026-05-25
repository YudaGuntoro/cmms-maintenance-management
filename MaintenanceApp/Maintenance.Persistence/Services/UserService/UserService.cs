using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.Shared;
using Maintenance.Repository.UserRepository;

namespace Maintenance.Persistence.Services.UserService;

public class UserService : IUserService
{
    protected readonly IUserRepository Context;

    public UserService(IUserRepository data) => Context = data;

    public async Task<List<UserResponse>> GetUsersAsync(string? search, AppUserRole? role, AppUserStatus? status)
    {
        var users = await Context.GetUsersAsync(search, role, status);
        return users.Select(UserResponseMapper.ToResponse).ToList();
    }

    public async Task<UserResponse?> GetUserAsync(int id)
    {
        var user = await Context.GetUserAsync(id);
        return user == null ? null : UserResponseMapper.ToResponse(user);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        ValidateCreateRequest(request);
        await EnsureUsernameIsAvailableAsync(request.Username, null);
        await EnsureEmailIsAvailableAsync(request.Email, null);

        var salt = AuthPasswordHasher.CreateSalt();
        var user = new AppUser
        {
            Username = request.Username,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Role = request.Role,
            Status = request.Status,
            PasswordSalt = salt,
            PasswordHash = AuthPasswordHasher.HashPassword(request.Password, salt)
        };

        return UserResponseMapper.ToResponse(await Context.CreateUserAsync(user));
    }

    public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        ValidateUpdateRequest(request);
        await EnsureUsernameIsAvailableAsync(request.Username, id);
        await EnsureEmailIsAvailableAsync(request.Email, id);

        var user = new AppUser
        {
            Username = request.Username,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Role = request.Role,
            Status = request.Status
        };

        var updated = await Context.UpdateUserAsync(id, user);
        return updated == null ? null : UserResponseMapper.ToResponse(updated);
    }

    public async Task<UserResponse?> ChangePasswordAsync(int id, ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        var salt = AuthPasswordHasher.CreateSalt();
        var updated = await Context.UpdatePasswordAsync(id, AuthPasswordHasher.HashPassword(request.Password, salt), salt);
        return updated == null ? null : UserResponseMapper.ToResponse(updated);
    }

    public Task<bool> DeleteUserAsync(int id)
    {
        return Context.DeleteUserAsync(id);
    }

    private static void ValidateCreateRequest(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }
    }

    private static void ValidateUpdateRequest(UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }
    }

    private async Task EnsureUsernameIsAvailableAsync(string username, int? currentUserId)
    {
        var existing = await Context.GetUserByUsernameAsync(username.Trim());
        if (existing != null && existing.Id != currentUserId)
        {
            throw new InvalidOperationException("Username already exists.");
        }
    }

    private async Task EnsureEmailIsAvailableAsync(string? email, int? currentUserId)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var existing = await Context.GetUserByEmailAsync(email.Trim());
        if (existing != null && existing.Id != currentUserId)
        {
            throw new InvalidOperationException("Email already exists.");
        }
    }
}
