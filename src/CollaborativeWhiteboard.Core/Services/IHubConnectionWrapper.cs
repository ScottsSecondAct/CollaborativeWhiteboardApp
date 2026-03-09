using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.Core.Services;

public interface IHubConnectionWrapper
{
    HubConnectionState State { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task InvokeAsync(string methodName, object?[] args, CancellationToken cancellationToken = default);
    void RegisterHandler(string methodName, Action<string, string, object> handler);
}
