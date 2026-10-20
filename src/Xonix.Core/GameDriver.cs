namespace Xonix.Core;

/// <summary>
/// Drives a <see cref="Game"/> from a host: tracks held keys and runs fixed 60 Hz steps from wall time.
/// </summary>
/// <remarks>
/// The most recently pressed key that is still held wins. A key pressed and released between two
/// player moves still counts as one move, so quick taps are never lost.
/// </remarks>
/// <param name="game">Game to drive.</param>
public sealed class GameDriver(Game game)
{
    /// <summary>Duration of one simulation step in milliseconds.</summary>
    public const double StepMs = 1000.0 / 60.0;

    private const int _maxStepsPerAdvance = 5;

    private readonly List<Direction> _held = new();
    private double _pendingSteps;

    /// <summary>Gets the driven game.</summary>
    public Game Game { get; } = game;

    /// <summary>
    /// Records a direction key press.
    /// </summary>
    /// <param name="direction">Direction of the pressed key.</param>
    public void Press(Direction direction)
    {
        _held.Remove(direction);
        _held.Add(direction);
        Game.Steer(direction);
    }

    /// <summary>
    /// Records a direction key release.
    /// </summary>
    /// <param name="direction">Direction of the released key.</param>
    public void Release(Direction direction) => _held.Remove(direction);

    /// <summary>
    /// Runs as many simulation steps as the elapsed time allows.
    /// </summary>
    /// <remarks>Long stalls (a hidden tab, a debugger break) are capped so the game never fast-forwards.</remarks>
    /// <param name="elapsedMs">Wall time since the previous call.</param>
    public void Advance(double elapsedMs)
    {
        // Counting in steps, not milliseconds, keeps the cap exact despite 1000/60 not being representable.
        _pendingSteps = Math.Min(_pendingSteps + elapsedMs / StepMs, _maxStepsPerAdvance);
        while (_pendingSteps >= 1)
        {
            if (_held.Count > 0)
            {
                Game.Steer(_held[^1]);
            }

            Game.Update();
            _pendingSteps--;
        }
    }
}
