using Reservation.Domain.Entities;

namespace Reservation.Application.Abstractions.Persistence;

/// <summary>
/// Contract for persisting and querying users. Implemented in Infrastructure.
/// </summary>
public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Email is unique (docs/domain-model.md §3) and is the login identifier.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
