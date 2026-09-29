using Reservation.Application.Common;
using Reservation.Application.Features.Professionals;
using Reservation.Application.Tests.Fakes;

namespace Reservation.Application.Tests;

public class ProfessionalUseCasesTests
{
    private readonly InMemoryProfessionalRepository _professionals = new();
    private readonly ProfessionalUseCases _useCases;

    public ProfessionalUseCasesTests()
    {
        _useCases = new ProfessionalUseCases(_professionals);
    }

    [Fact]
    public async Task Update_WithUnknownId_ReturnsNotFound()
    {
        var result = await _useCases.UpdateAsync(Guid.NewGuid(), "Barbero", null, true);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Update_WithInvalidName_ReturnsValidation()
    {
        var created = (await _useCases.CreateAsync("Barbero Uno", null)).Value!;

        var result = await _useCases.UpdateAsync(created.Id, "   ", null, true);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Update_WithValidData_SucceedsAndPersists()
    {
        var created = (await _useCases.CreateAsync("Barbero Uno", null)).Value!;

        var result = await _useCases.UpdateAsync(created.Id, "Barbero Dos", "Colorista", true);

        Assert.True(result.IsSuccess);
        Assert.Equal("Barbero Dos", result.Value!.Name);
        Assert.Equal("Colorista", result.Value.Description);
        Assert.Equal("Barbero Dos", _professionals.Items.Single().Name);
    }

    [Fact]
    public async Task Update_WithInactiveFlag_Deactivates()
    {
        var created = (await _useCases.CreateAsync("Barbero Uno", null)).Value!;

        var result = await _useCases.UpdateAsync(created.Id, "Barbero Uno", null, false);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.IsActive);
    }

    [Fact]
    public async Task Deactivate_WithUnknownId_ReturnsNotFound()
    {
        var result = await _useCases.DeactivateAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Deactivate_SoftDeactivatesAndPersists()
    {
        var created = (await _useCases.CreateAsync("Barbero Uno", null)).Value!;

        var result = await _useCases.DeactivateAsync(created.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(created.Id, result.Value);
        Assert.False(_professionals.Items.Single().IsActive);
    }
}
