using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservation.Application.Abstractions.Persistence;
using Reservation.Application.Abstractions.Services;
using Reservation.Infrastructure.Data;
using Reservation.Infrastructure.Data.Repositories;
using Reservation.Infrastructure.Security;

namespace Reservation.Infrastructure;

/// <summary>
/// Registration of Infrastructure dependencies so the API can add them
/// with a single call: services.AddInfrastructure(configuration).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' is missing. Define it in configuration (e.g. appsettings.json or environment variable ConnectionStrings__Postgres).");

        services.AddDbContext<ReservationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories share the scoped lifetime of the DbContext.
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
        services.AddScoped<IProfessionalServiceRepository, ProfessionalServiceRepository>();
        services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        // Security: strongly typed JWT configuration + implementations of the
        // Application abstractions (password hashing, token generation, current user).
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
