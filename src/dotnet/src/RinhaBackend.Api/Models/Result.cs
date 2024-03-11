namespace RinhaBackend.Api;

[GenerateSerializer, Alias("ResultT")]
public readonly struct Result<T>
{
    [Id(0)]
    public T Value { get; }

    [Id(1)]
    public bool IsSuccess { get; }
    
    [Id(2)]
    public string? Error { get; }
    
    public bool IsFailure => !IsSuccess;

    public Result()
    {
        Value = default!;
        IsSuccess = false;
        Error = string.Empty;
    }

    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
        Error = string.Empty;
    }

    private Result(string? error)
    {
        Value = default!;
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string? error = null) => new(error);
}

public readonly struct Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string? error = null) => Result<T>.Failure(error);
}
