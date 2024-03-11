using System.Text.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using RinhaBackend.Api.Data;

namespace RinhaBackend.Api;

public static class ServicesConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<Store>()
            .AddSingleton<IStore>(provider => provider.GetRequiredService<Store>())
            .AddHostedService(provider => provider.GetRequiredService<Store>())
            .ConfigureHttpJsonOptions(options =>
                    {
                        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        options.SerializerOptions.TypeInfoResolverChain.Insert(0, JsonContext.Default);
                    });

        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

        services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<TransacaoEntidade>();
            return provider.Create(policy);
        });

#if DEBUG
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
#endif
        return services;
    }
}
