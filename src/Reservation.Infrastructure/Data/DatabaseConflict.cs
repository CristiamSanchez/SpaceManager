using Microsoft.EntityFrameworkCore;
using Npgsql;
using Reservation.Application.Common;

namespace Reservation.Infrastructure.Data;

/// <summary>
/// Saves changes, translating a PostgreSQL unique-constraint violation (SQLSTATE 23505)
/// into <see cref="ConflictException"/> so concurrent duplicate requests become a
/// controlled 409 instead of a 500. PostgreSQL error codes stay here: neither Domain,
/// Application nor the API ever see them.
/// </summary>
internal static class DatabaseConflict
{
    public static async Task SaveChangesAsync(
        ReservationDbContext dbContext,
        string conflictMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // conflictMessage is provided by the caller and must match the
            // application-level pre-check message (no PostgreSQL text is forwarded).
            throw new ConflictException(conflictMessage);
        }
    }
}
