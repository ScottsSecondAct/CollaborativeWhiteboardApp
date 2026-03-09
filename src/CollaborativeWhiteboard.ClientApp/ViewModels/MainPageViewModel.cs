using CollaborativeWhiteboard.ClientApp.Models;
using CollaborativeWhiteboard.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CollaborativeWhiteboard.ClientApp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly DrawingService _drawingService;
    private readonly MessagingService _messagingService;

    public string UserId { get; } = Guid.NewGuid().ToString("N")[..8];

    [ObservableProperty]
    string _hubUrl = "http://localhost:5000/drawingHub";

    [ObservableProperty]
    bool _isConnected;

    [ObservableProperty]
    Color _selectedColor = Colors.Black;

    [ObservableProperty]
    float _strokeSize = 4f;

    [ObservableProperty]
    string _messageText = string.Empty;

    public ObservableCollection<ChatMessageViewModel> Messages { get; } = new();

    public event Action<string, string, object>? DrawActionReceived;

    public MainPageViewModel(DrawingService drawingService, MessagingService messagingService)
    {
        _drawingService = drawingService;
        _messagingService = messagingService;

        _drawingService.OnDrawActionReceived += (userId, actionType, actionData) =>
            DrawActionReceived?.Invoke(userId, actionType, actionData);
    }

    [RelayCommand]
    async Task Connect()
    {
        if (IsConnected) return;

        await _drawingService.InitializeAsync(HubUrl);
        IsConnected = _drawingService.IsConnected;

        if (IsConnected)
        {
            _messagingService.Attach(_drawingService.Connection!);
            _messagingService.OnChatMessageReceived += OnChatMessageReceived;
        }
    }

    [RelayCommand]
    async Task Disconnect()
    {
        if (!IsConnected) return;

        _messagingService.OnChatMessageReceived -= OnChatMessageReceived;
        await _drawingService.StopAsync();
        IsConnected = false;
    }

    [RelayCommand]
    void SelectColor(string colorHex) => SelectedColor = Color.FromArgb(colorHex);

    [RelayCommand]
    void SelectSize(string size) => StrokeSize = size switch
    {
        "S" => 2f,
        "L" => 10f,
        _   => 4f,
    };

    [RelayCommand]
    async Task SendMessage()
    {
        var text = MessageText.Trim();
        if (string.IsNullOrEmpty(text) || !IsConnected) return;

        MessageText = string.Empty;

        Messages.Add(new ChatMessageViewModel
        {
            UserId = UserId,
            Text = text,
            SentAt = DateTimeOffset.UtcNow,
            IsOwnMessage = true,
        });

        await _messagingService.SendChatMessageAsync(UserId, text);
    }

    public async Task SendStrokeAsync(DrawStroke stroke)
    {
        if (!IsConnected) return;

        var c = stroke.Color;
        var colorHex = $"#{(int)(c.Alpha * 255):X2}{(int)(c.Red * 255):X2}{(int)(c.Green * 255):X2}{(int)(c.Blue * 255):X2}";

        var dto = new DrawStrokeDto(
            UserId: UserId,
            ColorHex: colorHex,
            StrokeSize: stroke.StrokeSize,
            Points: stroke.Points.Select(p => new PointDto(p.X, p.Y)).ToList());

        var json = JsonSerializer.Serialize(dto);
        await _drawingService.SendDrawAction(UserId, "draw", json);
    }

    private void OnChatMessageReceived(string userId, string text, DateTimeOffset sentAt)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Messages.Add(new ChatMessageViewModel
            {
                UserId = userId,
                Text = text,
                SentAt = sentAt,
                IsOwnMessage = false,
            });
        });
    }
}
