using Reservation.Application.Abstractions.Persistence;
using Reservation.Application.Abstractions.Services;
using Reservation.Application.Common;

namespace Reservation.Application.Features.Users;

/// <summary>
/// Use cases for users. Registration/login belong to the Auth feature.
/// Ownership rule: a Client may only read their own record; an Admin may read any record.
/// </summary>
public class UserUseCases
{
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;

    public UserUseCases(IUserRepository users, ICurrentUser currentUser)
    {
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<Result<UserModel>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // HTTP authorization already requires an authenticated caller; this keeps the
        // rule correct wherever the use case is invoked from.
        if (!_currentUser.IsAuthenticated)
            return Result<UserModel>.Unauthorized("Authentication is required.");

        // Ownership is checked before reading, so a forbidden request never reveals
        // whether the requested user exists.
        if (!_currentUser.IsAdmin && _currentUser.UserId != id)
            return Result<UserModel>.Forbidden("You can only access your own user information.");

        var user = await _users.GetByIdAsync(id, cancellationToken);
        return user is null
            ? Result<UserModel>.NotFound($"User '{id}' was not found.")
            : Result<UserModel>.Success(UserModel.From(user));
    }
}
