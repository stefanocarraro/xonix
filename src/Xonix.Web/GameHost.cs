using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Xonix.Core;
using Xonix.Core.Audio;
using Xonix.Core.Rendering;

namespace Xonix.Web;

/// <summary>
/// Bridges the browser and the game: JavaScript drives the frame loop and input,
/// C# runs the simulation and hands back a finished framebuffer.
/// </summary>
[SupportedOSPlatform("browser")]
public static partial class GameHost
{
    private const string _module = "xonix";

    private static readonly Game _game = new(new Random());
    private static readonly GameDriver _driver = new(_game);
    private static readonly FrameBuffer _frame = new(Renderer.ScreenWidth, Renderer.ScreenHeight);
    private static readonly Renderer _renderer = new(_frame);
    private static int _savedHighScore;

    /// <summary>
    /// Loads the JavaScript module, hands it the synthesized sounds and starts the animation loop.
    /// </summary>
    /// <returns>A task that completes once the loop is running.</returns>
    public static async Task StartAsync()
    {
        await JSHost.ImportAsync(_module, "../js/xonix.js");
        foreach (var sound in Enum.GetValues<Sound>())
        {
            LoadSound(sound.ToString(), Synth.RenderWav(sound));
        }

        _savedHighScore = LoadHighScore();
        _game.HighScore = _savedHighScore;
        await StartLoop();
    }

    /// <summary>
    /// Advances the simulation by the elapsed wall time and presents a new frame.
    /// </summary>
    /// <param name="elapsedMs">Milliseconds since the previous animation frame.</param>
    [JSExport]
    public static void Frame(double elapsedMs)
    {
        _driver.Advance(elapsedMs);
        foreach (var sound in _game.DrainSounds())
        {
            PlaySound(sound.ToString());
        }

        PersistHighScore();
        _renderer.Draw(_game);
        Present(MemoryMarshal.AsBytes(_frame.Pixels.AsSpan()));
    }

    /// <summary>
    /// Handles a key press.
    /// </summary>
    /// <param name="code">The <c>KeyboardEvent.code</c> value.</param>
    [JSExport]
    public static void KeyDown(string code)
    {
        if (DirectionOf(code) is { } direction)
        {
            _driver.Press(direction);
        }
        else if (code is "Enter" or "Space" or "NumpadEnter")
        {
            _game.PressStart();
        }
        else if (code == "KeyP")
        {
            _game.TogglePause();
        }
    }

    /// <summary>
    /// Handles a key release.
    /// </summary>
    /// <param name="code">The <c>KeyboardEvent.code</c> value.</param>
    [JSExport]
    public static void KeyUp(string code)
    {
        if (DirectionOf(code) is { } direction)
        {
            _driver.Release(direction);
        }
    }

    private static Direction? DirectionOf(string code) => code switch
    {
        "ArrowUp" => Direction.Up,
        "ArrowDown" => Direction.Down,
        "ArrowLeft" => Direction.Left,
        "ArrowRight" => Direction.Right,
        _ => null
    };

    private static void PersistHighScore()
    {
        if (_game.State != GameState.Playing && _game.HighScore > _savedHighScore)
        {
            _savedHighScore = _game.HighScore;
            SaveHighScore(_savedHighScore);
        }
    }

    [JSImport("start", _module)]
    private static partial Task StartLoop();

    [JSImport("loadSound", _module)]
    private static partial void LoadSound(string name, [JSMarshalAs<JSType.MemoryView>] Span<byte> wav);

    [JSImport("present", _module)]
    private static partial void Present([JSMarshalAs<JSType.MemoryView>] Span<byte> pixels);

    [JSImport("playSound", _module)]
    private static partial void PlaySound(string name);

    [JSImport("loadHighScore", _module)]
    private static partial int LoadHighScore();

    [JSImport("saveHighScore", _module)]
    private static partial void SaveHighScore(int score);
}
