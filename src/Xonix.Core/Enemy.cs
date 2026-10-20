namespace Xonix.Core;

/// <summary>
/// Diagonal mover that bounces inside its territory: sea balls in the sea, land eaters on land.
/// </summary>
public sealed class Enemy
{
    /// <summary>
    /// Initializes an enemy.
    /// </summary>
    /// <param name="position">Starting cell.</param>
    /// <param name="dx">Horizontal heading, -1 or 1.</param>
    /// <param name="dy">Vertical heading, -1 or 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">A heading is not -1 or 1.</exception>
    public Enemy(Cell position, int dx, int dy)
    {
        if (Math.Abs(dx) != 1 || Math.Abs(dy) != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(dx), (dx, dy), "Enemies move diagonally: headings must be -1 or 1");
        }

        Position = position;
        Dx = dx;
        Dy = dy;
    }

    /// <summary>Gets the current cell.</summary>
    public Cell Position { get; private set; }

    /// <summary>Gets the horizontal heading, -1 or 1.</summary>
    public int Dx { get; private set; }

    /// <summary>Gets the vertical heading, -1 or 1.</summary>
    public int Dy { get; private set; }

    /// <summary>
    /// Moves one diagonal cell, bouncing off closed cells like a billiard ball; stays put when boxed in.
    /// </summary>
    /// <remarks>A move always passes through an open side neighbour, so the enemy never cuts a diagonal corner.</remarks>
    /// <param name="open">Tells whether a cell belongs to the enemy's territory.</param>
    public void Step(Func<Cell, bool> open)
    {
        int x = Position.X;
        int y = Position.Y;
        bool sideOpen = open(new(x + Dx, y));
        bool verticalOpen = open(new(x, y + Dy));
        if (!sideOpen)
        {
            Dx = -Dx;
        }

        if (!verticalOpen)
        {
            Dy = -Dy;
        }

        if (sideOpen && verticalOpen && !open(new(x + Dx, y + Dy)))
        {
            Dx = -Dx;
            Dy = -Dy;
        }

        var target = new Cell(x + Dx, y + Dy);
        if (open(target) && (open(new(x + Dx, y)) || open(new(x, y + Dy))))
        {
            Position = target;
        }
    }
}
