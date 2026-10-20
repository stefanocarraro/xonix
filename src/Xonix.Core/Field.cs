namespace Xonix.Core;

/// <summary>
/// State of one field cell.
/// </summary>
public enum Tile : byte
{
    Sea,
    Land,
    Trail
}

/// <summary>
/// Player movement directions.
/// </summary>
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Field position in cells; may lie outside the field.
/// </summary>
/// <param name="X">Column, 0 at the left.</param>
/// <param name="Y">Row, 0 at the top.</param>
public readonly record struct Cell(int X, int Y)
{
    /// <summary>
    /// Computes the neighbouring cell in a direction.
    /// </summary>
    /// <param name="direction">Direction to step in.</param>
    /// <returns>The adjacent cell.</returns>
    public Cell Step(Direction direction) => direction switch
    {
        Direction.Up => this with { Y = Y - 1 },
        Direction.Down => this with { Y = Y + 1 },
        Direction.Left => this with { X = X - 1 },
        Direction.Right => this with { X = X + 1 },
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown direction")
    };

    /// <summary>
    /// Tells whether another cell is this one or one of its four side neighbours.
    /// </summary>
    /// <param name="other">Cell to compare.</param>
    /// <returns><c>true</c> when the cells share a side or coincide.</returns>
    public bool Touches(Cell other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y) <= 1;
}

/// <summary>
/// Playfield grid: a land frame around the sea the player has to claim.
/// </summary>
public sealed class Field
{
    /// <summary>Width in cells.</summary>
    public const int Width = 80;

    /// <summary>Height in cells.</summary>
    public const int Height = 48;

    /// <summary>Thickness of the land frame in cells.</summary>
    public const int Border = 2;

    /// <summary>Cells inside the frame, i.e. the sea at the start of a level.</summary>
    public const int InteriorCells = (Width - 2 * Border) * (Height - 2 * Border);

    private readonly Tile[] _tiles = new Tile[Width * Height];

    /// <summary>
    /// Initializes a field with an empty sea.
    /// </summary>
    public Field() => Reset();

    /// <summary>
    /// Gets or sets a tile.
    /// </summary>
    /// <param name="cell">Position inside the field.</param>
    /// <exception cref="ArgumentOutOfRangeException">The cell lies outside the field.</exception>
    public Tile this[Cell cell]
    {
        get => _tiles[Index(cell)];
        internal set => _tiles[Index(cell)] = value;
    }

    /// <summary>Gets the claimed share of the interior, rounded down to a whole percent.</summary>
    public int FilledPercent =>
        100 * (_tiles.Count(tile => tile == Tile.Land) - (Width * Height - InteriorCells)) / InteriorCells;

    /// <summary>
    /// Tells whether a cell lies inside the field.
    /// </summary>
    /// <param name="cell">Cell to test.</param>
    /// <returns><c>true</c> for cells on the grid.</returns>
    public static bool Contains(Cell cell) => (uint)cell.X < Width && (uint)cell.Y < Height;

    /// <summary>
    /// Restores the land frame and the empty sea.
    /// </summary>
    public void Reset()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                bool frame = x < Border || y < Border || x >= Width - Border || y >= Height - Border;
                _tiles[y * Width + x] = frame ? Tile.Land : Tile.Sea;
            }
        }
    }

    /// <summary>
    /// Closes a trail: the trail turns into land, and so does every sea region holding no ball.
    /// </summary>
    /// <remarks>Regions are 4-connected; <see cref="Enemy.Step"/> never slips a ball between two diagonal walls.</remarks>
    /// <param name="balls">Positions of the sea balls.</param>
    /// <returns>Cells turned into land.</returns>
    public int Capture(IEnumerable<Cell> balls)
    {
        var reached = new bool[_tiles.Length];
        var pending = new Stack<Cell>(balls);
        while (pending.TryPop(out var cell))
        {
            if (!Contains(cell) || this[cell] != Tile.Sea || reached[Index(cell)])
            {
                continue;
            }

            reached[Index(cell)] = true;
            foreach (var direction in Enum.GetValues<Direction>())
            {
                pending.Push(cell.Step(direction));
            }
        }

        int captured = 0;
        for (int i = 0; i < _tiles.Length; i++)
        {
            if (_tiles[i] == Tile.Trail || (_tiles[i] == Tile.Sea && !reached[i]))
            {
                _tiles[i] = Tile.Land;
                captured++;
            }
        }

        return captured;
    }

    private static int Index(Cell cell) => Contains(cell)
        ? cell.Y * Width + cell.X
        : throw new ArgumentOutOfRangeException(nameof(cell), cell, "Cell outside the field");
}
