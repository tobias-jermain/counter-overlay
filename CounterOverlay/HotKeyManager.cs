using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CounterOverlay;

/// <summary>
/// Registers system-wide hotkeys using a hidden window to receive WM_HOTKEY messages.
/// Global hotkeys work even when the game has focus, since RegisterHotKey is OS-level.
/// </summary>
public sealed class HotKeyManager : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int WM_HOTKEY = 0x0312;

    private readonly Window _messageWindow;
    private HwndSource? _source;
    private readonly Dictionary<int, Action> _handlers = new();
    private int _nextId = 1;

    public HotKeyManager(Window messageWindow)
    {
        _messageWindow = messageWindow;
        _messageWindow.SourceInitialized += (_, _) =>
        {
            _source = (HwndSource)PresentationSource.FromVisual(_messageWindow)!;
            _source.AddHook(WndProc);
        };
    }

    /// <summary>Registers a hotkey; returns the id used, or -1 if registration failed (e.g. already bound by another app).</summary>
    public int Register(uint modifiers, uint vk, Action onPressed)
    {
        if (vk == 0 || _source == null) return -1;
        int id = _nextId++;
        if (!RegisterHotKey(_source.Handle, id, modifiers, vk))
            return -1;
        _handlers[id] = onPressed;
        return id;
    }

    public void UnregisterAll()
    {
        if (_source == null) return;
        foreach (var id in _handlers.Keys)
            UnregisterHotKey(_source.Handle, id);
        _handlers.Clear();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && _handlers.TryGetValue(wParam.ToInt32(), out var action))
        {
            action();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        UnregisterAll();
        _source?.RemoveHook(WndProc);
    }
}
