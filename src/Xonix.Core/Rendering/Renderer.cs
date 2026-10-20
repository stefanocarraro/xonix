namespace Xonix.Core.Rendering;

/// <summary>
/// Draws a <see cref="Game"/> snapshot onto a framebuffer at the DOS resolution of 320x200.
/// </summary>
/// <param name="target">Surface of <see cref="ScreenWidth"/> by <see cref="ScreenHeight"/> pixels.</param>
public sealed class Renderer(FrameBuffer target)
{
    /// <summary>Framebuffer width in pixels.</summary>
    public const int ScreenWidth = 320;

    /// <summary>Framebuffer height in pixels.</summary>
    public const int ScreenHeight = 200;

    /// <summary>Side of one field cell in pixels.</summary>
    public const int CellSize = 4;

    private const int _hudTop = Field.Height * CellSize + 1;

    // 4x4 masks, one row per string.
    private static readonly string[] _ball = [".##.", "####", "####", ".##."];
    private static readonly string[] _eater = ["####", "#..#", "#..#", "####"];

    /// <summary>
    /// Renders one frame.
    /// </summary>
    /// <param name="game">Game to draw.</param>
    public void Draw(Game game)
    {
        target.Clear(Palette.Sea);
        if (game.State == GameState.Title)
        {
            DrawTitle(game);
            return;
        }

        DrawField(game);
        DrawHud(game);
        DrawOverlay(game);
    }

    private void DrawTitle(Game game)
    {
        DrawCentered("XONIX", 30, Palette.Land, 6);
        if (game.Frame / 30 % 2 == 0)
        {
            DrawCentered("PRESS ENTER TO START", 100, Palette.Yellow);
        }

        DrawCentered($"CLAIM {Game.TargetPercent}% OF THE SEA", 124, Palette.White);
        DrawCentered("BALLS KILL YOUR TRAIL - EATERS KILL YOU", 136, Palette.White);
        DrawCentered("ARROWS MOVE   P PAUSE   M MUTE", 152, Palette.White);
        DrawCentered($"HI SCORE {game.HighScore}", 176, Palette.Yellow);
    }

    private void DrawField(Game game)
    {
        for (int y = 0; y < Field.Height; y++)
        {
            for (int x = 0; x < Field.Width; x++)
            {
                uint color = game.Field[new Cell(x, y)] switch
                {
                    Tile.Land => Palette.Land,
                    Tile.Trail => Palette.Trail,
                    _ => Palette.Sea
                };
                target.FillRect(x * CellSize, y * CellSize, CellSize, CellSize, color);
            }
        }

        foreach (var eater in game.Eaters)
        {
            DrawMask(_eater, eater.Position, Palette.Blue);
        }

        foreach (var ball in game.Balls)
        {
            DrawMask(_ball, ball.Position, Palette.White);
        }

        // The player blinks while dying.
        if (game.State != GameState.Dying || game.StateTimer / 8 % 2 == 0)
        {
            var player = game.Player;
            target.FillRect(player.X * CellSize, player.Y * CellSize, CellSize, CellSize, Palette.White);
            target.FillRect(player.X * CellSize + 1, player.Y * CellSize + 1, CellSize - 2, CellSize - 2, Palette.Red);
        }
    }

    private void DrawHud(Game game)
    {
        string hud = $"SCORE {game.Score} LIVES {game.SpareLives + 1} LEVEL {game.Level} FILL {game.Field.FilledPercent}/{Game.TargetPercent}% TIME {game.SecondsLeft}";
        Font.Draw(target, hud, 2, _hudTop, Palette.White);
    }

    private void DrawOverlay(Game game)
    {
        string? message = game.State switch
        {
            GameState.LevelComplete => $"LEVEL {game.Level} COMPLETE",
            GameState.GameOver => "GAME OVER",
            _ when game.IsPaused => "PAUSED",
            _ => null
        };
        if (message is null)
        {
            return;
        }

        const int scale = 2;
        const int top = 84;
        int width = Font.Measure(message, scale);
        target.FillRect((ScreenWidth - width) / 2 - 6, top - 6, width + 12, Font.Height * scale + 12, Palette.Sea);
        DrawCentered(message, top, Palette.Yellow, scale);
    }

    private void DrawCentered(string text, int top, uint color, int scale = 1) =>
        Font.Draw(target, text, (ScreenWidth - Font.Measure(text, scale)) / 2, top, color, scale);

    private void DrawMask(string[] mask, Cell cell, uint color)
    {
        for (int row = 0; row < mask.Length; row++)
        {
            for (int col = 0; col < mask[row].Length; col++)
            {
                if (mask[row][col] == '#')
                {
                    target.Set(cell.X * CellSize + col, cell.Y * CellSize + row, color);
                }
            }
        }
    }
}
