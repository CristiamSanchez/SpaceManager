using Reservation.Application.Abstractions.Services;

namespace Reservation.Application.Tests.Fakes;

/// <summary>
/// Deterministic password hasher for tests. Real hashing (PBKDF2) is an
/// Infrastructure concern and is not exercised here.
/// </summary>
public class FakePasswordHasher : IPasswordHasher
{
    public int HashCallCount { get; private set; }

    public string Hash(string password)
    {
        HashCallCount++;
        return $"hashed:{password}";
    }

    public bool Verify(string password, string passwordHash) =>
        passwordHash == $"hashed:{password}";
}
