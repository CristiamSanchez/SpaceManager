namespace Reservation.Api.Common;

/// <summary>Simple ASP.NET Core authorization policies (no custom framework).</summary>
public static class Policies
{
    /// <summary>Requires the Admin role (see AddAuthorization in Program.cs).</summary>
    public const string AdminOnly = "AdminOnly";
}
