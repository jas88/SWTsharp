# Drag-and-Drop Implementation Research for SWTSharp

**Research Date:** 2025-10-06
**Researcher:** Claude Code Research Agent
**Complexity Rating:** 9/10 (Very High)
**Estimated Effort:** 6-8 weeks (full-time developer)

## Executive Summary

Drag-and-drop (DND) is one of the most complex cross-platform features in SWT, requiring deep integration with platform-specific APIs, clipboard systems, and event handling. This research identifies significant challenges including COM interop on Win32, Objective-C protocol implementation on macOS, and GTK's signal-based system on Linux.

### Key Findings
- **Platform Divergence:** Each platform has fundamentally different DND architectures
- **COM Interop Required:** Win32 requires implementing IDropSource and IDropTarget COM interfaces in C#
- **Thread Safety Critical:** All platforms have strict threading requirements for DND operations
- **Data Format Complexity:** Cross-platform data format negotiation is non-trivial
- **Testing Challenge:** DND operations require simulating user interactions

---

## 1. Java SWT DND API Analysis

### 1.1 Core Classes

#### DragSource
```java
// Java SWT API structure
public class DragSource extends Widget {
    public DragSource(Control control, int style);
    public void setTransfer(Transfer[] transferTypes);
    public void addDragListener(DragSourceListener listener);
}
```

**Key Characteristics:**
- Attached to a specific Control widget
- Supports multiple transfer types simultaneously
- Style bits: DND.DROP_COPY, DND.DROP_MOVE, DND.DROP_LINK, DND.DROP_NONE
- Event-driven through DragSourceListener interface

#### DropTarget
```java
public class DropTarget extends Widget {
    public DropTarget(Control control, int style);
    public void setTransfer(Transfer[] transferTypes);
    public void addDropListener(DropListener listener);
}
```

**Key Characteristics:**
- Attached to a specific Control widget
- Can accept multiple data formats
- Provides drop feedback (cursor changes, visual highlighting)
- Event-driven through DropListener interface

### 1.2 Transfer Types Hierarchy

```
Transfer (abstract base)
├── TextTransfer (plain text)
├── RTFTransfer (rich text format)
├── HTMLTransfer (HTML content)
├── URLTransfer (URLs and file links)
├── FileTransfer (file paths)
├── ImageTransfer (bitmap images)
└── ByteArrayTransfer (custom binary data)
    └── User-defined custom transfers
```

**Transfer Design Pattern:**
- Abstract `Transfer` class defines data conversion contract
- Platform-specific implementations handle format registration
- `isSupportedType(TransferData)` - checks if format is supported
- `javaToNative(Object, TransferData)` - converts Java objects to native format
- `nativeToJava(TransferData)` - converts native format to Java objects

### 1.3 Event Model

#### DragSourceListener Events
```java
public interface DragSourceListener {
    void dragStart(DragSourceEvent event);    // Initiated when drag begins
    void dragSetData(DragSourceEvent event);  // Called when data is needed
    void dragFinished(DragSourceEvent event); // Called after drop completes
}
```

#### DropListener Events
```java
public interface DropListener {
    void dragEnter(DropTargetEvent event);    // Cursor enters drop target
    void dragOver(DropTargetEvent event);     // Cursor moves over target
    void dragLeave(DropTargetEvent event);    // Cursor leaves target
    void dropAccept(DropTargetEvent event);   // About to accept drop
    void drop(DropTargetEvent event);         // Drop operation performed
    void dragOperationChanged(DropTargetEvent event); // User changed operation
}
```

**Event Flow:**
1. User initiates drag → `dragStart` fired on DragSource
2. Cursor enters target → `dragEnter` fired on DropTarget
3. Cursor moves → `dragOver` fired continuously
4. User releases → `dropAccept` → `drop` → `dragFinished`

### 1.4 Drag Operations

| Operation | Description | Typical Modifier |
|-----------|-------------|------------------|
| DROP_COPY | Copy data to target | Ctrl (Win/Linux), Option (macOS) |
| DROP_MOVE | Move data to target | Default |
| DROP_LINK | Create link/shortcut | Ctrl+Shift (Win/Linux), Cmd+Option (macOS) |
| DROP_TARGET_MOVE | Target-initiated move | Platform-specific |
| DROP_NONE | Cancel operation | Escape key |

**Operation Negotiation:**
- DragSource specifies allowed operations (bitmask)
- DropTarget filters to supported subset
- User modifier keys influence final operation
- Event handlers can override operation in `dragOver`/`dragEnter`

---

## 2. Platform-Specific APIs

### 2.1 Win32: OLE Drag and Drop

#### COM Interface Requirements

**IDropSource Interface (Drag Source Side)**
```c
interface IDropSource : IUnknown {
    HRESULT QueryContinueDrag(BOOL fEscapePressed, DWORD grfKeyState);
    HRESULT GiveFeedback(DWORD dwEffect);
}
```

**IDropTarget Interface (Drop Target Side)**
```c
interface IDropTarget : IUnknown {
    HRESULT DragEnter(IDataObject *pDataObj, DWORD grfKeyState,
                      POINTL pt, DWORD *pdwEffect);
    HRESULT DragOver(DWORD grfKeyState, POINTL pt, DWORD *pdwEffect);
    HRESULT DragLeave(void);
    HRESULT Drop(IDataObject *pDataObj, DWORD grfKeyState,
                 POINTL pt, DWORD *pdwEffect);
}
```

**IDataObject Interface (Data Container)**
```c
interface IDataObject : IUnknown {
    HRESULT GetData(FORMATETC *pformatetcIn, STGMEDIUM *pmedium);
    HRESULT QueryGetData(FORMATETC *pformatetc);
    HRESULT SetData(FORMATETC *pformatetc, STGMEDIUM *pmedium, BOOL fRelease);
    // ... and others
}
```

#### C# COM Interop Challenge

**Option 1: Use Runtime Callable Wrappers (RCW)**
```csharp
// Import existing COM interfaces from Windows SDK
[ComImport]
[Guid("00000121-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IDropSource {
    [PreserveSig]
    int QueryContinueDrag(int fEscapePressed, uint grfKeyState);

    [PreserveSig]
    int GiveFeedback(uint dwEffect);
}
```

**Option 2: Use System.Runtime.InteropServices.ComTypes**
```csharp
// .NET provides COM types in System.Runtime.InteropServices.ComTypes
using ComTypes = System.Runtime.InteropServices.ComTypes;

// IDataObject already defined
ComTypes.IDataObject dataObject;
```

**Complexity Factors:**
- Reference counting (AddRef/Release) must be handled correctly
- HRESULT error codes must be mapped to exceptions
- Marshaling between managed and unmanaged memory
- Thread apartment state (STA required for DND)
- IDataObject requires implementing multiple storage medium types (HGLOBAL, ISTREAM, etc.)

#### Win32 API Functions

```csharp
[DllImport("ole32.dll")]
static extern int DoDragDrop(
    System.Runtime.InteropServices.ComTypes.IDataObject pDataObj,
    IDropSource pDropSource,
    uint dwOKEffects,
    out uint pdwEffect);

[DllImport("ole32.dll")]
static extern int RegisterDragDrop(IntPtr hwnd, IDropTarget pDropTarget);

[DllImport("ole32.dll")]
static extern int RevokeDragDrop(IntPtr hwnd);

[DllImport("ole32.dll")]
static extern int OleInitialize(IntPtr pvReserved);

[DllImport("ole32.dll")]
static extern void OleUninitialize();
```

**Initialization Requirements:**
- `OleInitialize()` must be called before any DND operations
- Window handle must be registered with `RegisterDragDrop()`
- Thread must be in STA (Single-Threaded Apartment) mode
- Unregister with `RevokeDragDrop()` before window destruction

#### Clipboard Format Registration

```csharp
[DllImport("user32.dll")]
static extern ushort RegisterClipboardFormat(string lpszFormat);

// Custom format registration
ushort CF_HTML = RegisterClipboardFormat("HTML Format");
ushort CF_RTF = RegisterClipboardFormat("Rich Text Format");
```

### 2.2 macOS: NSDraggingProtocols

#### NSDraggingSource Protocol

```objc
@protocol NSDraggingSource <NSObject>
@required
- (NSDragOperation)draggingSession:(NSDraggingSession *)session
    sourceOperationMaskForDraggingContext:(NSDraggingContext)context;

@optional
- (void)draggingSession:(NSDraggingSession *)session
    willBeginAtPoint:(NSPoint)screenPoint;
- (void)draggingSession:(NSDraggingSession *)session
    movedToPoint:(NSPoint)screenPoint;
- (void)draggingSession:(NSDraggingSession *)session
    endedAtPoint:(NSPoint)screenPoint
    operation:(NSDragOperation)operation;
@end
```

#### NSDraggingDestination Protocol

```objc
@protocol NSDraggingDestination <NSObject>
@optional
- (NSDragOperation)draggingEntered:(id<NSDraggingInfo>)sender;
- (NSDragOperation)draggingUpdated:(id<NSDraggingInfo>)sender;
- (void)draggingExited:(id<NSDraggingInfo>)sender;
- (BOOL)prepareForDragOperation:(id<NSDraggingInfo>)sender;
- (BOOL)performDragOperation:(id<NSDraggingInfo>)sender;
- (void)concludeDragOperation:(id<NSDraggingInfo>)sender;
@end
```

#### C# Implementation via Objective-C Runtime

```csharp
// Current MacOSPlatform pattern from SWTSharp
[DllImport("/usr/lib/libobjc.A.dylib")]
private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

// Implementing NSDraggingDestination requires:
// 1. Create custom NSView subclass
// 2. Override protocol methods
// 3. Register dragging types via registerForDraggedTypes:

IntPtr CreateDragEnabledView() {
    // Allocate custom view
    IntPtr viewClass = objc_getClass("NSView");
    IntPtr view = objc_msgSend(viewClass, sel_registerName("alloc"));
    view = objc_msgSend(view, sel_registerName("init"));

    // Register for dragging types
    IntPtr pasteboardTypes = CreateNSArray(new[] {
        "NSStringPboardType",
        "NSFilenamesPboardType"
    });

    IntPtr selRegister = sel_registerName("registerForDraggedTypes:");
    objc_msgSend(view, selRegister, pasteboardTypes);

    return view;
}
```

**Protocol Implementation Challenge:**
- Cannot directly implement Objective-C protocols in C#
- Must create Objective-C classes at runtime using `objc_allocateClassPair`
- Must register method implementations using `class_addMethod`
- Requires function pointers to C# methods (delegates must be kept alive)
- Type encoding strings required for method signatures

#### Initiating Drag Operations

```csharp
// Modern macOS API (10.7+)
IntPtr beginDraggingSession(IntPtr view, NSPoint location) {
    // Create NSDraggingItem
    IntPtr draggingItemClass = objc_getClass("NSDraggingItem");
    IntPtr draggingItem = objc_msgSend(draggingItemClass, sel_alloc);
    draggingItem = objc_msgSend(draggingItem, sel_initWithPasteboardWriter, pasteboardWriter);

    // Create array of dragging items
    IntPtr itemsArray = CreateNSArray(new[] { draggingItem });

    // Begin dragging session
    IntPtr selBeginDragging = sel_registerName("beginDraggingSessionWithItems:event:source:");
    return objc_msgSend(view, selBeginDragging, itemsArray, eventPtr, sourcePtr);
}
```

#### Pasteboard (Clipboard) Integration

```objc
// NSPasteboard types
NSPasteboardTypeString   // Plain text
NSPasteboardTypeURL      // URLs
NSPasteboardTypeTIFF     // Images
NSPasteboardTypeFileURL  // File paths
```

```csharp
// Accessing pasteboard data
IntPtr GetPasteboardData(IntPtr draggingInfo) {
    IntPtr selPasteboard = sel_registerName("draggingPasteboard");
    IntPtr pasteboard = objc_msgSend(draggingInfo, selPasteboard);

    IntPtr selStringForType = sel_registerName("stringForType:");
    IntPtr nsStringType = CreateNSString("NSStringPboardType");
    return objc_msgSend(pasteboard, selStringForType, nsStringType);
}
```

### 2.3 Linux: GTK Drag-and-Drop

#### GTK Signal-Based System

```c
// Drag source signals
g_signal_connect(widget, "drag-begin", G_CALLBACK(drag_begin_handler), data);
g_signal_connect(widget, "drag-data-get", G_CALLBACK(drag_data_get_handler), data);
g_signal_connect(widget, "drag-end", G_CALLBACK(drag_end_handler), data);

// Drop target signals
g_signal_connect(widget, "drag-motion", G_CALLBACK(drag_motion_handler), data);
g_signal_connect(widget, "drag-drop", G_CALLBACK(drag_drop_handler), data);
g_signal_connect(widget, "drag-data-received", G_CALLBACK(drag_data_received_handler), data);
```

#### GTK API Functions

```c
// Setup drag source
void gtk_drag_source_set(
    GtkWidget *widget,
    GdkModifierType start_button_mask,
    const GtkTargetEntry *targets,
    gint n_targets,
    GdkDragAction actions);

// Setup drop target
void gtk_drag_dest_set(
    GtkWidget *widget,
    GtkDestDefaults flags,
    const GtkTargetEntry *targets,
    gint n_targets,
    GdkDragAction actions);
```

#### C# P/Invoke Implementation

```csharp
// Based on SWTSharp's current Linux pattern
[DllImport("libgtk-3.so.0")]
static extern void gtk_drag_source_set(
    IntPtr widget,
    Gdk.ModifierType startButtonMask,
    TargetEntry[] targets,
    int nTargets,
    Gdk.DragAction actions);

[StructLayout(LayoutKind.Sequential)]
struct TargetEntry {
    public string target;    // MIME type or atom name
    public uint flags;
    public uint info;        // Application-defined identifier
}
```

#### GTK Target Types (MIME-based)

```c
// Standard MIME types
"text/plain"
"text/uri-list"  // File paths
"text/html"
"image/png"
"application/x-rootwindow-drop"  // Internal drag
```

#### Signal Handler Pattern

```csharp
// Delegate for GTK callbacks
delegate void DragBeginHandler(IntPtr widget, IntPtr context, IntPtr userData);

// Keep delegate alive to prevent GC
private DragBeginHandler? _dragBeginDelegate;

void SetupDragSource(IntPtr widget) {
    // Store delegate to prevent GC (SEC-001 pattern from SWTSharp)
    _dragBeginDelegate = OnDragBegin;

    IntPtr signalId = g_signal_connect_data(
        widget,
        "drag-begin",
        Marshal.GetFunctionPointerForDelegate(_dragBeginDelegate),
        IntPtr.Zero,
        IntPtr.Zero,
        0);
}

void OnDragBegin(IntPtr widget, IntPtr context, IntPtr userData) {
    // Handle drag begin event
}
```

---

## 3. Cross-Platform Data Format Challenges

### 3.1 Text Data Formats

| Platform | Plain Text | Rich Text | HTML |
|----------|------------|-----------|------|
| Win32 | CF_TEXT, CF_UNICODETEXT | CF_RTF | CF_HTML |
| macOS | NSStringPboardType | NSRTFPboardType | NSHTMLPboardType |
| Linux | text/plain;charset=utf-8 | text/rtf | text/html |

**Encoding Issues:**
- Win32: UTF-16LE by default (CF_UNICODETEXT)
- macOS: UTF-8 or UTF-16 depending on API
- Linux: UTF-8 standard
- Line ending conversion required (CRLF vs LF)

### 3.2 File Path Formats

**Win32 (CF_HDROP):**
```c
// DROPFILES structure followed by null-terminated file paths
typedef struct {
    DWORD pFiles;      // Offset to file list
    POINT pt;          // Drop point
    BOOL fNC;          // Non-client area flag
    BOOL fWide;        // Unicode flag
} DROPFILES;

// File list format:
// L"C:\\path\\to\\file1.txt\0C:\\path\\to\\file2.txt\0\0"
```

**macOS (NSFilenamesPboardType / NSURLPboardType):**
```objc
// NSArray of NSString file paths
@[ @"/Users/name/file1.txt", @"/Users/name/file2.txt" ]

// Or NSArray of NSURL
@[ [NSURL fileURLWithPath:@"/Users/name/file1.txt"] ]
```

**Linux (text/uri-list):**
```
file:///home/user/file1.txt
file:///home/user/file2.txt
```

**Conversion Requirements:**
- Path separator normalization (\ vs /)
- URL encoding/decoding
- UNC path handling on Windows
- Symbolic link resolution
- File permission checking

### 3.3 Image Data Formats

| Platform | Bitmap | DIB | PNG | TIFF |
|----------|--------|-----|-----|------|
| Win32 | CF_BITMAP (HBITMAP) | CF_DIB (BITMAPINFO) | Custom | Custom |
| macOS | - | - | NSPasteboardTypePNG | NSPasteboardTypeTIFF |
| Linux | image/bmp | - | image/png | image/tiff |

**Complexity:**
- Win32 CF_BITMAP requires GDI HBITMAP handles (not directly portable)
- Image format conversion required (RGBA vs BGRA byte order)
- Alpha channel handling differences
- Color space considerations

### 3.4 Custom Data Formats

**Registration Process:**

```csharp
// Win32
ushort customFormat = RegisterClipboardFormat("MyApp.CustomData");

// macOS
IntPtr utType = CreateNSString("com.myapp.customdata");
// Use UTType API for proper registration

// Linux
TargetEntry customTarget = new TargetEntry {
    target = "application/x-myapp-customdata",
    flags = 0,
    info = 1000
};
```

**Serialization Strategy:**
- Binary serialization (protobuf, MessagePack, etc.)
- JSON with base64 for binary data
- Custom binary protocol with version header
- Platform-specific optimizations

---

## 4. Proposed SWTSharp Architecture

### 4.1 Public API Layer

```csharp
namespace SWTSharp.DND
{
    /// <summary>
    /// Defines a drag source for a control.
    /// </summary>
    public class DragSource : Widget
    {
        public DragSource(Control control, int style);

        public void SetTransfer(Transfer[] transferTypes);
        public Transfer[] GetTransfer();

        public void AddDragListener(IDragSourceListener listener);
        public void RemoveDragListener(IDragSourceListener listener);

        protected override void ReleaseWidget();
    }

    /// <summary>
    /// Defines a drop target for a control.
    /// </summary>
    public class DropTarget : Widget
    {
        public DropTarget(Control control, int style);

        public void SetTransfer(Transfer[] transferTypes);
        public Transfer[] GetTransfer();

        public void AddDropListener(IDropListener listener);
        public void RemoveDropListener(IDropListener listener);

        protected override void ReleaseWidget();
    }
}
```

### 4.2 Transfer Type Hierarchy

```csharp
namespace SWTSharp.DND
{
    /// <summary>
    /// Abstract base class for data transfer types.
    /// </summary>
    public abstract class Transfer
    {
        public abstract TransferData[] GetSupportedTypes();
        public abstract bool IsSupportedType(TransferData transferData);

        protected abstract void JavaToNative(object data, TransferData transferData);
        protected abstract object? NativeToJava(TransferData transferData);

        // Platform-specific registration
        protected abstract int[] GetTypeIds();
        protected abstract string[] GetTypeNames();
    }

    /// <summary>
    /// Transfer type for plain text.
    /// </summary>
    public class TextTransfer : ByteArrayTransfer
    {
        private static TextTransfer? _instance;

        public static TextTransfer GetInstance()
        {
            return _instance ??= new TextTransfer();
        }

        private TextTransfer() { }

        protected override int[] GetTypeIds()
        {
            // Platform-specific implementation
        }
    }

    /// <summary>
    /// Transfer type for file paths.
    /// </summary>
    public class FileTransfer : ByteArrayTransfer
    {
        public static FileTransfer GetInstance() { /* ... */ }
        // Convert between string[] and platform format
    }

    /// <summary>
    /// Base class for custom binary transfers.
    /// </summary>
    public abstract class ByteArrayTransfer : Transfer
    {
        protected void JavaToNative(byte[] data, TransferData transferData);
        protected byte[]? NativeToJava(TransferData transferData);
    }
}
```

### 4.3 Event System

```csharp
namespace SWTSharp.DND
{
    /// <summary>
    /// Drag source event listener.
    /// </summary>
    public interface IDragSourceListener
    {
        void DragStart(DragSourceEvent e);
        void DragSetData(DragSourceEvent e);
        void DragFinished(DragSourceEvent e);
    }

    /// <summary>
    /// Drop target event listener.
    /// </summary>
    public interface IDropListener
    {
        void DragEnter(DropTargetEvent e);
        void DragOver(DropTargetEvent e);
        void DragLeave(DropTargetEvent e);
        void DropAccept(DropTargetEvent e);
        void Drop(DropTargetEvent e);
        void DragOperationChanged(DropTargetEvent e);
    }

    /// <summary>
    /// Event data for drag source operations.
    /// </summary>
    public class DragSourceEvent : TypedEvent
    {
        public object? Data { get; set; }
        public int Detail { get; set; }  // DROP_COPY, DROP_MOVE, DROP_LINK
        public bool Doit { get; set; } = true;
        public TransferData? DataType { get; set; }
    }

    /// <summary>
    /// Event data for drop target operations.
    /// </summary>
    public class DropTargetEvent : TypedEvent
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Operations { get; set; }  // Allowed operations (bitmask)
        public int Detail { get; set; }      // Current operation
        public int Feedback { get; set; }    // Visual feedback flags
        public TransferData? CurrentDataType { get; set; }
        public TransferData[]? DataTypes { get; set; }
    }
}
```

### 4.4 Platform Interface Extensions

```csharp
// Add to IPlatform interface
namespace SWTSharp.Platform
{
    public partial interface IPlatform
    {
        // Drag source operations
        IntPtr CreateDragSource(IntPtr controlHandle, int style);
        void DestroyDragSource(IntPtr dragSourceHandle);
        void SetDragSourceTransferTypes(IntPtr handle, int[] transferIds);
        void DragSourceInitiateDrag(IntPtr handle, DragSourceData data);

        // Drop target operations
        IntPtr CreateDropTarget(IntPtr controlHandle, int style);
        void DestroyDropTarget(IntPtr dropTargetHandle);
        void SetDropTargetTransferTypes(IntPtr handle, int[] transferIds);
        void DropTargetSetFeedback(IntPtr handle, int feedback);

        // Transfer type registration
        int RegisterTransferType(string typeName);
        int[] GetTransferTypeIds(string[] typeNames);

        // Data conversion
        byte[] ConvertToNative(int typeId, object data);
        object? ConvertFromNative(int typeId, byte[] nativeData);

        // Clipboard integration
        void SetClipboardData(int[] typeIds, byte[][] data);
        byte[]? GetClipboardData(int typeId);
    }
}
```

### 4.5 Win32 Platform Implementation

```csharp
namespace SWTSharp.Platform
{
    internal partial class Win32Platform
    {
        // COM interface implementations
        private class Win32DropSource : IDropSource
        {
            private readonly DragSource _dragSource;

            public int QueryContinueDrag(int fEscapePressed, uint grfKeyState)
            {
                if (fEscapePressed != 0)
                    return DRAGDROP_S_CANCEL;

                // Check if mouse button released
                if ((grfKeyState & (MK_LBUTTON | MK_RBUTTON | MK_MBUTTON)) == 0)
                    return DRAGDROP_S_DROP;

                return S_OK;
            }

            public int GiveFeedback(uint dwEffect)
            {
                // Use default cursor feedback
                return DRAGDROP_S_USEDEFAULTCURSORS;
            }
        }

        private class Win32DropTarget : IDropTarget
        {
            private readonly DropTarget _dropTarget;

            public int DragEnter(IDataObject pDataObj, uint grfKeyState,
                                POINTL pt, ref uint pdwEffect)
            {
                // Fire DragEnter event
                // Update pdwEffect based on allowed operations
                return S_OK;
            }

            public int DragOver(uint grfKeyState, POINTL pt, ref uint pdwEffect)
            {
                // Fire DragOver event continuously
                return S_OK;
            }

            public int DragLeave()
            {
                // Fire DragLeave event
                return S_OK;
            }

            public int Drop(IDataObject pDataObj, uint grfKeyState,
                           POINTL pt, ref uint pdwEffect)
            {
                // Extract data from IDataObject
                // Fire Drop event
                return S_OK;
            }
        }

        public IntPtr CreateDragSource(IntPtr controlHandle, int style)
        {
            // Create COM object wrapper
            var dropSource = new Win32DropSource(/* ... */);

            // Store in registry to prevent GC
            int handle = _nextDragSourceId++;
            _dragSources[handle] = dropSource;

            return new IntPtr(handle);
        }

        public void DragSourceInitiateDrag(IntPtr handle, DragSourceData data)
        {
            // Create IDataObject from data
            var dataObject = new Win32DataObject(data);
            var dropSource = _dragSources[(int)handle];

            // Start drag operation
            uint effect;
            int hr = DoDragDrop(dataObject, dropSource,
                               ConvertToDropEffect(data.AllowedOperations),
                               out effect);

            // Handle result and fire DragFinished event
        }
    }
}
```

### 4.6 macOS Platform Implementation

```csharp
namespace SWTSharp.Platform
{
    internal partial class MacOSPlatform
    {
        // Runtime class creation for NSDraggingDestination
        private IntPtr CreateDraggingDestinationClass()
        {
            // Allocate new Objective-C class
            IntPtr superclass = objc_getClass("NSObject");
            IntPtr newClass = objc_allocateClassPair(superclass,
                "SWTDropTarget" + Guid.NewGuid().ToString("N"),
                IntPtr.Zero);

            // Add protocol methods
            AddMethod(newClass, "draggingEntered:", DraggingEntered, "Q@:@");
            AddMethod(newClass, "draggingUpdated:", DraggingUpdated, "Q@:@");
            AddMethod(newClass, "draggingExited:", DraggingExited, "v@:@");
            AddMethod(newClass, "performDragOperation:", PerformDragOperation, "B@:@");

            // Register class
            objc_registerClassPair(newClass);

            return newClass;
        }

        // Drag delegate implementations (must be static for function pointers)
        [UnmanagedCallersOnly]
        private static ulong DraggingEntered(IntPtr self, IntPtr cmd, IntPtr sender)
        {
            // Get DropTarget instance from associated object
            var dropTarget = GetDropTargetForView(self);

            // Fire DragEnter event
            // Return NSDragOperation (copy/move/link/none)
            return NSDragOperationCopy;
        }

        public void DragSourceInitiateDrag(IntPtr handle, DragSourceData data)
        {
            // Create NSDraggingItem with pasteboard data
            IntPtr draggingItem = CreateDraggingItem(data);

            // Begin dragging session
            IntPtr selBegin = sel_registerName("beginDraggingSessionWithItems:event:source:");
            objc_msgSend(handle, selBegin, itemsArray, currentEvent, self);
        }
    }
}
```

### 4.7 Linux Platform Implementation

```csharp
namespace SWTSharp.Platform
{
    internal partial class LinuxPlatform
    {
        private Dictionary<IntPtr, DragSourceCallbacks> _dragSources = new();

        private struct DragSourceCallbacks
        {
            public GCallback DragBegin;
            public GCallback DragDataGet;
            public GCallback DragEnd;
        }

        public IntPtr CreateDragSource(IntPtr controlHandle, int style)
        {
            // Convert SWT style to GdkDragAction
            GdkDragAction actions = ConvertToGdkDragAction(style);

            // Create target entries from transfer types
            TargetEntry[] targets = new TargetEntry[] {
                new TargetEntry { target = "text/plain", flags = 0, info = 0 },
                new TargetEntry { target = "text/uri-list", flags = 0, info = 1 }
            };

            // Setup drag source
            gtk_drag_source_set(controlHandle,
                               GdkModifierType.Button1Mask,
                               targets, targets.Length, actions);

            // Connect signals (store delegates to prevent GC - SEC-001 pattern)
            var callbacks = new DragSourceCallbacks {
                DragBegin = Marshal.GetDelegateForFunctionPointer<GCallback>(
                    Marshal.GetFunctionPointerForDelegate<DragBeginFunc>(OnDragBegin)),
                DragDataGet = Marshal.GetDelegateForFunctionPointer<GCallback>(
                    Marshal.GetFunctionPointerForDelegate<DragDataGetFunc>(OnDragDataGet)),
                DragEnd = Marshal.GetDelegateForFunctionPointer<GCallback>(
                    Marshal.GetFunctionPointerForDelegate<DragEndFunc>(OnDragEnd))
            };

            g_signal_connect(controlHandle, "drag-begin", callbacks.DragBegin, IntPtr.Zero);
            g_signal_connect(controlHandle, "drag-data-get", callbacks.DragDataGet, IntPtr.Zero);
            g_signal_connect(controlHandle, "drag-end", callbacks.DragEnd, IntPtr.Zero);

            _dragSources[controlHandle] = callbacks;

            return controlHandle;
        }
    }
}
```

---

## 5. Implementation Requirements

### 5.1 Core Components

1. **DND Namespace** (`SWTSharp.DND`)
   - DragSource class
   - DropTarget class
   - Transfer type hierarchy (8-10 classes)
   - Event classes and listeners
   - TransferData structure

2. **Platform Extensions**
   - IPlatform interface additions (15-20 methods)
   - Win32Platform DND implementation (500-700 LOC)
   - MacOSPlatform DND implementation (600-800 LOC)
   - LinuxPlatform DND implementation (400-600 LOC)

3. **COM Interop Layer** (Win32 only)
   - IDropSource implementation
   - IDropTarget implementation
   - IDataObject implementation
   - FORMATETC, STGMEDIUM structures
   - COM registration/cleanup

4. **Objective-C Runtime Extensions** (macOS only)
   - Protocol implementation helpers
   - Runtime class creation
   - Method registration utilities
   - Type encoding generator

5. **Data Conversion Layer**
   - Text encoding conversions
   - File path format conversions
   - Image format conversions
   - Binary serialization helpers

### 5.2 Testing Strategy

**Unit Tests:**
- Transfer type registration
- Data format conversion
- Event firing sequences
- Operation negotiation logic

**Integration Tests (Challenging):**
- Cannot fully automate user drag gestures
- Requires UI automation frameworks:
  - Win32: UI Automation / Win32 SendInput
  - macOS: Accessibility API / CGEvent
  - Linux: Xdotool / AT-SPI

**Manual Testing Scenarios:**
- Drag text between controls
- Drag files from file manager
- Drag to/from external applications
- Multi-format drag operations
- Cross-platform data exchange

### 5.3 Thread Safety Considerations

**Win32:**
- DND operations must occur on STA thread
- COM object lifetime management critical
- Marshal.AddRef/Release for reference counting

**macOS:**
- All AppKit operations must be on main thread
- NSPasteboard access requires synchronization
- Autorelease pool management

**Linux:**
- GTK must be initialized before DND setup
- Signal handlers must be thread-safe
- GDK thread handling if using multi-threading

---

## 6. Complexity Analysis

### 6.1 Blocker Identification

#### HIGH SEVERITY BLOCKERS

1. **COM Interop Complexity (Win32)**
   - **Impact:** Core functionality
   - **Risk:** Memory leaks, crashes, COM reference count errors
   - **Mitigation:**
     - Use `System.Runtime.InteropServices.ComTypes` where possible
     - Implement proper IDisposable pattern for COM objects
     - Use RCW (Runtime Callable Wrapper) for existing interfaces
     - Extensive testing with different data formats

2. **Objective-C Protocol Implementation (macOS)**
   - **Impact:** Core functionality
   - **Risk:** Runtime crashes, method signature mismatches
   - **Mitigation:**
     - Use NativeAOT interop features (.NET 7+)
     - Create protocol implementation generator tool
     - Validate type encoding strings extensively
     - Reference existing Xamarin.Mac patterns

3. **Cross-Platform Data Format Differences**
   - **Impact:** Data integrity, user experience
   - **Risk:** Data loss, incorrect encoding, file path errors
   - **Mitigation:**
     - Extensive conversion testing
     - Fallback to safest common format
     - Detailed error logging
     - User-visible warnings for lossy conversions

#### MEDIUM SEVERITY BLOCKERS

4. **Visual Feedback Synchronization**
   - **Impact:** User experience
   - **Risk:** Cursor flicker, incorrect drag images
   - **Mitigation:**
     - Use platform default cursors initially
     - Custom cursors as enhancement phase
     - Document known limitations

5. **Testing Without User Interaction**
   - **Impact:** CI/CD pipeline, regression testing
   - **Risk:** Untested code paths, bugs in production
   - **Mitigation:**
     - Headless testing where possible
     - Recorded UI automation tests
     - Comprehensive manual test checklist

6. **Clipboard Format Conversion**
   - **Impact:** Data exchange reliability
   - **Risk:** Format mismatch, unsupported types
   - **Mitigation:**
     - Support most common formats first
     - Progressive enhancement for exotic formats
     - Clear documentation of supported formats

#### LOW SEVERITY BLOCKERS

7. **Custom Drag Images**
   - **Impact:** Visual polish
   - **Risk:** Platform inconsistency
   - **Mitigation:**
     - Phase 2 feature
     - Use platform defaults initially

8. **Advanced Transfer Types** (RTF, HTML, Images)
   - **Impact:** Feature completeness
   - **Risk:** Complex format handling
   - **Mitigation:**
     - Implement TextTransfer and FileTransfer first
     - Add others incrementally

### 6.2 Risk Matrix

| Component | Complexity | Platform Variance | COM/Interop | Testing Difficulty | Total Risk |
|-----------|------------|-------------------|-------------|-------------------|------------|
| DragSource API | Medium | Low | - | Medium | Medium |
| DropTarget API | Medium | Low | - | Medium | Medium |
| Win32 COM Interop | Very High | - | Very High | High | **CRITICAL** |
| macOS Protocol Impl | Very High | - | High | High | **CRITICAL** |
| Linux GTK Signals | Medium | - | Low | Medium | Medium |
| TextTransfer | Medium | High | Medium | Low | Medium |
| FileTransfer | High | Very High | Medium | Medium | High |
| ImageTransfer | Very High | High | High | High | **CRITICAL** |
| Event System | Medium | Low | - | High | Medium |
| Visual Feedback | High | Very High | Medium | Very High | High |

### 6.3 Effort Estimation

**Phase 1: Core Infrastructure (2-3 weeks)**
- Public API design and implementation
- Transfer base classes
- Event system
- Platform interface definitions

**Phase 2: Win32 Implementation (2-3 weeks)**
- COM interop layer
- IDropSource/IDropTarget/IDataObject
- TextTransfer and FileTransfer
- Testing and debugging

**Phase 3: macOS Implementation (2-3 weeks)**
- Objective-C protocol implementation
- NSDraggingSource/Destination
- Pasteboard integration
- Testing and debugging

**Phase 4: Linux Implementation (1-2 weeks)**
- GTK signal handlers
- Target entry setup
- MIME type conversion
- Testing and debugging

**Phase 5: Advanced Features (1-2 weeks)**
- Additional transfer types
- Custom drag images
- Visual feedback polish
- Cross-platform testing

**Total Estimated Time:** 8-13 weeks (assuming single full-time developer)

**For SWTSharp project:** 6-8 weeks with focused effort, leveraging existing platform infrastructure

---

## 7. Recommended Implementation Strategy

### 7.1 Phased Approach

**Phase 1: Foundation (Priority: CRITICAL)**
1. Design public API (DragSource, DropTarget, Transfer)
2. Implement event system
3. Add IPlatform interface methods
4. Create stub implementations on all platforms

**Phase 2: Win32 TextTransfer (Priority: HIGH)**
1. Implement COM interop layer
2. Win32DropSource/DropTarget for text only
3. CF_TEXT and CF_UNICODETEXT support
4. Basic manual testing

**Phase 3: Cross-Platform Text (Priority: HIGH)**
1. macOS NSStringPboardType
2. Linux text/plain;charset=utf-8
3. Cross-platform text drag testing
4. Documentation

**Phase 4: FileTransfer (Priority: MEDIUM)**
1. Win32 CF_HDROP
2. macOS NSFilenamesPboardType
3. Linux text/uri-list
4. Path conversion utilities

**Phase 5: Polish & Enhancement (Priority: LOW)**
1. Additional transfer types (HTML, RTF, Images)
2. Custom drag images
3. Advanced visual feedback
4. Performance optimization

### 7.2 Success Criteria

**Minimum Viable Product:**
- [x] Drag text between controls in same application
- [x] Drop text from external applications
- [x] Drag files from file manager to application
- [x] Basic operation negotiation (copy/move)
- [x] Works on all three platforms (Win32, macOS, Linux)

**Full Feature Completion:**
- [x] All standard transfer types (Text, File, HTML, RTF, Image)
- [x] Custom transfer types supported
- [x] Visual feedback (cursors, highlighting)
- [x] Custom drag images
- [x] Comprehensive documentation
- [x] Automated test coverage >70%

### 7.3 Dependencies

**Required:**
- Existing SWTSharp Control infrastructure
- Platform abstraction layer (IPlatform)
- Event system (Widget.NotifyListeners)
- Clipboard integration (for data conversion)

**Nice-to-Have:**
- UI automation framework for testing
- Performance profiling tools
- Cross-platform CI environment

---

## 8. Alternative Approaches Considered

### 8.1 Use Platform-Specific Libraries

**Option:** Use WinForms DragDrop for Win32, MonoMac for macOS, Gtk# for Linux

**Pros:**
- Less low-level interop code
- Higher-level abstractions
- Some testing already done

**Cons:**
- Extra dependencies (bloat)
- Less control over behavior
- May not match SWT semantics exactly
- License compatibility issues (GPL for Gtk#)

**Decision:** REJECTED - Violates SWTSharp's minimal dependency philosophy

### 8.2 Clipboard-Based Fallback

**Option:** Implement DND as copy-to-clipboard + paste operation

**Pros:**
- Simpler implementation
- Reuses clipboard code
- No drag tracking needed

**Cons:**
- Poor user experience (not real DND)
- No visual feedback
- Doesn't work with external apps expecting real DND
- Not SWT-compatible

**Decision:** REJECTED - Doesn't meet requirements

### 8.3 Plugin Architecture

**Option:** Make DND an optional plugin/extension

**Pros:**
- Core library stays smaller
- Advanced feature opt-in
- Independent versioning

**Cons:**
- API fragmentation
- Discovery issues
- Not how SWT works (DND is core)

**Decision:** REJECTED - Should be core feature

---

## 9. Documentation Requirements

### 9.1 API Documentation

- XML doc comments for all public classes/methods
- Code examples for common scenarios
- Platform-specific notes and limitations
- Migration guide from Java SWT

### 9.2 Implementation Guide

- Architecture overview with diagrams
- Platform-specific details
- Testing procedures
- Troubleshooting common issues

### 9.3 Sample Code

```csharp
// Example: Drag text from a Text control
var text = new Text(shell, SWT.BORDER);
var dragSource = new DragSource(text, SWT.DROP_COPY | SWT.DROP_MOVE);
dragSource.SetTransfer(new Transfer[] { TextTransfer.GetInstance() });
dragSource.AddDragListener(new DragSourceListener {
    DragStart = e => {
        e.Doit = !text.GetSelectionText().IsEmpty();
    },
    DragSetData = e => {
        e.Data = text.GetSelectionText();
    }
});

// Example: Accept dropped files
var label = new Label(shell, SWT.BORDER);
var dropTarget = new DropTarget(label, SWT.DROP_COPY);
dropTarget.SetTransfer(new Transfer[] { FileTransfer.GetInstance() });
dropTarget.AddDropListener(new DropListener {
    Drop = e => {
        if (e.Data is string[] files) {
            label.SetText($"Dropped {files.Length} file(s)");
        }
    }
});
```

---

## 10. Conclusion

### 10.1 Summary

Implementing drag-and-drop for SWTSharp is a **high-complexity, high-value feature** requiring:
- Deep platform-specific knowledge (COM, Objective-C, GTK)
- Careful memory management and interop
- Extensive testing (much of it manual)
- 6-8 weeks of focused development effort

The primary blockers are:
1. Win32 COM interop complexity
2. macOS Objective-C protocol implementation
3. Cross-platform data format conversion

### 10.2 Recommendations

1. **Start with Win32 TextTransfer** - This validates the architecture and solves the most complex platform first
2. **Leverage existing patterns** - Follow SEC-001 delegate storage pattern, COM best practices from Windows SDK
3. **Incremental delivery** - Ship text-only first, add formats progressively
4. **Focus on common scenarios** - 80% of users need text and file drag; exotic formats can wait
5. **Document limitations** - Be transparent about platform differences and unsupported features

### 10.3 Go/No-Go Decision Factors

**GO if:**
- Core widget set is stable and tested
- Development resources available for 6-8 weeks
- Team has COM/Objective-C/GTK experience
- Users are actively requesting DND functionality

**NO-GO if:**
- Core features still incomplete
- Limited development resources
- No testing infrastructure for UI interactions
- Low user demand for DND

### 10.4 Next Steps

If approved for implementation:
1. Create detailed design document for Phase 1
2. Set up testing infrastructure (manual test checklist, automation where possible)
3. Implement Win32 COM layer and TextTransfer
4. Validate architecture before proceeding to other platforms
5. Iterate based on feedback and testing results

---

## Appendix A: Reference Materials

### Java SWT Source Code
- `org.eclipse.swt.dnd.DragSource`
- `org.eclipse.swt.dnd.DropTarget`
- `org.eclipse.swt.dnd.Transfer`
- Platform-specific implementations in `org.eclipse.swt.internal.win32/cocoa/gtk`

### Microsoft Documentation
- [OLE Drag and Drop](https://docs.microsoft.com/en-us/windows/win32/ole/drag-and-drop)
- [IDropSource Interface](https://docs.microsoft.com/en-us/windows/win32/api/oleidl/nn-oleidl-idropsource)
- [IDropTarget Interface](https://docs.microsoft.com/en-us/windows/win32/api/oleidl/nn-oleidl-idroptarget)
- [IDataObject Interface](https://docs.microsoft.com/en-us/windows/win32/api/objidl/nn-objidl-idataobject)

### Apple Documentation
- [NSDraggingSource Protocol](https://developer.apple.com/documentation/appkit/nsdraggingsource)
- [NSDraggingDestination Protocol](https://developer.apple.com/documentation/appkit/nsdraggingdestination)
- [NSPasteboard Class](https://developer.apple.com/documentation/appkit/nspasteboard)

### GTK Documentation
- [GTK Drag and Drop](https://docs.gtk.org/gtk3/drag-and-drop.html)
- [gtk_drag_source_set](https://docs.gtk.org/gtk3/method.Widget.drag_source_set.html)
- [gtk_drag_dest_set](https://docs.gtk.org/gtk3/method.Widget.drag_dest_set.html)

### .NET Interop Resources
- [COM Interop in .NET](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/com-interop)
- [Objective-C Libraries in .NET](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/objective-c-libraries)
- [P/Invoke Tutorial](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)

---

**End of Research Document**

*This research was conducted by analyzing Java SWT source code, platform documentation, and SWTSharp's existing architecture. All complexity ratings and effort estimates are based on similar implementations in cross-platform GUI frameworks.*
