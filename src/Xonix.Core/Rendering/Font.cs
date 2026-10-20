namespace Xonix.Core.Rendering;

/// <summary>
/// 5x7 bitmap font with arcade-style capitals, digits and the symbols the HUD needs.
/// </summary>
public static class Font
{
    /// <summary>Horizontal advance of one character at scale 1.</summary>
    public const int Advance = 6;

    /// <summary>Glyph height at scale 1.</summary>
    public const int Height = 7;

    private const int _glyphWidth = 5;

    private static readonly Dictionary<char, string> _glyphs = new()
    {
        ['0'] = "01110 10001 10011 10101 11001 10001 01110",
        ['1'] = "00100 01100 00100 00100 00100 00100 01110",
        ['2'] = "01110 10001 00001 00010 00100 01000 11111",
        ['3'] = "11111 00010 00100 00010 00001 10001 01110",
        ['4'] = "00010 00110 01010 10010 11111 00010 00010",
        ['5'] = "11111 10000 11110 00001 00001 10001 01110",
        ['6'] = "00110 01000 10000 11110 10001 10001 01110",
        ['7'] = "11111 00001 00010 00100 01000 01000 01000",
        ['8'] = "01110 10001 10001 01110 10001 10001 01110",
        ['9'] = "01110 10001 10001 01111 00001 00010 01100",
        ['A'] = "01110 10001 10001 11111 10001 10001 10001",
        ['B'] = "11110 10001 10001 11110 10001 10001 11110",
        ['C'] = "01110 10001 10000 10000 10000 10001 01110",
        ['D'] = "11100 10010 10001 10001 10001 10010 11100",
        ['E'] = "11111 10000 10000 11110 10000 10000 11111",
        ['F'] = "11111 10000 10000 11110 10000 10000 10000",
        ['G'] = "01110 10001 10000 10111 10001 10001 01111",
        ['H'] = "10001 10001 10001 11111 10001 10001 10001",
        ['I'] = "01110 00100 00100 00100 00100 00100 01110",
        ['J'] = "00111 00010 00010 00010 00010 10010 01100",
        ['K'] = "10001 10010 10100 11000 10100 10010 10001",
        ['L'] = "10000 10000 10000 10000 10000 10000 11111",
        ['M'] = "10001 11011 10101 10101 10001 10001 10001",
        ['N'] = "10001 10001 11001 10101 10011 10001 10001",
        ['O'] = "01110 10001 10001 10001 10001 10001 01110",
        ['P'] = "11110 10001 10001 11110 10000 10000 10000",
        ['Q'] = "01110 10001 10001 10001 10101 10010 01101",
        ['R'] = "11110 10001 10001 11110 10100 10010 10001",
        ['S'] = "01111 10000 10000 01110 00001 00001 11110",
        ['T'] = "11111 00100 00100 00100 00100 00100 00100",
        ['U'] = "10001 10001 10001 10001 10001 10001 01110",
        ['V'] = "10001 10001 10001 10001 10001 01010 00100",
        ['W'] = "10001 10001 10001 10101 10101 10101 01010",
        ['X'] = "10001 10001 01010 00100 01010 10001 10001",
        ['Y'] = "10001 10001 10001 01010 00100 00100 00100",
        ['Z'] = "11111 00001 00010 00100 01000 10000 11111",
        [' '] = "00000 00000 00000 00000 00000 00000 00000",
        ['!'] = "00100 00100 00100 00100 00100 00000 00100",
        ['?'] = "01110 10001 00001 00010 00100 00000 00100",
        ['@'] = "01110 10001 10111 10101 10111 10000 01110",
        ['#'] = "01010 01010 11111 01010 11111 01010 01010",
        ['*'] = "00000 00100 10101 01110 10101 00100 00000",
        [':'] = "00000 01100 01100 00000 01100 01100 00000",
        ['-'] = "00000 00000 00000 11111 00000 00000 00000",
        ['.'] = "00000 00000 00000 00000 00000 01100 01100",
        ['%'] = "11001 11001 00010 00100 01000 10011 10011",
        ['='] = "00000 00000 11111 00000 11111 00000 00000",
        ['/'] = "00001 00010 00010 00100 01000 01000 10000",
        ['%'] = "11001 11001 00010 00100 01000 10011 10011"
    };

    /// <summary>
    /// Measures a string.
    /// </summary>
    /// <param name="text">Text to measure.</param>
    /// <param name="scale">Integer magnification.</param>
    /// <returns>Width in pixels.</returns>
    public static int Measure(string text, int scale = 1) => text.Length * Advance * scale;

    /// <summary>
    /// Draws a string with its top-left corner at the given position.
    /// </summary>
    /// <param name="target">Destination surface.</param>
    /// <param name="text">Text to draw; lowercase letters are drawn as capitals.</param>
    /// <param name="left">Left column.</param>
    /// <param name="top">Top row.</param>
    /// <param name="color">Text color.</param>
    /// <param name="scale">Integer magnification.</param>
    /// <exception cref="ArgumentException">The text contains a character the font cannot draw.</exception>
    public static void Draw(FrameBuffer target, string text, int left, int top, uint color, int scale = 1)
    {
        foreach (char symbol in text.ToUpperInvariant())
        {
            if (!_glyphs.TryGetValue(symbol, out string? glyph))
            {
                throw new ArgumentException($"Font has no glyph for '{symbol}'", nameof(text));
            }

            DrawGlyph(target, glyph, left, top, color, scale);
            left += Advance * scale;
        }
    }

    private static void DrawGlyph(FrameBuffer target, string glyph, int left, int top, uint color, int scale)
    {
        for (int row = 0; row < Height; row++)
        {
            for (int col = 0; col < _glyphWidth; col++)
            {
                if (glyph[row * (_glyphWidth + 1) + col] == '1')
                {
                    target.FillRect(left + col * scale, top + row * scale, scale, scale, color);
                }
            }
        }
    }
}
