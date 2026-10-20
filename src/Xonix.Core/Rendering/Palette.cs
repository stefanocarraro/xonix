namespace Xonix.Core.Rendering;

/// <summary>
/// Fixed colors of the game, taken from the CGA palette of the DOS original.
/// </summary>
public static class Palette
{
    /// <summary>Screen background and open sea.</summary>
    public static readonly uint Sea = FrameBuffer.Rgb(0, 0, 0);

    /// <summary>Claimed land.</summary>
    public static readonly uint Land = FrameBuffer.Rgb(0, 170, 170);

    /// <summary>Trail the player leaves at sea.</summary>
    public static readonly uint Trail = FrameBuffer.Rgb(170, 0, 170);

    /// <summary>Player, balls and text.</summary>
    public static readonly uint White = FrameBuffer.Rgb(255, 255, 255);

    /// <summary>Player core.</summary>
    public static readonly uint Red = FrameBuffer.Rgb(255, 85, 85);

    /// <summary>Land eaters.</summary>
    public static readonly uint Blue = FrameBuffer.Rgb(0, 0, 170);

    /// <summary>Highlights.</summary>
    public static readonly uint Yellow = FrameBuffer.Rgb(255, 255, 85);
}
