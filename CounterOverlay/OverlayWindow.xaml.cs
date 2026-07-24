using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace CounterOverlay;

public partial class OverlayWindow : Window
{
    private int _count;
    private bool _clickThrough;

    public OverlayWindow()
    {
        InitializeComponent();
    }

    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            CountText.Text = _count.ToString();
        }
    }

    public void ApplySettings(AppSettings settings)
    {
        Left = settings.Left;
        Top = settings.Top;
        CountText.FontSize = settings.FontSize;
        LabelText.Text = settings.Label;
        try
        {
            CountText.Foreground = (Brush)new BrushConverter().ConvertFromString(settings.TextColor)!;
        }
        catch
        {
            CountText.Foreground = Brushes.White;
        }
        SetClickThrough(settings.ClickThrough);
    }

    public void SetClickThrough(bool enabled)
    {
        _clickThrough = enabled;
        if (!IsLoaded) return;

        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return;

        int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
        exStyle |= NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE;

        if (enabled)
            exStyle |= NativeMethods.WS_EX_TRANSPARENT;
        else
            exStyle &= ~NativeMethods.WS_EX_TRANSPARENT;

        NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, exStyle);

        // Give visual feedback that the overlay is draggable/editable in this mode.
        RootBorder.BorderBrush = enabled
            ? new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF))
            : Brushes.Orange;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        SetClickThrough(_clickThrough);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (!_clickThrough)
        {
            DragMove();
        }
    }
}
