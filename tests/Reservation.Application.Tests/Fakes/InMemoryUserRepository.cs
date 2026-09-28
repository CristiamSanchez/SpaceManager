using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Entities;

namespace Reservation.Application.Tests.Fakes;

public class InMemoryUserRepository : IUserRepository
{
    public List<User> Items { get; } = new();

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        Items.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(u => u.Email == email));
}
