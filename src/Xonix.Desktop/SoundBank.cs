using Raylib_cs;
using Xonix.Core.Audio;
using GameSound = Xonix.Core.Sound;

namespace Xonix.Desktop;

/// <summary>
/// Every sound effect synthesized once at startup and loaded into the raylib audio device.
/// </summary>
/// <remarks>Requires <see cref="Raylib.InitAudioDevice"/> to have been called.</remarks>
internal sealed class SoundBank : IDisposable
{
    private readonly Dictionary<GameSound, Sound> _sounds = new();

    public SoundBank()
    {
        foreach (var sound in Enum.GetValues<GameSound>())
        {
            var wave = Raylib.LoadWaveFromMemory(".wav", Synth.RenderWav(sound));
            _sounds[sound] = Raylib.LoadSoundFromWave(wave);
            Raylib.UnloadWave(wave);
        }
    }

    /// <summary>Gets or sets a value indicating whether playback is silenced.</summary>
    public bool Muted { get; set; }

    /// <summary>
    /// Starts a sound effect.
    /// </summary>
    /// <param name="sound">Effect to play.</param>
    public void Play(GameSound sound)
    {
        if (!Muted)
        {
            Raylib.PlaySound(_sounds[sound]);
        }
    }

    public void Dispose()
    {
        foreach (var sound in _sounds.Values)
        {
            Raylib.UnloadSound(sound);
        }

        _sounds.Clear();
    }
}
