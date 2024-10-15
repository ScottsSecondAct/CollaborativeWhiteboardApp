using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace CollaborativeWhiteboard.ClientApp.Canvas;
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

        // Draw lines between each point in the points list
        for (int i = 0; i < _points.Count - 1; i++)
        {
            canvas.DrawLine(_points[i], _points[i + 1]);
        }
    }
}
