namespace SWTSharp.Events;

/// <summary>
/// Instances of this class are internal to SWT and are used to connect typed listeners
/// to the underlying native event system.
/// </summary>
/// <remarks>
/// IMPORTANT: This class is not part of the SWT public API. It is marked public only so
/// that it can be shared within the packages provided by SWT. It should never be accessed
/// from application code.
/// </remarks>
public class TypedListener : IListener
{
    private readonly object _eventListener;

    /// <summary>
    /// Constructs a new instance of this class given an event listener.
    /// </summary>
    /// <param name="listener">The event listener to be stored in the receiver</param>
    /// <exception cref="ArgumentNullException">If the listener is null</exception>
    public TypedListener(object listener)
    {
        _eventListener = listener ?? throw new ArgumentNullException(nameof(listener));
    }

    /// <summary>
    /// Gets the receiver's event listener.
    /// </summary>
    /// <returns>The receiver's event listener</returns>
    public object GetEventListener() => _eventListener;

    /// <summary>
    /// Handles the given event.
    /// </summary>
    /// <param name="e">The event to be handled</param>
    public void HandleEvent(Event e)
    {
        switch (e.Type)
        {
            case SWT.Selection:
                if (_eventListener is ISelectionListener selectionListener)
                {
                    selectionListener.WidgetSelected(new SelectionEvent(e));
                }
                break;

            case SWT.DefaultSelection:
                if (_eventListener is ISelectionListener defaultSelectionListener)
                {
                    defaultSelectionListener.WidgetDefaultSelected(new SelectionEvent(e));
                }
                break;

            case SWT.MouseDown:
                if (_eventListener is IMouseListener mouseDownListener)
                {
                    mouseDownListener.MouseDown(new MouseEvent(e));
                }
                break;

            case SWT.MouseUp:
                if (_eventListener is IMouseListener mouseUpListener)
                {
                    mouseUpListener.MouseUp(new MouseEvent(e));
                }
                break;

            case SWT.MouseDoubleClick:
                if (_eventListener is IMouseListener mouseDoubleClickListener)
                {
                    mouseDoubleClickListener.MouseDoubleClick(new MouseEvent(e));
                }
                break;

            case SWT.MouseMove:
                if (_eventListener is IMouseMoveListener mouseMoveListener)
                {
                    mouseMoveListener.MouseMove(new MouseEvent(e));
                }
                break;

            case SWT.MouseEnter:
                if (_eventListener is IMouseTrackListener mouseEnterListener)
                {
                    mouseEnterListener.MouseEnter(new MouseEvent(e));
                }
                break;

            case SWT.MouseExit:
                if (_eventListener is IMouseTrackListener mouseExitListener)
                {
                    mouseExitListener.MouseExit(new MouseEvent(e));
                }
                break;

            case SWT.MouseHover:
                if (_eventListener is IMouseTrackListener mouseHoverListener)
                {
                    mouseHoverListener.MouseHover(new MouseEvent(e));
                }
                break;

            case SWT.KeyDown:
                if (_eventListener is IKeyListener keyDownListener)
                {
                    keyDownListener.KeyPressed(new KeyEvent(e));
                }
                break;

            case SWT.KeyUp:
                if (_eventListener is IKeyListener keyUpListener)
                {
                    keyUpListener.KeyReleased(new KeyEvent(e));
                }
                break;

            case SWT.FocusIn:
                if (_eventListener is IFocusListener focusInListener)
                {
                    focusInListener.FocusGained(new FocusEvent(e));
                }
                break;

            case SWT.FocusOut:
                if (_eventListener is IFocusListener focusOutListener)
                {
                    focusOutListener.FocusLost(new FocusEvent(e));
                }
                break;

            case SWT.Move:
                if (_eventListener is IControlListener moveListener)
                {
                    moveListener.ControlMoved(new ControlEvent(e));
                }
                break;

            case SWT.Resize:
                if (_eventListener is IControlListener resizeListener)
                {
                    resizeListener.ControlResized(new ControlEvent(e));
                }
                break;

            case SWT.Dispose:
                if (_eventListener is IDisposeListener disposeListener)
                {
                    disposeListener.WidgetDisposed(new DisposeEvent(e));
                }
                break;
        }
    }
}
