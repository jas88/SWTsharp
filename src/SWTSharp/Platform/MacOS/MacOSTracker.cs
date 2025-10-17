using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS (Cocoa) implementation of tracker widget.
/// Uses NSWindow overlay for rubber-band rectangle feedback.
/// </summary>
internal class MacOSTracker : IPlatformTracker
{
    private Rectangle[] _rectangles = Array.Empty<Rectangle>();
    private bool _stippled;
    private int _cursorType;
    private readonly IntPtr _parentView;
    private readonly int _style;
    private bool _disposed;
    private bool _tracking;
    private IntPtr _overlayWindow;
    private IntPtr _trackingArea;

    public event EventHandler<TrackerEventArgs>? MouseMove;
    public event EventHandler<TrackerEventArgs>? Resize;

    public MacOSTracker(IntPtr parentView, int style)
    {
        _parentView = parentView;
        _style = style;
    }

    public bool Open()
    {
        if (_tracking || _rectangles.Length == 0)
            return false;

        _tracking = true;

        try
        {
            // Create transparent overlay window
            _overlayWindow = CreateOverlayWindow();
            if (_overlayWindow == IntPtr.Zero)
                return false;

            // Get the main window to attach to
            IntPtr mainWindow = _parentView != IntPtr.Zero
                ? objc_msgSend(_parentView, sel_getUid("window"))
                : IntPtr.Zero;

            if (mainWindow != IntPtr.Zero)
            {
                // Add as child window
                objc_msgSend(mainWindow, sel_getUid("addChildWindow:ordered:"),
                    _overlayWindow, 1 /* NSWindowAbove */);
            }

            // Make window visible
            objc_msgSend(_overlayWindow, sel_getUid("orderFront:"), IntPtr.Zero);

            // Draw initial rectangles
            DrawRectangles();

            // Track mouse events
            bool completed = TrackMouse();

            // Clean up overlay
            if (mainWindow != IntPtr.Zero)
            {
                objc_msgSend(mainWindow, sel_getUid("removeChildWindow:"), _overlayWindow);
            }
            objc_msgSend(_overlayWindow, sel_getUid("close"));

            return completed;
        }
        finally
        {
            _tracking = false;
            _overlayWindow = IntPtr.Zero;
        }
    }

    public void Close()
    {
        _tracking = false;
    }

    public void SetRectangles(Rectangle[] rectangles)
    {
        _rectangles = rectangles ?? Array.Empty<Rectangle>();
    }

    public Rectangle[] GetRectangles()
    {
        return _rectangles;
    }

    public void SetStippled(bool stippled)
    {
        _stippled = stippled;
    }

    public bool GetStippled()
    {
        return _stippled;
    }

    public void SetCursor(int cursorType)
    {
        _cursorType = cursorType;
    }

    private IntPtr CreateOverlayWindow()
    {
        // Get screen frame
        IntPtr screen = objc_msgSend(objc_getClass("NSScreen"), sel_getUid("mainScreen"));
        NSRect screenFrame = objc_msgSend_stret_NSRect(screen, sel_getUid("frame"));

        // Create window with transparent background
        IntPtr window = objc_msgSend(objc_getClass("NSWindow"), sel_getUid("alloc"));

        window = objc_msgSend(window, sel_getUid("initWithContentRect:styleMask:backing:defer:"),
            screenFrame,
            0, // Borderless
            2, // NSBackingStoreBuffered
            false);

        // Make window transparent
        objc_msgSend(window, sel_getUid("setOpaque:"), false);
        objc_msgSend(window, sel_getUid("setBackgroundColor:"),
            objc_msgSend(objc_getClass("NSColor"), sel_getUid("clearColor")));
        objc_msgSend(window, sel_getUid("setIgnoresMouseEvents:"), false);
        objc_msgSend(window, sel_getUid("setLevel:"), 3); // NSFloatingWindowLevel

        return window;
    }

    private void DrawRectangles()
    {
        if (_overlayWindow == IntPtr.Zero)
            return;

        // Get content view
        IntPtr contentView = objc_msgSend(_overlayWindow, sel_getUid("contentView"));

        // Trigger redraw
        objc_msgSend(contentView, sel_getUid("setNeedsDisplay:"), true);

        // Custom drawing would go here - for simplicity, using CALayer
        IntPtr layer = objc_msgSend(contentView, sel_getUid("layer"));
        if (layer == IntPtr.Zero)
        {
            objc_msgSend(contentView, sel_getUid("setWantsLayer:"), true);
            layer = objc_msgSend(contentView, sel_getUid("layer"));
        }

        // Remove old sublayers
        IntPtr sublayers = objc_msgSend(layer, sel_getUid("sublayers"));
        if (sublayers != IntPtr.Zero)
        {
            objc_msgSend(sublayers, sel_getUid("makeObjectsPerformSelector:"),
                sel_getUid("removeFromSuperlayer"));
        }

        // Draw each rectangle as a CAShapeLayer
        foreach (var rect in _rectangles)
        {
            IntPtr shapeLayer = objc_msgSend(objc_getClass("CAShapeLayer"), sel_getUid("layer"));

            // Create path
            IntPtr path = objc_msgSend(objc_getClass("NSBezierPath"), sel_getUid("bezierPath"));
            NSRect nsRect = new NSRect { x = rect.X, y = rect.Y, width = rect.Width, height = rect.Height };
            objc_msgSend_rect(path, sel_getUid("appendBezierPathWithRect:"), nsRect);

            // Set path on shape layer
            IntPtr cgPath = objc_msgSend(path, sel_getUid("CGPath"));
            objc_msgSend(shapeLayer, sel_getUid("setPath:"), cgPath);

            // Set stroke color (white for visibility)
            IntPtr color = objc_msgSend(objc_getClass("NSColor"), sel_getUid("whiteColor"));
            IntPtr cgColor = objc_msgSend(color, sel_getUid("CGColor"));
            objc_msgSend(shapeLayer, sel_getUid("setStrokeColor:"), cgColor);
            objc_msgSend(shapeLayer, sel_getUid("setFillColor:"), IntPtr.Zero);
            objc_msgSend(shapeLayer, sel_getUid("setLineWidth:"), 2.0);

            if (_stippled)
            {
                // Set line dash pattern
                double[] pattern = { 5.0, 5.0 };
                IntPtr nsArray = CreateNSArray(pattern);
                objc_msgSend(shapeLayer, sel_getUid("setLineDashPattern:"), nsArray);
            }

            // Add to layer
            objc_msgSend(layer, sel_getUid("addSublayer:"), shapeLayer);
        }
    }

    private bool TrackMouse()
    {
        NSPoint startPoint = GetMouseLocation();
        Rectangle[] originalRects = (Rectangle[])_rectangles.Clone();

        // Run tracking loop
        IntPtr app = objc_msgSend(objc_getClass("NSApplication"), sel_getUid("sharedApplication"));

        while (_tracking)
        {
            // Get next event
            IntPtr evt = objc_msgSend(app, sel_getUid("nextEventMatchingMask:untilDate:inMode:dequeue:"),
                ulong.MaxValue, // NSAnyEventMask
                objc_msgSend(objc_getClass("NSDate"), sel_getUid("distantFuture")),
                objc_msgSend(objc_getClass("NSString"), sel_getUid("stringWithUTF8String:"), "kCFRunLoopDefaultMode"),
                true);

            if (evt == IntPtr.Zero)
                continue;

            int eventType = (int)objc_msgSend(evt, sel_getUid("type"));

            if (eventType == 2) // NSLeftMouseUp
            {
                return true; // Completed
            }
            else if (eventType == 10) // NSKeyDown
            {
                ushort keyCode = (ushort)objc_msgSend(evt, sel_getUid("keyCode"));
                if (keyCode == 53) // Escape key
                {
                    _rectangles = originalRects;
                    DrawRectangles();
                    return false; // Cancelled
                }
            }
            else if (eventType == 6) // NSMouseMoved or NSLeftMouseDragged
            {
                NSPoint currentPoint = GetMouseLocation();

                int dx = (int)(currentPoint.x - startPoint.x);
                int dy = (int)(currentPoint.y - startPoint.y);

                // Update rectangles
                for (int i = 0; i < _rectangles.Length; i++)
                {
                    if ((_style & SWT.RESIZE) != 0)
                    {
                        _rectangles[i] = new Rectangle(
                            originalRects[i].X,
                            originalRects[i].Y,
                            originalRects[i].Width + dx,
                            originalRects[i].Height + dy
                        );

                        var args = new TrackerEventArgs
                        {
                            X = (int)currentPoint.x,
                            Y = (int)currentPoint.y,
                            Bounds = _rectangles[i]
                        };
                        Resize?.Invoke(this, args);
                    }
                    else
                    {
                        _rectangles[i] = new Rectangle(
                            originalRects[i].X + dx,
                            originalRects[i].Y + dy,
                            originalRects[i].Width,
                            originalRects[i].Height
                        );
                    }
                }

                DrawRectangles();

                var moveArgs = new TrackerEventArgs
                {
                    X = (int)currentPoint.x,
                    Y = (int)currentPoint.y,
                    Bounds = _rectangles.Length > 0 ? _rectangles[0] : new Rectangle()
                };
                MouseMove?.Invoke(this, moveArgs);
            }

            // Send event to app
            objc_msgSend(app, sel_getUid("sendEvent:"), evt);
        }

        return false;
    }

    private NSPoint GetMouseLocation()
    {
        IntPtr location = objc_msgSend(objc_getClass("NSEvent"), sel_getUid("mouseLocation"));
        return Marshal.PtrToStructure<NSPoint>(location);
    }

    private IntPtr CreateNSArray(double[] values)
    {
        IntPtr array = objc_msgSend(objc_getClass("NSMutableArray"), sel_getUid("arrayWithCapacity:"), values.Length);
        foreach (double val in values)
        {
            IntPtr num = objc_msgSend(objc_getClass("NSNumber"), sel_getUid("numberWithDouble:"), val);
            objc_msgSend(array, sel_getUid("addObject:"), num);
        }
        return array;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Close();
        System.GC.SuppressFinalize(this);
    }

    // Objective-C runtime declarations
    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr sel_getUid(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, int arg2);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, NSRect arg1, int arg2, int arg3, bool arg4);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, int arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, double arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg1, IntPtr arg2, IntPtr arg3, bool arg4);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, string arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, NSRect rect);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend_stret")]
    private static extern NSRect objc_msgSend_stret_NSRect(IntPtr receiver, IntPtr selector);

    [StructLayout(LayoutKind.Sequential)]
    private struct NSRect
    {
        public double x;
        public double y;
        public double width;
        public double height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NSPoint
    {
        public double x;
        public double y;
    }
}
