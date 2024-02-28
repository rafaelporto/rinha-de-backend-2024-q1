using RinhaBackend.Api.Data;

namespace RinhaBackend.Api;

public static class ServicesConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<Store>()
            .AddSingleton<IStore>(provider => provider.GetRequiredService<Store>())
            .AddHostedService(provider => provider.GetRequiredService<Store>());

#if DEBUG
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
#endif
        return services;
    }
}
