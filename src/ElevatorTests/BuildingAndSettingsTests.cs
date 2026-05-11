using ElevatorSimulation;
using ElevatorSimulation.Models;

namespace ElevatorTests;

public class BuildingAndSettingsTests
{
    [Fact]
    public void Building_Totals_AreCalculatedFromState()
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
                [1] = [new Passenger(), new Passenger()],
                [2] = [new Passenger()]
            },
            PendingPassengers = [new Passenger(), new Passenger(), new Passenger()]
        };

        Assert.Equal(5, building.TotalPassengersMoved);
        Assert.Equal(3, building.TotalWaiting);
        Assert.Equal(3, building.TotalPending);
    }

    [Fact]
    public void SimulationSettings_Defaults_AreStable()
    {
        var settings = new SimulationSettings();

        Assert.Equal(1.0, settings.TickIntervalSeconds);
        Assert.Equal(0.3, settings.SpawnChance);
        Assert.Equal(1000, settings.MaxTicks);
    }
}
