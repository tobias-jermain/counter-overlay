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

        IncrementBox.Text = HotkeyDisplay.Format(_incrementMod, _incrementVk);
        ResetBox.Text = HotkeyDisplay.Format(_resetMod, _resetVk);
        LabelBox.Text = settings.Label;
        FontSizeBox.Text = settings.FontSize.ToString();
        ColorBox.Text = settings.TextColor;
        ClickThroughBox.IsChecked = settings.ClickThrough;
    }

    private void HotkeyBox_GotFocus(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Press a key combination...";
        StatusText.Foreground = System.Windows.Media.Brushes.Orange;
    }

    private void IncrementBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        e.Handled = true;
        if (!HotkeyDisplay.TryCapture(e, out var mod, out var vk)) return;
        _incrementMod = mod;
        _incrementVk = vk;
        IncrementBox.Text = HotkeyDisplay.Format(mod, vk);
        StatusText.Text = "";
    }

    private void ResetBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        e.Handled = true;
        if (!HotkeyDisplay.TryCapture(e, out var mod, out var vk)) return;
        _resetMod = mod;
        _resetVk = vk;
        ResetBox.Text = HotkeyDisplay.Format(mod, vk);
        StatusText.Text = "";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _settings.IncrementModifiers = _incrementMod;
        _settings.IncrementKey = _incrementVk;
        _settings.ResetModifiers = _resetMod;
        _settings.ResetKey = _resetVk;
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
