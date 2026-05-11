namespace ElevatorSimulation.Models;

public class Building
{
    public int FloorCount { get; init; } = 5;
    public List<Elevator> Elevators { get; init; } = [];
    public Dictionary<int, List<Passenger>> WaitingPassengers { get; init; } = [];
    public List<Passenger> PendingPassengers { get; init; } = [];
    public int Tick { get; set; }
    public bool Paused { get; set; }
    public bool IsFinished { get; set; }
    public string StatusMessage { get; set; } = "Running";
    public double TotalPassengerWaitTimeSeconds { get; set; }
    public int BoardedPassengerCount { get; set; }
    public double AveragePassengerWaitTimeSeconds { get; set; }
    public int WaitTimeUpdatedTick { get; set; }

    public int TotalPassengersMoved => Elevators.Sum(e => e.PassengersMoved);
    public int TotalWaiting => WaitingPassengers.Values.Sum(l => l.Count);
    public int TotalPending => PendingPassengers.Count;
}
