<div align="center">
  <img src="assets/logo.png" alt="Precision Cursor logo" width="170">
  <h1>Precision Cursor</h1>
  <p><strong>A lightweight Windows tray tool for clean horizontal, vertical, and 45-degree cursor movement.</strong></p>

  <p>
    <a href="https://github.com/gauravsoodtech/Precision-Cursor/actions/workflows/ci.yml"><img alt="CI" src="https://github.com/gauravsoodtech/Precision-Cursor/actions/workflows/ci.yml/badge.svg"></a>
    <a href="LICENSE"><img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-blue.svg"></a>
    <img alt="Platform: Windows" src="https://img.shields.io/badge/platform-Windows-0078D6.svg">
    <img alt=".NET Framework 4.8" src="https://img.shields.io/badge/.NET%20Framework-4.8-512BD4.svg">
  </p>
</div>

Precision Cursor helps you draw, annotate, align, and move the pointer in straight paths without fighting hand jitter. Enable it from the tray or with a hotkey, then move naturally: the app detects your intended direction and locks movement to the nearest 8-way line.

## Highlights

- Global Windows tray app with no setup window or background clutter.
- 8-way line locking: left, right, up, down, and four diagonals.
- Raw mouse input direction detection for more natural turns.
- Sticky smoothing to reduce wobble and stair-step diagonals.
- Low-level mouse suppression so unsnapped movement does not leak through.
- Emergency disable with `Esc`.
- No NuGet packages required.

## Controls

| Action | Control |
| --- | --- |
| Toggle line lock | `Ctrl+Alt+L` |
| Disable immediately | `Esc` |
| Toggle from tray | Double-click the tray icon or use the tray menu |
| Quit | Tray menu -> `Exit` |

## How It Works

Precision Cursor combines raw mouse input with a low-level mouse hook:

1. Raw input reads the real hardware movement delta.
2. The lock engine chooses the nearest 8-way direction.
3. Direction smoothing keeps movement stable while still allowing intentional turns.
4. The mouse hook suppresses unsnapped physical movement while enabled.
5. The app places the cursor only at the locked position.

This makes horizontal, vertical, and diagonal movement feel consistent across drawing apps, annotation tools, desktop workflows, and browser-based canvases.

## Quick Start

Requirements:

- Windows 10 or later.
- .NET Framework 4.8 runtime.

Build and run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1 -Configuration Release
.\src\PrecisionCursor\bin\Release\PrecisionCursor.exe
```

The app starts in the Windows tray. Press `Ctrl+Alt+L` to enable it, move the mouse, and press `Esc` whenever you want normal cursor movement back.

For detailed usage notes and troubleshooting, see [docs/USAGE.md](docs/USAGE.md).

## Test

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test.ps1 -Configuration Release
```

The tests cover the core snapping and direction-lock logic separately from the tray app.

## Project Structure

```text
assets/                      Project logo
scripts/                     Build and test scripts
src/PrecisionCursor/         Windows Forms tray app, hooks, raw input
src/PrecisionCursor.Core/    Cursor snapping and smoothing logic
tests/PrecisionCursor.Tests/ Console test runner for core behavior
```

## Roadmap

- Configurable hotkeys.
- Optional startup on login.
- User-selectable lock modes, such as horizontal-only or vertical-only.
- Release packaging for easier installation.

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.

## Security

If you find a security issue, please follow [SECURITY.md](SECURITY.md).

## Privacy

Precision Cursor runs locally and does not include telemetry or network features. See [docs/PRIVACY.md](docs/PRIVACY.md).

## License

Precision Cursor is released under the [MIT License](LICENSE).

## Logo

The logo was generated with GPT Image and integrated as the Windows executable and tray icon.
