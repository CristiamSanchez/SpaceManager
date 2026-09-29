using Reservation.Domain.Entities;

namespace Reservation.Domain.Tests;

public class ProfessionalTests
{
    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Professional("", null));
    }

    [Fact]
    public void Constructor_WithValidName_CreatesActiveProfessional()
    {
        var professional = new Professional("Barbero Uno", "Especialista en cortes");

        Assert.NotEqual(Guid.Empty, professional.Id);
        Assert.Equal("Barbero Uno", professional.Name);
        Assert.Equal("Especialista en cortes", professional.Description);
        Assert.True(professional.IsActive);
    }

    [Fact]
    public void Update_WithEmptyName_ThrowsArgumentException()
    {
        var professional = new Professional("Barbero Uno", null);

        Assert.Throws<ArgumentException>(() => professional.Update("  ", "desc"));
    }

    [Fact]
    public void Update_WithValidData_UpdatesFields()
    {
        var professional = new Professional("Barbero Uno", null);

        professional.Update("Barbero Dos", "Colorista");

        Assert.Equal("Barbero Dos", professional.Name);
        Assert.Equal("Colorista", professional.Description);
    }
}
