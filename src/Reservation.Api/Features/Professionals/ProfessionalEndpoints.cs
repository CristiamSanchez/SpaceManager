using Reservation.Api.Common;
using Reservation.Application.Features.Professionals;
using Reservation.Domain.Entities;

namespace Reservation.Api.Features.Professionals;

public record ProfessionalResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive);

public record CreateProfessionalRequest(
    string? Name,
    string? Description);

public record UpdateProfessionalRequest(
    string? Name,
    string? Description,
    bool IsActive);

public static class ProfessionalEndpoints
{
    public static IEndpointRouteBuilder MapProfessionalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/professionals");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync).RequireAuthorization(Policies.AdminOnly);
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(Policies.AdminOnly);
        // Soft deactivation (IsActive = false), mirroring the availability endpoint.
        group.MapDelete("/{id:guid}", DeactivateAsync).RequireAuthorization(Policies.AdminOnly);

        return app;
    }

    private static async Task<IResult> GetAllAsync(
        ProfessionalUseCases useCases,
        CancellationToken cancellationToken)
    {
        var professionals = await useCases.GetAllAsync(cancellationToken);
        return Results.Ok(professionals.Select(p => Map(p)));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ProfessionalUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Results.Ok(Map(result.Value!)) : ResultTranslation.ToHttpError(result);
    }

    private static async Task<IResult> CreateAsync(
        CreateProfessionalRequest request,
        ProfessionalUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.CreateAsync(
            request.Name,
            request.Description,
            cancellationToken);

        if (!result.IsSuccess)
            return ResultTranslation.ToHttpError(result);

        var professional = result.Value!;
        return Results.Created($"/api/professionals/{professional.Id}", Map(professional));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateProfessionalRequest request,
        ProfessionalUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.UpdateAsync(
            id,
            request.Name,
            request.Description,
            request.IsActive,
            cancellationToken);

        return result.IsSuccess ? Results.Ok(Map(result.Value!)) : ResultTranslation.ToHttpError(result);
    }

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        ProfessionalUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.DeactivateAsync(id, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ResultTranslation.ToHttpError(result);
    }

    private static ProfessionalResponse Map(Professional professional) =>
        new(professional.Id, professional.Name, professional.Description, professional.IsActive);
}
