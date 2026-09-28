using Reservation.Domain.Entities;

namespace Reservation.Application.Abstractions.Persistence;

/// <summary>
/// Contract for the professional ↔ service association (N:M).
/// Implemented in Infrastructure, which also enforces uniqueness of the pair.
/// </summary>
public interface IProfessionalServiceRepository
{
    Task AddAsync(ProfessionalService association, CancellationToken cancellationToken = default);

    /// <summary>Required by docs/domain-model.md Regla 4: the professional must offer the service.</summary>
    Task<bool> ExistsAsync(Guid professionalId, Guid serviceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfessionalService>> GetByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default);

    /// <summary>Removes the pair if present; does nothing when it does not exist.</summary>
    Task DeleteAsync(Guid professionalId, Guid serviceId, CancellationToken cancellationToken = default);
}
