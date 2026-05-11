using ElevatorSimulation.Models;

namespace ElevatorSimulation.Services;

public class Dispatcher
{
    private int _passengerCounter;

    public string AssignOrQueue(Passenger passenger, Building building)
    {
        var best = FindBestElevator(passenger, building);
        if (best is not null)
        {
            Assign(passenger, best, building);
            return "assigned";
        }

        building.PendingPassengers.Add(passenger);
        return "queued";
    }

    public void RetryPending(Building building)
    {
        if (building.PendingPassengers.Count == 0) return;

        var toRetry = building.PendingPassengers.ToList();
        building.PendingPassengers.Clear();

        foreach (var p in toRetry)
        {
            var best = FindBestElevator(p, building);
            if (best is not null)
                Assign(p, best, building);
            else
                building.PendingPassengers.Add(p);
        }
    }

    public Passenger CreatePassenger(int originFloor, int destinationFloor, int currentTick)
    {
        _passengerCounter++;
        return new Passenger
        {
            Id = $"psg-{_passengerCounter:D4}",
            OriginFloor = originFloor,
            DestinationFloor = destinationFloor,
            RequestedTick = currentTick
        };
    }

    private static Elevator? FindBestElevator(Passenger passenger, Building building)
    {
        Elevator? best = null;
        var bestScore = int.MaxValue;

        foreach (var elevator in building.Elevators)
        {
            if (elevator.IsFull) continue;

            var distance = Math.Abs(elevator.CurrentFloor - passenger.OriginFloor);

            // Prefer idle elevators
            if (elevator.IsIdle)
            {
                if (distance < bestScore)
                {
                    best = elevator;
                    bestScore = distance;
                }
                continue;
            }

            // Accept same-direction elevators that haven't passed origin yet
            if (elevator.Direction == passenger.Direction)
            {
                var enRoute = elevator.Direction == "Up"
                    ? elevator.CurrentFloor <= passenger.OriginFloor
                    : elevator.CurrentFloor >= passenger.OriginFloor;

                if (enRoute && distance < bestScore)
                {
                    best = elevator;
                    bestScore = distance;
                }
            }
        }

        return best;
    }

    private static void Assign(Passenger passenger, Elevator elevator, Building building)
    {
        var wasIdle = elevator.IsIdle;
        var floor = passenger.OriginFloor;
        if (!building.WaitingPassengers.TryGetValue(floor, out var list))
        {
            list = [];
            building.WaitingPassengers[floor] = list;
        }
        list.Add(passenger);
        elevator.ScheduledStops.Add(floor);
        elevator.ScheduledStops.Add(passenger.DestinationFloor);

        if (wasIdle)
        {
            elevator.Direction = passenger.Direction;
        }
    }
}
