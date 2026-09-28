using Reservation.Api.Common;
using Reservation.Application.Features.Reservations;
using Reservation.Domain.Enums;
// "Reservation" alone resolves to the root namespace inside Reservation.*, so alias the entity.
using ReservationEntity = Reservation.Domain.Entities.Reservation;

namespace Reservation.Api.Features.Reservations;

// EndAt is deliberately absent: it is calculated from the service duration.
public record CreateReservationRequest(
    Guid? ProfessionalId,
    Guid? ServiceId,
    DateTime? StartAt,
    string? Notes,
    Guid? UserId);

public record ReservationResponse(
    Guid Id,
    Guid UserId,
    Guid ProfessionalId,
    Guid ServiceId,
    DateTime StartAt,
    DateTime EndAt,
    ReservationStatus Status,
    string? Notes,
    DateTime CreatedAt);

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reservations");

        // All operations require authentication; role/ownership rules are enforced
        // in Application (Client → own, Admin → any).
        group.MapPost("/", CreateAsync).RequireAuthorization();
        group.MapGet("/", ListAsync).RequireAuthorization();
        group.MapGet("/{id:guid}", GetByIdAsync).RequireAuthorization();
        group.MapDelete("/{id:guid}", CancelAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateReservationRequest request,
        ReservationUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.CreateAsync(
            request.ProfessionalId,
            request.ServiceId,
            request.StartAt,
            request.Notes,
            request.UserId,
            cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/reservations/{result.Value!.Id}", Map(result.Value))
            : ResultTranslation.ToHttpError(result);
    }

    private static async Task<IResult> ListAsync(
        ReservationUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.ListAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(Map))
            : ResultTranslation.ToHttpError(result);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ReservationUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Results.Ok(Map(result.Value!)) : ResultTranslation.ToHttpError(result);
    }

    private static async Task<IResult> CancelAsync(
        Guid id,
        ReservationUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.CancelAsync(id, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ResultTranslation.ToHttpError(result);
    }

    private static ReservationResponse Map(ReservationEntity reservation) =>
        new(
            reservation.Id,
            reservation.UserId,
            reservation.ProfessionalId,
            reservation.ServiceId,
            reservation.StartAt,
            reservation.EndAt,
            reservation.Status,
            reservation.Notes,
            reservation.CreatedAt);
}
