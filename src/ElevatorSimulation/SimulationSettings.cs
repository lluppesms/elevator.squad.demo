namespace ElevatorSimulation;

public class SimulationSettings
{
    public double TickIntervalSeconds { get; set; } = 1.0;
    public double SpawnChance { get; set; } = 0.3;
    public int MaxTicks { get; set; } = 1000;
}
