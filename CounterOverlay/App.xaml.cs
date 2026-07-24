using System.Windows;
using System.Windows.Interop;
using System.Drawing;
using Forms = System.Windows.Forms;

namespace CounterOverlay;

public partial class App : System.Windows.Application
{
    private OverlayWindow _overlay = null!;
    private HotKeyManager _hotkeys = null!;
    private AppSettings _settings = null!;
    private Forms.NotifyIcon _trayIcon = null!;
    private SettingsWindow? _settingsWindow;

    private int _incrementHotkeyId = -1;
    private int _resetHotkeyId = -1;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settings = AppSettings.Load();

        _overlay = new OverlayWindow();
        _hotkeys = new HotKeyManager(_overlay);
        _overlay.ApplySettings(_settings);
        _overlay.Show();

        // RegisterHotKey requires the window handle, which now exists after Show().
        RegisterHotkeys();

        SetupTrayIcon();
    }

    private void RegisterHotkeys()
    {
        _hotkeys.UnregisterAll();
        _incrementHotkeyId = _hotkeys.Register(_settings.IncrementModifiers, _settings.IncrementKey, Increment);
        _resetHotkeyId = _hotkeys.Register(_settings.ResetModifiers, _settings.ResetKey, ResetCounter);

        if (_incrementHotkeyId == -1 || _resetHotkeyId == -1)
        {
            Forms.MessageBox.Show(
                "One or more hotkeys could not be registered (they may already be in use by another application). " +
                "Open Settings from the tray icon to choose different keys.",
                "Counter Overlay",
                Forms.MessageBoxButtons.OK,
                Forms.MessageBoxIcon.Warning);
        }
    }

    private void Increment()
    {
        _overlay.Count++;
    }

    private void ResetCounter()
    {
        _overlay.Count = 0;
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new Forms.NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "Counter Overlay"
        };

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Settings", null, (_, _) => OpenSettings());
        menu.Items.Add("Reset Counter", null, (_, _) => ResetCounter());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => OpenSettings();
    }

    private void OpenSettings()
    {
        if (_settingsWindow != null)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(_settings, OnSettingsSaved, ResetCounter, ExitApp);
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
    }

    private void OnSettingsSaved()
    {
        _overlay.ApplySettings(_settings);
        RegisterHotkeys();
    }

    private void ExitApp()
    {
        _hotkeys.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _settings.Left = _overlay.Left;
        _settings.Top = _overlay.Top;
        _settings.Save();
        Shutdown();
    }
}
