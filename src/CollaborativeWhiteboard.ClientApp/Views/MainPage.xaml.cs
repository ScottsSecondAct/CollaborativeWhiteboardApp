using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace CollaborativeWhiteboard.ClientApp.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly List<PointF> _points = new();

        public MainPage()
        {
            InitializeComponent();

            // Set drawable and event handlers
            DrawingCanvas.Drawable = new DrawableGraphics(_points);
            DrawingCanvas.StartInteraction += OnStartInteraction;
            DrawingCanvas.DragInteraction += OnDragInteraction;
            DrawingCanvas.EndInteraction += OnEndInteraction;
        }

        private void OnStartInteraction(object sender, TouchEventArgs e)
        {
            _points.Clear();
            _points.Add(e.Touches[0]);
            DrawingCanvas.Invalidate();
        }

        private void OnDragInteraction(object sender, TouchEventArgs e)
        {
            _points.Add(e.Touches[0]);
            DrawingCanvas.Invalidate();
        }

        private void OnEndInteraction(object sender, TouchEventArgs e)
        {
            _points.Add(e.Touches[0]);
            DrawingCanvas.Invalidate();
        }
    }

    public class DrawableGraphics : IDrawable
    {
        private readonly List<PointF> _points;

        public DrawableGraphics(List<PointF> points)
        {
            _points = points;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;

            for (int i = 0; i < _points.Count - 1; i++)
            {
                canvas.DrawLine(_points[i], _points[i + 1]);
            }
        }
    }
}