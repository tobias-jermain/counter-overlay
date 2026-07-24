namespace CounterOverlay;

/// <summary>
/// Installs a system-wide low-level mouse hook so extra mouse buttons (middle, X1, X2)
/// can act as counter hotkeys. RegisterHotKey only handles keyboard keys, so this is the
/// mouse equivalent. The hook observes button presses without consuming them, leaving the
/// click to reach the game as normal.
/// </summary>
public sealed class MouseHookManager : IDisposable
{
    private readonly Dictionary<MouseButtonBinding, Action> _handlers = new();

    // Keep a managed reference to the delegate; if it is collected while the hook is
    // installed the callback address becomes invalid and the process crashes.
    private readonly NativeMethods.LowLevelMouseProc _proc;
    private IntPtr _hookHandle = IntPtr.Zero;

    /// <summary>While true, bound buttons are ignored — used so rebinding in Settings doesn't also fire the old binding.</summary>
    public bool Suspended { get; set; }

    public MouseHookManager()
    {
        _proc = HookCallback;
    }

    /// <summary>Binds an action to a mouse button. Passing None clears nothing and is ignored.</summary>
    public void Bind(MouseButtonBinding button, Action onPressed)
    {
        if (button == MouseButtonBinding.None) return;
        _handlers[button] = onPressed;
        EnsureHookInstalled();
    }

    public void UnbindAll()
    {
        _handlers.Clear();
        RemoveHook();
    }

    private void EnsureHookInstalled()
    {
        if (_hookHandle != IntPtr.Zero) return;
        _hookHandle = NativeMethods.SetWindowsHookEx(
            NativeMethods.WH_MOUSE_LL, _proc, NativeMethods.GetModuleHandle(null), 0);
    }

    private void RemoveHook()
    {
        if (_hookHandle == IntPtr.Zero) return;
        NativeMethods.UnhookWindowsHookEx(_hookHandle);
        _hookHandle = IntPtr.Zero;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && !Suspended)
        {
            var button = Decode(wParam.ToInt32(), lParam);
            if (button != MouseButtonBinding.None && _handlers.TryGetValue(button, out var action))
            {
                // The hook runs on the low-level input thread; marshal onto the UI thread
                // and return immediately so we never stall global mouse input.
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(action);
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private static MouseButtonBinding Decode(int message, IntPtr lParam)
    {
        switch (message)
        {
            case NativeMethods.WM_MBUTTONDOWN:
                return MouseButtonBinding.Middle;

            case NativeMethods.WM_XBUTTONDOWN:
                var data = System.Runtime.InteropServices.Marshal
                    .PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
                var xButton = (data.MouseData >> 16) & 0xFFFF;
                return xButton == NativeMethods.XBUTTON1
                    ? MouseButtonBinding.XButton1
                    : xButton == NativeMethods.XBUTTON2
                        ? MouseButtonBinding.XButton2
                        : MouseButtonBinding.None;

            default:
                return MouseButtonBinding.None;
        }
    }

    public void Dispose() => UnbindAll();
}
