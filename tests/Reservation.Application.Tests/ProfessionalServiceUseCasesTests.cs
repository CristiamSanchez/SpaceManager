using Reservation.Application.Common;
using Reservation.Application.Features.Professionals;
using Reservation.Application.Tests.Fakes;
using Reservation.Domain.Entities;

namespace Reservation.Application.Tests;

public class ProfessionalServiceUseCasesTests
{
    private readonly InMemoryProfessionalRepository _professionals = new();
    private readonly InMemoryServiceRepository _services = new();
    private readonly InMemoryProfessionalServiceRepository _professionalServices = new();
    private readonly ProfessionalServiceUseCases _useCases;

    public ProfessionalServiceUseCasesTests()
    {
        _useCases = new ProfessionalServiceUseCases(_professionals, _services, _professionalServices);
    }

    private Professional SeedProfessional(bool active = true)
    {
        var professional = new Professional("Barbero Uno", null);
        if (!active)
            professional.Deactivate();
        _professionals.Items.Add(professional);
        return professional;
    }

    private Service SeedService(bool active = true)
    {
        var service = new Service("Corte", null, 30, 10m);
        if (!active)
            service.Deactivate();
        _services.Items.Add(service);
        return service;
    }

    [Fact]
    public async Task Assign_WithMissingProfessional_ReturnsNotFound()
    {
        var service = SeedService();

        var result = await _useCases.AssignAsync(Guid.NewGuid(), service.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Assign_WithMissingService_ReturnsNotFound()
    {
        var professional = SeedProfessional();

        var result = await _useCases.AssignAsync(professional.Id, Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Kind);
    }

    [Fact]
    public async Task Assign_WithInactiveProfessional_ReturnsValidation()
    {
        var professional = SeedProfessional(active: false);
        var service = SeedService();

        var result = await _useCases.AssignAsync(professional.Id, service.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Assign_WithInactiveService_ReturnsValidation()
    {
        var professional = SeedProfessional();
        var service = SeedService(active: false);

        var result = await _useCases.AssignAsync(professional.Id, service.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Validation, result.Kind);
    }

    [Fact]
    public async Task Assign_WhenRelationshipAlreadyExists_ReturnsConflict()
    {
        var professional = SeedProfessional();
        var service = SeedService();
        _professionalServices.Items.Add(new ProfessionalService(professional.Id, service.Id));

        var result = await _useCases.AssignAsync(professional.Id, service.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Conflict, result.Kind);
        Assert.Single(_professionalServices.Items);
    }

    [Fact]
    public async Task Assign_WithValidData_SucceedsAndPersists()
    {
        var professional = SeedProfessional();
        var service = SeedService();

        var result = await _useCases.AssignAsync(professional.Id, service.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(service.Id, result.Value!.Id);
        Assert.Contains(result.Value, _services.Items);
        Assert.Single(_professionalServices.Items);
    }

    [Fact]
    public async Task Remove_ExistingRelationship_Succeeds()
    {
        var professional = SeedProfessional();
        var service = SeedService();
        _professionalServices.Items.Add(new ProfessionalService(professional.Id, service.Id));

        var result = await _useCases.RemoveAsync(professional.Id, service.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(service.Id, result.Value);
        Assert.Empty(_professionalServices.Items);
    }
}
