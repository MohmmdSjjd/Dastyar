namespace Dastyar.Application.Common;

public sealed record Result<T>(bool Succeeded, T? Data, string? Error)
    where T : class
{
    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, null, error);
}
