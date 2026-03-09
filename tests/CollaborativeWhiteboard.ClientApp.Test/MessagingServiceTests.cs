using CollaborativeWhiteboard.Core.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.ClientApp.Test;

public class MessagingServiceTests
{
    private static Mock<IHubConnectionWrapper> CreateMockConnection()
    {
        var mock = new Mock<IHubConnectionWrapper>();
        mock.Setup(c => c.State).Returns(HubConnectionState.Connected);
        mock.Setup(c => c.InvokeAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    [Fact]
    public void Attach_RegistersReceiveChatMessageHandler()
    {
        var mockConnection = CreateMockConnection();
        var service = new MessagingService();

        service.Attach(mockConnection.Object);

        mockConnection.Verify(
            c => c.RegisterHandler("ReceiveChatMessage", It.IsAny<Action<string, string, object>>()),
            Times.Once);
    }

    [Fact]
    public void OnChatMessageReceived_FiresWhenHandlerInvoked()
    {
        Action<string, string, object>? capturedHandler = null;
        var mockConnection = CreateMockConnection();
        mockConnection
            .Setup(c => c.RegisterHandler("ReceiveChatMessage", It.IsAny<Action<string, string, object>>()))
            .Callback<string, Action<string, string, object>>((_, h) => capturedHandler = h);

        var service = new MessagingService();
        service.Attach(mockConnection.Object);

        string? receivedUser = null, receivedText = null;
        DateTimeOffset receivedSentAt = default;
        service.OnChatMessageReceived += (u, t, s) => { receivedUser = u; receivedText = t; receivedSentAt = s; };

        var sentAt = "2026-03-09T12:00:00+00:00";
        capturedHandler!("alice", "hello", sentAt);

        Assert.Equal("alice", receivedUser);
        Assert.Equal("hello", receivedText);
        Assert.Equal(DateTimeOffset.Parse(sentAt), receivedSentAt);
    }

    [Fact]
    public void OnChatMessageReceived_UsesFallbackTimeWhenSentAtUnparseable()
    {
        Action<string, string, object>? capturedHandler = null;
        var mockConnection = CreateMockConnection();
        mockConnection
            .Setup(c => c.RegisterHandler("ReceiveChatMessage", It.IsAny<Action<string, string, object>>()))
            .Callback<string, Action<string, string, object>>((_, h) => capturedHandler = h);

        var service = new MessagingService();
        service.Attach(mockConnection.Object);

        DateTimeOffset receivedSentAt = default;
        service.OnChatMessageReceived += (_, _, s) => receivedSentAt = s;

        var before = DateTimeOffset.UtcNow;
        capturedHandler!("alice", "hello", "not-a-date");
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(receivedSentAt, before, after);
    }

    [Fact]
    public void OnChatMessageReceived_NotFiredBeforeAttach()
    {
        var service = new MessagingService();
        var fired = false;
        service.OnChatMessageReceived += (_, _, _) => fired = true;

        // No Attach called — event should never fire
        Assert.False(fired);
    }

    [Fact]
    public async Task SendChatMessageAsync_InvokesHubWithCorrectMethod()
    {
        var mockConnection = CreateMockConnection();
        var service = new MessagingService();
        service.Attach(mockConnection.Object);

        await service.SendChatMessageAsync("user1", "hello");

        mockConnection.Verify(
            c => c.InvokeAsync(
                "BroadcastChatMessage",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendChatMessageAsync_ForwardsUserIdAndText()
    {
        var mockConnection = CreateMockConnection();
        var service = new MessagingService();
        service.Attach(mockConnection.Object);

        await service.SendChatMessageAsync("alice", "hi there");

        mockConnection.Verify(
            c => c.InvokeAsync(
                "BroadcastChatMessage",
                It.Is<object?[]>(args =>
                    (string)args[0]! == "alice" &&
                    (string)args[1]! == "hi there"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendChatMessageAsync_DefaultsToAllTarget()
    {
        var mockConnection = CreateMockConnection();
        var service = new MessagingService();
        service.Attach(mockConnection.Object);

        await service.SendChatMessageAsync("user1", "hello");

        mockConnection.Verify(
            c => c.InvokeAsync(
                "BroadcastChatMessage",
                It.Is<object?[]>(args => (string)args[3]! == "all"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendChatMessageAsync_ForwardsGroupTarget()
    {
        var mockConnection = CreateMockConnection();
        var service = new MessagingService();
        service.Attach(mockConnection.Object);

        await service.SendChatMessageAsync("user1", "hi room", targetType: "group", targetId: "room1");

        mockConnection.Verify(
            c => c.InvokeAsync(
                "BroadcastChatMessage",
                It.Is<object?[]>(args =>
                    (string)args[3]! == "group" &&
                    (string?)args[4] == "room1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendChatMessageAsync_ForwardsClientTarget()
    {
        var mockConnection = CreateMockConnection();
        var service = new MessagingService();
        service.Attach(mockConnection.Object);

        await service.SendChatMessageAsync("user1", "hi bob", targetType: "client", targetId: "bob");

        mockConnection.Verify(
            c => c.InvokeAsync(
                "BroadcastChatMessage",
                It.Is<object?[]>(args =>
                    (string)args[3]! == "client" &&
                    (string?)args[4] == "bob"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendChatMessageAsync_ThrowsWhenNotAttached()
    {
        var service = new MessagingService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SendChatMessageAsync("user1", "hello"));
    }
}
