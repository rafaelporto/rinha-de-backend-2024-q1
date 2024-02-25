using Newtonsoft.Json;

namespace RinhaBackend.Api.Data;

public class GrainEntity<TState>
{
    public string? Id { get; set; }

    public string StateName { get; set; } = default!;

    public TState State { get; set; } = default!;
}
