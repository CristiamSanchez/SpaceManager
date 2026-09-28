using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Entities;
using Reservation.Domain.Enums;
// "Reservation" alone resolves to the root namespace inside Reservation.*, so alias the entity.
using ReservationEntity = Reservation.Domain.Entities.Reservation;

namespace Reservation.Application.Tests.Fakes;

/// <summary>
/// In-memory version of IReservationRepository. GetConflictingAsync follows the
/// documented repository contract: Status != Cancelled AND StartAt &lt; requestedEnd
/// AND EndAt &gt; requestedStart.
/// </summary>
public class InMemoryReservationRepository : IReservationRepository
{
    public List<ReservationEntity> Items { get; } = new();

    public Task AddAsync(ReservationEntity reservation, CancellationToken cancellationToken = default)
    {
        Items.Add(reservation);
        return Task.CompletedTask;
    }

    public Task<ReservationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(r => r.Id == id));

    public Task<IReadOnlyList<ReservationEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ReservationEntity>>(Items.Where(r => r.UserId == userId).ToList());

    public Task<IReadOnlyList<ReservationEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ReservationEntity>>(Items.ToList());

    public Task UpdateAsync(ReservationEntity reservation, CancellationToken cancellationToken = default)
    {
        // Entities are stored by reference, so mutations are already visible.
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ReservationEntity>> GetConflictingAsync(
        Guid professionalId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ReservationEntity>>(Items
            .Where(r => r.ProfessionalId == professionalId
                && r.Status != ReservationStatus.Cancelled
                && r.StartAt < endAt
                && r.EndAt > startAt)
            .ToList());
}
