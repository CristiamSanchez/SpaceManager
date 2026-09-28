using Reservation.Application.Common;
using Reservation.Application.Features.Reservations;
using Reservation.Application.Tests.Fakes;
using Reservation.Domain.Entities;
using Reservation.Domain.Enums;
// "Availability" alone resolves to this namespace inside Reservation.Application.*, so alias the entity.
using AvailabilityEntity = Reservation.Domain.Entities.Availability;
// "Reservation" alone resolves to the root namespace inside Reservation.*, so alias the entity.
using ReservationEntity = Reservation.Domain.Entities.Reservation;

namespace Reservation.Application.Tests;

public class ReservationUseCasesTests
{
    // Fixed future reference point: tomorrow at 12:00 UTC. Availability periods
    // are seeded around it, so tests behave the same regardless of run time.
    private static readonly DateTime StartTomorrowNoon = DateTime.UtcNow.Date.AddDays(1).AddHours(12);

    private readonly InMemoryReservationRepository _reservations = new();
    private readonly InMemoryUserRepository _users = new();
    private readonly InMemoryProfessionalRepository _professionals = new();
    private readonly InMemoryServiceRepository _services = new();
    private readonly InMemoryProfessionalServiceRepository _professionalServices = new();
    private readonly InMemoryAvailabilityRepository _availability = new();
    private readonly FakeCurrentUser _currentUser = new();
    private readonly ReservationUseCases _useCases;

    public ReservationUseCasesTests()
    {
        _useCases = new ReservationUseCases(
            _reservations, _users, _professionals, _services, _professionalServices, _availability, _currentUser);
    }

    // ---- seeding helpers -------------------------------------------------

    private User SeedUser(string email, bool active = true)
    {
        var user = new User("User", email, "hash", UserRole.Client);
        if (!active)
            user.Deactivate();
        _users.Items.Add(user);
        return user;
    }

    private Professional SeedProfessional(bool active = true)
    {
        var professional = new Professional("Barbero Uno", null);
        if (!active)
            professional.Deactivate();
        _professionals.Items.Add(professional);
        return professional;
    }

    private Service SeedService(int durationInMinutes = 30, bool active = true)
    {
        var service = new Service("Corte", null, durationInMinutes, 10m);
        if (!active)
            service.Deactivate();
        _services.Items.Add(service);
        return service;
    }

    private void SeedOffer(Professional professional, Service service) =>
        _professionalServices.Items.Add(new ProfessionalService(professional.Id, service.Id));

    /// <summary>Availability 09:00–17:00 on the day of StartTomorrowNoon (covers noon).</summary>
    private void SeedAvailability(Professional professional) =>
        _availability.Items.Add(new AvailabilityEntity(
            professional.Id, StartTomorrowNoon.DayOfWeek, new TimeOnly(9, 0), new TimeOnly(17, 0)));

    private ReservationEntity SeedReservation(Guid userId, Guid professionalId, DateTime startAt, Service? service = null)
    {
        var reservation = new ReservationEntity(userId, professionalId, service ?? SeedService(), startAt);
        _reservations.Items.Add(reservation);
        return reservation;
    }

    /// <summary>Active user + professional + service + offer; the current user owns the user.</summary>
    private (User User, Professional Professional, Service Service) SeedBookingSetup()
    {
        var user = SeedUser("client@test.com");
        var professional = SeedProfessional();
        var service = SeedService();
        SeedOffer(professional, service);
        _currentUser.IsAuthenticated = true;
        _currentUser.IsAdmin = false;
        _currentUser.UserId = user.Id;
        return (user, professional, service);
    }

    // ---- Create ----------------------------------------------------------

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsUnauthorized()
    {
        _currentUser.IsAuthenticated = false;

        var result = await _useCases.CreateAsync(
            Guid.NewGuid(), Guid.NewGuid(), StartTomorrowNoon, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Unauthorized, result.Kind);
    }

    [Fact]
    public async Task Create_ClientSendingAnotherUsersId_ReturnsForbidden()
    {
        _currentUser.IsAuthenticated = true;
        _currentUser.IsAdmin = false;
        _currentUser.UserId = Guid.NewGuid();

        var result = await _useCases.CreateAsync(
            Guid.NewGuid(), Guid.NewGuid(), StartTomorrowNoon, null, Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Forbidden, result.Kind);
    }

    [Fact]
    public async Task Create_WithMissingUser_ReturnsNotFound()
    {
        // The current user id points to no record.
        _currentUser.UserId = Guid.NewGuid();

        var result = await _useCases.CreateAsync(
            Guid.NewGuid(), Guid.NewGuid(), StartTomorrowNoon, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Create_WithInactiveUser_ReturnsValidation()
    {
        var user = SeedUser("client@test.com", active: false);
        _currentUser.UserId = user.Id;

        var result = await _useCases.CreateAsync(
            Guid.NewGuid(), Guid.NewGuid(), StartTomorrowNoon, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Create_WithMissingProfessional_ReturnsNotFound()
    {
        var user = SeedUser("client@test.com");
        _currentUser.UserId = user.Id;

        var result = await _useCases.CreateAsync(
            Guid.NewGuid(), Guid.NewGuid(), StartTomorrowNoon, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Create_WithInactiveProfessional_ReturnsValidation()
    {
        var user = SeedUser("client@test.com");
        var professional = SeedProfessional(active: false);
        _currentUser.UserId = user.Id;

        var result = await _useCases.CreateAsync(
            professional.Id, Guid.NewGuid(), StartTomorrowNoon, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Create_WithMissingService_ReturnsNotFound()
    {
        var user = SeedUser("client@test.com");
        var professional = SeedProfessional();
        _currentUser.UserId = user.Id;

        var result = await _useCases.CreateAsync(
            professional.Id, Guid.NewGuid(), StartTomorrowNoon, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Create_WithInactiveService_ReturnsValidation()
    {
        var user = SeedUser("client@test.com");
        var professional = SeedProfessional();
        var service = SeedService(active: false);
        _currentUser.UserId = user.Id;

        var result = await _useCases.CreateAsync(
            professional.Id, service.Id, StartTomorrowNoon, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Create_WhenProfessionalDoesNotOfferService_ReturnsConflict()
    {
        var user = SeedUser("client@test.com");
        var professional = SeedProfessional();
        var service = SeedService();
        _currentUser.UserId = user.Id;
        // No SeedOffer: the pair is not registered.

        var result = await _useCases.CreateAsync(
            professional.Id, service.Id, StartTomorrowNoon, null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Conflict, result.Kind);
    }

    [Fact]
    public async Task Create_WithPastStartTime_ReturnsValidation()
    {
        var (_, professional, service) = SeedBookingSetup();

        var result = await _useCases.CreateAsync(
            professional.Id, service.Id, DateTime.UtcNow.AddHours(-1), null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Create_OutsideAvailability_ReturnsValidation()
    {
        var (_, professional, service) = SeedBookingSetup();
        SeedAvailability(professional);

        // 20:00 UTC: inside the seeded day but outside the 09:00–17:00 period.
        var result = await _useCases.CreateAsync(
            professional.Id, service.Id, StartTomorrowNoon.AddHours(8), null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Create_WhenOverlappingReservationExists_ReturnsConflict()
    {
        var (_, professional, service) = SeedBookingSetup();
        SeedAvailability(professional);
        SeedReservation(_currentUser.UserId, professional.Id, StartTomorrowNoon, service);

        // Starts 15 minutes after the existing reservation began.
        var result = await _useCases.CreateAsync(
            professional.Id, service.Id, StartTomorrowNoon.AddMinutes(15), null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Conflict, result.Kind);
    }

    [Fact]
    public async Task Create_WhenOverlappingReservationIsCancelled_DoesNotBlock()
    {
        var (_, professional, service) = SeedBookingSetup();
        SeedAvailability(professional);
        var existing = SeedReservation(_currentUser.UserId, professional.Id, StartTomorrowNoon, service);
        existing.Cancel();

        var result = await _useCases.CreateAsync(
            professional.Id, service.Id, StartTomorrowNoon.AddMinutes(15), null, null);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Pending, result.Value!.Status);
    }

    [Fact]
    public async Task Create_WithValidData_SucceedsWithPendingReservation()
    {
        var (user, professional, service) = SeedBookingSetup();
        SeedAvailability(professional);

        var result = await _useCases.CreateAsync(
            professional.Id, service.Id, StartTomorrowNoon, "Primera vez", null);

        Assert.True(result.IsSuccess);
        var reservation = result.Value!;
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Equal(user.Id, reservation.UserId);
        Assert.Equal(professional.Id, reservation.ProfessionalId);
        Assert.Equal(service.Id, reservation.ServiceId);
        Assert.Equal(StartTomorrowNoon.AddMinutes(service.DurationInMinutes), reservation.EndAt);
        Assert.Contains(reservation, _reservations.Items); // persisted
    }

    // ---- List ------------------------------------------------------------

    [Fact]
    public async Task List_Client_ReceivesOnlyOwnReservations()
    {
        var mine1 = SeedUser("mine1@test.com");
        var other = SeedUser("other@test.com");
        _currentUser.UserId = mine1.Id;
        _currentUser.IsAdmin = false;
        SeedReservation(mine1.Id, Guid.NewGuid(), StartTomorrowNoon);
        SeedReservation(mine1.Id, Guid.NewGuid(), StartTomorrowNoon.AddHours(2));
        SeedReservation(other.Id, Guid.NewGuid(), StartTomorrowNoon);

        var result = await _useCases.ListAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.All(result.Value, r => Assert.Equal(mine1.Id, r.UserId));
        Assert.DoesNotContain(result.Value, r => r.UserId == other.Id);
    }

    [Fact]
    public async Task List_Admin_ReceivesAllReservations()
    {
        var userA = SeedUser("a@test.com");
        var userB = SeedUser("b@test.com");
        _currentUser.IsAdmin = true;
        SeedReservation(userA.Id, Guid.NewGuid(), StartTomorrowNoon);
        SeedReservation(userB.Id, Guid.NewGuid(), StartTomorrowNoon);

        var result = await _useCases.ListAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    // ---- Get by id -------------------------------------------------------

    [Fact]
    public async Task GetById_Client_GetsOwnReservation()
    {
        var owner = SeedUser("owner@test.com");
        _currentUser.IsAdmin = false;
        _currentUser.UserId = owner.Id;
        var reservation = SeedReservation(owner.Id, Guid.NewGuid(), StartTomorrowNoon);

        var result = await _useCases.GetByIdAsync(reservation.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(reservation.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetById_Client_WhenReservationBelongsToAnotherUser_ReturnsForbidden()
    {
        var other = SeedUser("other@test.com");
        _currentUser.IsAdmin = false;
        _currentUser.UserId = Guid.NewGuid();
        var reservation = SeedReservation(other.Id, Guid.NewGuid(), StartTomorrowNoon);

        var result = await _useCases.GetByIdAsync(reservation.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Forbidden, result.Kind);
    }

    [Fact]
    public async Task GetById_Admin_GetsAnotherUsersReservation()
    {
        var other = SeedUser("other@test.com");
        _currentUser.IsAdmin = true;
        _currentUser.UserId = Guid.NewGuid();
        var reservation = SeedReservation(other.Id, Guid.NewGuid(), StartTomorrowNoon);

        var result = await _useCases.GetByIdAsync(reservation.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(reservation.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetById_WhenReservationIsMissing_ReturnsNotFound()
    {
        _currentUser.IsAdmin = true;

        var result = await _useCases.GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    // ---- Cancel ----------------------------------------------------------

    [Fact]
    public async Task Cancel_Client_CancelsOwnReservation()
    {
        var owner = SeedUser("owner@test.com");
        _currentUser.IsAdmin = false;
        _currentUser.UserId = owner.Id;
        var reservation = SeedReservation(owner.Id, Guid.NewGuid(), StartTomorrowNoon);

        var result = await _useCases.CancelAsync(reservation.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(reservation.Id, result.Value);
    }

    [Fact]
    public async Task Cancel_Client_WhenBelongsToAnotherUser_ReturnsForbidden()
    {
        var other = SeedUser("other@test.com");
        _currentUser.IsAdmin = false;
        _currentUser.UserId = Guid.NewGuid();
        var reservation = SeedReservation(other.Id, Guid.NewGuid(), StartTomorrowNoon);

        var result = await _useCases.CancelAsync(reservation.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Forbidden, result.Kind);
        Assert.Equal(ReservationStatus.Pending, reservation.Status); // untouched
    }

    [Fact]
    public async Task Cancel_Admin_CancelsAnyReservation()
    {
        var other = SeedUser("other@test.com");
        _currentUser.IsAdmin = true;
        _currentUser.UserId = Guid.NewGuid();
        var reservation = SeedReservation(other.Id, Guid.NewGuid(), StartTomorrowNoon);

        var result = await _useCases.CancelAsync(reservation.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public async Task Cancel_WhenAlreadyCancelled_ReturnsConflict()
    {
        var owner = SeedUser("owner@test.com");
        _currentUser.IsAdmin = false;
        _currentUser.UserId = owner.Id;
        var reservation = SeedReservation(owner.Id, Guid.NewGuid(), StartTomorrowNoon);
        reservation.Cancel();

        var result = await _useCases.CancelAsync(reservation.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Conflict, result.Kind);
    }

    [Fact]
    public async Task Cancel_PersistsCancelledStatusWithoutDeletingRecord()
    {
        var owner = SeedUser("owner@test.com");
        _currentUser.IsAdmin = false;
        _currentUser.UserId = owner.Id;
        var reservation = SeedReservation(owner.Id, Guid.NewGuid(), StartTomorrowNoon);

        await _useCases.CancelAsync(reservation.Id);

        var stored = Assert.Single(_reservations.Items); // still there: soft cancel only
        Assert.Equal(ReservationStatus.Cancelled, stored.Status);
    }
}
