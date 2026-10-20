namespace Xonix.Core.Tests;

public class GameTests
{
    private static readonly Enemy[] _none = [];

    [Fact]
    public void PressStart_StartsLevelOne()
    {
        var game = new Game(new Random(1));

        game.PressStart();

        Assert.Equal(GameState.Playing, game.State);
        Assert.Equal(Game.PlayerStart, game.Player);
        Assert.Equal(Game.StartingSpareLives, game.SpareLives);
        Assert.Equal(Game.BallsFor(1), game.Balls.Count);
        Assert.Equal(Game.EatersFor(1), game.Eaters.Count);
        Assert.All(game.Balls, ball => Assert.Equal(Tile.Sea, game.Field[ball.Position]));
        Assert.All(game.Eaters, eater => Assert.Equal(Tile.Land, game.Field[eater.Position]));
        Assert.Equal(Sound.Start, Assert.Single(game.DrainSounds()));
    }

    [Fact]
    public void PressStart_WhilePlaying_IsIgnored()
    {
        var game = Playing();
        Move(game, Direction.Right);

        game.PressStart();

        Assert.Equal(new Cell(41, 0), game.Player);
    }

    [Fact]
    public void OnLand_PlayerWaitsForSteering()
    {
        var game = Playing();

        Run(game, Game.StepFrames * 3);

        Assert.Equal(Game.PlayerStart, game.Player);
    }

    [Fact]
    public void OnLand_EdgeOfField_Blocks()
    {
        var game = Playing();

        Move(game, Direction.Up);

        Assert.Equal(Game.PlayerStart, game.Player);
    }

    [Fact]
    public void AtSea_PlayerLeavesTrailAndKeepsHeading()
    {
        var game = Playing();
        Move(game, Direction.Down);
        Move(game, Direction.Down);

        Run(game, Game.StepFrames);

        Assert.Equal(new Cell(40, 3), game.Player);
        Assert.Equal(Tile.Trail, game.Field[new Cell(40, 2)]);
        Assert.Equal(Tile.Trail, game.Field[new Cell(40, 3)]);
    }

    [Fact]
    public void AtSea_UTurnIsIgnored()
    {
        var game = Playing();
        Move(game, Direction.Down);
        Move(game, Direction.Down);

        Move(game, Direction.Up);

        Assert.Equal(new Cell(40, 3), game.Player);
    }

    [Fact]
    public void CrossingOwnTrail_Dies()
    {
        var game = Playing();
        Path(game, Direction.Down, Direction.Down, Direction.Down, Direction.Right, Direction.Up);

        Move(game, Direction.Left);

        Assert.Equal(GameState.Dying, game.State);
        Assert.Equal(Sound.Death, Assert.Single(game.DrainSounds()));
    }

    [Fact]
    public void ReachingLand_ClaimsTrailAndEnclosedSea()
    {
        var game = Playing();

        Path(game, Direction.Down, Direction.Down, Direction.Down, Direction.Down,
            Direction.Right, Direction.Right, Direction.Up, Direction.Up, Direction.Up);

        Assert.Equal(new Cell(42, 1), game.Player);
        Assert.Equal(9, game.Score);
        Assert.Equal(9, game.HighScore);
        Assert.Equal(Tile.Land, game.Field[new Cell(41, 3)]);
        Assert.Equal(Tile.Land, game.Field[new Cell(40, 2)]);
        Assert.Equal(Sound.Capture, Assert.Single(game.DrainSounds()));
    }

    [Fact]
    public void BallTouchingTrail_KillsPlayer()
    {
        var game = Playing();
        game.SetEnemies([new Enemy(new Cell(41, 4), -1, -1)], _none);

        Path(game, Direction.Down, Direction.Down);

        Assert.Equal(GameState.Dying, game.State);
    }

    [Fact]
    public void EaterTouchingPlayer_KillsPlayer()
    {
        var game = Playing();
        game.SetEnemies(_none, [new Enemy(new Cell(44, 0), -1, 1)]);

        Run(game, Game.StepFrames * 4);

        Assert.Equal(GameState.Dying, game.State);
    }

    [Fact]
    public void Death_ClearsTrailCostsALifeAndRestarts()
    {
        var game = Playing();
        Die(game);

        Run(game, Game.DyingFrames);

        Assert.Equal(GameState.Playing, game.State);
        Assert.Equal(Game.StartingSpareLives - 1, game.SpareLives);
        Assert.Equal(Game.PlayerStart, game.Player);
        Assert.Equal(Tile.Sea, game.Field[new Cell(40, 2)]);
    }

    [Fact]
    public void LastLifeLost_ShowsGameOverThenTitle()
    {
        var game = Playing();
        for (int life = 0; life < Game.StartingSpareLives; life++)
        {
            Die(game);
            Run(game, Game.DyingFrames);
        }

        Die(game);
        Run(game, Game.DyingFrames);

        Assert.Equal(GameState.GameOver, game.State);
        Assert.Contains(Sound.GameOver, game.DrainSounds());
        Run(game, Game.GameOverFrames);
        Assert.Equal(GameState.Title, game.State);
    }

    [Fact]
    public void ReachingTarget_CompletesLevelThenStartsNext()
    {
        var game = Playing();
        game.SetEnemies([new Enemy(new Cell(70, 3), 1, 1)], _none);
        // Leaves only a three-row strip of sea at the top.
        for (int y = 5; y < Field.Height - Field.Border; y++)
        {
            for (int x = Field.Border; x < Field.Width - Field.Border; x++)
            {
                game.Field[new Cell(x, y)] = Tile.Land;
            }
        }

        Path(game, Direction.Down, Direction.Down, Direction.Down, Direction.Down, Direction.Down);

        Assert.Equal(GameState.LevelComplete, game.State);
        // Five moves take 15 frames, so the clock still shows the full 120 seconds.
        Assert.Equal(3 + 38 * 3 + Game.LevelBonus + Game.SecondBonus * 120, game.Score);
        Assert.Equal(Game.StartingSpareLives + 1, game.SpareLives);
        Assert.Equal(Sound.LevelComplete, Assert.Single(game.DrainSounds()));

        Run(game, Game.LevelCompleteFrames);

        Assert.Equal(GameState.Playing, game.State);
        Assert.Equal(2, game.Level);
        Assert.Equal(Game.BallsFor(2), game.Balls.Count);
        Assert.Equal(0, game.Field.FilledPercent);
    }

    [Fact]
    public void TimeRunningOut_KillsPlayerAndRestartsClock()
    {
        var game = Playing();

        Run(game, Game.TimeLimitFrames - 1);
        Assert.Equal(GameState.Playing, game.State);
        Run(game, 1);

        Assert.Equal(GameState.Dying, game.State);
        Assert.Equal(Sound.Death, Assert.Single(game.DrainSounds()));
        Run(game, Game.DyingFrames);
        Assert.Equal(Game.TimeLimitFrames, game.TimeLeft);
        Assert.Equal(Game.StartingSpareLives - 1, game.SpareLives);
    }

    [Fact]
    public void Pause_FreezesEverything()
    {
        var game = Playing();
        game.SetEnemies([new Enemy(new Cell(60, 30), 1, 1)], _none);
        game.TogglePause();

        game.Steer(Direction.Right);
        Run(game, Game.StepFrames * 3);

        Assert.Equal(Game.PlayerStart, game.Player);
        Assert.Equal(new Cell(60, 30), game.Balls[0].Position);
        Assert.Equal(Game.TimeLimitFrames, game.TimeLeft);
        game.TogglePause();
        Run(game, Game.StepFrames);
        Assert.Equal(new Cell(41, 0), game.Player);
    }

    [Fact]
    public void TogglePause_OnTitle_IsIgnored()
    {
        var game = new Game(new Random(1));

        game.TogglePause();

        Assert.False(game.IsPaused);
    }

    /// <summary>
    /// Starts a game with one ball far from the top-middle start, where most tests play.
    /// </summary>
    private static Game Playing()
    {
        var game = new Game(new Random(1));
        game.PressStart();
        game.DrainSounds();
        game.SetEnemies([new Enemy(new Cell(60, 30), 1, 1)], _none);
        return game;
    }

    private static void Die(Game game)
    {
        game.SetEnemies([new Enemy(new Cell(41, 4), -1, -1)], _none);
        Path(game, Direction.Down, Direction.Down);
        Assert.Equal(GameState.Dying, game.State);
    }

    private static void Path(Game game, params Direction[] moves)
    {
        foreach (var move in moves)
        {
            Move(game, move);
        }
    }

    private static void Move(Game game, Direction direction)
    {
        game.Steer(direction);
        Run(game, Game.StepFrames);
    }

    private static void Run(Game game, int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            game.Update();
        }
    }
}
