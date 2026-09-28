using Reservation.Domain.Entities;

namespace Reservation.Application.Abstractions.Persistence;

/// <summary>
/// Contract for persisting and querying professionals. Implemented in Infrastructure.
/// </summary>
public interface IProfessionalRepository
{
    Task AddAsync(Professional professional, CancellationToken cancellationToken = default);

    Task<Professional?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Professional>> GetAllAsync(CancellationToken cancellationToken = default);
}
