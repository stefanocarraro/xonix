using System.Numerics;
using Raylib_cs;
using Xonix.Core;
using Xonix.Core.Rendering;

namespace Xonix.Desktop;

/// <summary>
/// Native window host: raylib provides the window, keyboard and audio; Xonix.Core does everything else.
/// </summary>
internal static class DesktopHost
{
    private const int _initialScale = 3;
    private const int _width = Renderer.ScreenWidth;
    private const int _height = Renderer.ScreenHeight;

    private static readonly (KeyboardKey Key, Direction Direction)[] _directionKeys =
    [
        (KeyboardKey.Up, Direction.Up),
        (KeyboardKey.Down, Direction.Down),
        (KeyboardKey.Left, Direction.Left),
        (KeyboardKey.Right, Direction.Right)
    ];

    /// <summary>
    /// Opens the window and runs the game until it is closed (window button or Esc).
    /// </summary>
    public static void Run()
    {
        var highScores = new HighScoreFile(HighScoreFile.DefaultPath);
        var game = new Game(new Random()) { HighScore = highScores.Load() };
        var driver = new GameDriver(game);
        var frame = new FrameBuffer(_width, _height);
        var renderer = new Renderer(frame);
        int savedHighScore = game.HighScore;

        Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(_width * _initialScale, _height * _initialScale, "Xonix");
        Raylib.SetWindowMinSize(_width, _height);
        Raylib.InitAudioDevice();
        var texture = CreateScreenTexture();
        var sounds = new SoundBank();
        try
        {
            while (!Raylib.WindowShouldClose())
            {
                HandleKeys(driver, sounds);
                driver.Advance(Raylib.GetFrameTime() * 1000.0);
                foreach (var sound in game.DrainSounds())
                {
                    sounds.Play(sound);
                }

                if (game.State != GameState.Playing && game.HighScore > savedHighScore)
                {
                    savedHighScore = game.HighScore;
                    highScores.Save(savedHighScore);
                }

                renderer.Draw(game);
                Raylib.UpdateTexture(texture, frame.Pixels);
                Present(texture);
            }
        }
        finally
        {
            sounds.Dispose();
            Raylib.UnloadTexture(texture);
            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
        }
    }

    private static Texture2D CreateScreenTexture()
    {
        var image = Raylib.GenImageColor(_width, _height, Color.Black);
        var texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);
        Raylib.SetTextureFilter(texture, TextureFilter.Point);
        return texture;
    }

    private static void HandleKeys(GameDriver driver, SoundBank sounds)
    {
        foreach (var (key, direction) in _directionKeys)
        {
            // Both checks run: a tap can press and release within one frame.
            if (Raylib.IsKeyPressed(key))
            {
                driver.Press(direction);
            }

            if (Raylib.IsKeyReleased(key))
            {
                driver.Release(direction);
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.KpEnter) || Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            driver.Game.PressStart();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.P))
        {
            driver.Game.TogglePause();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.M))
        {
            sounds.Muted = !sounds.Muted;
        }
    }

    /// <summary>
    /// Scales the framebuffer to the window, keeping the aspect ratio and sharp pixels.
    /// </summary>
    /// <param name="texture">Texture holding the latest frame.</param>
    private static void Present(Texture2D texture)
    {
        float scale = Math.Min(Raylib.GetScreenWidth() / (float)_width, Raylib.GetScreenHeight() / (float)_height);
        float width = _width * scale;
        float height = _height * scale;
        var target = new Rectangle((Raylib.GetScreenWidth() - width) / 2, (Raylib.GetScreenHeight() - height) / 2, width, height);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawTexturePro(texture, new Rectangle(0, 0, _width, _height), target, Vector2.Zero, 0, Color.White);
        Raylib.EndDrawing();
    }
}
