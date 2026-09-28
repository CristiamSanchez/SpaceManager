using Reservation.Domain.Entities;

namespace Reservation.Application.Abstractions.Services;

/// <summary>Result of generating an access token.</summary>
public record TokenResult(string AccessToken, DateTime ExpiresAtUtc);

/// <summary>
/// Generates access tokens for authenticated users. Application does not depend on
/// JWT implementation details; Infrastructure provides the concrete implementation.
/// </summary>
public interface ITokenService
{
    /// <summary>Creates a token containing at least the user's ID, email and role.</summary>
    TokenResult CreateToken(User user);
}
