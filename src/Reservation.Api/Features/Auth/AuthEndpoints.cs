using Reservation.Api.Common;
using Reservation.Application.Features.Auth;

namespace Reservation.Api.Features.Auth;

public record RegisterRequest(string? Name, string? Email, string? Password);

public record LoginRequest(string? Email, string? Password);

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        AuthUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.RegisterAsync(
            request.Name,
            request.Email,
            request.Password,
            cancellationToken);

        if (!result.IsSuccess)
            return ResultTranslation.ToHttpError(result);

        var user = result.Value!;
        return Results.Created($"/api/users/{user.Id}", user);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AuthUseCases useCases,
        CancellationToken cancellationToken)
    {
        var result = await useCases.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value!) : ResultTranslation.ToHttpError(result);
    }
}
