using ElevatorSimulation.Models;

namespace ElevatorTests;

public class ModelBehaviorTests
{
    [Fact]
    public void BuildingTotals_AggregateFromCollections()
    {
        var building = new Building
        {
            Elevators =
            [
                new Elevator { PassengersMoved = 2 },
                new Elevator { PassengersMoved = 3 }
            ],
            WaitingPassengers =
            {
                [1] = [new Passenger()],
                [2] = [new Passenger(), new Passenger()]
            },
            PendingPassengers = [new Passenger(), new Passenger(), new Passenger()]
        };

        Assert.Equal(5, building.TotalPassengersMoved);
        Assert.Equal(3, building.TotalWaiting);
        Assert.Equal(3, building.TotalPending);
    }

    [Fact]
    public void ElevatorStateProperties_ReflectCapacityAndSchedule()
    {
        var elevator = new Elevator
        {
            Direction = "Idle",
            Capacity = 2
        };

        Assert.True(elevator.IsIdle);
        Assert.False(elevator.IsFull);

        elevator.Passengers.Add(new Passenger());
        elevator.Passengers.Add(new Passenger());
        elevator.ScheduledStops.Add(3);

        Assert.True(elevator.IsFull);
        Assert.False(elevator.IsIdle);
    }
}
