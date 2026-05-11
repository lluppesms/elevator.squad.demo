using System.Reflection;
using ElevatorSimulation;
using ElevatorSimulation.Models;
using ElevatorSimulation.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ElevatorTests;

public class SimulationEngineTests
{
    [Fact]
    public async Task AddPassengerAsync_AssignsPassengerAndPublishesSnapshot()
    {
        var engine = CreateEngine();
        Building? published = null;
        engine.StateChanged += snapshot =>
        {
            published = snapshot;
            return Task.CompletedTask;
        };

        var passenger = await engine.AddPassengerAsync(1, 4);

        Assert.NotNull(passenger);
        Assert.Equal("psg-0001", passenger.Id);
        Assert.NotNull(published);
        Assert.NotSame(engine.Building, published);
        Assert.Single(engine.Building.WaitingPassengers[1]);
    }

    [Fact]
    public async Task SetPausedAsync_TogglesPausedAndStatusMessage()
    {
        var engine = CreateEngine();

        await engine.SetPausedAsync(true);
        Assert.True(engine.Building.Paused);
        Assert.Equal("Paused", engine.Building.StatusMessage);

        await engine.SetPausedAsync(false);
        Assert.False(engine.Building.Paused);
        Assert.Equal("Running", engine.Building.StatusMessage);
    }

    [Fact]
    public async Task RestartAsync_ResetsMutatedState()
    {
        var engine = CreateEngine();
        var building = engine.Building;
        building.Tick = 45;
        building.Paused = true;
        building.IsFinished = true;
        building.StatusMessage = "Custom";
        building.TotalPassengerWaitTimeSeconds = 12;
        building.BoardedPassengerCount = 2;
        building.AveragePassengerWaitTimeSeconds = 6;
        building.WaitTimeUpdatedTick = 20;
        building.PendingPassengers.Add(new Passenger());
        building.WaitingPassengers[2].Add(new Passenger());
        building.Elevators[0].CurrentFloor = 5;
        building.Elevators[0].Direction = "Up";
        building.Elevators[0].DoorState = "Open";
        building.Elevators[0].DoorTicksRemaining = 2;
        building.Elevators[0].PassengersMoved = 10;
        building.Elevators[0].Passengers.Add(new Passenger());
        building.Elevators[0].ScheduledStops.Add(5);

        await engine.RestartAsync();

        Assert.Equal(0, building.Tick);
        Assert.False(building.Paused);
        Assert.False(building.IsFinished);
        Assert.Equal("Running", building.StatusMessage);
        Assert.Equal(0, building.TotalPassengerWaitTimeSeconds);
        Assert.Equal(0, building.BoardedPassengerCount);
        Assert.Equal(0, building.AveragePassengerWaitTimeSeconds);
        Assert.Equal(0, building.WaitTimeUpdatedTick);
        Assert.Empty(building.PendingPassengers);
        Assert.All(building.WaitingPassengers.Values, floor => Assert.Empty(floor));
        Assert.All(building.Elevators, e =>
        {
            Assert.Equal("Idle", e.Direction);
            Assert.Equal("Closed", e.DoorState);
            Assert.Equal(0, e.DoorTicksRemaining);
            Assert.Equal(0, e.PassengersMoved);
            Assert.Empty(e.Passengers);
            Assert.Empty(e.ScheduledStops);
        });
    }

    [Fact]
    public async Task GetSnapshotAsync_ReturnsDeepCopy()
    {
        var engine = CreateEngine();
        await engine.AddPassengerAsync(1, 3);

        var snapshot = await engine.GetSnapshotAsync();
        snapshot.Tick = 99;
        snapshot.Elevators[0].CurrentFloor = 4;
        snapshot.WaitingPassengers[1].Clear();

        Assert.NotEqual(snapshot.Tick, engine.Building.Tick);
        Assert.NotEqual(snapshot.Elevators[0].CurrentFloor, engine.Building.Elevators[0].CurrentFloor);
        Assert.NotEqual(snapshot.WaitingPassengers[1].Count, engine.Building.WaitingPassengers[1].Count);
    }

    [Fact]
    public async Task TickAsync_MaxTicksReached_MarksSimulationFinished()
    {
        var engine = CreateEngine(maxTicks: 2);
        engine.Building.Tick = 1;

        await InvokePrivateTickAsync(engine);

        Assert.Equal(2, engine.Building.Tick);
        Assert.True(engine.Building.Paused);
        Assert.True(engine.Building.IsFinished);
        Assert.Contains("Simulation complete", engine.Building.StatusMessage);
    }

    [Fact]
    public async Task TickAsync_OpenDoorCountdown_ClosesDoorAndSetsDirection()
    {
        var engine = CreateEngine();
        var elevator = engine.Building.Elevators[0];
        elevator.DoorState = "Open";
        elevator.DoorTicksRemaining = 1;
        elevator.ScheduledStops.Add(5);

        await InvokePrivateTickAsync(engine);

        Assert.Equal("Closed", elevator.DoorState);
        Assert.Equal("Up", elevator.Direction);
    }

    [Fact]
    public async Task TickAsync_ServiceFloor_DropsOffBoardsAndRemovesEmptyWaitingFloor()
    {
        var engine = CreateEngine(tickIntervalSeconds: 2.0);
        var building = engine.Building;
        building.Tick = 9;

        var elevator = building.Elevators[1];
        elevator.CurrentFloor = 2;
        elevator.Direction = "Up";
        elevator.ScheduledStops.Add(2);
        elevator.Passengers.Add(new Passenger
        {
            Id = "drop",
            OriginFloor = 1,
            DestinationFloor = 2,
            RequestedTick = 0
        });

        building.WaitingPassengers[2].Add(new Passenger
        {
            Id = "board",
            OriginFloor = 2,
            DestinationFloor = 5,
            RequestedTick = 5
        });

        await InvokePrivateTickAsync(engine);

        Assert.Equal("Open", elevator.DoorState);
        Assert.Equal(1, elevator.DoorTicksRemaining);
        Assert.DoesNotContain(2, elevator.ScheduledStops);
        Assert.Single(elevator.Passengers);
        Assert.Equal(1, elevator.PassengersMoved);
        Assert.Equal(1, building.BoardedPassengerCount);
        Assert.Equal(10, building.TotalPassengerWaitTimeSeconds);
        Assert.False(building.WaitingPassengers.ContainsKey(2));
    }

    [Fact]
    public async Task TickAsync_NoScheduledStops_SetsElevatorIdle()
    {
        var engine = CreateEngine();
        var elevator = engine.Building.Elevators[2];
        elevator.Direction = "Down";
        elevator.ScheduledStops.Clear();

        await InvokePrivateTickAsync(engine);

        Assert.Equal("Idle", elevator.Direction);
    }

    [Fact]
    public async Task TickAsync_NoStopInCurrentDirection_ReversesDirection()
    {
        var engine = CreateEngine();
        var elevator = engine.Building.Elevators[0];
        elevator.CurrentFloor = 4;
        elevator.Direction = "Up";
        elevator.ScheduledStops.Add(2);

        await InvokePrivateTickAsync(engine);

        Assert.Equal("Down", elevator.Direction);
        Assert.Equal(4, elevator.CurrentFloor);
    }

    [Fact]
    public async Task TickAsync_StopAhead_MovesElevatorOneFloor()
    {
        var engine = CreateEngine();
        var elevator = engine.Building.Elevators[0];
        elevator.CurrentFloor = 1;
        elevator.Direction = "Up";
        elevator.ScheduledStops.Add(4);

        await InvokePrivateTickAsync(engine);

        Assert.Equal(2, elevator.CurrentFloor);
    }

    [Fact]
    public async Task TickAsync_RefreshesAverageWaitTimeWhenThresholdReached()
    {
        var engine = CreateEngine();
        var building = engine.Building;
        building.Tick = 59;
        building.WaitTimeUpdatedTick = 0;
        building.BoardedPassengerCount = 2;
        building.TotalPassengerWaitTimeSeconds = 30;

        await InvokePrivateTickAsync(engine);

        Assert.Equal(15, building.AveragePassengerWaitTimeSeconds);
        Assert.Equal(60, building.WaitTimeUpdatedTick);
    }

    [Fact]
    public async Task ExecuteAsync_StartAndStop_RunsTicks()
    {
        var engine = CreateEngine(tickIntervalSeconds: 0.01, spawnChance: 0, maxTicks: 1000);

        await engine.StartAsync(CancellationToken.None);
        await Task.Delay(60);
        await engine.StopAsync(CancellationToken.None);

        Assert.True(engine.Building.Tick > 0);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaused_DoesNotAdvanceTicks()
    {
        var engine = CreateEngine(tickIntervalSeconds: 0.01, spawnChance: 0, maxTicks: 1000);
        await engine.SetPausedAsync(true);

        await engine.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await engine.StopAsync(CancellationToken.None);

        Assert.Equal(0, engine.Building.Tick);
    }

    private static SimulationEngine CreateEngine(
        double tickIntervalSeconds = 1.0,
        double spawnChance = 0.0,
        int maxTicks = 1000)
    {
        var options = Options.Create(new SimulationSettings
        {
            TickIntervalSeconds = tickIntervalSeconds,
            SpawnChance = spawnChance,
            MaxTicks = maxTicks
        });

        return new SimulationEngine(options, NullLogger<SimulationEngine>.Instance);
    }

    private static async Task InvokePrivateTickAsync(SimulationEngine engine)
    {
        var tickMethod = typeof(SimulationEngine).GetMethod("TickAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(tickMethod);

        var result = tickMethod.Invoke(engine, null);
        var task = Assert.IsAssignableFrom<Task>(result);
        await task;
    }
}
