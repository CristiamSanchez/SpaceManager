using Reservation.Application.Common;
using Reservation.Application.Features.Users;
using Reservation.Application.Tests.Fakes;
using Reservation.Domain.Entities;
using Reservation.Domain.Enums;

namespace Reservation.Application.Tests;

public class UserUseCasesTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly FakeCurrentUser _currentUser = new();
    private readonly UserUseCases _useCases;

    public UserUseCasesTests()
    {
        _useCases = new UserUseCases(_users, _currentUser);
    }

    private User SeedUser(string email)
    {
        var user = new User("User", email, "hash", UserRole.Client);
        _users.Items.Add(user);
        return user;
    }

    [Fact]
    public async Task GetById_WhenUnauthenticated_ReturnsUnauthorized()
    {
        _currentUser.IsAuthenticated = false;

        var result = await _useCases.GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Unauthorized, result.Kind);
    }

    [Fact]
    public async Task GetById_ClientRequestingAnotherUsersRecord_ReturnsForbidden()
    {
        var other = SeedUser("other@test.com");
        _currentUser.IsAuthenticated = true;
        _currentUser.IsAdmin = false;
        _currentUser.UserId = Guid.NewGuid(); // someone else

        var result = await _useCases.GetByIdAsync(other.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Forbidden, result.Kind);
    }

    [Fact]
    public async Task GetById_ClientRequestingOwnRecord_Succeeds()
    {
        var own = SeedUser("own@test.com");
        _currentUser.IsAdmin = false;
        _currentUser.UserId = own.Id;

        var result = await _useCases.GetByIdAsync(own.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(own.Id, result.Value!.Id);
        Assert.Equal("own@test.com", result.Value.Email);
    }

    [Fact]
    public async Task GetById_AdminRequestingAnotherUsersRecord_Succeeds()
    {
        var other = SeedUser("other@test.com");
        _currentUser.IsAdmin = true;
        _currentUser.UserId = Guid.NewGuid();

        var result = await _useCases.GetByIdAsync(other.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(other.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetById_WhenUserIsMissing_ReturnsNotFound()
    {
        _currentUser.IsAdmin = true;

        var result = await _useCases.GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }
}
