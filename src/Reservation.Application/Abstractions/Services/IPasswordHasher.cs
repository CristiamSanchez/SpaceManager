namespace Reservation.Application.Abstractions.Services;

/// <summary>
/// Hashes and verifies passwords. Implementation lives in Infrastructure;
/// plaintext passwords are never stored.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
