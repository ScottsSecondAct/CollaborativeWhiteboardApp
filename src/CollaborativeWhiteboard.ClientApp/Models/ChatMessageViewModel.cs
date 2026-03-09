using System;

namespace CollaborativeWhiteboard.ClientApp.Models;

public class ChatMessageViewModel
{
    public string UserId { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public DateTimeOffset SentAt { get; init; }
    public bool IsOwnMessage { get; init; }
    public string TimeDisplay => SentAt.ToLocalTime().ToString("HH:mm");
}
