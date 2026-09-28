using Reservation.Domain.Entities;
using Reservation.Domain.Enums;
// "Reservation" alone resolves to the root namespace inside Reservation.*, so alias the entity.
using ReservationEntity = Reservation.Domain.Entities.Reservation;

namespace Reservation.Domain.Tests;

public class ReservationTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProfessionalId = Guid.NewGuid();

    private static Service CreateService(int durationInMinutes = 30)
        => new Service("Corte", null, durationInMinutes, 10m);

    [Fact]
    public void Constructor_CreatesReservationWithPendingStatus()
    {
        var reservation = new ReservationEntity(UserId, ProfessionalId, CreateService(), DateTime.UtcNow.AddDays(1));

        Assert.NotEqual(Guid.Empty, reservation.Id);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
    }

    [Fact]
    public void Constructor_DerivesEndAtFromServiceDuration()
    {
        var startAt = new DateTime(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc);

        var reservation = new ReservationEntity(UserId, ProfessionalId, CreateService(45), startAt);

        Assert.Equal(startAt.AddMinutes(45), reservation.EndAt);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new ReservationEntity(Guid.Empty, ProfessionalId, CreateService(), DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Constructor_WithEmptyProfessionalId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new ReservationEntity(UserId, Guid.Empty, CreateService(), DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Constructor_EndAtIsAlwaysAfterStartAt()
    {
        // Service.DurationInMinutes > 0 is a Service invariant, so the derived
        // EndAt can never be before or equal to StartAt.
        var startAt = new DateTime(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc);

        var reservation = new ReservationEntity(UserId, ProfessionalId, CreateService(1), startAt);

        Assert.True(reservation.EndAt > reservation.StartAt);
    }

    [Fact]
    public void Cancel_SetsStatusToCancelled()
    {
        var reservation = new ReservationEntity(UserId, ProfessionalId, CreateService(), DateTime.UtcNow.AddDays(1));

        reservation.Cancel();

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsInvalidOperationException()
    {
        var reservation = new ReservationEntity(UserId, ProfessionalId, CreateService(), DateTime.UtcNow.AddDays(1));
        reservation.Cancel();

        Assert.Throws<InvalidOperationException>(() => reservation.Cancel());
    }
}
