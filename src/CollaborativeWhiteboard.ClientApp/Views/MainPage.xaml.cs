using CollaborativeWhiteboard.ClientApp.Canvas;
using CollaborativeWhiteboard.ClientApp.Models;
using CollaborativeWhiteboard.ClientApp.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Text.Json;

namespace CollaborativeWhiteboard.ClientApp.Views;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private readonly List<DrawStroke> _strokes = new();
    private DrawStroke? _activeStroke;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;

        DrawingCanvas.Drawable = new DrawableGraphics(_strokes);
        DrawingCanvas.StartInteraction += OnStartInteraction;
        DrawingCanvas.DragInteraction += OnDragInteraction;
        DrawingCanvas.EndInteraction += OnEndInteraction;

        viewModel.DrawActionReceived += OnRemoteDrawActionReceived;
    }

    private void OnStartInteraction(object sender, TouchEventArgs e)
    {
        _activeStroke = new DrawStroke
        {
            UserId = _viewModel.UserId,
            Color = _viewModel.SelectedColor,
            StrokeSize = _viewModel.StrokeSize,
        };
        _activeStroke.Points.Add(e.Touches[0]);
        _strokes.Add(_activeStroke);
        DrawingCanvas.Invalidate();
    }

    private void OnDragInteraction(object sender, TouchEventArgs e)
    {
        _activeStroke?.Points.Add(e.Touches[0]);
        DrawingCanvas.Invalidate();
    }

    private async void OnEndInteraction(object sender, TouchEventArgs e)
    {
        if (_activeStroke is null) return;

        _activeStroke.Points.Add(e.Touches[0]);
        var completedStroke = _activeStroke;
        _activeStroke = null;

        DrawingCanvas.Invalidate();
        await _viewModel.SendStrokeAsync(completedStroke);
    }

    private void OnRemoteDrawActionReceived(string userId, string actionType, object actionData)
    {
        if (actionType != "draw" || actionData is not string json) return;

        var dto = JsonSerializer.Deserialize<DrawStrokeDto>(json);
        if (dto is null) return;

        var stroke = new DrawStroke
        {
            UserId = dto.UserId,
            Color = Color.FromArgb(dto.ColorHex),
            StrokeSize = dto.StrokeSize,
            Points = dto.Points.ConvertAll(p => new PointF(p.X, p.Y))
        };

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _strokes.Add(stroke);
            DrawingCanvas.Invalidate();
        });
    }
}
