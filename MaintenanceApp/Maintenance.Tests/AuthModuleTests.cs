using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Maintenance.Persistence.Services.AuthService;
using Maintenance.Persistence.Services.UserService;
using Maintenance.Repository.AuthRepository;
using Maintenance.Repository.UserRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Maintenance.Tests;

public class AuthModuleTests
{
    [Fact]
    public async Task CreateUserAsync_HashesPassword_AndLoginSucceeds()
    {
        await using var db = CreateDbContext();
        var userService = new UserService(new UserRepository(db));
        var authService = CreateAuthService(db);

        var created = await userService.CreateUserAsync(new CreateUserRequest
        {
            Username = "admin",
            Password = "admin123",
            FullName = "CMMS Administrator",
            Email = "admin@cmms.local",
            Role = AppUserRole.ADMIN,
            Status = AppUserStatus.ACTIVE
        });

        var storedUser = await db.Users.FirstAsync();
        var login = await authService.LoginAsync(new LoginRequest
        {
            Username = "admin",
            Password = "admin123"
        });

        Assert.True(created.Id > 0);
        Assert.NotEqual("admin123", storedUser.PasswordHash);
        Assert.False(string.IsNullOrWhiteSpace(storedUser.PasswordSalt));
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Equal(AppUserRole.ADMIN, login.User.Role);
        Assert.NotNull((await db.Users.FirstAsync()).LastLoginAt);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_IsRejected()
    {
        await using var db = CreateDbContext();
        var userService = new UserService(new UserRepository(db));
        var authService = CreateAuthService(db);

        await userService.CreateUserAsync(new CreateUserRequest
        {
            Username = "inactive",
            Password = "secret",
            FullName = "Inactive User",
            Role = AppUserRole.VIEWER,
            Status = AppUserStatus.INACTIVE
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginAsync(new LoginRequest { Username = "inactive", Password = "secret" }));
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateUsername_IsRejected()
    {
        await using var db = CreateDbContext();
        var userService = new UserService(new UserRepository(db));

        await userService.CreateUserAsync(new CreateUserRequest
        {
            Username = "planner",
            Password = "secret",
            FullName = "Planner One"
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            userService.CreateUserAsync(new CreateUserRequest
            {
                Username = "planner",
                Password = "secret",
                FullName = "Planner Two"
            }));
    }

    private static MaintenanceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MaintenanceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MaintenanceDbContext(options);
    }

    private static AuthService CreateAuthService(MaintenanceDbContext db)
    {
        return new AuthService(
            new AuthRepository(db),
            Options.Create(new JwtSettings
            {
                Issuer = "MaintenanceApp.Tests",
                Audience = "MaintenanceApp.Tests",
                SigningKey = "MaintenanceApp-Tests-Jwt-Signing-Key-2026-Long-Enough",
                ExpiresHours = 8
            }));
    }
}
