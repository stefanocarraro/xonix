# Xonix clone in C# / .NET 10

An experiment in game generation: a recreation of Xonix (Ilan Raab and Dani Katz, MS-DOS, 1984), itself a
home-computer take on Taito's arcade game Qix (1981). The game was created by
[Claude](https://claude.com/claude-code), Anthropic's AI model, as a coding assistant: the whole project, from
design to code, tests and verification, was built in a conversation between a developer and Claude Code,
starting from a one-line request to recreate the DOS original.

It is an unofficial fan project for learning purposes. All graphics and sounds are generated from code;
no assets from the original game are used.

The same game runs in two hosts:

- **Web**: Blazor WebAssembly, the game loop runs in C# inside the browser.
- **Desktop**: a native executable for Windows and Linux built on [Raylib-cs](https://github.com/ChrisDill/Raylib-cs).

## Gameplay

You move a cursor around a field of sea framed by land. Leave the land to draw a trail across the sea; when you
reach land again, the trail and every sea region without a ball become land. Claim 75% of the sea to complete
the level.

- Sea balls bounce around the sea; if one touches your trail, you lose a life.
- Land eaters bounce around the land; if one touches you, you lose a life.
- Crossing your own trail costs a life.
- Each level has a 120-second clock; running out costs a life and restarts the clock.
- Level N has N+1 sea balls and (N+1)/2 land eaters.
- Scoring: 1 point per claimed cell; completing a level adds 1000 x level number, 10 points per second left
  and an extra life. The high score is kept between sessions.

## Controls

| Key | Action |
| --- | --- |
| Arrow keys | Move |
| Enter or Space | Start |
| P | Pause |
| M | Mute |
| Esc | Quit (desktop) |

## Project layout

```
src/Xonix.Core      Game rules, field capture, enemies, pixel-art renderer, sound synthesis (pure C#, no platform code)
src/Xonix.Web       Blazor WebAssembly host (canvas, keyboard, Web Audio, localStorage via JS interop)
src/Xonix.Desktop   Raylib-cs host (window, keyboard, audio, high score file)
tests/Xonix.Core.Tests  xUnit tests
```

`Xonix.Core` renders each frame into a 320x200 RGBA framebuffer, the DOS resolution; each host only copies it
to the screen, feeds the keys in and plays the sounds the core synthesizes.

## Download

Ready-to-run executables for Windows and Linux (x64) are attached to the
[latest release](https://github.com/stefanocarraro/xonix/releases/latest). No .NET installation is needed.

- Windows: unzip `xonix-1.0.0-win-x64.zip` and run `xonix.exe`. The executable is not signed, so SmartScreen
  may ask for confirmation.
- Linux: `tar -xzf xonix-1.0.0-linux-x64.tar.gz && ./xonix-1.0.0-linux-x64/xonix`. Not yet tested on a Linux
  machine.

## Requirements

- .NET 10 SDK (to build and run from source)
- Desktop on Linux: glibc 2.38 or newer (Ubuntu 24.04+, Debian 13+, Fedora 39+), an X11 or Wayland
  session with OpenGL, and ALSA or PulseAudio. The bundled native raylib library needs that glibc version.

## Run

```
dotnet run --project src/Xonix.Web        # browser version
dotnet run --project src/Xonix.Desktop    # desktop version
dotnet test                               # unit tests
```

## Publish

Single self-contained, trimmed desktop files (about 15 MB each) and a static website:

```
dotnet publish src/Xonix.Desktop -c Release -r win-x64   --self-contained -p:PublishSingleFile=true -o publish/win-x64
dotnet publish src/Xonix.Desktop -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o publish/linux-x64
dotnet publish src/Xonix.Web -c Release -o publish/web    # serve publish/web/wwwroot from any static host
```

## Known simplifications

- Rules are recreated from memory of the DOS game, not from its code: speeds, scoring, the 120-second clock and
  the number of enemies per level are tuned by feel.
- Pixels are square; the DOS original showed 320x200 stretched to a 4:3 screen.
- The player and all enemies move at the same fixed speed on every level.

## Inspiration

- [Xonix in the Wikipedia index of DOS games](https://en.wikipedia.org/wiki/Index_of_DOS_games_(X)), authors and release year
- [Qix on Wikipedia](https://en.wikipedia.org/wiki/Qix), the arcade game Xonix derives from
- [stefanocarraro/qbert](https://github.com/stefanocarraro/qbert), the Q*bert clone whose architecture this project reuses

## License

Code released under the [MIT License](LICENSE).

Xonix and Qix are trademarks or copyrighted works of their respective owners. This project is not affiliated
with or endorsed by them.
