namespace Reservation.Application.Abstractions.Services;

/// <summary>
/// Gives use cases access to the identity of the authenticated user without
/// depending on ASP.NET Core, HTTP or JWT. The API provides the implementation.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Identifier of the current user, or <see cref="Guid.Empty"/> when unauthenticated.</summary>
    Guid UserId { get; }

    bool IsAuthenticated { get; }

    /// <summary>True when the authenticated user has the Admin role.</summary>
    bool IsAdmin { get; }
}
