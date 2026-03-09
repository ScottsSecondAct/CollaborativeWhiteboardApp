using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace CollaborativeWhiteboard.ClientApp.Models;

public class DrawStroke
{
    public string UserId { get; set; } = string.Empty;
    public Color Color { get; set; } = Colors.Black;
    public float StrokeSize { get; set; } = 2f;
    public List<PointF> Points { get; set; } = new();
}
