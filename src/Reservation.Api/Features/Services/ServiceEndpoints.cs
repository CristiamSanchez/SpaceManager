using Reservation.Api.Common;
using Reservation.Application.Features.Services;
using Reservation.Domain.Entities;

namespace Reservation.Api.Features.Services;

public record ServiceResponse(
    Guid Id,
    string Name,
    string? Description,
    int DurationInMinutes,
    decimal Price,
    bool IsActive);

public record CreateServiceRequest(
    string? Name,
    string? Description,
    int? DurationInMinutes,
    decimal? Price);

public record UpdateServiceRequest(
    string? Name,
    string? Description,
    int? DurationInMinutes,
    decimal? Price,
    bool IsActive);

public static class ServiceEndpoints
{
    public static IEndpointRouteBuilder MapServiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/services");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync).RequireAuthorization(Policies.AdminOnly);
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(Policies.AdminOnly);
        // Soft deactivation (IsActive = false), mirroring the availability endpoint.
        group.MapDelete("/{id:guid}", DeactivateAsync).RequireAuthorization(Policies.AdminOnly);

        return app;
    }

    private static async Task<IResult> GetAllAsync(
        ServiceUseCases useCases,
        CancellationToken cancellationToken)
    {
        var services = await useCases.GetAllAsync(cancellationToken);
        return Results.Ok(services.Select(s => Map(s)));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ServiceUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Results.Ok(Map(result.Value!)) : ResultTranslation.ToHttpError(result);
    }

    private static async Task<IResult> CreateAsync(
        CreateServiceRequest request,
        ServiceUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.CreateAsync(
            request.Name,
            request.Description,
            request.DurationInMinutes,
            request.Price,
            cancellationToken);

        if (!result.IsSuccess)
            return ResultTranslation.ToHttpError(result);

        var service = result.Value!;
        return Results.Created($"/api/services/{service.Id}", Map(service));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateServiceRequest request,
        ServiceUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.UpdateAsync(
            id,
            request.Name,
            request.Description,
            request.DurationInMinutes,
            request.Price,
            request.IsActive,
            cancellationToken);

        return result.IsSuccess ? Results.Ok(Map(result.Value!)) : ResultTranslation.ToHttpError(result);
    }

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        ServiceUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.DeactivateAsync(id, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ResultTranslation.ToHttpError(result);
    }

    private static ServiceResponse Map(Service service) =>
        new(service.Id, service.Name, service.Description, service.DurationInMinutes, service.Price, service.IsActive);
}
