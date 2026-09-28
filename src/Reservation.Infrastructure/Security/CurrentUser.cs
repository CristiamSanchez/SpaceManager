using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Reservation.Application.Abstractions.Services;
using Reservation.Domain.Enums;

namespace Reservation.Infrastructure.Security;

/// <summary>
/// Exposes the authenticated user (from HttpContext.User) to the Application layer.
/// Unauthenticated or missing claims → Guid.Empty / false, never an invalid identifier.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user is not null
                && user.Identity?.IsAuthenticated == true
                && user.IsInRole(UserRole.Admin.ToString());
        }
    }
}
