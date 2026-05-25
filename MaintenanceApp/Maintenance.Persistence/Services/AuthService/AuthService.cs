using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.Shared;
using Maintenance.Repository.AuthRepository;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Maintenance.Persistence.Services.AuthService;

public class AuthService : IAuthService
{
    protected readonly IAuthRepository Context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IAuthRepository data, IOptions<JwtSettings> jwtOptions)
    {
        Context = data;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var user = await Context.GetUserForLoginAsync(request.Username.Trim());
        if (user == null || !AuthPasswordHasher.VerifyPassword(request.Password, user.PasswordSalt, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        if (user.Status != AppUserStatus.ACTIVE)
        {
            throw new UnauthorizedAccessException("User is inactive.");
        }

        var loginAt = DateTime.Now;
        var expiresAt = loginAt.AddHours(_jwtSettings.ExpiresHours);
        await Context.UpdateLastLoginAsync(user.Id, loginAt);
        user.LastLoginAt = loginAt;

        return new LoginResponse
        {
            AccessToken = CreateJwt(user, loginAt, expiresAt),
            TokenType = "Bearer",
            ExpiresAt = expiresAt,
            User = UserResponseMapper.ToResponse(user)
        };
    }

    private string CreateJwt(AppUser user, DateTime issuedAt, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("username", user.Username),
            new("full_name", user.FullName),
            new("role", user.Role.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
