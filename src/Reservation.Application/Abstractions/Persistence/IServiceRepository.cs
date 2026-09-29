using Reservation.Domain.Entities;

namespace Reservation.Application.Abstractions.Persistence;

/// <summary>
/// Contract for persisting and querying services. Implemented in Infrastructure.
/// </summary>
public interface IServiceRepository
{
    Task AddAsync(Service service, CancellationToken cancellationToken = default);

    Task UpdateAsync(Service service, CancellationToken cancellationToken = default);

    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken cancellationToken = default);
}
