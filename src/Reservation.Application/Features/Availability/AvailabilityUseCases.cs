using Reservation.Application.Abstractions.Persistence;
using Reservation.Application.Common;
// "Availability" alone resolves to this namespace inside Reservation.Application.Features.*, so alias the entity.
using AvailabilityEntity = Reservation.Domain.Entities.Availability;

namespace Reservation.Application.Features.Availability;

/// <summary>Use cases for professional availability (docs/domain-model.md §7).</summary>
public class AvailabilityUseCases
{
    private readonly IAvailabilityRepository _availability;
    private readonly IProfessionalRepository _professionals;

    public AvailabilityUseCases(
        IAvailabilityRepository availability,
        IProfessionalRepository professionals)
    {
        _availability = availability;
        _professionals = professionals;
    }

    /// <summary>Active periods only, ordered by DayOfWeek then StartTime (docs phase 10 §8–§9).</summary>
    public async Task<IReadOnlyList<AvailabilityEntity>> GetByProfessionalAsync(
        Guid professionalId,
        CancellationToken cancellationToken = default)
    {
        var periods = await _availability.GetByProfessionalAsync(professionalId, cancellationToken);

        // DayOfWeek is persisted as text, so database ordering would be alphabetical
        // (Friday < Monday < ...) instead of calendar order; the sort is done here,
        // where DayOfWeek is the enum value.
        return periods
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .ToList();
    }

    public async Task<Result<AvailabilityEntity>> CreateAsync(
        Guid professionalId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken = default)
    {
        if (professionalId == Guid.Empty)
            return Result<AvailabilityEntity>.Failure("ProfessionalId must be a valid identifier.");

        // Cross-entity rule: the professional must exist before availability is created.
        if (await _professionals.GetByIdAsync(professionalId, cancellationToken) is null)
            return Result<AvailabilityEntity>.NotFound($"Professional '{professionalId}' was not found.");

        // Domain invariant (docs/domain-model.md §7): StartTime must be earlier than EndTime.
        if (startTime >= endTime)
            return Result<AvailabilityEntity>.Failure("StartTime must be earlier than EndTime.");

        // Overlap prevention among active periods of the same professional/day.
        if (await _availability.HasOverlapAsync(professionalId, dayOfWeek, startTime, endTime, cancellationToken: cancellationToken))
            return Result<AvailabilityEntity>.Conflict(
                "The professional already has an active availability period overlapping this time range.");

        var availability = new AvailabilityEntity(professionalId, dayOfWeek, startTime, endTime);
        await _availability.AddAsync(availability, cancellationToken);

        return Result<AvailabilityEntity>.Success(availability);
    }

    public async Task<Result<AvailabilityEntity>> UpdateAsync(
        Guid professionalId,
        Guid availabilityId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (professionalId == Guid.Empty)
            return Result<AvailabilityEntity>.Failure("ProfessionalId must be a valid identifier.");
        if (availabilityId == Guid.Empty)
            return Result<AvailabilityEntity>.Failure("AvailabilityId must be a valid identifier.");

        if (await _professionals.GetByIdAsync(professionalId, cancellationToken) is null)
            return Result<AvailabilityEntity>.NotFound($"Professional '{professionalId}' was not found.");

        var availability = await _availability.GetByIdAsync(availabilityId, cancellationToken);
        if (availability is null)
            return Result<AvailabilityEntity>.NotFound($"Availability '{availabilityId}' was not found.");

        // The record must belong to the professional in the route; the professional
        // itself can never be changed through this endpoint.
        if (availability.ProfessionalId != professionalId)
            return Result<AvailabilityEntity>.NotFound($"Availability '{availabilityId}' was not found.");

        if (startTime >= endTime)
            return Result<AvailabilityEntity>.Failure("StartTime must be earlier than EndTime.");

        // Overlap only matters for the resulting record when it stays active;
        // the current record is excluded from the query.
        if (isActive && await _availability.HasOverlapAsync(
                professionalId, dayOfWeek, startTime, endTime, availabilityId, cancellationToken))
            return Result<AvailabilityEntity>.Conflict(
                "The professional already has an active availability period overlapping this time range.");

        availability.Update(dayOfWeek, startTime, endTime);
        if (isActive)
            availability.Activate();
        else
            availability.Deactivate();

        await _availability.UpdateAsync(availability, cancellationToken);

        return Result<AvailabilityEntity>.Success(availability);
    }

    /// <summary>Soft deactivation (IsActive = false); availability history is preserved.</summary>
    public async Task<Result<Guid>> DeactivateAsync(
        Guid professionalId,
        Guid availabilityId,
        CancellationToken cancellationToken = default)
    {
        if (professionalId == Guid.Empty)
            return Result<Guid>.Failure("ProfessionalId must be a valid identifier.");
        if (availabilityId == Guid.Empty)
            return Result<Guid>.Failure("AvailabilityId must be a valid identifier.");

        if (await _professionals.GetByIdAsync(professionalId, cancellationToken) is null)
            return Result<Guid>.NotFound($"Professional '{professionalId}' was not found.");

        var availability = await _availability.GetByIdAsync(availabilityId, cancellationToken);
        if (availability is null)
            return Result<Guid>.NotFound($"Availability '{availabilityId}' was not found.");

        if (availability.ProfessionalId != professionalId)
            return Result<Guid>.NotFound($"Availability '{availabilityId}' was not found.");

        availability.Deactivate();
        await _availability.UpdateAsync(availability, cancellationToken);

        return Result<Guid>.Success(availabilityId);
    }
}
