# Usage

Precision Cursor runs as a Windows tray app. It does not open a large main window.

## Start the App

Build the app in Release mode:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1 -Configuration Release
```

Run the executable:

```powershell
.\src\PrecisionCursor\bin\Release\PrecisionCursor.exe
```

Look for the Precision Cursor icon in the Windows tray.

## Controls

| Action | Control |
| --- | --- |
| Enable or disable line lock | `Ctrl+Alt+L` |
| Disable immediately | `Esc` |
| Toggle from tray | Double-click the tray icon |
| Open controls | Right-click the tray icon |
| Quit | Tray menu -> `Exit` |

## Movement Behavior

When enabled, Precision Cursor detects your current movement direction and snaps the pointer to the nearest of these directions:

- Left or right.
- Up or down.
- Up-left or up-right diagonal.
- Down-left or down-right diagonal.

You do not need to hold a mouse button. The lock applies to normal cursor movement while enabled.

## Tips

- Use `Esc` before switching to tasks that need fully free cursor movement.
- Move with a clear direction when starting a line so the lock can settle quickly.
- If a diagonal looks too stepped in a drawing app, try slower movement or a lower mouse sensitivity.
- If the app feels stuck, press `Esc` or exit from the tray menu.

## Troubleshooting

### The tray icon is hidden

Open the Windows tray overflow menu and drag the Precision Cursor icon into the visible tray area.

### The hotkey does not toggle the app

Another app may already be using `Ctrl+Alt+L`. Use the tray menu to toggle Precision Cursor, or close the conflicting app.

### The app does not start

Install the .NET Framework 4.8 runtime and build the project again in Release mode.

### Movement is not being constrained

Make sure the tray status says `Enabled`. If it still does not lock movement, exit the app and start it again.
