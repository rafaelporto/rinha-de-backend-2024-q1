using System.Text.Json;

namespace RinhaBackend.Api.Data;

public class GrainEntity<TState>(int id, TState state)
{
    public int? Id { get; set; } = id;
    public TState State { get; set; } = state;
}
