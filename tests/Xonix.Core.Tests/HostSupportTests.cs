using Xonix.Core.Audio;
using Xonix.Core.Rendering;

namespace Xonix.Core.Tests;

public class HostSupportTests
{
    [Fact]
    public void Driver_TapBetweenMoves_MovesOnce()
    {
        var driver = Driver();

        driver.Press(Direction.Right);
        driver.Release(Direction.Right);
        Frames(driver, Game.StepFrames * 3);

        Assert.Equal(new Cell(41, 0), driver.Game.Player);
    }

    [Fact]
    public void Driver_HeldKey_KeepsMoving()
    {
        var driver = Driver();

        driver.Press(Direction.Right);
        Frames(driver, Game.StepFrames * 3);

        Assert.Equal(new Cell(43, 0), driver.Game.Player);
    }

    [Fact]
    public void Driver_LatestHeldKeyWins()
    {
        var driver = Driver();

        driver.Press(Direction.Right);
        driver.Press(Direction.Left);
        Frames(driver, Game.StepFrames * 2);

        Assert.Equal(new Cell(38, 0), driver.Game.Player);
    }

    [Fact]
    public void Driver_LongStall_IsCapped()
    {
        var driver = Driver();

        driver.Press(Direction.Right);
        driver.Advance(10_000);

        Assert.Equal(new Cell(41, 0), driver.Game.Player);
    }

    [Fact]
    public void Synth_EverySound_RendersWav()
    {
        foreach (var sound in Enum.GetValues<Sound>())
        {
            byte[] wav = Synth.RenderWav(sound);

            Assert.Equal("RIFF"u8.ToArray(), wav[..4]);
            Assert.True(wav.Length > 1000, $"{sound} is too short");
        }
    }

    [Fact]
    public void Renderer_EveryState_Draws()
    {
        var frame = new FrameBuffer(Renderer.ScreenWidth, Renderer.ScreenHeight);
        var renderer = new Renderer(frame);
        var game = new Game(new Random(1));

        renderer.Draw(game);
        game.PressStart();
        game.SetEnemies([], []);
        renderer.Draw(game);
        Assert.Equal(Palette.Land, frame.Pixels[0]);
        Assert.Equal(Palette.Sea, frame.Pixels[Field.Border * Renderer.CellSize * (Renderer.ScreenWidth + 1)]);

        game.TogglePause();
        renderer.Draw(game);
        game.TogglePause();
        game.SetEnemies([new Enemy(new Cell(41, 4), -1, -1)], []);
        for (int i = 0; i < Game.StepFrames * 2; i++)
        {
            game.Steer(Direction.Down);
            game.Update();
        }

        Assert.Equal(GameState.Dying, game.State);
        renderer.Draw(game);
    }

    private static GameDriver Driver()
    {
        var game = new Game(new Random(1));
        game.PressStart();
        game.SetEnemies([], []);
        return new GameDriver(game);
    }

    private static void Frames(GameDriver driver, int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            driver.Advance(GameDriver.StepMs);
        }
    }
}
