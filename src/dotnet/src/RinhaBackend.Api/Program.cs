using RinhaBackend.Api;
using RinhaBackend.Api.Endpoints;

var builder = ClusterBuilder.CreateBuilder(args);

builder.Services.ConfigureServices();

var app = builder.Build();

#if DEBUG
app.UseSwagger()
   .UseSwaggerUI(setup =>
        {
            setup.RoutePrefix = string.Empty;
            setup.SwaggerEndpoint("/swagger/v1/swagger.json", "Rinha de backend dotnet API");
        });
#endif

app.MapPostTransacao();
app.MapGetExtrato();

app.Run();
