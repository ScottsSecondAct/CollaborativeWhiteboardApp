// MessagingService.cs
using System;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.Core.Services;

public class MessagingService
{
    private IHubConnectionWrapper? _connection;

    // userId, text, sentAt (ISO 8601 string)
    public event Action<string, string, DateTimeOffset>? OnChatMessageReceived;

    // Called once, after DrawingService.InitializeAsync, passing the shared connection.
    public void Attach(IHubConnectionWrapper connection)
    {
        _connection = connection;

        _connection.RegisterHandler("ReceiveChatMessage", (userId, text, sentAtObj) =>
        {
            var sentAt = DateTimeOffset.TryParse(sentAtObj?.ToString(), out var parsed)
                ? parsed
                : DateTimeOffset.UtcNow;

            OnChatMessageReceived?.Invoke(userId, text, sentAt);
        });
    }

    public async Task SendChatMessageAsync(
        string userId, string text,
        string targetType = "all", string? targetId = null)
    {
        if (_connection is null)
            throw new InvalidOperationException("MessagingService is not attached to a connection. Call Attach first.");

        var sentAt = DateTimeOffset.UtcNow.ToString("o");
        await _connection.InvokeAsync(
            "BroadcastChatMessage",
            new object?[] { userId, text, sentAt, targetType, targetId });
    }
}
