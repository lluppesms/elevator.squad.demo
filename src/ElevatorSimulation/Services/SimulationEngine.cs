using System.Text.Json;
using ElevatorSimulation.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ElevatorSimulation.Services;

public class SimulationEngine : BackgroundService
{
    private readonly Building _building;
    private readonly Dispatcher _dispatcher;
    private readonly SimulationSettings _settings;
    private readonly ILogger<SimulationEngine> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Random _random = new();

    public event Func<Building, Task>? StateChanged;

    public SimulationEngine(
        IOptions<SimulationSettings> settings,
        ILogger<SimulationEngine> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _dispatcher = new Dispatcher();
        _building = CreateBuilding();
    }

    public Building Building => _building;

    public async Task<Passenger?> AddPassengerAsync(int originFloor, int destinationFloor)
    {
        await _lock.WaitAsync();
        try
        {
            var passenger = _dispatcher.CreatePassenger(originFloor, destinationFloor, _building.Tick);
            _dispatcher.AssignOrQueue(passenger, _building);
            await PublishAsync();
            return passenger;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetPausedAsync(bool paused)
    {
        await _lock.WaitAsync();
        try
        {
            _building.Paused = paused;
            _building.StatusMessage = paused ? "Paused" : "Running";
            await PublishAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RestartAsync()
    {
        await _lock.WaitAsync();
        try
        {
            ResetBuilding();
            await PublishAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Building> GetSnapshotAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return DeepCopy(_building);
        }
        finally
        {
            _lock.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_settings.TickIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(interval, stoppingToken);
            await ProcessTickAsync(stoppingToken);
        }
    }

    internal async Task ProcessTickAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_building.Paused && !_building.IsFinished)
                await TickAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task TickAsync()
    {
        _building.Tick++;

        if (_building.Tick >= _settings.MaxTicks)
        {
            _building.Paused = true;
            _building.IsFinished = true;
            _building.StatusMessage = "Simulation complete — maximum of 1 000 ticks reached.";
            await PublishAsync();
            return;
        }

        _dispatcher.RetryPending(_building);

        foreach (var elevator in _building.Elevators)
            AdvanceElevator(elevator);

        MaybeSpawnPassenger();
        MaybeRefreshWaitTime();

        await PublishAsync();
    }

    private void AdvanceElevator(Elevator elevator)
    {
        if (elevator.DoorState == "Open")
        {
            elevator.DoorTicksRemaining--;
            if (elevator.DoorTicksRemaining <= 0)
            {
                elevator.DoorState = "Closed";
                UpdateDirection(elevator);
            }
            return;
        }

        // Service this floor if it is a scheduled stop
        if (elevator.ScheduledStops.Contains(elevator.CurrentFloor))
        {
            ServiceFloor(elevator);
            return;
        }

        // Move toward next stop
        if (elevator.ScheduledStops.Count == 0)
        {
            elevator.Direction = "Idle";
            return;
        }

        var nextStop = elevator.Direction == "Up"
            ? elevator.ScheduledStops.Where(f => f > elevator.CurrentFloor).DefaultIfEmpty(-1).Min()
            : elevator.ScheduledStops.Where(f => f < elevator.CurrentFloor).DefaultIfEmpty(-1).Max();

        if (nextStop == -1)
        {
            UpdateDirection(elevator);
            return;
        }

        elevator.CurrentFloor += elevator.Direction == "Up" ? 1 : -1;
    }

    private void ServiceFloor(Elevator elevator)
    {
        elevator.DoorState = "Open";
        elevator.DoorTicksRemaining = 1;
        elevator.ScheduledStops.Remove(elevator.CurrentFloor);

        // Drop off passengers whose destination is this floor
        var departing = elevator.Passengers
            .Where(p => p.DestinationFloor == elevator.CurrentFloor)
            .ToList();
        foreach (var p in departing)
        {
            elevator.Passengers.Remove(p);
            elevator.PassengersMoved++;
        }

        // Board waiting passengers heading the same direction (or idle)
        if (_building.WaitingPassengers.TryGetValue(elevator.CurrentFloor, out var waiting))
        {
            var boarding = waiting
                .Where(p => elevator.Direction == "Idle" || p.Direction == elevator.Direction)
                .Take(elevator.Capacity - elevator.Passengers.Count)
                .ToList();

            foreach (var p in boarding)
            {
                waiting.Remove(p);
                elevator.Passengers.Add(p);

                var waitSeconds = (_building.Tick - p.RequestedTick) * _settings.TickIntervalSeconds;
                _building.TotalPassengerWaitTimeSeconds += waitSeconds;
                _building.BoardedPassengerCount++;
            }

            if (waiting.Count == 0)
                _building.WaitingPassengers.Remove(elevator.CurrentFloor);
        }
    }

    private static void UpdateDirection(Elevator elevator)
    {
        if (elevator.ScheduledStops.Count == 0)
        {
            elevator.Direction = "Idle";
            return;
        }

        var anyAbove = elevator.ScheduledStops.Any(f => f > elevator.CurrentFloor);
        elevator.Direction = anyAbove ? "Up" : "Down";
    }

    private void MaybeSpawnPassenger()
    {
        if (_random.NextDouble() >= _settings.SpawnChance) return;

        var origin = _random.Next(1, _building.FloorCount + 1);
        var dest = _random.Next(1, _building.FloorCount + 1);
        while (dest == origin)
            dest = _random.Next(1, _building.FloorCount + 1);

        var passenger = _dispatcher.CreatePassenger(origin, dest, _building.Tick);
        var result = _dispatcher.AssignOrQueue(passenger, _building);

        _logger.LogDebug("Tick {Tick}: spawned {Id} ({Origin}→{Dest}) — {Result}",
            _building.Tick, passenger.Id, origin, dest, result);
    }

    private void MaybeRefreshWaitTime()
    {
        if (_building.Tick - _building.WaitTimeUpdatedTick < 60) return;
        if (_building.BoardedPassengerCount == 0) return;

        _building.AveragePassengerWaitTimeSeconds =
            _building.TotalPassengerWaitTimeSeconds / _building.BoardedPassengerCount;
        _building.WaitTimeUpdatedTick = _building.Tick;
    }

    private Task PublishAsync()
    {
        if (StateChanged is null) return Task.CompletedTask;
        var snapshot = DeepCopy(_building);
        return StateChanged.Invoke(snapshot);
    }

    private void ResetBuilding()
    {
        _building.Tick = 0;
        _building.Paused = false;
        _building.IsFinished = false;
        _building.StatusMessage = "Running";
        _building.TotalPassengerWaitTimeSeconds = 0;
        _building.BoardedPassengerCount = 0;
        _building.AveragePassengerWaitTimeSeconds = 0;
        _building.WaitTimeUpdatedTick = 0;
        _building.WaitingPassengers.Clear();
        _building.PendingPassengers.Clear();

        for (var i = 0; i < _building.Elevators.Count; i++)
        {
            var e = _building.Elevators[i];
            e.CurrentFloor = i + 1;
            e.Direction = "Idle";
            e.DoorState = "Closed";
            e.DoorTicksRemaining = 0;
            e.PassengersMoved = 0;
            e.Passengers.Clear();
            e.ScheduledStops.Clear();
        }
    }

    private static Building CreateBuilding()
    {
        var building = new Building
        {
            FloorCount = 5,
            Elevators =
            [
                new Elevator { Id = "ev-01", CurrentFloor = 1 },
                new Elevator { Id = "ev-02", CurrentFloor = 2 },
                new Elevator { Id = "ev-03", CurrentFloor = 3 },
                new Elevator { Id = "ev-04", CurrentFloor = 4 }
            ]
        };

        for (var floor = 1; floor <= building.FloorCount; floor++)
            building.WaitingPassengers[floor] = [];

        return building;
    }

    private static Building DeepCopy(Building source)
    {
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<Building>(json)!;
    }
}
