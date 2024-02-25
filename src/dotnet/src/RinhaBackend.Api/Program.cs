using Orleans.Runtime.Hosting;
using RinhaBackend.Api.Data;
using RinhaBackend.Api.Endpoints;
using RinhaBackend.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(static siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.Services.AddGrainStorage("contasStorage",
        (serviceProvider, name) => serviceProvider.GetRequiredKeyedService<ContaGrainStorage>("contasStorage"));
});

builder.Services.AddSingleton<Store>();
builder.Services.AddSingleton<IStore>(provider => provider.GetRequiredService<Store>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<Store>());
builder.Services.AddKeyedSingleton<ContaGrainStorage>("contasStorage");
builder.Services.AddHostedService<GrainsWarmUpService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(setup =>
            {
                setup.RoutePrefix = string.Empty;
                setup.SwaggerEndpoint("/swagger/v1/swagger.json", "Rinha de backend dotnet API");
            });
}

app.MapPostTransacao();
app.MapGetExtrato();

app.Run();
