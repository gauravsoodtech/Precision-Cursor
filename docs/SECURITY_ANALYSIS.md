# Security Analysis

DPI Assistant is a local Windows mouse utility. It is designed to be visible, understandable, and removable like a normal tray application.

## Purpose

The app helps with precise cursor movement for Paint, drawing, annotation, alignment, and accessibility workflows. When enabled, it constrains pointer movement to horizontal, vertical, or diagonal lines.

## Windows APIs Used

- `RegisterRawInputDevices`: receives raw mouse movement deltas so the app can understand the user's intended movement direction.
- `SetWindowsHookEx` with `WH_MOUSE_LL`: observes mouse movement and suppresses unsnapped physical movement while assistance is enabled.
- `SetWindowsHookEx` with `WH_KEYBOARD_LL`: handles the global toggle hotkey.
- `SetCursorPos`: places the cursor at the assisted position.
- `GetAsyncKeyState`: checks whether the toggle modifier keys are currently pressed.

These APIs are documented Windows user-mode APIs. DPI Assistant does not install a kernel driver.

## Observable Behavior

DPI Assistant is detectable as a normal Windows process and executable. Security, administration, and anti-cheat tools may observe:

- Process name and executable path.
- Assembly metadata, icon, and file version details.
- Low-level keyboard and mouse hook registration.
- Raw input registration.
- Programmatic cursor placement while assistance is enabled.
- Local files created by build or packaging tools.

This visibility is intentional. The app does not try to hide, bypass monitoring, or impersonate a Windows system component.

## Privacy and Network Behavior

DPI Assistant does not include telemetry, analytics, update checks, network clients, or upload behavior. It does not collect, sell, or transmit user data.

Input handling is local and in memory:

- Mouse movement is used to calculate the assisted cursor position.
- `Ctrl+Alt+L` toggles assistance.

The app does not create logs by default.

## Explicit Non-Goals

DPI Assistant does not:

- Install drivers.
- Inject into games or other applications.
- Hide processes, files, windows, or hooks.
- Escalate privileges.
- Modify anti-cheat, EDR, antivirus, or kernel security tools.
- Bypass security monitoring.
- Access the network.
- Collect user data.

## Risk Notes

Global input tools can be sensitive in some environments because they observe input and can move the cursor. DPI Assistant keeps that behavior transparent, documented, and user-controlled. Users who need the lowest possible risk with security-sensitive software should close any global input utility they do not need for that session.
