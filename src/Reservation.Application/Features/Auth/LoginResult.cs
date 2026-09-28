using Reservation.Application.Features.Users;

namespace Reservation.Application.Features.Auth;

/// <summary>
/// Safe authentication response: access token, expiration and basic user information.
/// Never contains PasswordHash.
/// </summary>
public record LoginResult(
    string AccessToken,
    DateTime ExpiresAtUtc,
    UserModel User);
