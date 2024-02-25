using System.Text.Json;
using Orleans.Storage;
using RinhaBackend.Api.Models;

namespace RinhaBackend.Api.Data;

public sealed class ContaGrainStorage(IStore store) : IGrainStorage
{
    public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        using var session = store.OpenSession();
        var id = grainId.Key.ToString();

        session.Delete(id);
        session.SaveChangesAsync();

        return Task.CompletedTask;
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
       using var session = store.OpenSession();

        var id = grainId.Key.ToString();

        var grainEntity = await session.LoadAsync<GrainEntity<T>>(id);

        if (grainEntity is not null)
            grainState.State = grainEntity.State;

        else
            grainState.State = default!;
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        using var session = store.OpenSession();
        
        try
        {
            var grainEntity = new GrainEntity<T>
            {
                Id = grainId.Key.ToString(),
                StateName = stateName,
                State = grainState.State
            };

            await session.StoreAsync(grainEntity);
            await session.SaveChangesAsync();
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}
