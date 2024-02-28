using System.Net.Sockets;
using Orleans.Configuration;

namespace RinhaBackend.Api;

public static class ClusterBuilder
{
    public static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ClusterConfig clusterOptions = new();
        builder.Configuration.GetSection(ClusterConfig.CONFIG_NAME)
            .Bind(clusterOptions);

        builder.Services.Configure<ClusterConfig>(
            builder.Configuration.GetSection(
                key: ClusterConfig.CONFIG_NAME));

        builder.Host.UseOrleans(siloBuilder =>
        {
            siloBuilder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = clusterOptions.ClusterId;
                options.ServiceId = clusterOptions.ServiceId;
            })
            .Configure<SiloOptions>(options => options.SiloName = "silo-contas")
            .ConfigureEndpoints(11111, 30000, AddressFamily.InterNetwork, true)
            .UseAdoNetClustering(options =>
            {
                options.Invariant = clusterOptions.AdoNetInvariant;
                options.ConnectionString = clusterOptions.ConnectionString;
            });
        });

        return builder;
    }
}
