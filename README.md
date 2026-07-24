# Counter Overlay

A lightweight always-on-top overlay for Windows that displays a counter over any game or application. Press a customizable hotkey to increment it, and another to reset it back to zero.

## Features

- Transparent, always-on-top overlay that sits above fullscreen/windowed games
- Click-through by default so it never blocks game input
- Global hotkeys (work even while the game has focus) for increment and reset
- Fully customizable: hotkeys, label text, font size, text color, and position
- Lives in the system tray — right-click for Settings, Reset, or Exit
- Settings and overlay position are saved automatically between sessions

## Requirements

- Windows 10/11
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (or the SDK, if building from source)

## Build & Run

```
cd CounterOverlay
dotnet run
```

Or build a standalone executable:

```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The resulting `.exe` will be under `bin/Release/net8.0-windows/win-x64/publish/`.

## Usage

1. Launch the app — a small counter overlay appears in the top-left corner, and an icon appears in the system tray.
2. Press **F7** (default) to increment the counter, and **F8** (default) to reset it. These work globally, even while a game is focused.
3. Right-click the tray icon and choose **Settings** to:
   - Rebind either hotkey (click the box, then press your desired key combo)
   - Change the label, font size, and text color
   - Toggle click-through mode — uncheck it to drag the overlay to a new position, then re-check it before playing
4. Right-click the tray icon and choose **Exit** to close the app.

## Notes

- If a hotkey fails to register, it's likely already bound by another running application. Pick a different combination in Settings.
- The overlay uses a layered, click-through window so it should display over both windowed and borderless-fullscreen games. True exclusive-fullscreen games may not show any overlay, which is a Windows/DirectX limitation, not specific to this app — switch the game to borderless/windowed mode for the overlay to appear.
