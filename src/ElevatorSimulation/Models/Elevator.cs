namespace ElevatorSimulation.Models;

public class Elevator
{
    public string Id { get; init; } = string.Empty;
    public int CurrentFloor { get; set; }
    public string Direction { get; set; } = "Idle";
    public string DoorState { get; set; } = "Closed";
    public int Capacity { get; init; } = 8;
    public List<Passenger> Passengers { get; } = [];
    public HashSet<int> ScheduledStops { get; } = [];
    public int DoorTicksRemaining { get; set; }
    public int PassengersMoved { get; set; }

    public bool IsFull => Passengers.Count >= Capacity;
    public bool IsIdle => Direction == "Idle" && ScheduledStops.Count == 0;
}
