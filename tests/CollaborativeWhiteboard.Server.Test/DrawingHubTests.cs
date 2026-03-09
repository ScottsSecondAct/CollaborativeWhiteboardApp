using CollaborativeWhiteboard.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.Server.Test;

public class DrawingHubTests
{
    private static Mock<HubCallerContext> CreateMockContext(string connectionId = "test-conn-id")
    {
        var mock = new Mock<HubCallerContext>();
        mock.Setup(c => c.ConnectionId).Returns(connectionId);
        return mock;
    }

    private static DrawingHub CreateHub(
        out Mock<IClientProxy> mockOthers,
        ConcurrentDictionary<string, string>? userConnections = null,
        string connectionId = "test-conn-id")
    {
        mockOthers = new Mock<IClientProxy>();

        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.Others).Returns(mockOthers.Object);

        var hub = new DrawingHub(userConnections ?? new ConcurrentDictionary<string, string>());
        hub.Clients = mockClients.Object;
        hub.Context = CreateMockContext(connectionId).Object;
        return hub;
    }

    // ── BroadcastDrawAction ────────────────────────────────────────────────

    [Fact]
    public async Task BroadcastDrawAction_SendsToOthers()
    {
        var hub = CreateHub(out var mockOthers);

        await hub.BroadcastDrawAction("user1", "draw", "payload");

        mockOthers.Verify(
            c => c.SendCoreAsync("ReceiveDrawAction", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastDrawAction_ForwardsCorrectArguments()
    {
        var hub = CreateHub(out var mockOthers);

        await hub.BroadcastDrawAction("alice", "draw", "strokeData");

        mockOthers.Verify(
            c => c.SendCoreAsync(
                "ReceiveDrawAction",
                It.Is<object?[]>(args =>
                    args.Length == 3 &&
                    (string)args[0]! == "alice" &&
                    (string)args[1]! == "draw" &&
                    args[2]!.ToString() == "strokeData"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastDrawAction_DoesNotSendToCaller()
    {
        var mockCaller = new Mock<ISingleClientProxy>();
        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.Others).Returns(new Mock<IClientProxy>().Object);
        mockClients.Setup(c => c.Caller).Returns(mockCaller.Object);

        var hub = new DrawingHub(new ConcurrentDictionary<string, string>())
        {
            Clients = mockClients.Object,
            Context = CreateMockContext().Object
        };

        await hub.BroadcastDrawAction("user1", "draw", "data");

        mockCaller.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── BroadcastChatMessage — "all" (default) ────────────────────────────

    [Fact]
    public async Task BroadcastChatMessage_SendsToOthers()
    {
        var hub = CreateHub(out var mockOthers);

        await hub.BroadcastChatMessage("user1", "hello", "2026-03-09T12:00:00Z");

        mockOthers.Verify(
            c => c.SendCoreAsync("ReceiveChatMessage", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastChatMessage_ForwardsCorrectArguments()
    {
        var hub = CreateHub(out var mockOthers);

        await hub.BroadcastChatMessage("alice", "hello world", "2026-03-09T12:00:00Z");

        mockOthers.Verify(
            c => c.SendCoreAsync(
                "ReceiveChatMessage",
                It.Is<object?[]>(args =>
                    args.Length == 3 &&
                    (string)args[0]! == "alice" &&
                    (string)args[1]! == "hello world" &&
                    (string)args[2]! == "2026-03-09T12:00:00Z"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastChatMessage_DoesNotSendToCaller()
    {
        var mockCaller = new Mock<ISingleClientProxy>();
        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.Others).Returns(new Mock<IClientProxy>().Object);
        mockClients.Setup(c => c.Caller).Returns(mockCaller.Object);

        var hub = new DrawingHub(new ConcurrentDictionary<string, string>())
        {
            Clients = mockClients.Object,
            Context = CreateMockContext().Object
        };

        await hub.BroadcastChatMessage("user1", "hello", "2026-03-09T12:00:00Z");

        mockCaller.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── BroadcastChatMessage — "group" ────────────────────────────────────

    [Fact]
    public async Task BroadcastChatMessage_ToGroup_SendsToOthersInGroup()
    {
        var mockGroupProxy = new Mock<IClientProxy>();
        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.OthersInGroup("room1")).Returns(mockGroupProxy.Object);

        var hub = new DrawingHub(new ConcurrentDictionary<string, string>())
        {
            Clients = mockClients.Object,
            Context = CreateMockContext().Object
        };

        await hub.BroadcastChatMessage("user1", "hi room", "2026-03-09T12:00:00Z", "group", "room1");

        mockGroupProxy.Verify(
            c => c.SendCoreAsync("ReceiveChatMessage", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastChatMessage_ToGroup_ForwardsCorrectArguments()
    {
        var mockGroupProxy = new Mock<IClientProxy>();
        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.OthersInGroup("room1")).Returns(mockGroupProxy.Object);

        var hub = new DrawingHub(new ConcurrentDictionary<string, string>())
        {
            Clients = mockClients.Object,
            Context = CreateMockContext().Object
        };

        await hub.BroadcastChatMessage("alice", "hi room", "2026-03-09T12:00:00Z", "group", "room1");

        mockGroupProxy.Verify(
            c => c.SendCoreAsync(
                "ReceiveChatMessage",
                It.Is<object?[]>(args =>
                    (string)args[0]! == "alice" &&
                    (string)args[1]! == "hi room"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── BroadcastChatMessage — "client" ───────────────────────────────────

    [Fact]
    public async Task BroadcastChatMessage_ToClient_LooksUpConnectionIdAndSends()
    {
        var userConnections = new ConcurrentDictionary<string, string>();
        userConnections["bob"] = "bob-conn-id";

        var mockClientProxy = new Mock<ISingleClientProxy>();
        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.Client("bob-conn-id")).Returns(mockClientProxy.Object);

        var hub = new DrawingHub(userConnections)
        {
            Clients = mockClients.Object,
            Context = CreateMockContext().Object
        };

        await hub.BroadcastChatMessage("alice", "hi bob", "2026-03-09T12:00:00Z", "client", "bob");

        mockClientProxy.Verify(
            c => c.SendCoreAsync("ReceiveChatMessage", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastChatMessage_ToClient_FallsBackToRawIdWhenUserNotRegistered()
    {
        var mockClientProxy = new Mock<ISingleClientProxy>();
        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.Client("raw-conn-id")).Returns(mockClientProxy.Object);

        var hub = new DrawingHub(new ConcurrentDictionary<string, string>())
        {
            Clients = mockClients.Object,
            Context = CreateMockContext().Object
        };

        // "unknown-user" not in map → targetId used as raw connection ID
        await hub.BroadcastChatMessage("alice", "hello", "2026-03-09T12:00:00Z", "client", "raw-conn-id");

        mockClients.Verify(c => c.Client("raw-conn-id"), Times.Once);
    }

    // ── RegisterUser ──────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterUser_StoresConnectionId()
    {
        var userConnections = new ConcurrentDictionary<string, string>();
        var hub = CreateHub(out _, userConnections, connectionId: "conn-abc");

        await hub.RegisterUser("alice");

        Assert.True(userConnections.TryGetValue("alice", out var stored));
        Assert.Equal("conn-abc", stored);
    }

    // ── JoinGroup / LeaveGroup ────────────────────────────────────────────

    [Fact]
    public async Task JoinGroup_AddsCallerToGroup()
    {
        var mockGroups = new Mock<IGroupManager>();
        var hub = new DrawingHub(new ConcurrentDictionary<string, string>())
        {
            Clients = new Mock<IHubCallerClients>().Object,
            Groups = mockGroups.Object,
            Context = CreateMockContext("my-conn-id").Object
        };

        await hub.JoinGroup("room1");

        mockGroups.Verify(
            g => g.AddToGroupAsync("my-conn-id", "room1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LeaveGroup_RemovesCallerFromGroup()
    {
        var mockGroups = new Mock<IGroupManager>();
        var hub = new DrawingHub(new ConcurrentDictionary<string, string>())
        {
            Clients = new Mock<IHubCallerClients>().Object,
            Groups = mockGroups.Object,
            Context = CreateMockContext("my-conn-id").Object
        };

        await hub.LeaveGroup("room1");

        mockGroups.Verify(
            g => g.RemoveFromGroupAsync("my-conn-id", "room1", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
