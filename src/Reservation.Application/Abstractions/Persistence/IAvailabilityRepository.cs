using Reservation.Domain.Entities;

namespace Reservation.Application.Abstractions.Persistence;

/// <summary>
/// Contract for persisting and querying availability periods.
/// Implemented in Infrastructure.
/// </summary>
public interface IAvailabilityRepository
{
    Task AddAsync(Availability availability, CancellationToken cancellationToken = default);

    /// <summary>Active records only, for the public availability view.</summary>
    Task<IReadOnlyList<Availability>> GetByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default);

    Task<Availability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateAsync(Availability availability, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asks the database whether an active period for the same professional/day overlaps
    /// [startTime, endTime). When updating, pass the current record's id to exclude it.
    /// </summary>
    Task<bool> HasOverlapAsync(
        Guid professionalId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeAvailabilityId = null,
        CancellationToken cancellationToken = default);
}
