using Reservation.Application.Common;

namespace Reservation.Api.Common;

/// <summary>
/// Translates application failures into HTTP responses:
/// Validation → 400, Unauthorized → 401, Forbidden → 403, NotFound → 404, Conflict → 409.
/// </summary>
public static class ResultTranslation
{
    public static IResult ToHttpError<T>(Result<T> result)
    {
        if (result.Kind == ErrorKind.Unauthorized)
            return Results.Unauthorized();
        if (result.Kind == ErrorKind.Forbidden)
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        if (result.Kind == ErrorKind.Conflict)
            return Results.Conflict(result.Error);
        if (result.Kind == ErrorKind.NotFound)
            return Results.NotFound(result.Error);
        return Results.BadRequest(result.Error);
    }
}
