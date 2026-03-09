using CollaborativeWhiteboard.Core.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.ClientApp.Test;

public class DrawingServiceTests
{
    private static (DrawingService service, Mock<IHubConnectionWrapper> mockConnection) CreateService(
        HubConnectionState initialState = HubConnectionState.Connected)
    {
        var mockConnection = new Mock<IHubConnectionWrapper>();
        mockConnection.Setup(c => c.State).Returns(initialState);
        mockConnection.Setup(c => c.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockConnection.Setup(c => c.StopAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockConnection.Setup(c => c.InvokeAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var service = new DrawingService(_ => mockConnection.Object);
        return (service, mockConnection);
    }

    [Fact]
    public async Task InitializeAsync_StartsConnection()
    {
        var (service, mockConnection) = CreateService();

        await service.InitializeAsync("http://localhost/hub");

        mockConnection.Verify(c => c.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_RegistersReceiveDrawActionHandler()
    {
        var (service, mockConnection) = CreateService();

        await service.InitializeAsync("http://localhost/hub");

        mockConnection.Verify(c => c.RegisterHandler("ReceiveDrawAction", It.IsAny<Action<string, string, object>>()), Times.Once);
    }

    [Fact]
    public async Task OnDrawActionReceived_FiresWhenHandlerInvoked()
    {
        Action<string, string, object>? capturedHandler = null;
        var mockConnection = new Mock<IHubConnectionWrapper>();
        mockConnection.Setup(c => c.State).Returns(HubConnectionState.Connected);
        mockConnection.Setup(c => c.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockConnection.Setup(c => c.RegisterHandler("ReceiveDrawAction", It.IsAny<Action<string, string, object>>()))
                      .Callback<string, Action<string, string, object>>((_, h) => capturedHandler = h);

        var service = new DrawingService(_ => mockConnection.Object);
        await service.InitializeAsync("http://localhost/hub");

        string? receivedUser = null, receivedType = null;
        object? receivedData = null;
        service.OnDrawActionReceived += (u, t, d) => { receivedUser = u; receivedType = t; receivedData = d; };

        capturedHandler!("alice", "draw", "strokeJson");

        Assert.Equal("alice", receivedUser);
        Assert.Equal("draw", receivedType);
        Assert.Equal("strokeJson", receivedData);
    }

    [Fact]
    public async Task SendDrawAction_InvokesHubWhenConnected()
    {
        var (service, mockConnection) = CreateService(HubConnectionState.Connected);
        await service.InitializeAsync("http://localhost/hub");

        await service.SendDrawAction("user1", "draw", "data");

        mockConnection.Verify(
            c => c.InvokeAsync(
                "BroadcastDrawAction",
                It.Is<object?[]>(args =>
                    args.Length == 3 &&
                    (string)args[0]! == "user1" &&
                    (string)args[1]! == "draw" &&
                    args[2]!.ToString() == "data"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendDrawAction_DoesNotInvokeWhenDisconnected()
    {
        var (service, mockConnection) = CreateService(HubConnectionState.Disconnected);
        await service.InitializeAsync("http://localhost/hub");

        await service.SendDrawAction("user1", "draw", "data");

        mockConnection.Verify(
            c => c.InvokeAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StopAsync_StopsConnectionWhenConnected()
    {
        var (service, mockConnection) = CreateService(HubConnectionState.Connected);
        await service.InitializeAsync("http://localhost/hub");

        await service.StopAsync();

        mockConnection.Verify(c => c.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StopAsync_DoesNothingWhenAlreadyDisconnected()
    {
        var (service, mockConnection) = CreateService(HubConnectionState.Disconnected);
        await service.InitializeAsync("http://localhost/hub");

        await service.StopAsync();

        mockConnection.Verify(c => c.StopAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
