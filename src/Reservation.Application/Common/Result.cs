namespace Reservation.Application.Common;

/// <summary>
/// Kind of application failure, so the API can translate it into the right HTTP status
/// (Validation → 400, NotFound → 404, Unauthorized → 401, Conflict → 409)
/// without Application knowing anything about HTTP.
/// </summary>
public enum ErrorKind
{
    Validation = 0,
    NotFound = 1,
    Unauthorized = 2,
    Conflict = 3,
    Forbidden = 4
}

/// <summary>
/// Minimal application result: success with a value, or failure with an error message and kind.
/// Deliberately small — not a framework.
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    /// <summary>Kind of failure, or null when successful.</summary>
    public ErrorKind? Kind { get; }

    private Result(bool isSuccess, T? value, string? error, ErrorKind? kind)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Kind = kind;
    }

    public static Result<T> Success(T value) => new(true, value, null, null);

    public static Result<T> Failure(string error) => new(false, default, error, ErrorKind.Validation);

    public static Result<T> NotFound(string error) => new(false, default, error, ErrorKind.NotFound);

    public static Result<T> Unauthorized(string error) => new(false, default, error, ErrorKind.Unauthorized);

    public static Result<T> Conflict(string error) => new(false, default, error, ErrorKind.Conflict);

    public static Result<T> Forbidden(string error) => new(false, default, error, ErrorKind.Forbidden);
}
