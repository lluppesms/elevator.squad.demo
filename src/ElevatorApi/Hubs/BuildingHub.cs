using ElevatorSimulation.Models;
using Microsoft.AspNetCore.SignalR;

namespace ElevatorApi.Hubs;

public class BuildingHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task BroadcastState(IHubContext<BuildingHub> hubContext, Building snapshot)
    {
        await hubContext.Clients.All.SendAsync("BuildingState", snapshot);
    }
}
