using ElevatorSimulation.Models;
using ElevatorSimulation.Services;
using Xunit;

namespace ElevatorTests;

public class DispatcherTests
{
    private static Building CreateBuilding() => new()
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

    [Fact]
    public void AssignOrQueue_IdleElevator_AssignsNearestCab()
    {
        var dispatcher = new Dispatcher();
        var building = CreateBuilding();
        var passenger = dispatcher.CreatePassenger(3, 5, 0);

        var result = dispatcher.AssignOrQueue(passenger, building);

        Assert.Equal("assigned", result);
        // ev-03 is at floor 3 — nearest idle elevator
        var ev03 = building.Elevators.Single(e => e.Id == "ev-03");
        Assert.Contains(3, ev03.ScheduledStops);
        Assert.Contains(5, ev03.ScheduledStops);
    }

    [Fact]
    public void AssignOrQueue_IdleElevator_UpdatesDirectionToPassengerDirection()
    {
        var dispatcher = new Dispatcher();
        var building = CreateBuilding();
        var passenger = dispatcher.CreatePassenger(4, 1, 0);

        var result = dispatcher.AssignOrQueue(passenger, building);

        Assert.Equal("assigned", result);
        var ev04 = building.Elevators.Single(e => e.Id == "ev-04");
        Assert.Equal("Down", ev04.Direction);
    }

    [Fact]
    public void AssignOrQueue_AllElevatorsAtCapacity_QueuesPassenger()
    {
        var dispatcher = new Dispatcher();
        var building = CreateBuilding();

        // Fill all elevators to capacity
        foreach (var elevator in building.Elevators)
            for (var i = 0; i < elevator.Capacity; i++)
                elevator.Passengers.Add(new Passenger { Id = $"p{i}", OriginFloor = 1, DestinationFloor = 5 });

        var passenger = dispatcher.CreatePassenger(1, 5, 0);
        var result = dispatcher.AssignOrQueue(passenger, building);

        Assert.Equal("queued", result);
        Assert.Single(building.PendingPassengers);
    }

    [Fact]
    public void AssignOrQueue_SameDirectionEnRoute_PrefersSameDirectionCab()
    {
        var dispatcher = new Dispatcher();
        var building = CreateBuilding();

        // Make all elevators busy going down except ev-01 which is going up
        var ev01 = building.Elevators.Single(e => e.Id == "ev-01");
        ev01.Direction = "Up";
        ev01.ScheduledStops.Add(5);

        foreach (var ev in building.Elevators.Where(e => e.Id != "ev-01"))
        {
            ev.Direction = "Down";
            ev.ScheduledStops.Add(1);
        }

        // Passenger wants to go up from floor 2 — only ev-01 is compatible
        var passenger = dispatcher.CreatePassenger(2, 4, 0);
        var result = dispatcher.AssignOrQueue(passenger, building);

        Assert.Equal("assigned", result);
        Assert.Contains(2, ev01.ScheduledStops);
    }

    [Fact]
    public void AssignOrQueue_DownDirectionEnRoute_AssignsCompatibleCab()
    {
        var dispatcher = new Dispatcher();
        var building = CreateBuilding();

        var downCab = building.Elevators.Single(e => e.Id == "ev-04");
        downCab.Direction = "Down";
        downCab.CurrentFloor = 5;
        downCab.ScheduledStops.Add(1);

        foreach (var ev in building.Elevators.Where(e => e.Id != "ev-04"))
        {
            ev.Direction = "Up";
            ev.ScheduledStops.Add(5);
        }

        var passenger = dispatcher.CreatePassenger(3, 1, 0);
        var result = dispatcher.AssignOrQueue(passenger, building);

        Assert.Equal("assigned", result);
        Assert.Contains(3, downCab.ScheduledStops);
        Assert.Contains(1, downCab.ScheduledStops);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(5, 1)]
    [InlineData(2, 3)]
    [InlineData(4, 2)]
    public void CreatePassenger_ValidFloors_SetsDirectionCorrectly(int origin, int dest)
    {
        var dispatcher = new Dispatcher();
        var passenger = dispatcher.CreatePassenger(origin, dest, 0);

        var expectedDirection = dest > origin ? "Up" : "Down";
        Assert.Equal(expectedDirection, passenger.Direction);
        Assert.Equal(origin, passenger.OriginFloor);
        Assert.Equal(dest, passenger.DestinationFloor);
    }

    [Fact]
    public void CreatePassenger_AssignsSequentialIds()
    {
        var dispatcher = new Dispatcher();

        var first = dispatcher.CreatePassenger(1, 2, 0);
        var second = dispatcher.CreatePassenger(2, 3, 0);

        Assert.Equal("psg-0001", first.Id);
        Assert.Equal("psg-0002", second.Id);
    }

    [Fact]
    public void RetryPending_ElevatorBecomesAvailable_AssignsPendingPassenger()
    {
        var dispatcher = new Dispatcher();
        var building = CreateBuilding();

        // Fill all elevators to capacity
        foreach (var elevator in building.Elevators)
            for (var i = 0; i < elevator.Capacity; i++)
                elevator.Passengers.Add(new Passenger { Id = $"p{i}", OriginFloor = 1, DestinationFloor = 5 });

        var passenger = dispatcher.CreatePassenger(2, 4, 0);
        dispatcher.AssignOrQueue(passenger, building);
        Assert.Single(building.PendingPassengers);

        // Free up space in ev-01
        building.Elevators[0].Passengers.Clear();

        dispatcher.RetryPending(building);

        Assert.Empty(building.PendingPassengers);
    }

    [Fact]
    public void RetryPending_WhenNoPendingPassengers_DoesNothing()
    {
        var dispatcher = new Dispatcher();
        var building = CreateBuilding();
        var beforeStops = building.Elevators.Select(e => e.ScheduledStops.Count).ToArray();

        dispatcher.RetryPending(building);

        Assert.Equal(beforeStops, building.Elevators.Select(e => e.ScheduledStops.Count).ToArray());
        Assert.Empty(building.PendingPassengers);
    }

    [Fact]
    public void RetryPending_WhenStillNoCapacity_RequeuesPassenger()
    {
        var dispatcher = new Dispatcher();
        var building = CreateBuilding();

        foreach (var elevator in building.Elevators)
            for (var i = 0; i < elevator.Capacity; i++)
                elevator.Passengers.Add(new Passenger { Id = $"p{i}", OriginFloor = 1, DestinationFloor = 5 });

        var passenger = dispatcher.CreatePassenger(2, 4, 0);
        dispatcher.AssignOrQueue(passenger, building);
        Assert.Single(building.PendingPassengers);

        dispatcher.RetryPending(building);

        Assert.Single(building.PendingPassengers);
    }
}
