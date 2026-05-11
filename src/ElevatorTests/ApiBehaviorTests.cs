using System.Net;
using System.Net.Http.Json;
using ElevatorSimulation.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ElevatorTests;

public class ApiBehaviorTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiBehaviorTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetState_ReturnsSnapshot()
    {
        var response = await _client.GetAsync("/api/state");

        response.EnsureSuccessStatusCode();
        var snapshot = await response.Content.ReadFromJsonAsync<Building>();

        Assert.NotNull(snapshot);
        Assert.Equal(5, snapshot.FloorCount);
        Assert.Equal(4, snapshot.Elevators.Count);
    }

    [Fact]
    public async Task PostPassengers_WhenOriginMatchesDestination_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/passengers",
            new { originFloor = 3, destinationFloor = 3 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "\"Origin and destination floors must differ.\"",
            await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(2, 6)]
    public async Task PostPassengers_WhenFloorOutOfRange_ReturnsBadRequest(int originFloor, int destinationFloor)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/passengers",
            new { originFloor, destinationFloor });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "\"Floor numbers must be between 1 and 5.\"",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task PostPassengers_WithValidPayload_CreatesPassenger()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/passengers",
            new { originFloor = 1, destinationFloor = 5 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var passenger = await response.Content.ReadFromJsonAsync<Passenger>();
        Assert.NotNull(passenger);
        Assert.StartsWith("psg-", passenger.Id);
        Assert.Equal(1, passenger.OriginFloor);
        Assert.Equal(5, passenger.DestinationFloor);
    }

    [Fact]
    public async Task ControlEndpoint_PausesSimulation()
    {
        var pauseResponse = await _client.PostAsJsonAsync("/api/control", new { paused = true });
        pauseResponse.EnsureSuccessStatusCode();

        var snapshot = await _client.GetFromJsonAsync<Building>("/api/state");
        Assert.NotNull(snapshot);
        Assert.True(snapshot.Paused);
        Assert.Equal("Paused", snapshot.StatusMessage);
    }

    [Fact]
    public async Task RestartEndpoint_ResetsMutatedState()
    {
        await _client.PostAsJsonAsync("/api/passengers", new { originFloor = 2, destinationFloor = 5 });
        await _client.PostAsJsonAsync("/api/control", new { paused = true });

        var response = await _client.PostAsync("/api/restart", content: null);
        response.EnsureSuccessStatusCode();

        var snapshot = await response.Content.ReadFromJsonAsync<Building>();
        Assert.NotNull(snapshot);
        Assert.Equal(0, snapshot.Tick);
        Assert.False(snapshot.Paused);
        Assert.False(snapshot.IsFinished);
        Assert.Equal("Running", snapshot.StatusMessage);
    }
}
