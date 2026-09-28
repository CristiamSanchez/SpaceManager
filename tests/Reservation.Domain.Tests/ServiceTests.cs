using Reservation.Domain.Entities;

namespace Reservation.Domain.Tests;

public class ServiceTests
{
    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Service("   ", "description", 30, 10m));
    }

    [Fact]
    public void Constructor_WithZeroDuration_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Service("Corte", null, 0, 10m));
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Service("Corte", null, 30, -1m));
    }

    [Fact]
    public void Constructor_WithValidData_CreatesActiveService()
    {
        var service = new Service("Corte de cabello", "Descripción", 30, 15.50m);

        Assert.NotEqual(Guid.Empty, service.Id);
        Assert.Equal("Corte de cabello", service.Name);
        Assert.Equal(30, service.DurationInMinutes);
        Assert.Equal(15.50m, service.Price);
        Assert.True(service.IsActive);
    }
}
