using System.Collections.Generic;

namespace CollaborativeWhiteboard.ClientApp.Models;

/// <summary>
/// JSON-serializable wire format for a DrawStroke.
/// Color is stored as an ARGB hex string (e.g. "#FF000000").
/// </summary>
public record DrawStrokeDto(
    string UserId,
    string ColorHex,
    float StrokeSize,
    List<PointDto> Points);

public record PointDto(float X, float Y);
