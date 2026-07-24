using System.Windows.Input;

namespace CounterOverlay;

internal static class HotkeyDisplay
{
    /// <summary>Converts a key press event into a virtual-key code and modifier flags, ignoring bare modifier keys.</summary>
    public static bool TryCapture(System.Windows.Input.KeyEventArgs e, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        {
            return false;
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) modifiers |= NativeMethods.MOD_CONTROL;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) modifiers |= NativeMethods.MOD_ALT;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) modifiers |= NativeMethods.MOD_SHIFT;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Windows)) modifiers |= NativeMethods.MOD_WIN;

        vk = (uint)KeyInterop.VirtualKeyFromKey(key);
        return vk != 0;
    }

    /// <summary>Maps a WPF mouse press to a bindable extra button, or None for left/right.</summary>
    public static MouseButtonBinding CaptureMouse(System.Windows.Input.MouseButtonEventArgs e) => e.ChangedButton switch
    {
        System.Windows.Input.MouseButton.Middle => MouseButtonBinding.Middle,
        System.Windows.Input.MouseButton.XButton1 => MouseButtonBinding.XButton1,
        System.Windows.Input.MouseButton.XButton2 => MouseButtonBinding.XButton2,
        _ => MouseButtonBinding.None,
    };

    public static string Format(MouseButtonBinding button) => button switch
    {
        MouseButtonBinding.Middle => "Middle Mouse",
        MouseButtonBinding.XButton1 => "Mouse 4",
        MouseButtonBinding.XButton2 => "Mouse 5",
        _ => "(none)",
    };

    /// <summary>Formats whichever binding is active — a mouse button takes precedence over the key.</summary>
    public static string Format(uint modifiers, uint vk, MouseButtonBinding button) =>
        button != MouseButtonBinding.None ? Format(button) : Format(modifiers, vk);

    public static string Format(uint modifiers, uint vk)
    {
        if (vk == 0) return "(none)";
        var parts = new List<string>();
        if ((modifiers & NativeMethods.MOD_CONTROL) != 0) parts.Add("Ctrl");
        if ((modifiers & NativeMethods.MOD_ALT) != 0) parts.Add("Alt");
        if ((modifiers & NativeMethods.MOD_SHIFT) != 0) parts.Add("Shift");
        if ((modifiers & NativeMethods.MOD_WIN) != 0) parts.Add("Win");

        var key = KeyInterop.KeyFromVirtualKey((int)vk);
        parts.Add(key.ToString());
        return string.Join("+", parts);
    }
}
