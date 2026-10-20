namespace Xonix.Core;

/// <summary>
/// Top-level phases of a session.
/// </summary>
public enum GameState
{
    Title,
    Playing,
    Dying,
    LevelComplete,
    GameOver
}

/// <summary>
/// Sound effects the host should play; drained once per frame.
/// </summary>
public enum Sound
{
    Start,
    Capture,
    Death,
    LevelComplete,
    GameOver
}

/// <summary>
/// Xonix rules engine: advances the whole simulation one fixed 60 Hz frame at a time.
/// </summary>
public sealed class Game
{
    /// <summary>Claimed share of the sea that completes a level.</summary>
    public const int TargetPercent = 75;

    /// <summary>Spare lives at the start of a game.</summary>
    public const int StartingSpareLives = 2;

    /// <summary>Frames between two moves of the player and the enemies.</summary>
    public const int StepFrames = 3;

    /// <summary>Bonus per level number for completing a level.</summary>
    public const int LevelBonus = 1000;

    /// <summary>Bonus per second left on the clock when a level is completed.</summary>
    public const int SecondBonus = 10;

    /// <summary>Frames of the death pause.</summary>
    public const int DyingFrames = 90;

    /// <summary>Frames of the level-complete screen.</summary>
    public const int LevelCompleteFrames = 150;

    /// <summary>Frames the game-over screen stays up.</summary>
    public const int GameOverFrames = 240;

    /// <summary>Frames available to finish a level; running out costs a life and restarts the clock.</summary>
    public const int TimeLimitFrames = 120 * 60;

    /// <summary>Player cell at the start of a level and after each death: top middle of the frame.</summary>
    public static readonly Cell PlayerStart = new(Field.Width / 2, 0);

    private readonly Random _random;
    private readonly List<Enemy> _balls = new();
    private readonly List<Enemy> _eaters = new();
    private readonly List<Cell> _trail = new();
    private readonly List<Sound> _sounds = new();
    private Direction? _steer;
    private Direction _heading = Direction.Down;

    /// <summary>
    /// Initializes a game on the title screen.
    /// </summary>
    /// <param name="random">Randomness source; pass a seeded instance for reproducible runs.</param>
    public Game(Random random) => _random = random;

    /// <summary>Gets the current phase.</summary>
    public GameState State { get; private set; }

    /// <summary>Gets the frames left in a timed phase (dying, level complete, game over).</summary>
    public int StateTimer { get; private set; }

    /// <summary>Gets the frames simulated since construction; drives animations.</summary>
    public int Frame { get; private set; }

    /// <summary>Gets the current score.</summary>
    public int Score { get; private set; }

    /// <summary>Gets or sets the best score; the host loads it from storage.</summary>
    public int HighScore { get; set; }

    /// <summary>Gets the lives left after the current one.</summary>
    public int SpareLives { get; private set; }

    /// <summary>Gets the 1-based level.</summary>
    public int Level { get; private set; } = 1;

    /// <summary>Gets the frames left on the level clock.</summary>
    public int TimeLeft { get; private set; } = TimeLimitFrames;

    /// <summary>Gets the whole seconds left on the level clock, rounded up as the HUD shows them.</summary>
    public int SecondsLeft => (TimeLeft + 59) / 60;

    /// <summary>Gets a value indicating whether the simulation is frozen.</summary>
    public bool IsPaused { get; private set; }

    /// <summary>Gets the playfield.</summary>
    public Field Field { get; } = new();

    /// <summary>Gets the player cell.</summary>
    public Cell Player { get; private set; } = PlayerStart;

    /// <summary>Gets the balls bouncing in the sea; touching the trail kills the player.</summary>
    public IReadOnlyList<Enemy> Balls => _balls;

    /// <summary>Gets the eaters bouncing on land; touching the player kills it.</summary>
    public IReadOnlyList<Enemy> Eaters => _eaters;

    /// <summary>
    /// Computes the number of sea balls of a level.
    /// </summary>
    /// <param name="level">1-based level.</param>
    /// <returns>Ball count.</returns>
    public static int BallsFor(int level) => level + 1;

    /// <summary>
    /// Computes the number of land eaters of a level.
    /// </summary>
    /// <param name="level">1-based level.</param>
    /// <returns>Eater count.</returns>
    public static int EatersFor(int level) => (level + 1) / 2;

    /// <summary>
    /// Asks the player to move; the request holds until the next player move consumes it.
    /// </summary>
    /// <remarks>On land the player moves only on request; at sea it keeps its heading and ignores U-turns.</remarks>
    /// <param name="direction">Requested direction.</param>
    public void Steer(Direction direction) => _steer = direction;

    /// <summary>
    /// Starts a new game from the title or game-over screen; ignored otherwise.
    /// </summary>
    public void PressStart()
    {
        if (State is not (GameState.Title or GameState.GameOver))
        {
            return;
        }

        Score = 0;
        SpareLives = StartingSpareLives;
        StartLevel(1);
        _sounds.Add(Sound.Start);
    }

    /// <summary>
    /// Freezes or resumes play; ignored outside play.
    /// </summary>
    public void TogglePause()
    {
        if (State == GameState.Playing)
        {
            IsPaused = !IsPaused;
        }
    }

    /// <summary>
    /// Returns and forgets the sounds queued since the previous call.
    /// </summary>
    /// <returns>Sounds in the order they happened.</returns>
    public Sound[] DrainSounds()
    {
        var sounds = _sounds.ToArray();
        _sounds.Clear();
        return sounds;
    }

    /// <summary>
    /// Advances the simulation by one frame.
    /// </summary>
    public void Update()
    {
        if (IsPaused)
        {
            return;
        }

        Frame++;
        switch (State)
        {
            case GameState.Playing when --TimeLeft == 0:
                Die();
                break;
            case GameState.Playing when Frame % StepFrames == 0:
                Step();
                break;
            case GameState.Dying when --StateTimer == 0:
                Respawn();
                break;
            case GameState.LevelComplete when --StateTimer == 0:
                StartLevel(Level + 1);
                break;
            case GameState.GameOver when --StateTimer == 0:
                State = GameState.Title;
                break;
        }
    }

    /// <summary>
    /// Replaces the enemies; lets tests set up exact situations.
    /// </summary>
    /// <param name="balls">New sea balls.</param>
    /// <param name="eaters">New land eaters.</param>
    internal void SetEnemies(IEnumerable<Enemy> balls, IEnumerable<Enemy> eaters)
    {
        _balls.Clear();
        _balls.AddRange(balls);
        _eaters.Clear();
        _eaters.AddRange(eaters);
    }

    private static Direction Opposite(Direction direction) => direction switch
    {
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        _ => Direction.Left
    };

    private void Step()
    {
        MovePlayer();
        if (State != GameState.Playing)
        {
            return;
        }

        foreach (var ball in _balls)
        {
            ball.Step(cell => Field.Contains(cell) && Field[cell] == Tile.Sea);
        }

        foreach (var eater in _eaters)
        {
            eater.Step(cell => Field.Contains(cell) && Field[cell] == Tile.Land);
        }

        CheckCollisions();
    }

    private void MovePlayer()
    {
        bool atSea = Field[Player] == Tile.Trail;
        Direction? direction = atSea
            ? _steer is { } steer && steer != Opposite(_heading) ? steer : _heading
            : _steer;
        _steer = null;
        if (direction is not { } move || !Field.Contains(Player.Step(move)))
        {
            return;
        }

        _heading = move;
        Player = Player.Step(move);
        switch (Field[Player])
        {
            case Tile.Trail:
                Die();
                return;
            case Tile.Sea:
                Field[Player] = Tile.Trail;
                _trail.Add(Player);
                break;
            case Tile.Land when atSea:
                Capture();
                break;
        }

        CheckCollisions();
    }

    private void Capture()
    {
        Score += Field.Capture(_balls.Select(ball => ball.Position));
        _trail.Clear();
        if (Field.FilledPercent >= TargetPercent)
        {
            Score += LevelBonus * Level + SecondBonus * SecondsLeft;
            SpareLives++;
            State = GameState.LevelComplete;
            StateTimer = LevelCompleteFrames;
            _sounds.Add(Sound.LevelComplete);
        }
        else
        {
            _sounds.Add(Sound.Capture);
        }

        HighScore = Math.Max(HighScore, Score);
    }

    private void CheckCollisions()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        bool trailHit = _balls.Any(ball => _trail.Any(cell => cell.Touches(ball.Position)));
        bool playerHit = _eaters.Any(eater => eater.Position.Touches(Player));
        if (trailHit || playerHit)
        {
            Die();
        }
    }

    private void Die()
    {
        State = GameState.Dying;
        StateTimer = DyingFrames;
        _sounds.Add(Sound.Death);
    }

    private void Respawn()
    {
        foreach (var cell in _trail)
        {
            Field[cell] = Tile.Sea;
        }

        _trail.Clear();
        if (SpareLives == 0)
        {
            State = GameState.GameOver;
            StateTimer = GameOverFrames;
            _sounds.Add(Sound.GameOver);
            return;
        }

        SpareLives--;
        TimeLeft = TimeLimitFrames;
        ResetPlayer();
        SpawnEaters();
        State = GameState.Playing;
    }

    private void StartLevel(int level)
    {
        Level = level;
        Field.Reset();
        _trail.Clear();
        ResetPlayer();
        _balls.Clear();
        for (int i = 0; i < BallsFor(level); i++)
        {
            var position = new Cell(
                _random.Next(Field.Border, Field.Width - Field.Border),
                _random.Next(Field.Border, Field.Height - Field.Border));
            _balls.Add(new Enemy(position, RandomHeading(), RandomHeading()));
        }

        SpawnEaters();
        TimeLeft = TimeLimitFrames;
        IsPaused = false;
        State = GameState.Playing;
    }

    /// <summary>
    /// Places the eaters on the bottom row, as far from <see cref="PlayerStart"/> as the frame allows.
    /// </summary>
    private void SpawnEaters()
    {
        _eaters.Clear();
        for (int i = 0; i < EatersFor(Level); i++)
        {
            _eaters.Add(new Enemy(new Cell(_random.Next(Field.Width), Field.Height - 1), RandomHeading(), -1));
        }
    }

    private void ResetPlayer()
    {
        Player = PlayerStart;
        _steer = null;
        _heading = Direction.Down;
    }

    private int RandomHeading() => _random.Next(2) * 2 - 1;
}
