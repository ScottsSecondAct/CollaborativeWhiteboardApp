using CollaborativeWhiteboard.ClientApp.Canvas;
using CollaborativeWhiteboard.ClientApp.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace CollaborativeWhiteboard.ClientApp.Views;

public partial class MainPage : ContentPage
{
    private readonly List<DrawStroke> _strokes = new();
    private DrawStroke? _activeStroke;

    public MainPage()
    {
        InitializeComponent();

        DrawingCanvas.Drawable = new DrawableGraphics(_strokes);
        DrawingCanvas.StartInteraction += OnStartInteraction;
        DrawingCanvas.DragInteraction += OnDragInteraction;
        DrawingCanvas.EndInteraction += OnEndInteraction;
    }

    private void OnStartInteraction(object sender, TouchEventArgs e)
    {
        _activeStroke = new DrawStroke();
        _activeStroke.Points.Add(e.Touches[0]);
        _strokes.Add(_activeStroke);
        DrawingCanvas.Invalidate();
    }

    private void OnDragInteraction(object sender, TouchEventArgs e)
    {
        _activeStroke?.Points.Add(e.Touches[0]);
        DrawingCanvas.Invalidate();
    }

    private void OnEndInteraction(object sender, TouchEventArgs e)
    {
        _activeStroke?.Points.Add(e.Touches[0]);
        _activeStroke = null;
        DrawingCanvas.Invalidate();
    }
}
