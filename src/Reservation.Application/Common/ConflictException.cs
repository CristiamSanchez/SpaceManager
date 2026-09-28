namespace Reservation.Application.Common;

/// <summary>
/// Raised by Infrastructure when the database rejects an operation because of an
/// existing unique constraint (e.g. duplicate email or duplicate professional/service
/// pair) that survived the Application pre-check because of a concurrent request.
/// The API's global exception handler translates it to 409; the message is always a
/// safe, application-authored string — never database details.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
