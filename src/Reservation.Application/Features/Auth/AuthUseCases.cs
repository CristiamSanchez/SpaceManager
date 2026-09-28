using Reservation.Application.Abstractions.Persistence;
using Reservation.Application.Abstractions.Services;
using Reservation.Application.Common;
using Reservation.Application.Features.Users;
using Reservation.Domain.Entities;
using Reservation.Domain.Enums;

namespace Reservation.Application.Features.Auth;

/// <summary>
/// Registration and login use cases (docs/domain-model.md §3, README §4).
/// Authorization/roles are handled in later phases.
/// </summary>
public class AuthUseCases
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthUseCases(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<UserModel>> RegisterAsync(
        string? name,
        string? email,
        string? password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<UserModel>.Failure("Name is required.");
        if (string.IsNullOrWhiteSpace(email))
            return Result<UserModel>.Failure("Email is required.");
        if (string.IsNullOrWhiteSpace(password))
            return Result<UserModel>.Failure("Password is required.");

        if (await _users.GetByEmailAsync(email, cancellationToken) is not null)
            return Result<UserModel>.Conflict("Email is already registered.");

        // Password is stored only as a hash; default role is Client.
        var user = new User(name, email, _passwordHasher.Hash(password), UserRole.Client);
        await _users.AddAsync(user, cancellationToken);

        return Result<UserModel>.Success(UserModel.From(user));
    }

    public async Task<Result<LoginResult>> LoginAsync(
        string? email,
        string? password,
        CancellationToken cancellationToken = default)
    {
        // Same failure whether the email exists or the password is wrong:
        // never disclose whether an account exists.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return Result<LoginResult>.Unauthorized("Invalid email or password.");

        var user = await _users.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
            return Result<LoginResult>.Unauthorized("Invalid email or password.");

        if (!user.IsActive)
            return Result<LoginResult>.Unauthorized("Your account is inactive.");

        var token = _tokenService.CreateToken(user);

        return Result<LoginResult>.Success(
            new LoginResult(token.AccessToken, token.ExpiresAtUtc, UserModel.From(user)));
    }
}
