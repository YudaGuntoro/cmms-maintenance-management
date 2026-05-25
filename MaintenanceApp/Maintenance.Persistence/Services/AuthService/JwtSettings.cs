namespace Maintenance.Persistence.Services.AuthService;

public class JwtSettings
{
    public string Issuer { get; set; } = "MaintenanceApp";
    public string Audience { get; set; } = "MaintenanceApp.Frontend";
    public string SigningKey { get; set; } = "MaintenanceApp-Development-Jwt-Signing-Key-2026-Change-Me";
    public int ExpiresHours { get; set; } = 8;
}
