using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.AuthService;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
