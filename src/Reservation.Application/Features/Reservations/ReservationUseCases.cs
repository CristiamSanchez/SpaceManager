using Reservation.Application.Abstractions.Persistence;
using Reservation.Application.Abstractions.Services;
using Reservation.Application.Common;
using Reservation.Domain.Enums;
// "Reservation" alone resolves to the root namespace inside Reservation.*, so alias the entity.
using ReservationEntity = Reservation.Domain.Entities.Reservation;

namespace Reservation.Application.Features.Reservations;

/// <summary>
/// Reservation workflow (docs/domain-model.md §8): create, list, read and cancel,
/// enforcing the ten reservation rules and the ownership table of docs phase 11 §13.
/// </summary>
public class ReservationUseCases
{
    private readonly IReservationRepository _reservations;
    private readonly IUserRepository _users;
    private readonly IProfessionalRepository _professionals;
    private readonly IServiceRepository _services;
    private readonly IProfessionalServiceRepository _professionalServices;
    private readonly IAvailabilityRepository _availability;
    private readonly ICurrentUser _currentUser;

    public ReservationUseCases(
        IReservationRepository reservations,
        IUserRepository users,
        IProfessionalRepository professionals,
        IServiceRepository services,
        IProfessionalServiceRepository professionalServices,
        IAvailabilityRepository availability,
        ICurrentUser currentUser)
    {
        _reservations = reservations;
        _users = users;
        _professionals = professionals;
        _services = services;
        _professionalServices = professionalServices;
        _availability = availability;
        _currentUser = currentUser;
    }

    /// <summary>Client → only their own reservations; Admin → all reservations.</summary>
    public async Task<Result<IReadOnlyList<ReservationEntity>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated())
            return Result<IReadOnlyList<ReservationEntity>>.Unauthorized("Authentication is required.");

        var reservations = _currentUser.IsAdmin
            ? await _reservations.GetAllAsync(cancellationToken)
            : await _reservations.GetByUserIdAsync(_currentUser.UserId, cancellationToken);

        return Result<IReadOnlyList<ReservationEntity>>.Success(reservations);
    }

    public async Task<Result<ReservationEntity>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated())
            return Result<ReservationEntity>.Unauthorized("Authentication is required.");

        var reservation = await _reservations.GetByIdAsync(id, cancellationToken);
        if (reservation is null)
            return Result<ReservationEntity>.NotFound($"Reservation '{id}' was not found.");

        if (!IsOwner(reservation))
            return Result<ReservationEntity>.Forbidden("You can only access your own reservations.");

        return Result<ReservationEntity>.Success(reservation);
    }

    public async Task<Result<ReservationEntity>> CreateAsync(
        Guid? professionalId,
        Guid? serviceId,
        DateTime? startAt,
        string? notes,
        Guid? requestedUserId,
        CancellationToken cancellationToken = default)
    {
        // Request basics (docs phase 11 §2): EndAt is never accepted — it is derived
        // from the service duration inside the domain constructor.
        if (professionalId is null || professionalId == Guid.Empty)
            return Result<ReservationEntity>.Failure("ProfessionalId is required.");
        if (serviceId is null || serviceId == Guid.Empty)
            return Result<ReservationEntity>.Failure("ServiceId is required.");
        if (startAt is null || startAt.Value == default)
            return Result<ReservationEntity>.Failure("StartAt is required.");

        if (!IsAuthenticated())
            return Result<ReservationEntity>.Unauthorized("Authentication is required.");

        // Identity: Client → always their own id; Admin → may optionally specify a user.
        // A Client sending another user's id gets 403 instead of a silent overwrite.
        Guid targetUserId;
        if (requestedUserId is null || requestedUserId == Guid.Empty || requestedUserId == _currentUser.UserId)
            targetUserId = _currentUser.UserId;
        else if (_currentUser.IsAdmin)
            targetUserId = requestedUserId.Value;
        else
            return Result<ReservationEntity>.Forbidden("You cannot create a reservation for another user.");

        // Rule 1: user exists and is active.
        var user = await _users.GetByIdAsync(targetUserId, cancellationToken);
        if (user is null)
            return Result<ReservationEntity>.NotFound($"User '{targetUserId}' was not found.");
        if (!user.IsActive)
            return Result<ReservationEntity>.Failure("The user is not active.");

        // Rules 2 and 3: professional and service exist and are active.
        var professional = await _professionals.GetByIdAsync(professionalId.Value, cancellationToken);
        if (professional is null)
            return Result<ReservationEntity>.NotFound($"Professional '{professionalId}' was not found.");
        if (!professional.IsActive)
            return Result<ReservationEntity>.Failure("Professional is not active.");

        var service = await _services.GetByIdAsync(serviceId.Value, cancellationToken);
        if (service is null)
            return Result<ReservationEntity>.NotFound($"Service '{serviceId}' was not found.");
        if (!service.IsActive)
            return Result<ReservationEntity>.Failure("Service is not active.");

        // Rule 4: the professional offers the service (existing repository, no bypass).
        if (!await _professionalServices.ExistsAsync(professionalId.Value, serviceId.Value, cancellationToken))
            return Result<ReservationEntity>.Conflict("The professional does not offer the selected service.");

        // Rule 5 + convention: StartAt is UTC. Offsets are normalized once here and never
        // converted again (no time-zone infrastructure in this phase).
        var startAtUtc = startAt.Value.Kind switch
        {
            DateTimeKind.Utc => startAt.Value,
            DateTimeKind.Local => startAt.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(startAt.Value, DateTimeKind.Utc) // Unspecified → treated as UTC
        };
        if (startAtUtc < DateTime.UtcNow)
            return Result<ReservationEntity>.Failure("StartAt must be in the future.");

        // Rules 6, 9: EndAt comes from the service duration; status starts as Pending.
        var reservation = new ReservationEntity(targetUserId, professionalId.Value, service, startAtUtc, notes);

        // Rule 7: the reservation must fit inside one active availability period.
        // Convention: availability DayOfWeek/TimeOnly values are compared directly against
        // the UTC timestamps — a single implicit business timezone = UTC. A real
        // multi-timezone system would require an explicit professional/business timezone.
        var startTod = TimeOnly.FromDateTime(startAtUtc);
        var endTod = TimeOnly.FromDateTime(reservation.EndAt);
        var periods = await _availability.GetByProfessionalAsync(professionalId.Value, cancellationToken);
        var insideAvailability = periods.Any(p =>
            p.DayOfWeek == startAtUtc.DayOfWeek
            && p.StartTime <= startTod
            && startTod < endTod // also rejects a reservation crossing midnight
            && endTod <= p.EndTime);
        if (!insideAvailability)
            return Result<ReservationEntity>.Failure(
                "The reservation is outside the professional's availability.");

        // Rule 8 (+10): no overlapping non-cancelled reservation, queried in the database.
        var conflicts = await _reservations.GetConflictingAsync(
            professionalId.Value, startAtUtc, reservation.EndAt, cancellationToken);
        if (conflicts.Count > 0)
            return Result<ReservationEntity>.Conflict(
                "The professional already has a reservation overlapping the requested period.");

        await _reservations.AddAsync(reservation, cancellationToken);

        return Result<ReservationEntity>.Success(reservation);
    }

    /// <summary>Cancels by setting status Cancelled (soft; the record is kept). Admin → any, Client → own.</summary>
    public async Task<Result<Guid>> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated())
            return Result<Guid>.Unauthorized("Authentication is required.");

        var reservation = await _reservations.GetByIdAsync(id, cancellationToken);
        if (reservation is null)
            return Result<Guid>.NotFound($"Reservation '{id}' was not found.");

        if (!IsOwner(reservation))
            return Result<Guid>.Forbidden("You can only cancel your own reservations.");

        if (reservation.Status == ReservationStatus.Cancelled)
            return Result<Guid>.Conflict("The reservation is already cancelled.");

        reservation.Cancel(); // domain behavior: status → Cancelled (+ UpdatedAt)
        await _reservations.UpdateAsync(reservation, cancellationToken);

        return Result<Guid>.Success(id);
    }

    private bool IsAuthenticated() => _currentUser.IsAuthenticated && _currentUser.UserId != Guid.Empty;

    private bool IsOwner(ReservationEntity reservation)
        => _currentUser.IsAdmin || reservation.UserId == _currentUser.UserId;
}
