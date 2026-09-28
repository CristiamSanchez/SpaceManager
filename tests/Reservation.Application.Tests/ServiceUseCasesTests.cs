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
}
