namespace ElevatorSimulation.Models;

public class Passenger
{
    public string Id { get; init; } = string.Empty;
    public int OriginFloor { get; init; }
    public int DestinationFloor { get; init; }
    public int RequestedTick { get; init; }

    public string Direction => DestinationFloor > OriginFloor ? "Up" : "Down";
}
