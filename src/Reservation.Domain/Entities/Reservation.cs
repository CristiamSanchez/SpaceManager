using Reservation.Domain.Enums;

namespace Reservation.Domain.Entities;

/// <summary>
/// Reservation made by a user for a professional and a service. See docs/domain-model.md §8.
/// Conflict, availability and ownership checks are coordinated by Application.
/// </summary>
public class Reservation
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public ReservationStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a reservation with status Pending. EndAt is calculated from the
    /// service duration (docs/domain-model.md, Regla 5).
    /// </summary>
    public Reservation(Guid userId, Guid professionalId, Service service, DateTime startAt, string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId must be a valid identifier.", nameof(userId));
        if (professionalId == Guid.Empty)
            throw new ArgumentException("ProfessionalId must be a valid identifier.", nameof(professionalId));

        Id = Guid.NewGuid();
        UserId = userId;
        ProfessionalId = professionalId;
        ServiceId = service.Id;
        StartAt = startAt;
        // Service.DurationInMinutes > 0 is guaranteed by the Service invariant,
        // so StartAt is always earlier than EndAt.
        EndAt = startAt.AddMinutes(service.DurationInMinutes);
        Status = ReservationStatus.Pending;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Private parameterless constructor for entity reconstitution.
    private Reservation() { }

    /// <summary>
    /// Cancels the reservation by changing its status; the record is kept for history
    /// (docs/domain-model.md §8, Regla 8 — no physical deletion).
    /// </summary>
    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("The reservation is already cancelled.");

        Status = ReservationStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
