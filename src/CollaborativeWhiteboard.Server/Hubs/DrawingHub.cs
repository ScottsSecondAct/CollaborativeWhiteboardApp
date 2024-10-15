// DrawingHub.cs
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.Server.Hubs;
public class DrawingHub : Hub
{
    // Broadcasts a drawing action to all connected clients
    public async Task BroadcastDrawAction(string userId, string actionType, object actionData)
    {
        await Clients.Others.SendAsync("ReceiveDrawAction", userId, actionType, actionData);
    }
}