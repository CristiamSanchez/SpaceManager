using Microsoft.EntityFrameworkCore;
using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Entities;

namespace Reservation.Infrastructure.Data.Repositories;

public class AvailabilityRepository : IAvailabilityRepository
{
    private readonly ReservationDbContext _dbContext;

    public AvailabilityRepository(ReservationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Availability availability, CancellationToken cancellationToken = default)
    {
        await _dbContext.Availabilities.AddAsync(availability, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Public view: active records only (docs phase 10 §8).</summary>
    public async Task<IReadOnlyList<Availability>> GetByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default)
        => await _dbContext.Availabilities
            .AsNoTracking()
            .Where(a => a.ProfessionalId == professionalId && a.IsActive)
            .ToListAsync(cancellationToken);

    public async Task<Availability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Availabilities
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task UpdateAsync(Availability availability, CancellationToken cancellationToken = default)
    {
        _dbContext.Availabilities.Update(availability);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // Overlap rule (docs/domain-model.md §7): Existing.StartTime < New.EndTime AND
    // Existing.EndTime > New.StartTime, among active records of the same professional/day.
    // Executed by the database; adjacency (08:00-09:00 next to 09:00-12:00) is allowed.
    public Task<bool> HasOverlapAsync(
        Guid professionalId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeAvailabilityId = null,
        CancellationToken cancellationToken = default)
        => _dbContext.Availabilities
            .AsNoTracking()
            .AnyAsync(a =>
                a.ProfessionalId == professionalId
                && a.DayOfWeek == dayOfWeek
                && a.IsActive
                && a.StartTime < endTime
                && a.EndTime > startTime
                && (excludeAvailabilityId == null || a.Id != excludeAvailabilityId.Value),
                cancellationToken);
}
