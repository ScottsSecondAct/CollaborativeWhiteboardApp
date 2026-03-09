// DrawingHub.cs
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.Server.Hubs;

public class DrawingHub : Hub
{
    private readonly ConcurrentDictionary<string, string> _userConnections;

    public DrawingHub(ConcurrentDictionary<string, string> userConnections)
    {
        _userConnections = userConnections;
    }

    // Registers the caller's userId → connectionId so they can receive direct messages
    public Task RegisterUser(string userId)
    {
        _userConnections[userId] = Context.ConnectionId;
        return Task.CompletedTask;
    }

    // Group management
    public Task JoinGroup(string groupName)
        => Groups.AddToGroupAsync(Context.ConnectionId, groupName);

    public Task LeaveGroup(string groupName)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

    // Broadcasts a drawing action to all other connected clients
    public async Task BroadcastDrawAction(string userId, string actionType, object actionData)
    {
        await Clients.Others.SendAsync("ReceiveDrawAction", userId, actionType, actionData);
    }

    // Broadcasts a chat message.
    // targetType: "all" (default) | "group" | "client"
    // targetId:   group name (for "group") or recipient userId (for "client"); ignored for "all"
    public async Task BroadcastChatMessage(
        string userId, string text, string sentAt,
        string targetType = "all", string? targetId = null)
    {
        IClientProxy proxy = targetType switch
        {
            "group"  => Clients.OthersInGroup(targetId!),
            "client" => Clients.Client(
                            _userConnections.TryGetValue(targetId!, out var connId)
                                ? connId
                                : targetId!),
            _        => Clients.Others
        };

        await proxy.SendAsync("ReceiveChatMessage", userId, text, sentAt);
    }
}
