using Reservation.Application.Common;
using Reservation.Application.Features.Auth;
using Reservation.Application.Tests.Fakes;
using Reservation.Domain.Entities;
using Reservation.Domain.Enums;

namespace Reservation.Application.Tests;

public class AuthUseCasesTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeTokenService _tokenService = new();
    private readonly AuthUseCases _useCases;

    public AuthUseCasesTests()
    {
        _useCases = new AuthUseCases(_users, _passwordHasher, _tokenService);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        _users.Items.Add(new User("Ana", "ana@test.com", "hash", UserRole.Client));

        var result = await _useCases.RegisterAsync("Ana Dos", "ana@test.com", "secret123");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Conflict, result.Kind);
        Assert.Single(_users.Items);
    }

    [Fact]
    public async Task Register_HashesPasswordThroughPasswordHasher()
    {
        var result = await _useCases.RegisterAsync("Ana", "ana@test.com", "secret123");

        Assert.True(result.IsSuccess);
        var stored = _users.Items.Single();
        Assert.Equal(1, _passwordHasher.HashCallCount);
        // The plaintext password is never stored.
        Assert.NotEqual("secret123", stored.PasswordHash);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var result = await _useCases.LoginAsync("nobody@test.com", "secret123");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Unauthorized, result.Kind);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        _users.Items.Add(new User("Ana", "ana@test.com", _passwordHasher.Hash("secret123"), UserRole.Client));

        var result = await _useCases.LoginAsync("ana@test.com", "wrong-password");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Unauthorized, result.Kind);
    }

    [Fact]
    public async Task Login_FailureMessage_IsIdenticalForUnknownEmailAndWrongPassword()
    {
        // Security: the response must never disclose whether an account exists.
        _users.Items.Add(new User("Ana", "ana@test.com", _passwordHasher.Hash("secret123"), UserRole.Client));

        var unknownEmail = await _useCases.LoginAsync("nobody@test.com", "secret123");
        var wrongPassword = await _useCases.LoginAsync("ana@test.com", "wrong-password");

        Assert.Equal(unknownEmail.Error, wrongPassword.Error);
    }

    [Fact]
    public async Task Login_WithValidCredentials_CreatesTokenThroughTokenService()
    {
        _users.Items.Add(new User("Ana", "ana@test.com", _passwordHasher.Hash("secret123"), UserRole.Client));

        var result = await _useCases.LoginAsync("ana@test.com", "secret123");

        Assert.True(result.IsSuccess);
        Assert.Equal(1, _tokenService.CreateTokenCallCount);
        Assert.Equal("test-token", result.Value!.AccessToken);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsExpectedUserInformation()
    {
        var user = new User("Ana", "ana@test.com", _passwordHasher.Hash("secret123"), UserRole.Client);
        _users.Items.Add(user);

        var result = await _useCases.LoginAsync("ana@test.com", "secret123");

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value!.User.Id);
        Assert.Equal("Ana", result.Value.User.Name);
        Assert.Equal("ana@test.com", result.Value.User.Email);
        Assert.Equal(UserRole.Client, result.Value.User.Role);
        Assert.True(result.Value.User.IsActive);
    }
}
