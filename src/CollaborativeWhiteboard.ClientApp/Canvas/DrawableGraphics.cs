using CollaborativeWhiteboard.ClientApp.Models;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace CollaborativeWhiteboard.ClientApp.Canvas;

public class DrawableGraphics : IDrawable
{
    private readonly List<DrawStroke> _strokes;

    public DrawableGraphics(List<DrawStroke> strokes)
    {
        _strokes = strokes;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        foreach (var stroke in _strokes)
        {
            if (stroke.Points.Count < 2) continue;

            canvas.StrokeColor = stroke.Color;
            canvas.StrokeSize = stroke.StrokeSize;

            for (int i = 0; i < stroke.Points.Count - 1; i++)
            {
                canvas.DrawLine(stroke.Points[i], stroke.Points[i + 1]);
            }
        }
    }
}
