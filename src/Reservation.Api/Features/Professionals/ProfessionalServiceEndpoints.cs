using Reservation.Api.Common;
using Reservation.Api.Features.Services;
using Reservation.Application.Features.Professionals;
using Reservation.Domain.Entities;

namespace Reservation.Api.Features.Professionals;

public record AssignServiceRequest(Guid? ServiceId);

public static class ProfessionalServiceEndpoints
{
    public static IEndpointRouteBuilder MapProfessionalServiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/professionals/{professionalId:guid}/services");

        // All three operations are Admin-managed (docs phase 9 §1).
        group.MapGet("/", GetServicesAsync).RequireAuthorization(Policies.AdminOnly);
        group.MapPost("/", AssignAsync).RequireAuthorization(Policies.AdminOnly);
        group.MapDelete("/{serviceId:guid}", RemoveAsync).RequireAuthorization(Policies.AdminOnly);

        return app;
    }

    private static async Task<IResult> GetServicesAsync(
        Guid professionalId,
        ProfessionalServiceUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.GetServicesAsync(professionalId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(Map))
            : ResultTranslation.ToHttpError(result);
    }

    private static async Task<IResult> AssignAsync(
        Guid professionalId,
        AssignServiceRequest request,
        ProfessionalServiceUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.AssignAsync(professionalId, request.ServiceId, cancellationToken);
        if (!result.IsSuccess)
            return ResultTranslation.ToHttpError(result);

        var service = result.Value!;
        return Results.Created(
            $"/api/professionals/{professionalId}/services/{service.Id}",
            Map(service));
    }

    private static async Task<IResult> RemoveAsync(
        Guid professionalId,
        Guid serviceId,
        ProfessionalServiceUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.RemoveAsync(professionalId, serviceId, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ResultTranslation.ToHttpError(result);
    }

    // Reuses the ServiceResponse DTO from the Services feature: same service shape.
    private static ServiceResponse Map(Service service) =>
        new(service.Id, service.Name, service.Description, service.DurationInMinutes, service.Price, service.IsActive);
}
