# Contributing

Thanks for taking the time to improve DPI Assistant.

## Development Setup

Requirements:

- Windows 10 or later.
- .NET Framework 4.8 Developer Pack.
- Visual Studio Build Tools, Visual Studio, or another MSBuild installation that can build .NET Framework projects.

Build:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1 -Configuration Release
```

Test:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test.ps1 -Configuration Release
```

## Pull Request Checklist

- Keep changes focused on one behavior or documentation improvement.
- Add or update tests when changing cursor snapping or smoothing logic.
- Run the Release test script before opening a pull request.
- Do not commit local logs, build output, credentials, tokens, or machine-specific files.
- Update `README.md` or `CHANGELOG.md` when the user-facing behavior changes.

## Project Notes

- `DpiAssistant.Core` should stay independent from Windows Forms so cursor logic remains easy to test.
- The tray app owns global hooks, raw input, notifications, and application lifecycle.
- Safety matters: `Esc` must remain a reliable immediate disable path.
