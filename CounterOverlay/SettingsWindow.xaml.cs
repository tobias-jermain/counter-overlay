using System.Windows;
using System.Windows.Input;

namespace CounterOverlay;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly Action _onSaved;
    private readonly Action _onResetCounter;
    private readonly Action _onExit;

    private uint _incrementMod;
    private uint _incrementVk;
    private uint _resetMod;
    private uint _resetVk;
    private MouseButtonBinding _incrementMouse;
    private MouseButtonBinding _resetMouse;

    public SettingsWindow(AppSettings settings, Action onSaved, Action onResetCounter, Action onExit)
    {
        InitializeComponent();
        _settings = settings;
        _onSaved = onSaved;
        _onResetCounter = onResetCounter;
        _onExit = onExit;

        _incrementMod = settings.IncrementModifiers;
        _incrementVk = settings.IncrementKey;
        _resetMod = settings.ResetModifiers;
        _resetVk = settings.ResetKey;
        _incrementMouse = settings.IncrementMouseButton;
        _resetMouse = settings.ResetMouseButton;

        IncrementBox.Text = HotkeyDisplay.Format(_incrementMod, _incrementVk, _incrementMouse);
        ResetBox.Text = HotkeyDisplay.Format(_resetMod, _resetVk, _resetMouse);
        LabelBox.Text = settings.Label;
        FontSizeBox.Text = settings.FontSize.ToString();
        ColorBox.Text = settings.TextColor;
        ClickThroughBox.IsChecked = settings.ClickThrough;
    }

    private void HotkeyBox_GotFocus(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Press a key, or middle/side mouse button...";
        StatusText.Foreground = System.Windows.Media.Brushes.Orange;
    }

    private void IncrementBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        e.Handled = true;
        if (!HotkeyDisplay.TryCapture(e, out var mod, out var vk)) return;
        _incrementMod = mod;
        _incrementVk = vk;
        _incrementMouse = MouseButtonBinding.None; // a keyboard binding replaces any mouse binding
        IncrementBox.Text = HotkeyDisplay.Format(mod, vk, _incrementMouse);
        StatusText.Text = "";
    }

    private void ResetBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        e.Handled = true;
        if (!HotkeyDisplay.TryCapture(e, out var mod, out var vk)) return;
        _resetMod = mod;
        _resetVk = vk;
        _resetMouse = MouseButtonBinding.None;
        ResetBox.Text = HotkeyDisplay.Format(mod, vk, _resetMouse);
        StatusText.Text = "";
    }

    private void IncrementBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var button = HotkeyDisplay.CaptureMouse(e);
        if (button == MouseButtonBinding.None) return; // let left/right click focus the box normally
        e.Handled = true;
        _incrementMouse = button;
        IncrementBox.Text = HotkeyDisplay.Format(_incrementMod, _incrementVk, button);
        StatusText.Text = "";
    }

    private void ResetBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var button = HotkeyDisplay.CaptureMouse(e);
        if (button == MouseButtonBinding.None) return;
        e.Handled = true;
        _resetMouse = button;
        ResetBox.Text = HotkeyDisplay.Format(_resetMod, _resetVk, button);
        StatusText.Text = "";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _settings.IncrementModifiers = _incrementMod;
        _settings.IncrementKey = _incrementVk;
        _settings.ResetModifiers = _resetMod;
        _settings.ResetKey = _resetVk;
        _settings.IncrementMouseButton = _incrementMouse;
        _settings.ResetMouseButton = _resetMouse;
        _settings.Label = string.IsNullOrWhiteSpace(LabelBox.Text) ? "Count" : LabelBox.Text;
        _settings.ClickThrough = ClickThroughBox.IsChecked ?? true;

        if (double.TryParse(FontSizeBox.Text, out var fontSize) && fontSize > 0)
            _settings.FontSize = fontSize;

        if (!string.IsNullOrWhiteSpace(ColorBox.Text))
            _settings.TextColor = ColorBox.Text.Trim();

        _settings.Save();
        _onSaved();
        StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
        StatusText.Text = "Saved.";
    }

    private void ResetCounterButton_Click(object sender, RoutedEventArgs e) => _onResetCounter();

    private void ExitButton_Click(object sender, RoutedEventArgs e) => _onExit();
}
