using ElevatorApi.Hubs;
using ElevatorSimulation;
using ElevatorSimulation.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.Configure<SimulationSettings>(
    builder.Configuration.GetSection("Simulation"));
builder.Services.AddSingleton<SimulationEngine>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<SimulationEngine>());
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseHttpsRedirection();

// Wire SimulationEngine state changes → SignalR broadcast
var engine = app.Services.GetRequiredService<SimulationEngine>();
var hubContext = app.Services.GetRequiredService<IHubContext<BuildingHub>>();
engine.StateChanged += async snapshot =>
    await hubContext.Clients.All.SendAsync("BuildingState", snapshot);

// --- REST endpoints ---

app.MapGet("/api/state", async (SimulationEngine sim) =>
    Results.Ok(await sim.GetSnapshotAsync()));

app.MapPost("/api/passengers", async (PassengerRequest req, SimulationEngine sim) =>
{
    if (req.OriginFloor == req.DestinationFloor)
        return Results.BadRequest("Origin and destination floors must differ.");
    if (req.OriginFloor is < 1 or > 5 || req.DestinationFloor is < 1 or > 5)
        return Results.BadRequest("Floor numbers must be between 1 and 5.");

    var passenger = await sim.AddPassengerAsync(req.OriginFloor, req.DestinationFloor);
    return Results.Created($"/api/passengers/{passenger!.Id}", passenger);
});

app.MapPost("/api/control", async (ControlRequest req, SimulationEngine sim) =>
{
    await sim.SetPausedAsync(req.Paused);
    return Results.Ok();
});

app.MapPost("/api/restart", async (SimulationEngine sim) =>
{
    await sim.RestartAsync();
    return Results.Ok(await sim.GetSnapshotAsync());
});

app.MapHub<BuildingHub>("/buildinghub");

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

record PassengerRequest(int OriginFloor, int DestinationFloor);
record ControlRequest(bool Paused);

public partial class Program;
