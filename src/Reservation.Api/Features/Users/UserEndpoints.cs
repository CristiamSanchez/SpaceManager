using Reservation.Api.Common;
using Reservation.Application.Features.Users;

namespace Reservation.Api.Features.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // Authenticated callers only (401 without token). The Application layer enforces
        // ownership: a Client reads only their own record, an Admin reads any record.
        // The response is the Application UserModel, which never contains PasswordHash.
        app.MapGet("/api/users/{id:guid}", GetByIdAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        UserUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value!) : ResultTranslation.ToHttpError(result);
    }
}
