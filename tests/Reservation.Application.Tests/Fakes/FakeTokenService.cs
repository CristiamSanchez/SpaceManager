using Reservation.Application.Abstractions.Services;
using Reservation.Domain.Entities;

namespace Reservation.Application.Tests.Fakes;

/// <summary>
/// Test double for ITokenService: records invocations and returns a fixed token,
/// so tests can verify the use case calls it without dealing with JWT details.
/// </summary>
public class FakeTokenService : ITokenService
{
    public int CreateTokenCallCount { get; private set; }
    public User? LastUser { get; private set; }

    public TokenResult CreateToken(User user)
    {
        CreateTokenCallCount++;
        LastUser = user;
        return new TokenResult("test-token", DateTime.UtcNow.AddMinutes(30));
    }
}
