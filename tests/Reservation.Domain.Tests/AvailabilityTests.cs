using Reservation.Domain.Entities;

namespace Reservation.Domain.Tests;

public class AvailabilityTests
{
    private static readonly Guid ProfessionalId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithStartNotBeforeEnd_Throws()
    {
        // Equal times and reversed times are both rejected: StartTime must be earlier.
        Assert.Throws<ArgumentException>(() =>
            new Availability(ProfessionalId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(9, 0)));
        Assert.Throws<ArgumentException>(() =>
            new Availability(ProfessionalId, DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(9, 0)));
    }

    [Fact]
    public void Constructor_WithValidRange_CreatesActiveAvailability()
    {
        var availability = new Availability(ProfessionalId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));

        Assert.NotEqual(Guid.Empty, availability.Id);
        Assert.Equal(ProfessionalId, availability.ProfessionalId);
        Assert.Equal(DayOfWeek.Monday, availability.DayOfWeek);
        Assert.Equal(new TimeOnly(9, 0), availability.StartTime);
        Assert.Equal(new TimeOnly(12, 0), availability.EndTime);
        Assert.True(availability.IsActive);
    }

    [Fact]
    public void Update_ChangesScheduleButPreservesProfessional()
    {
        var availability = new Availability(ProfessionalId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));

        availability.Update(DayOfWeek.Tuesday, new TimeOnly(14, 0), new TimeOnly(16, 0));

        Assert.Equal(ProfessionalId, availability.ProfessionalId);
        Assert.Equal(DayOfWeek.Tuesday, availability.DayOfWeek);
        Assert.Equal(new TimeOnly(14, 0), availability.StartTime);
        Assert.Equal(new TimeOnly(16, 0), availability.EndTime);
    }

    [Fact]
    public void Update_WithInvalidTimeRange_ThrowsArgumentException()
    {
        var availability = new Availability(ProfessionalId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));

        Assert.Throws<ArgumentException>(() =>
            availability.Update(DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(9, 0)));
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var availability = new Availability(ProfessionalId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));

        availability.Deactivate();

        Assert.False(availability.IsActive);
    }
}
