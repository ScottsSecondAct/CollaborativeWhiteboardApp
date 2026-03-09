using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.Core.Services;

public class HubConnectionWrapper : IHubConnectionWrapper
{
    private readonly HubConnection _connection;

    public HubConnectionWrapper(string hubUrl)
    {
        _connection = new HubConnectionBuilder().WithUrl(hubUrl).Build();
    }

    public HubConnectionState State => _connection.State;

    public Task StartAsync(CancellationToken cancellationToken = default)
        => _connection.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default)
        => _connection.StopAsync(cancellationToken);

    public Task InvokeAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        => _connection.InvokeCoreAsync(methodName, args, cancellationToken);

    public void RegisterHandler(string methodName, Action<string, string, object> handler)
        => _connection.On<string, string, object>(methodName, handler);
}
