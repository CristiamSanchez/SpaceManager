using Reservation.Application.Abstractions.Persistence;
using Reservation.Application.Common;
using Reservation.Domain.Entities;

namespace Reservation.Application.Features.Professionals;

/// <summary>
/// Use cases for the professional ↔ service relationship (docs/domain-model.md §6).
/// An Admin assigns/removes services offered by a professional.
/// </summary>
public class ProfessionalServiceUseCases
{
    private readonly IProfessionalRepository _professionals;
    private readonly IServiceRepository _services;
    private readonly IProfessionalServiceRepository _professionalServices;

    public ProfessionalServiceUseCases(
        IProfessionalRepository professionals,
        IServiceRepository services,
        IProfessionalServiceRepository professionalServices)
    {
        _professionals = professionals;
        _services = services;
        _professionalServices = professionalServices;
    }

    public async Task<Result<IReadOnlyList<Service>>> GetServicesAsync(
        Guid professionalId,
        CancellationToken cancellationToken = default)
    {
        if (professionalId == Guid.Empty)
            return Result<IReadOnlyList<Service>>.Failure("ProfessionalId must be a valid identifier.");

        var professional = await _professionals.GetByIdAsync(professionalId, cancellationToken);
        if (professional is null)
            return Result<IReadOnlyList<Service>>.NotFound($"Professional '{professionalId}' was not found.");

        var pairs = await _professionalServices.GetByProfessionalAsync(professionalId, cancellationToken);

        // Resolve each pair to its service using the existing service repository.
        var services = new List<Service>();
        foreach (var pair in pairs)
        {
            var service = await _services.GetByIdAsync(pair.ServiceId, cancellationToken);
            if (service is not null)
                services.Add(service);
        }

        return Result<IReadOnlyList<Service>>.Success(services);
    }

    public async Task<Result<Service>> AssignAsync(
        Guid professionalId,
        Guid? serviceId,
        CancellationToken cancellationToken = default)
    {
        if (professionalId == Guid.Empty)
            return Result<Service>.Failure("ProfessionalId must be a valid identifier.");
        if (serviceId is null || serviceId == Guid.Empty)
            return Result<Service>.Failure("ServiceId must be a valid identifier.");

        var professional = await _professionals.GetByIdAsync(professionalId, cancellationToken);
        if (professional is null)
            return Result<Service>.NotFound($"Professional '{professionalId}' was not found.");

        var service = await _services.GetByIdAsync(serviceId.Value, cancellationToken);
        if (service is null)
            return Result<Service>.NotFound($"Service '{serviceId}' was not found.");

        if (!professional.IsActive)
            return Result<Service>.Failure("Professional is not active.");
        if (!service.IsActive)
            return Result<Service>.Failure("Service is not active.");

        if (await _professionalServices.ExistsAsync(professionalId, serviceId.Value, cancellationToken))
            return Result<Service>.Conflict("The professional already offers this service.");

        await _professionalServices.AddAsync(
            new ProfessionalService(professionalId, serviceId.Value),
            cancellationToken);

        return Result<Service>.Success(service);
    }

    public async Task<Result<Guid>> RemoveAsync(
        Guid professionalId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        if (professionalId == Guid.Empty)
            return Result<Guid>.Failure("ProfessionalId must be a valid identifier.");
        if (serviceId == Guid.Empty)
            return Result<Guid>.Failure("ServiceId must be a valid identifier.");

        var professional = await _professionals.GetByIdAsync(professionalId, cancellationToken);
        if (professional is null)
            return Result<Guid>.NotFound($"Professional '{professionalId}' was not found.");

        if (!await _professionalServices.ExistsAsync(professionalId, serviceId, cancellationToken))
            return Result<Guid>.NotFound($"Professional '{professionalId}' does not offer service '{serviceId}'.");

        await _professionalServices.DeleteAsync(professionalId, serviceId, cancellationToken);

        return Result<Guid>.Success(serviceId);
    }
}
