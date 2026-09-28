using Reservation.Application.Abstractions.Services;

namespace Reservation.Application.Tests.Fakes;

/// <summary>
/// Test double for ICurrentUser. Tests configure authentication state,
/// UserId and IsAdmin per scenario. No HttpContext involved.
/// </summary>
public class FakeCurrentUser : ICurrentUser
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public bool IsAuthenticated { get; set; } = true;
    public bool IsAdmin { get; set; }
}
