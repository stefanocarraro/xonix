# Xonix

A clone of the 1984 DOS game Xonix, written in C# on .NET 10. It runs in the browser (Blazor WebAssembly) and as a native executable on Windows and Linux.

This project started as an experiment in game generation: the code was created by Claude (Anthropic's AI model) through Claude Code, from a one-line request to recreate the DOS original.

## Download

Ready-to-run executables for Windows and Linux (x64) are on the [Releases page](https://github.com/stefanocarraro/xonix/releases/latest). No .NET installation is needed.

- Windows: unzip `xonix-win-x64.zip` and run `xonix.exe`. The executable is not signed, so SmartScreen may ask for confirmation.
- Linux: `tar -xzf xonix-linux-x64.tar.gz && ./xonix`

## How to play

You move a cursor around a field of sea framed by land. Leave the land to draw a trail across the sea; when you reach land again, the trail and every sea region without a ball become land. Claim 75% of the sea to complete the level.

- Sea balls bounce around the sea. If one touches your trail, you lose a life.
- Land eaters bounce around the land. If one touches you, you lose a life.
- Crossing your own trail costs a life.
- Each level has a 120-second clock. Running out costs a life and restarts the clock.

Scoring: 1 point per claimed cell, plus 1000 x level number and 10 points per second left when a level is completed. Each completed level also gives an extra life. Level N has N+1 sea balls and (N+1)/2 land eaters.

| Key | Action |
|---|---|
| Arrows | Move |
| Enter / Space | Start |
| P | Pause |
| M | Mute |
| Esc | Quit (desktop) |

The high score is saved in the browser's `localStorage` on the web, and in `%AppData%\xonix` (Windows) or `~/.config/xonix` (Linux) on the desktop.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Run

```sh
# Desktop window
dotnet run --project src/Xonix.Desktop

# Web, then open the URL printed in the console
dotnet run --project src/Xonix.Web
```

## Test

```sh
dotnet test
```

## Publish

```sh
# Single-file, self-contained executables
dotnet publish src/Xonix.Desktop -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/win-x64
dotnet publish src/Xonix.Desktop -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o publish/linux-x64

# Static website: serve publish/web/wwwroot from any static host
dotnet publish src/Xonix.Web -c Release -o publish/web
```

The Linux build bundles raylib's native library; it needs a desktop with OpenGL. It has not yet been tested on a Linux machine.

## Project layout

| Project | Contents |
|---|---|
| `src/Xonix.Core` | Game rules, field capture, enemies, a 320x200 software renderer and the sound synthesizer. No platform dependencies. |
| `src/Xonix.Desktop` | Native host: window, keyboard and audio through [Raylib-cs](https://github.com/ChrisDill/Raylib-cs). |
| `src/Xonix.Web` | Browser host: Blazor WebAssembly with a canvas, keyboard input and Web Audio. |
| `tests/Xonix.Core.Tests` | xUnit tests for the core. |

The core runs at a fixed 60 Hz and draws into a framebuffer; each host only forwards keys, presents the frames and plays the sounds the core generates.

## License

[MIT](LICENSE)
