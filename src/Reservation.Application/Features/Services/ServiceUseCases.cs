using Reservation.Application.Abstractions.Persistence;
using Reservation.Application.Common;
using Reservation.Domain.Entities;

namespace Reservation.Application.Features.Services;

/// <summary>Use cases for services (docs/domain-model.md §4).</summary>
public class ServiceUseCases
{
    private readonly IServiceRepository _services;

    public ServiceUseCases(IServiceRepository services)
    {
        _services = services;
    }

    public async Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _services.GetAllAsync(cancellationToken);

    public async Task<Result<Service>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await _services.GetByIdAsync(id, cancellationToken);
        return service is null
            ? Result<Service>.NotFound($"Service '{id}' was not found.")
            : Result<Service>.Success(service);
    }

    public async Task<Result<Service>> CreateAsync(
        string? name,
        string? description,
        int? durationInMinutes,
        decimal? price,
        CancellationToken cancellationToken = default)
    {
        // Application-level pre-validation so invalid input becomes an expected
        // failure instead of a domain exception. Mirrors the Service invariants.
        if (string.IsNullOrWhiteSpace(name))
            return Result<Service>.Failure("Name is required.");
        if (durationInMinutes is null or <= 0)
            return Result<Service>.Failure("DurationInMinutes must be greater than zero.");
        if (price is null or < 0)
            return Result<Service>.Failure("Price cannot be negative.");

        var service = new Service(name, description, durationInMinutes.Value, price.Value);
        await _services.AddAsync(service, cancellationToken);

        return Result<Service>.Success(service);
    }
}
