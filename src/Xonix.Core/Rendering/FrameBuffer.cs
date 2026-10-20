namespace Xonix.Core.Rendering;

/// <summary>
/// RGBA pixel surface; each <see cref="uint"/> is laid out as R, G, B, A bytes on little-endian hosts,
/// which matches the browser <c>ImageData</c> layout.
/// </summary>
/// <param name="width">Width in pixels.</param>
/// <param name="height">Height in pixels.</param>
public sealed class FrameBuffer(int width, int height)
{
    /// <summary>Gets the width in pixels.</summary>
    public int Width { get; } = width;

    /// <summary>Gets the height in pixels.</summary>
    public int Height { get; } = height;

    /// <summary>Gets the pixels, row-major.</summary>
    public uint[] Pixels { get; } = new uint[width * height];

    /// <summary>
    /// Packs a color.
    /// </summary>
    /// <param name="r">Red, 0-255.</param>
    /// <param name="g">Green, 0-255.</param>
    /// <param name="b">Blue, 0-255.</param>
    /// <returns>An opaque color.</returns>
    public static uint Rgb(byte r, byte g, byte b) => 0xFF000000u | (uint)b << 16 | (uint)g << 8 | r;

    /// <summary>
    /// Fills the whole surface.
    /// </summary>
    /// <param name="color">Fill color.</param>
    public void Clear(uint color) => Array.Fill(Pixels, color);

    /// <summary>
    /// Writes one pixel; coordinates outside the surface are ignored.
    /// </summary>
    /// <param name="x">Column.</param>
    /// <param name="y">Row.</param>
    /// <param name="color">Pixel color.</param>
    public void Set(int x, int y, uint color)
    {
        if ((uint)x < (uint)Width && (uint)y < (uint)Height)
        {
            Pixels[y * Width + x] = color;
        }
    }

    /// <summary>
    /// Fills a rectangle, clipped to the surface.
    /// </summary>
    /// <param name="x">Left column.</param>
    /// <param name="y">Top row.</param>
    /// <param name="width">Rectangle width.</param>
    /// <param name="height">Rectangle height.</param>
    /// <param name="color">Fill color.</param>
    public void FillRect(int x, int y, int width, int height, uint color)
    {
        int left = Math.Max(0, x);
        int right = Math.Min(Width, x + width);
        for (int row = Math.Max(0, y); row < Math.Min(Height, y + height); row++)
        {
            if (right > left)
            {
                Array.Fill(Pixels, color, row * Width + left, right - left);
            }
        }
    }
}
