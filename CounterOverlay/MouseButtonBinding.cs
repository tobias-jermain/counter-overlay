namespace CounterOverlay;

/// <summary>
/// Extra mouse buttons that can be bound to a counter action. Left and right
/// are deliberately excluded — binding them would swallow or double up on normal
/// clicking in games.
/// </summary>
public enum MouseButtonBinding
{
    None = 0,
    Middle = 1,
    XButton1 = 2,
    XButton2 = 3,
}
