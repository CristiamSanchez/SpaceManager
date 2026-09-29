using Reservation.Application.Abstractions.Persistence;
using Reservation.Application.Common;
using Reservation.Domain.Entities;

namespace Reservation.Application.Features.Professionals;

/// <summary>Use cases for professionals (docs/domain-model.md §5).</summary>
public class ProfessionalUseCases
{
    private readonly IProfessionalRepository _professionals;

    public ProfessionalUseCases(IProfessionalRepository professionals)
    {
        _professionals = professionals;
    }

    public async Task<IReadOnlyList<Professional>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _professionals.GetAllAsync(cancellationToken);

    public async Task<Result<Professional>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var professional = await _professionals.GetByIdAsync(id, cancellationToken);
        return professional is null
            ? Result<Professional>.NotFound($"Professional '{id}' was not found.")
            : Result<Professional>.Success(professional);
    }

    public async Task<Result<Professional>> CreateAsync(
        string? name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Professional>.Failure("Name is required.");

        var professional = new Professional(name, description);
        await _professionals.AddAsync(professional, cancellationToken);

        return Result<Professional>.Success(professional);
    }

    public async Task<Result<Professional>> UpdateAsync(
        Guid id,
        string? name,
        string? description,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var professional = await _professionals.GetByIdAsync(id, cancellationToken);
        if (professional is null)
            return Result<Professional>.NotFound($"Professional '{id}' was not found.");

        if (string.IsNullOrWhiteSpace(name))
            return Result<Professional>.Failure("Name is required.");

        professional.Update(name, description);
        if (isActive)
            professional.Activate();
        else
            professional.Deactivate();

        await _professionals.UpdateAsync(professional, cancellationToken);

        return Result<Professional>.Success(professional);
    }

    /// <summary>Soft deactivation (IsActive = false); reservations keep their history.</summary>
    public async Task<Result<Guid>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var professional = await _professionals.GetByIdAsync(id, cancellationToken);
        if (professional is null)
            return Result<Guid>.NotFound($"Professional '{id}' was not found.");

        professional.Deactivate();
        await _professionals.UpdateAsync(professional, cancellationToken);

        return Result<Guid>.Success(id);
    }
}
