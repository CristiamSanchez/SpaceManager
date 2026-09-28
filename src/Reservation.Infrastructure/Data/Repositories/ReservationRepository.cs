using Microsoft.EntityFrameworkCore;
using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Enums;
using ReservationEntity = Reservation.Domain.Entities.Reservation;

namespace Reservation.Infrastructure.Data.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ReservationDbContext _dbContext;

    public ReservationRepository(ReservationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ReservationEntity reservation, CancellationToken cancellationToken = default)
    {
        await _dbContext.Reservations.AddAsync(reservation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<ReservationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ReservationEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ReservationEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Reservations
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(ReservationEntity reservation, CancellationToken cancellationToken = default)
    {
        _dbContext.Reservations.Update(reservation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReservationEntity>> GetConflictingAsync(
        Guid professionalId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default)
        => await _dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.ProfessionalId == professionalId
                && r.Status != ReservationStatus.Cancelled
                && r.StartAt < endAt
                && r.EndAt > startAt)
            .ToListAsync(cancellationToken);
}
