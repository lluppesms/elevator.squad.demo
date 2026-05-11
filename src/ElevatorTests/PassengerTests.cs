using ElevatorSimulation.Models;
using Xunit;

namespace ElevatorTests;

public class PassengerTests
{
    [Theory]
    [InlineData(1, 3, "Up")]
    [InlineData(5, 2, "Down")]
    [InlineData(2, 5, "Up")]
    [InlineData(4, 1, "Down")]
    public void Direction_IsCorrectlyDerived(int origin, int dest, string expected)
    {
        var passenger = new Passenger
        {
            Id = "psg-0001",
            OriginFloor = origin,
            DestinationFloor = dest,
            RequestedTick = 0
        };

        Assert.Equal(expected, passenger.Direction);
    }

    [Fact]
    public void Passenger_IdFormat_MatchesConvention()
    {
        var passenger = new Passenger
        {
            Id = "psg-0042",
            OriginFloor = 1,
            DestinationFloor = 5,
            RequestedTick = 10
        };

        Assert.StartsWith("psg-", passenger.Id);
    }
}
