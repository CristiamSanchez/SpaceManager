using System.Security.Cryptography;
using Reservation.Application.Abstractions.Services;

namespace Reservation.Infrastructure.Security;

/// <summary>
/// PBKDF2 (HMAC-SHA256) password hashing — the standard mechanism built into .NET.
/// Stored format: PBKDF2-SHA256$&lt;iterations&gt;$&lt;salt&gt;$&lt;hash&gt; (base64).
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const string Prefix = "PBKDF2-SHA256";
    private const int SaltSize = 16;      // 128-bit random salt
    private const int KeySize = 32;       // 256-bit derived key
    private const int Iterations = 600_000; // OWASP recommendation for PBKDF2-HMAC-SHA256

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            return false;

        try
        {
            var parts = passwordHash.Split('$');
            if (parts.Length != 4 || parts[0] != Prefix)
                return false;
            if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
                return false;

            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);

            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            // Malformed stored hash.
            return false;
        }
    }
}
