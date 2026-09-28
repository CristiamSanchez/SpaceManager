namespace Reservation.Domain.Enums;

/// <summary>Reservation states defined in docs/domain-model.md.</summary>
public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}
