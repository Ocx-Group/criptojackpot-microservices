namespace CryptoJackpot.Domain.Core.Responses;

public sealed record ResultResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public ErrorType? ErrorType { get; init; }
    public SuccessType SuccessType { get; init; } = SuccessType.Ok;

    public static ResultResponse<T> Ok(T? data = default) =>
        new() { Success = true, Data = data, SuccessType = SuccessType.Ok };

    public static ResultResponse<T> Created(T data) =>
        new() { Success = true, Data = data, SuccessType = SuccessType.Created };

    public static ResultResponse<T> NoContent() =>
        new() { Success = true, SuccessType = SuccessType.NoContent };

    public static ResultResponse<T> Failure(ErrorType error, string message) =>
        new() { Success = false, ErrorType = error, Message = message };
}
