namespace RinhaBackend.Api;

[GenerateSerializer, Alias("GrainResponse")]
public readonly struct GrainResponse
{
    [Id(0)]
    public byte[] Body { get; }

    [Id(1)]
    public bool IsSuccess { get; }
    
    [Id(2)]
    public string? Error { get; }
    
    [Id(3)]
    public int Code { get; }
    
    public readonly bool IsFailure => !IsSuccess;

    public GrainResponse()
    {
        Body = [];
        IsSuccess = false;
        Error = string.Empty;
        Code = 0;
    }

    private GrainResponse(int code, byte[] body)
    {
        Body = body;
        IsSuccess = true;
        Code = code;
        Error = null;
    }
    
    private GrainResponse(int code, string? error = null)
    {
        Body = [];
        IsSuccess = false;
        Error = error;
        Code = code;
    }

    public static GrainResponse Ok(byte[] body) => new(200, body);
    public static GrainResponse NotFound() => new(404);
    public static GrainResponse UnprocessableEntity(string? error = null) => new(422, error);
}
