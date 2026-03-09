// DrawingService.cs
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.Core.Services;

public class DrawingService
{
    private readonly Func<string, IHubConnectionWrapper> _connectionFactory;
    private IHubConnectionWrapper? _connection;

    public DrawingService()
        : this(url => new HubConnectionWrapper(url)) { }

    internal DrawingService(Func<string, IHubConnectionWrapper> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    internal IHubConnectionWrapper? Connection => _connection;

    public event Action<string, string, object>? OnDrawActionReceived;

    public async Task InitializeAsync(string hubUrl)
    {
        _connection = _connectionFactory(hubUrl);

        _connection.RegisterHandler("ReceiveDrawAction", (userId, actionType, actionData) =>
            OnDrawActionReceived?.Invoke(userId, actionType, actionData));

        await _connection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_connection != null && _connection.State != HubConnectionState.Disconnected)
            await _connection.StopAsync();
    }

    public async Task SendDrawAction(string userId, string actionType, object actionData)
    {
        if (_connection?.State == HubConnectionState.Connected)
            await _connection.InvokeAsync("BroadcastDrawAction", new object?[] { userId, actionType, actionData });
    }
}
