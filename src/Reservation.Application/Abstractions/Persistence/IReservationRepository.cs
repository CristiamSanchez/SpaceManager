using Reservation.Domain.Entities;
// "Reservation" alone resolves to the root namespace inside Reservation.*, so alias the entity.
using ReservationEntity = Reservation.Domain.Entities.Reservation;

namespace Reservation.Application.Abstractions.Persistence;

/// <summary>
/// Contract for persisting and querying reservations. Implemented in Infrastructure.
/// </summary>
public interface IReservationRepository
{
    Task AddAsync(ReservationEntity reservation, CancellationToken cancellationToken = default);

    Task<ReservationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReservationEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>All reservations (admin listing).</summary>
    Task<IReadOnlyList<ReservationEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(ReservationEntity reservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reservations of the professional that overlap the requested range and have a
    /// status other than Cancelled (docs/domain-model.md Reglas 7 and 10).
    /// Used later for conflict detection; the filtering happens in Infrastructure.
    /// </summary>
    Task<IReadOnlyList<ReservationEntity>> GetConflictingAsync(
        Guid professionalId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default);
}
