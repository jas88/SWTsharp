using System.Runtime.InteropServices;
#if NET8_0_OR_GREATER
using System.Buffers;
#endif
using SWTSharp.Events;

namespace SWTSharp;

/// <summary>
/// Base class for all SWT widgets.
/// This class provides the fundamental functionality for all user interface objects.
/// </summary>
public abstract class Widget : IDisposable
{
    private bool _disposed;
    private int _style;
    private object? _data;
    private Display? _display;
    private Dictionary<int, List<IListener>>? _eventTable;

    /// <summary>
    /// The widget's style bits.
    /// </summary>
    public int Style => _style;

    /// <summary>
    /// The display associated with this widget.
    /// </summary>
    public Display? Display => _display;

    /// <summary>
    /// Gets or sets application-defined data associated with the widget.
    /// </summary>
    public object? Data
    {
        get
        {
            CheckWidget();
            return _data;
        }
        set
        {
            CheckWidget();
            _data = value;
        }
    }

    /// <summary>
    /// Returns true if the widget has been disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Creates a new widget.
    /// </summary>
    protected Widget(Widget? parent, int style)
    {
        _style = style;
        if (parent != null)
        {
            _display = parent.Display;
            parent.CheckWidget();
        }
    }

    /// <summary>
    /// Checks whether the widget is valid (not disposed and on correct thread).
    /// </summary>
    /// <exception cref="SWTDisposedException">When widget is disposed</exception>
    /// <exception cref="SWTInvalidThreadException">When called from wrong thread</exception>
    protected void CheckWidget()
    {
        if (_disposed)
        {
            throw new SWTDisposedException("Widget has been disposed");
        }

        if (_display != null && !_display.IsValidThread())
        {
            throw new SWTInvalidThreadException("Invalid thread access");
        }
    }

    /// <summary>
    /// Disposes the widget and releases all associated resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the widget.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                ReleaseWidget();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Releases widget-specific resources. Override in subclasses.
    /// </summary>
    protected virtual void ReleaseWidget()
    {
        // LEAK-001: Clear event handlers to prevent memory leaks
        if (_eventTable != null)
        {
            foreach (var eventType in _eventTable.Keys.ToList())
            {
                if (_eventTable.TryGetValue(eventType, out var listeners))
                {
                    listeners.Clear();
                }
            }
            _eventTable.Clear();
            _eventTable = null;
        }

        _data = null;
        _display = null;
    }

    /// <summary>
    /// Returns the platform-specific handle for this widget.
    /// </summary>
    public virtual IntPtr Handle { get; protected set; } = IntPtr.Zero;

    /// <summary>
    /// Gets platform-specific data associated with the widget.
    /// </summary>
    public T? GetData<T>(string key) where T : class
    {
        CheckWidget();
        if (_data is Dictionary<string, object> dict && dict.TryGetValue(key, out var value))
        {
            return value as T;
        }
        return null;
    }

    /// <summary>
    /// Sets platform-specific data associated with the widget.
    /// </summary>
    public void SetData(string key, object value)
    {
        CheckWidget();
        if (_data == null)
        {
            _data = new Dictionary<string, object>();
        }

        if (_data is Dictionary<string, object> dict)
        {
            dict[key] = value;
        }
    }

    /// <summary>
    /// Returns a string representation of the widget.
    /// </summary>
    public override string ToString()
    {
        return $"{GetType().Name} {{style={_style:X}, disposed={_disposed}}}";
    }

    /// <summary>
    /// Sets the display for this widget.
    /// </summary>
    protected void SetDisplay(Display display)
    {
        _display = display;
    }

    /// <summary>
    /// Adds the listener to the collection of listeners who will be notified when an event
    /// of the given type occurs.
    /// </summary>
    /// <param name="eventType">The type of event to listen for</param>
    /// <param name="listener">The listener which should be notified when the event occurs</param>
    /// <exception cref="ArgumentNullException">If the listener is null</exception>
    public void AddListener(int eventType, IListener listener)
    {
        CheckWidget();
        if (listener == null)
        {
            throw new ArgumentNullException(nameof(listener));
        }

        _eventTable ??= new Dictionary<int, List<IListener>>();

        if (!_eventTable.TryGetValue(eventType, out var listeners))
        {
            listeners = new List<IListener>();
            _eventTable[eventType] = listeners;
        }

        listeners.Add(listener);
    }

    /// <summary>
    /// Removes the listener from the collection of listeners who will be notified when an event
    /// of the given type occurs.
    /// </summary>
    /// <param name="eventType">The type of event to listen for</param>
    /// <param name="listener">The listener which should no longer be notified when the event occurs</param>
    /// <exception cref="ArgumentNullException">If the listener is null</exception>
    public void RemoveListener(int eventType, IListener listener)
    {
        CheckWidget();
        if (listener == null)
        {
            throw new ArgumentNullException(nameof(listener));
        }

        if (_eventTable != null && _eventTable.TryGetValue(eventType, out var listeners))
        {
            listeners.Remove(listener);
            if (listeners.Count == 0)
            {
                _eventTable.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// Notifies all of the receiver's listeners for events of the given type that one such
    /// event has occurred by invoking their handleEvent() method.
    /// </summary>
    /// <param name="eventType">The type of event which has occurred</param>
    /// <param name="event">The event data</param>
    public void NotifyListeners(int eventType, Event @event)
    {
        if (_eventTable != null && _eventTable.TryGetValue(eventType, out var listeners))
        {
            @event.Type = eventType;
            @event.Widget = this;
            @event.Display ??= _display;

#if NET8_0_OR_GREATER
            // Use ArrayPool for better performance on .NET 8+
            var count = listeners.Count;
            var listenersCopy = ArrayPool<IListener>.Shared.Rent(count);
            try
            {
                listeners.CopyTo(listenersCopy, 0);
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        listenersCopy[i].HandleEvent(@event);
                    }
                    catch (OutOfMemoryException)
                    {
                        // SEC-005: Critical exception - rethrow immediately
                        throw;
                    }
                    catch (StackOverflowException)
                    {
                        // SEC-005: Critical exception - rethrow immediately
                        throw;
                    }
                    catch (AccessViolationException)
                    {
                        // SEC-005: Critical exception - rethrow immediately
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // SEC-005: Log properly instead of swallowing
                        var errorMessage = $"Exception in event handler for {GetType().Name}: {ex.GetType().Name} - {ex.Message}";
                        Console.Error.WriteLine(errorMessage);
                        System.Diagnostics.Debug.WriteLine(errorMessage);
                        System.Diagnostics.Trace.WriteLine(errorMessage);
                        NotifyApplicationError(ex, @event);
                    }
                }
            }
            finally
            {
                ArrayPool<IListener>.Shared.Return(listenersCopy, clearArray: true);
            }
#else
            // Create a copy of the listeners list to avoid modification issues during iteration
            var listenersCopy = new List<IListener>(listeners);
            foreach (var listener in listenersCopy)
            {
                try
                {
                    listener.HandleEvent(@event);
                }
                catch (OutOfMemoryException)
                {
                    // SEC-005: Critical exception - rethrow immediately
                    throw;
                }
                catch (StackOverflowException)
                {
                    // SEC-005: Critical exception - rethrow immediately
                    throw;
                }
                catch (AccessViolationException)
                {
                    // SEC-005: Critical exception - rethrow immediately
                    throw;
                }
                catch (Exception ex)
                {
                    // SEC-005: Log properly instead of swallowing
                    var errorMessage = $"Exception in event handler for {GetType().Name}: {ex.GetType().Name} - {ex.Message}";
                    Console.Error.WriteLine(errorMessage);
                    System.Diagnostics.Debug.WriteLine(errorMessage);
                    System.Diagnostics.Trace.WriteLine(errorMessage);
                    NotifyApplicationError(ex, @event);
                }
            }
#endif
        }
    }

    /// <summary>
    /// Adds a typed selection listener.
    /// </summary>
    /// <param name="listener">The selection listener to add</param>
    public void AddSelectionListener(ISelectionListener listener)
    {
        CheckWidget();
        if (listener == null)
        {
            throw new ArgumentNullException(nameof(listener));
        }

        var typedListener = new TypedListener(listener);
        AddListener(SWT.Selection, typedListener);
        AddListener(SWT.DefaultSelection, typedListener);
    }

    /// <summary>
    /// Removes a typed selection listener.
    /// </summary>
    /// <param name="listener">The selection listener to remove</param>
    public void RemoveSelectionListener(ISelectionListener listener)
    {
        CheckWidget();
        if (listener == null)
        {
            throw new ArgumentNullException(nameof(listener));
        }

        if (_eventTable != null)
        {
            RemoveTypedListener(SWT.Selection, listener);
            RemoveTypedListener(SWT.DefaultSelection, listener);
        }
    }

    /// <summary>
    /// Adds a typed mouse listener.
    /// </summary>
    /// <param name="listener">The mouse listener to add</param>
    public void AddMouseListener(IMouseListener listener)
    {
        CheckWidget();
        if (listener == null)
        {
            throw new ArgumentNullException(nameof(listener));
        }

        var typedListener = new TypedListener(listener);
        AddListener(SWT.MouseDown, typedListener);
        AddListener(SWT.MouseUp, typedListener);
        AddListener(SWT.MouseDoubleClick, typedListener);
    }

    /// <summary>
    /// Removes a typed mouse listener.
    /// </summary>
    /// <param name="listener">The mouse listener to remove</param>
    public void RemoveMouseListener(IMouseListener listener)
    {
        CheckWidget();
        if (listener == null)
        {
            throw new ArgumentNullException(nameof(listener));
        }

        if (_eventTable != null)
        {
            RemoveTypedListener(SWT.MouseDown, listener);
            RemoveTypedListener(SWT.MouseUp, listener);
            RemoveTypedListener(SWT.MouseDoubleClick, listener);
        }
    }

    /// <summary>
    /// Adds a typed key listener.
    /// </summary>
    /// <param name="listener">The key listener to add</param>
    public void AddKeyListener(IKeyListener listener)
    {
        CheckWidget();
        if (listener == null)
        {
            throw new ArgumentNullException(nameof(listener));
        }

        var typedListener = new TypedListener(listener);
        AddListener(SWT.KeyDown, typedListener);
        AddListener(SWT.KeyUp, typedListener);
    }

    /// <summary>
    /// Removes a typed key listener.
    /// </summary>
    /// <param name="listener">The key listener to remove</param>
    public void RemoveKeyListener(IKeyListener listener)
    {
        CheckWidget();
        if (listener == null)
        {
            throw new ArgumentNullException(nameof(listener));
        }

        if (_eventTable != null)
        {
            RemoveTypedListener(SWT.KeyDown, listener);
            RemoveTypedListener(SWT.KeyUp, listener);
        }
    }

    /// <summary>
    /// Helper method to remove a typed listener wrapped in a TypedListener.
    /// </summary>
    private void RemoveTypedListener(int eventType, object listener)
    {
        if (_eventTable != null && _eventTable.TryGetValue(eventType, out var listeners))
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                if (listeners[i] is TypedListener typedListener &&
                    typedListener.GetEventListener() == listener)
                {
                    listeners.RemoveAt(i);
                }
            }

            if (listeners.Count == 0)
            {
                _eventTable.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// SEC-005: Notify application-level error handler of unhandled exceptions in event handlers.
    /// This can be overridden by Display or other global handlers.
    /// </summary>
    protected virtual void NotifyApplicationError(Exception ex, Event @event)
    {
        // Subclasses or Display can override this to provide centralized error handling
        // Default implementation does nothing beyond logging (already done in NotifyListeners)
    }
}
