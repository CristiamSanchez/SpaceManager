using Reservation.Application.Common;
using Reservation.Application.Features.Availability;
using Reservation.Application.Tests.Fakes;
using Reservation.Domain.Entities;
// "Availability" alone resolves to this namespace inside Reservation.Application.*, so alias the entity.
using AvailabilityEntity = Reservation.Domain.Entities.Availability;

namespace Reservation.Application.Tests;

public class AvailabilityUseCasesTests
{
    private readonly InMemoryProfessionalRepository _professionals = new();
    private readonly InMemoryAvailabilityRepository _availability = new();
    private readonly AvailabilityUseCases _useCases;

    public AvailabilityUseCasesTests()
    {
        _useCases = new AvailabilityUseCases(_availability, _professionals);
    }

    private Professional SeedProfessional(bool active = true)
    {
        var professional = new Professional("Barbero Uno", null);
        if (!active)
            professional.Deactivate();
        _professionals.Items.Add(professional);
        return professional;
    }

    [Fact]
    public async Task Create_WithMissingProfessional_ReturnsNotFound()
    {
        var result = await _useCases.CreateAsync(
            Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Create_WithInvalidTimeRange_ReturnsValidation()
    {
        var professional = SeedProfessional();

        var result = await _useCases.CreateAsync(
            professional.Id, DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(9, 0));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Create_WhenOverlappingActivePeriodExists_ReturnsConflict()
    {
        var professional = SeedProfessional();
        _availability.Items.Add(
            new AvailabilityEntity(professional.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0)));

        var result = await _useCases.CreateAsync(
            professional.Id, DayOfWeek.Monday, new TimeOnly(10, 0), new TimeOnly(11, 0));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Conflict, result.Kind);
    }

    [Fact]
    public async Task Create_WithValidData_SucceedsAndPersists()
    {
        var professional = SeedProfessional();

        var result = await _useCases.CreateAsync(
            professional.Id, DayOfWeek.Monday, new TimeOnly(14, 0), new TimeOnly(16, 0));

        Assert.True(result.IsSuccess);
        Assert.Equal(professional.Id, result.Value!.ProfessionalId);
        Assert.Contains(result.Value, _availability.Items);
    }

    [Fact]
    public async Task Update_OnPeriodBelongingToAnotherProfessional_ReturnsNotFound()
    {
        // Phase 10 convention: a mismatched ownership returns NotFound instead of
        // Forbidden, so the response does not reveal that the record exists.
        var routeProfessional = SeedProfessional();
        var otherProfessional = SeedProfessional();
        var otherPeriod = new AvailabilityEntity(
            otherProfessional.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        _availability.Items.Add(otherPeriod);

        var result = await _useCases.UpdateAsync(
            routeProfessional.Id, otherPeriod.Id, DayOfWeek.Tuesday, new TimeOnly(9, 0), new TimeOnly(12, 0), isActive: true);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Deactivate_SetsPeriodToInactive()
    {
        var professional = SeedProfessional();
        var period = new AvailabilityEntity(
            professional.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        _availability.Items.Add(period);

        var result = await _useCases.DeactivateAsync(professional.Id, period.Id);

        Assert.True(result.IsSuccess);
        Assert.False(period.IsActive);
    }

    [Fact]
    public async Task GetByProfessional_ExcludesInactivePeriods()
    {
        var professional = SeedProfessional();
        var active = new AvailabilityEntity(
            professional.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        var inactive = new AvailabilityEntity(
            professional.Id, DayOfWeek.Tuesday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        inactive.Deactivate();
        _availability.Items.Add(active);
        _availability.Items.Add(inactive);

        var periods = await _useCases.GetByProfessionalAsync(professional.Id);

        Assert.Single(periods);
        Assert.Equal(active.Id, periods[0].Id);
    }
}
