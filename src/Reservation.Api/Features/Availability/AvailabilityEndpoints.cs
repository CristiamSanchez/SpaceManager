using Reservation.Api.Common;
using Reservation.Application.Features.Availability;
// "Availability" alone resolves to this namespace inside Reservation.Api.Features.*, so alias the entity.
using AvailabilityEntity = Reservation.Domain.Entities.Availability;

namespace Reservation.Api.Features.Availability;

public record AvailabilityResponse(
    Guid Id,
    Guid ProfessionalId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);

public record CreateAvailabilityRequest(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);

public record UpdateAvailabilityRequest(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);

public static class AvailabilityEndpoints
{
    public static IEndpointRouteBuilder MapAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/professionals/{professionalId:guid}/availability");

        group.MapGet("/", GetByProfessionalAsync);
        group.MapPost("/", CreateAsync).RequireAuthorization(Policies.AdminOnly);
        group.MapPut("/{availabilityId:guid}", UpdateAsync).RequireAuthorization(Policies.AdminOnly);
        group.MapDelete("/{availabilityId:guid}", DeactivateAsync).RequireAuthorization(Policies.AdminOnly);

        return app;
    }

    private static async Task<IResult> GetByProfessionalAsync(
        Guid professionalId,
        AvailabilityUseCases useCases,
        CancellationToken cancellationToken)
    {
        // Use case returns only active periods, ordered by DayOfWeek then StartTime.
        var availability = await useCases.GetByProfessionalAsync(professionalId, cancellationToken);
        return Results.Ok(availability.Select(a => Map(a)));
    }

    private static async Task<IResult> CreateAsync(
        Guid professionalId,
        CreateAvailabilityRequest request,
        AvailabilityUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.CreateAsync(
            professionalId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime,
            cancellationToken);

        if (!result.IsSuccess)
            return ResultTranslation.ToHttpError(result);

        var availability = result.Value!;
        return Results.Created(
            $"/api/professionals/{professionalId}/availability/{availability.Id}",
            Map(availability));
    }

    private static async Task<IResult> UpdateAsync(
        Guid professionalId,
        Guid availabilityId,
        UpdateAvailabilityRequest request,
        AvailabilityUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.UpdateAsync(
            professionalId,
            availabilityId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime,
            request.IsActive,
            cancellationToken);

        return result.IsSuccess ? Results.Ok(Map(result.Value!)) : ResultTranslation.ToHttpError(result);
    }

    private static async Task<IResult> DeactivateAsync(
        Guid professionalId,
        Guid availabilityId,
        AvailabilityUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.DeactivateAsync(professionalId, availabilityId, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ResultTranslation.ToHttpError(result);
    }

    private static AvailabilityResponse Map(AvailabilityEntity availability) =>
        new(
            availability.Id,
            availability.ProfessionalId,
            availability.DayOfWeek,
            availability.StartTime,
            availability.EndTime,
            availability.IsActive);
}
