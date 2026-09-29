using Reservation.Application.Common;
using Reservation.Application.Features.Services;
using Reservation.Application.Tests.Fakes;
using Reservation.Domain.Entities;

namespace Reservation.Application.Tests;

public class ServiceUseCasesTests
{
    private readonly InMemoryServiceRepository _services = new();
    private readonly ServiceUseCases _useCases;

    public ServiceUseCasesTests()
    {
        _useCases = new ServiceUseCases(_services);
    }

    [Fact]
    public async Task Create_WithInvalidName_ReturnsValidation()
    {
        var result = await _useCases.CreateAsync("   ", null, 30, 10m);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Create_WithInvalidDuration_ReturnsValidation()
    {
        var result = await _useCases.CreateAsync("Corte", null, 0, 10m);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Create_WithNegativePrice_ReturnsValidation()
    {
        var result = await _useCases.CreateAsync("Corte", null, 30, -1m);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Create_WithValidData_SucceedsAndPersists()
    {
        var result = await _useCases.CreateAsync("Corte", "Descripción", 30, 15.50m);

        Assert.True(result.IsSuccess);
        Assert.Equal("Corte", result.Value!.Name);
        Assert.Equal(30, result.Value.DurationInMinutes);
        Assert.Equal(15.50m, result.Value.Price);
        Assert.Contains(result.Value, _services.Items);
    }

    [Fact]
    public async Task Update_WithUnknownId_ReturnsNotFound()
    {
        var result = await _useCases.UpdateAsync(Guid.NewGuid(), "Corte", null, 30, 10m, true);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Update_WithInvalidName_ReturnsValidation()
    {
        var created = (await _useCases.CreateAsync("Corte", null, 30, 10m)).Value!;

        var result = await _useCases.UpdateAsync(created.Id, "   ", null, 45, 20m, true);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Update_WithNegativePrice_ReturnsValidation()
    {
        var created = (await _useCases.CreateAsync("Corte", null, 30, 10m)).Value!;

        var result = await _useCases.UpdateAsync(created.Id, "Corte", null, 45, -1m, true);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Update_WithValidData_SucceedsAndPersists()
    {
        var created = (await _useCases.CreateAsync("Corte", null, 30, 10m)).Value!;

        var result = await _useCases.UpdateAsync(created.Id, "Corte premium", "Incluye lavado", 45, 25m, true);

        Assert.True(result.IsSuccess);
        Assert.Equal("Corte premium", result.Value!.Name);
        Assert.Equal("Incluye lavado", result.Value.Description);
        Assert.Equal(45, result.Value.DurationInMinutes);
        Assert.Equal(25m, result.Value.Price);
        Assert.True(result.Value.IsActive);
        Assert.Equal("Corte premium", _services.Items.Single().Name);
    }

    [Fact]
    public async Task Update_WithInactiveFlag_Deactivates()
    {
        var created = (await _useCases.CreateAsync("Corte", null, 30, 10m)).Value!;

        var result = await _useCases.UpdateAsync(created.Id, "Corte", null, 30, 10m, false);

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
        var created = (await _useCases.CreateAsync("Corte", null, 30, 10m)).Value!;

        var result = await _useCases.DeactivateAsync(created.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(created.Id, result.Value);
        Assert.False(_services.Items.Single().IsActive);
    }
}
