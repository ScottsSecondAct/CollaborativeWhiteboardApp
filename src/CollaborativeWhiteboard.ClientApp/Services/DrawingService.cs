// DrawingService.cs
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.ClientApp.Services;
public class DrawingService
{
    private HubConnection _hubConnection;

    public event Action<string, string, object> OnDrawActionReceived;

    public async Task InitializeAsync(string hubUrl)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .Build();

        _hubConnection.On<string, string, object>("ReceiveDrawAction", (userId, actionType, actionData) =>
        {
            OnDrawActionReceived?.Invoke(userId, actionType, actionData);
        });

        await _hubConnection.StartAsync();
    }

    public async Task SendDrawAction(string userId, string actionType, object actionData)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("BroadcastDrawAction", userId, actionType, actionData);
        }
    }
}