namespace Reservation.Infrastructure.Security;

/// <summary>
/// Strongly typed JWT configuration (section "Jwt").
/// The secret must come from configuration/environment (e.g. Jwt__SecretKey);
/// never commit a real production secret.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
