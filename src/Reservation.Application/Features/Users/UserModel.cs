using Reservation.Domain.Entities;
using Reservation.Domain.Enums;

namespace Reservation.Application.Features.Users;

/// <summary>
/// Application-safe user model: PasswordHash is never included, so it can be
/// returned by use cases without leaking credentials.
/// </summary>
public record UserModel(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static UserModel From(User user) =>
        new(user.Id, user.Name, user.Email, user.Role, user.IsActive, user.CreatedAt, user.UpdatedAt);
}
