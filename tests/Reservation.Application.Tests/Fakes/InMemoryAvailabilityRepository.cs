using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Entities;

namespace Reservation.Application.Tests.Fakes;

/// <summary>
/// In-memory version of IAvailabilityRepository. The read and overlap methods
/// follow the documented repository contract (active-only listing, overlap
/// formula against active periods), not the EF Core implementation.
/// </summary>
public class InMemoryAvailabilityRepository : IAvailabilityRepository
{
    public List<Availability> Items { get; } = new();

    public Task AddAsync(Availability availability, CancellationToken cancellationToken = default)
    {
        Items.Add(availability);
        return Task.CompletedTask;
    }

    public Task<Availability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(a => a.Id == id));

    // Contract: only active periods are returned by the listing.
    public Task<IReadOnlyList<Availability>> GetByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Availability>>(
            Items.Where(a => a.ProfessionalId == professionalId && a.IsActive).ToList());

    public Task UpdateAsync(Availability availability, CancellationToken cancellationToken = default)
    {
        // Entities are stored by reference, so mutations are already visible.
        return Task.CompletedTask;
    }

    public Task<bool> HasOverlapAsync(
        Guid professionalId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeAvailabilityId = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(a =>
            a.ProfessionalId == professionalId
            && a.DayOfWeek == dayOfWeek
            && a.IsActive
            && a.StartTime < endTime
            && a.EndTime > startTime
            && (excludeAvailabilityId is null || a.Id != excludeAvailabilityId)));
}
