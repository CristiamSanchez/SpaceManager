using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Reservation.Api.Common;
using Reservation.Api.Features.Auth;
using Reservation.Api.Features.Availability;
using Reservation.Api.Features.Professionals;
using Reservation.Api.Features.Reservations;
using Reservation.Api.Features.Services;
using Reservation.Api.Features.Users;
using Reservation.Application.Features.Auth;
using Reservation.Application.Features.Availability;
using Reservation.Application.Features.Professionals;
using Reservation.Application.Features.Reservations;
using Reservation.Application.Features.Services;
using Reservation.Application.Features.Users;
using Reservation.Infrastructure;
using Reservation.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Enums (e.g. UserRole, DayOfWeek) are exchanged as strings, not numbers.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Infrastructure: DbContext, repositories, security implementations.
builder.Services.AddInfrastructure(builder.Configuration);

// Application use cases.
builder.Services.AddScoped<ServiceUseCases>();
builder.Services.AddScoped<ProfessionalUseCases>();
builder.Services.AddScoped<ProfessionalServiceUseCases>();
builder.Services.AddScoped<AvailabilityUseCases>();
builder.Services.AddScoped<UserUseCases>();
builder.Services.AddScoped<AuthUseCases>();
builder.Services.AddScoped<ReservationUseCases>();

// JWT bearer authentication (configuration from the "Jwt" section).
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException($"Missing configuration section '{JwtSettings.SectionName}'.");
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:SecretKey must be at least 32 characters. Provide it via configuration " +
        "(environment variable Jwt__SecretKey outside development).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Token claims are already emitted with ASP.NET Core claim types
        // (ClaimTypes.NameIdentifier / ClaimTypes.Role), so inbound mapping is
        // disabled: claims pass through verbatim and role claims are recognized as-is.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
// Global exception handling: unexpected exceptions → 500 (fixed message),
// known unique-constraint conflicts → 409. Details only in the logs.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// UseExceptionHandler() validates at startup that a fallback exists when neither
// ExceptionHandlingPath nor an ExceptionHandler delegate is set: it must resolve
// IProblemDetailsService (AddProblemDetails). Registered IExceptionHandler services
// are NOT checked by that validation. GlobalExceptionHandler still runs first and
// always handles, so the ProblemDetails fallback is never used at runtime.
builder.Services.AddProblemDetails();

builder.Services.AddAuthorization(options =>
{
    // Single Admin policy (role comes from the JWT role claim).
    options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole("Admin"));
});

// Development-only CORS: lets the Angular dev server (http://localhost:4200)
// call this API during local development. Explicit origin (never AllowAnyOrigin)
// and no credentials — the JWT travels in the Authorization header.
const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
    options.AddPolicy(DevCorsPolicy, policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors(DevCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapServiceEndpoints();
app.MapProfessionalEndpoints();
app.MapProfessionalServiceEndpoints();
app.MapAvailabilityEndpoints();
app.MapUserEndpoints();
app.MapAuthEndpoints();
app.MapReservationEndpoints();

app.Run();
