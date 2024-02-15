using  RinhaBackend.Api.Endpoints;
using RinhaBackend.Api.Grains;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(static siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("contas");
});

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

app.MapPostCriarContas();
app.MapPostTransacao();
app.MapGetExtrato();

app.Run();

