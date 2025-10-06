# SWT Graphics System Implementation Guide

## Executive Summary

This document provides a comprehensive analysis of the Eclipse SWT graphics system for implementation in SWTSharp. The graphics system centers around the `org.eclipse.swt.graphics` package, which provides device-independent and device-specific graphics capabilities including drawing contexts (GC), images, colors, fonts, and other visual resources.

## Package Overview: org.eclipse.swt.graphics

The `org.eclipse.swt.graphics` package contains all graphics-related classes in SWT. The package is organized around a hierarchy of resource classes with explicit lifecycle management.

### Class Hierarchy

```
Resource (abstract)
├── GC (Graphics Context)
├── Image
├── Color
├── Font
├── Cursor
├── Region
├── Pattern
├── Path
└── Transform
```

### Device Classes

```
Device (abstract)
├── Display
└── Printer
```

## Core Classes

### 1. GC (Graphics Context)

**Purpose**: The focal point for all SWT drawing operations. Encapsulates platform-specific drawing capabilities.

**Description**: A graphics context that can draw on any "Drawable" object (Control, Image, Display, or Printer).

#### Constructors

```java
// Constructor for drawing on a Control
GC(Drawable drawable)

// Constructor with explicit style bits
GC(Drawable drawable, int style)
```

**Important**: You must dispose of the GC when finished using `gc.dispose()`.

#### Coordinate System

- Origin (0,0) is at the top-left corner
- X-axis increases to the right
- Y-axis increases downward
- All coordinates are in pixels

#### Drawing Methods

##### Line Drawing

```java
// Draw a line between two points
void drawLine(int x1, int y1, int x2, int y2)

// Draw a polyline (connected line segments)
void drawPolyline(int[] pointArray)  // [x1,y1, x2,y2, x3,y3, ...]
```

##### Shape Drawing (Outline)

```java
// Rectangle
void drawRectangle(int x, int y, int width, int height)
void drawRectangle(Rectangle rect)
void drawRoundRectangle(int x, int y, int width, int height,
                        int arcWidth, int arcHeight)

// Oval/Ellipse
void drawOval(int x, int y, int width, int height)

// Arc
void drawArc(int x, int y, int width, int height,
             int startAngle, int arcAngle)
// Angles in degrees: 0° = 3 o'clock, positive = counter-clockwise

// Polygon
void drawPolygon(int[] pointArray)  // Automatically closes the path

// Focus rectangle (platform-specific dashed style)
void drawFocus(int x, int y, int width, int height)
```

##### Shape Filling

```java
// Fill rectangle with background color
void fillRectangle(int x, int y, int width, int height)
void fillRectangle(Rectangle rect)
void fillRoundRectangle(int x, int y, int width, int height,
                        int arcWidth, int arcHeight)

// Fill oval
void fillOval(int x, int y, int width, int height)

// Fill arc
void fillArc(int x, int y, int width, int height,
             int startAngle, int arcAngle)

// Fill polygon
void fillPolygon(int[] pointArray)

// Fill with gradient
void fillGradientRectangle(int x, int y, int width, int height,
                           boolean vertical)
// Gradient from foreground to background color
```

##### Text Drawing

```java
// Draw string (no tab expansion, no mnemonic processing)
void drawString(String string, int x, int y)
void drawString(String string, int x, int y, boolean isTransparent)

// Draw text (with tab expansion and mnemonic processing)
void drawText(String string, int x, int y)
void drawText(String string, int x, int y, int flags)
void drawText(String string, int x, int y, boolean isTransparent)

// Text flags
SWT.DRAW_DELIMITER    // Process CR/LF
SWT.DRAW_TAB         // Process tabs
SWT.DRAW_MNEMONIC    // Process & as underline
SWT.DRAW_TRANSPARENT // Don't fill background
```

##### Image Drawing

```java
// Draw image at position
void drawImage(Image image, int x, int y)

// Draw portion of source image to portion of destination
void drawImage(Image image,
               int srcX, int srcY, int srcWidth, int srcHeight,
               int destX, int destY, int destWidth, int destHeight)
```

##### Advanced Drawing

```java
// Copy rectangular area within the drawable
void copyArea(int srcX, int srcY, int width, int height,
              int destX, int destY)
void copyArea(int srcX, int srcY, int width, int height,
              int destX, int destY, boolean paint)

// Draw point
void drawPoint(int x, int y)

// Draw path (advanced vector graphics)
void drawPath(Path path)
void fillPath(Path path)
```

#### Graphics State Methods

##### Colors

```java
// Set foreground color (for lines, text, outlines)
void setForeground(Color color)
Color getForeground()

// Set background color (for fills, text background)
void setBackground(Color color)
Color getBackground()

// Set background pattern
void setBackgroundPattern(Pattern pattern)
Pattern getBackgroundPattern()

// Set foreground pattern
void setForegroundPattern(Pattern pattern)
Pattern getForegroundPattern()
```

##### Font and Text

```java
// Set font for text drawing
void setFont(Font font)
Font getFont()

// Get font metrics
FontMetrics getFontMetrics()

// Get text extent (size required to draw text)
Point textExtent(String string)
Point textExtent(String string, int flags)

// Get character width
int getCharWidth(char ch)

// Get string width
Point stringExtent(String string)
```

##### Line Attributes

```java
// Set line width
void setLineWidth(int lineWidth)
int getLineWidth()

// Set line style
void setLineStyle(int lineStyle)
int getLineStyle()
// Styles: SWT.LINE_SOLID, SWT.LINE_DASH, SWT.LINE_DOT,
//         SWT.LINE_DASHDOT, SWT.LINE_DASHDOTDOT, SWT.LINE_CUSTOM

// Set custom line dash pattern
void setLineDash(int[] dashes)
int[] getLineDash()

// Set line cap style
void setLineCap(int cap)
int getLineCap()
// Caps: SWT.CAP_FLAT, SWT.CAP_ROUND, SWT.CAP_SQUARE

// Set line join style
void setLineJoin(int join)
int getLineJoin()
// Joins: SWT.JOIN_MITER, SWT.JOIN_ROUND, SWT.JOIN_BEVEL

// Set miter limit
void setLineMiterLimit(float miterLimit)
float getLineMiterLimit()
```

##### Clipping

```java
// Set clipping rectangle
void setClipping(int x, int y, int width, int height)
void setClipping(Rectangle rect)

// Set clipping region
void setClipping(Region region)

// Set clipping path
void setClipping(Path path)

// Get clipping bounds
Rectangle getClipping()

// Get clipping region
void getClipping(Region region)
```

##### Transformations

```java
// Set transformation matrix
void setTransform(Transform transform)
Transform getTransform()
```

##### Other Attributes

```java
// Set XOR mode
void setXORMode(boolean xor)
boolean getXORMode()

// Set alpha (transparency) 0-255
void setAlpha(int alpha)
int getAlpha()

// Set antialiasing
void setAntialias(int antialias)
int getAntialias()
// Values: SWT.DEFAULT, SWT.OFF, SWT.ON

// Set text antialiasing
void setTextAntialias(int antialias)
int getTextAntialias()

// Set advanced mode (enables transformations, alpha, etc.)
void setAdvanced(boolean advanced)
boolean getAdvanced()

// Set interpolation for image scaling
void setInterpolation(int interpolation)
int getInterpolation()
// Values: SWT.DEFAULT, SWT.NONE, SWT.LOW, SWT.HIGH
```

#### Utility Methods

```java
// Check if GC is disposed
boolean isDisposed()

// Dispose GC (MUST be called)
void dispose()

// Get the device this GC draws on
Device getDevice()

// Get GC style
int getStyle()
```

---

### 2. Image

**Purpose**: Represents graphics prepared for display on a specific device.

**Device-Specific**: Images are bound to a device and must be recreated for different devices.

#### Constructors

```java
// Create from file path
Image(Device device, String filename)

// Create from InputStream
Image(Device device, InputStream stream)

// Create from ImageData (device-independent)
Image(Device device, ImageData data)

// Create from ImageData with transparency mask
Image(Device device, ImageData source, ImageData mask)

// Create blank image
Image(Device device, int width, int height)

// Create from Rectangle bounds
Image(Device device, Rectangle bounds)

// Copy constructor
Image(Device device, Image srcImage, int flag)
// Flags: SWT.IMAGE_COPY, SWT.IMAGE_DISABLE, SWT.IMAGE_GRAY
```

#### Methods

```java
// Get image bounds
Rectangle getBounds()

// Get ImageData (device-independent representation)
ImageData getImageData()
ImageData getImageData(int zoom)  // For HiDPI support

// Get background color (for transparency)
Color getBackground()
void setBackground(Color color)

// Check disposal
boolean isDisposed()

// Dispose (REQUIRED)
void dispose()

// Get device
Device getDevice()

// Get type
int getType()
// Types: SWT.BITMAP, SWT.ICON
```

#### Supported Image Formats

- BMP (Windows Bitmap)
- GIF (Graphics Interchange Format)
- ICO (Windows Icon)
- JPEG/JPG (Joint Photographic Experts Group)
- PNG (Portable Network Graphics)
- TIFF (Tagged Image File Format)

---

### 3. ImageData

**Purpose**: Device-independent description of an image.

**Key Feature**: Can be converted to Image for any device without loss of information.

#### Fields

```java
// Image dimensions
public int width;
public int height;

// Pixel data
public byte[] data;           // Pixel data array
public PaletteData palette;   // Color palette

// Transparency
public int transparentPixel;  // -1 if none
public byte[] maskData;       // Transparency mask
public byte[] alphaData;      // Per-pixel alpha (0-255)
public int alpha;             // Global alpha (-1 if per-pixel)

// Color depth
public int depth;             // Bits per pixel

// Scan line padding
public int scanlinePad;       // Byte boundary for each scan line

// Image type
public int type;              // SWT.IMAGE_BMP, etc.

// DPI information (for HiDPI)
public int x;                 // Horizontal DPI
public int y;                 // Vertical DPI

// Delay for animated images
public int delayTime;         // Milliseconds

// Disposal method for animated images
public int disposalMethod;
```

#### Constructors

```java
// Create from file
ImageData(String filename)

// Create from stream
ImageData(InputStream stream)

// Create blank image data
ImageData(int width, int height, int depth, PaletteData palette)

// Create with alpha data
ImageData(int width, int height, int depth, PaletteData palette,
          int scanlinePad, byte[] data, int maskPad, byte[] maskData,
          byte[] alphaData, int alpha, int transparentPixel,
          int type, int x, int y, int disposalMethod, int delayTime)
```

#### Methods

```java
// Get pixel value at coordinates
int getPixel(int x, int y)

// Set pixel value
void setPixel(int x, int y, int pixelValue)

// Get pixels from row
void getPixels(int x, int y, int getWidth, byte[] pixels, int startIndex)
void getPixels(int x, int y, int getWidth, int[] pixels, int startIndex)

// Set pixels in row
void setPixels(int x, int y, int putWidth, byte[] pixels, int startIndex)
void setPixels(int x, int y, int putWidth, int[] pixels, int startIndex)

// Alpha channel methods
int getAlpha(int x, int y)
void setAlpha(int x, int y, int alpha)
void getAlphas(int x, int y, int getWidth, byte[] alphas, int startIndex)
void setAlphas(int x, int y, int putWidth, byte[] alphas, int startIndex)

// Clone
Object clone()

// Scaling
ImageData scaledTo(int width, int height)
```

#### Transparency Types

```java
SWT.TRANSPARENCY_NONE      // No transparency
SWT.TRANSPARENCY_PIXEL     // One transparent pixel value
SWT.TRANSPARENCY_MASK      // Mask-based transparency
SWT.TRANSPARENCY_ALPHA     // Per-pixel alpha channel
```

---

### 4. Color

**Purpose**: Represents a color with red, green, blue, and optional alpha components.

**Device-Specific**: Colors are allocated on a specific device.

#### Constructors

```java
// Create from RGB values (0-255)
Color(Device device, int red, int green, int blue)

// Create from RGBA values (0-255)
Color(Device device, int red, int green, int blue, int alpha)

// Create from RGB object
Color(Device device, RGB rgb)

// Create from RGBA object
Color(Device device, RGBA rgba)
```

#### Methods

```java
// Get color components
int getRed()
int getGreen()
int getBlue()
int getAlpha()  // Returns 255 if no alpha

// Get RGB/RGBA
RGB getRGB()
RGBA getRGBA()

// Check disposal
boolean isDisposed()

// Dispose (REQUIRED - unless from Display.getSystemColor())
void dispose()

// Get device
Device getDevice()

// Equality
boolean equals(Object object)
int hashCode()

// String representation
String toString()
```

#### System Colors

**Important**: Do NOT dispose system colors - they are managed by Display.

```java
// Get system color (do NOT dispose these)
Color systemColor = display.getSystemColor(int colorId);

// System color constants (from SWT class)
SWT.COLOR_WHITE
SWT.COLOR_BLACK
SWT.COLOR_RED
SWT.COLOR_DARK_RED
SWT.COLOR_GREEN
SWT.COLOR_DARK_GREEN
SWT.COLOR_YELLOW
SWT.COLOR_DARK_YELLOW
SWT.COLOR_BLUE
SWT.COLOR_DARK_BLUE
SWT.COLOR_MAGENTA
SWT.COLOR_DARK_MAGENTA
SWT.COLOR_CYAN
SWT.COLOR_DARK_CYAN
SWT.COLOR_GRAY
SWT.COLOR_DARK_GRAY

// Widget-specific system colors
SWT.COLOR_WIDGET_FOREGROUND
SWT.COLOR_WIDGET_BACKGROUND
SWT.COLOR_WIDGET_BORDER
SWT.COLOR_WIDGET_DARK_SHADOW
SWT.COLOR_WIDGET_NORMAL_SHADOW
SWT.COLOR_WIDGET_LIGHT_SHADOW
SWT.COLOR_WIDGET_HIGHLIGHT_SHADOW

// List colors
SWT.COLOR_LIST_FOREGROUND
SWT.COLOR_LIST_BACKGROUND
SWT.COLOR_LIST_SELECTION
SWT.COLOR_LIST_SELECTION_TEXT

// Title colors
SWT.COLOR_TITLE_FOREGROUND
SWT.COLOR_TITLE_BACKGROUND
SWT.COLOR_TITLE_BACKGROUND_GRADIENT
SWT.COLOR_TITLE_INACTIVE_FOREGROUND
SWT.COLOR_TITLE_INACTIVE_BACKGROUND
SWT.COLOR_TITLE_INACTIVE_BACKGROUND_GRADIENT

// Tooltip colors
SWT.COLOR_INFO_FOREGROUND
SWT.COLOR_INFO_BACKGROUND

// Link color
SWT.COLOR_LINK_FOREGROUND
```

---

### 5. RGB and RGBA

**Purpose**: Device-independent color representation.

**Not Resources**: These are simple data objects - no disposal required.

#### RGB

```java
// Fields
public int red;    // 0-255
public int green;  // 0-255
public int blue;   // 0-255

// Constructors
RGB(int red, int green, int blue)

// Methods
boolean equals(Object object)
int hashCode()
String toString()

// HSB conversion
static RGB HSBtoRGB(float hue, float saturation, float brightness)
static float[] RGBtoHSB(int red, int green, int blue)
```

#### RGBA

```java
// Fields
public int red;    // 0-255
public int green;  // 0-255
public int blue;   // 0-255
public int alpha;  // 0-255 (0=transparent, 255=opaque)

// Constructors
RGBA(int red, int green, int blue, int alpha)

// Methods
RGB getRGB()
boolean equals(Object object)
int hashCode()
String toString()
```

---

### 6. Font

**Purpose**: Represents a font for text drawing.

**Device-Specific**: Fonts are allocated on a specific device.

#### Constructors

```java
// Create from FontData
Font(Device device, FontData fd)

// Create from FontData array (for fallback fonts)
Font(Device device, FontData[] fds)

// Create from name, height, and style
Font(Device device, String name, int height, int style)
```

#### Methods

```java
// Get font data
FontData[] getFontData()

// Check disposal
boolean isDisposed()

// Dispose (REQUIRED - unless from Control.getFont() or system font)
void dispose()

// Get device
Device getDevice()

// Equality
boolean equals(Object object)
int hashCode()

// String representation
String toString()
```

---

### 7. FontData

**Purpose**: Device-independent font description.

**Not a Resource**: No disposal required.

#### Fields

```java
// Font name
private String name;

// Font height in points
private float height;

// Font style
private int style;

// Locale (for complex scripts)
private String locale;
```

#### Constructors

```java
// Empty constructor
FontData()

// Create from string representation
FontData(String string)

// Create with name, height, style
FontData(String name, int height, int style)
```

#### Methods

```java
// Getters and setters
String getName()
void setName(String name)

float getHeight()
void setHeight(float height)

int getStyle()
void setStyle(int style)
// Styles: SWT.NORMAL, SWT.BOLD, SWT.ITALIC,
//         or SWT.BOLD | SWT.ITALIC

String getLocale()
void setLocale(String locale)

// Equality
boolean equals(Object object)
int hashCode()

// String representation (can be used in constructor)
String toString()
```

---

### 8. FontMetrics

**Purpose**: Provides measurements about a font.

**Not a Resource**: No disposal required.

#### Methods

```java
// Ascent (baseline to top)
int getAscent()

// Descent (baseline to bottom)
int getDescent()

// Height (ascent + descent + leading)
int getHeight()

// Leading (space between lines)
int getLeading()

// Average character width
int getAverageCharWidth()

// Equality
boolean equals(Object object)
int hashCode()
```

---

### 9. Cursor

**Purpose**: Represents a mouse cursor.

**Device-Specific**: Cursors are allocated on a specific device.

#### Constructors

```java
// Create system cursor
Cursor(Device device, int style)
// Styles: SWT.CURSOR_ARROW, SWT.CURSOR_WAIT, SWT.CURSOR_CROSS,
//         SWT.CURSOR_HAND, SWT.CURSOR_HELP, SWT.CURSOR_SIZEALL,
//         SWT.CURSOR_SIZENS, SWT.CURSOR_SIZEWE, SWT.CURSOR_SIZEN,
//         SWT.CURSOR_SIZES, SWT.CURSOR_SIZEE, SWT.CURSOR_SIZEW,
//         SWT.CURSOR_SIZENE, SWT.CURSOR_SIZESE, SWT.CURSOR_SIZESW,
//         SWT.CURSOR_SIZENW, SWT.CURSOR_IBEAM, SWT.CURSOR_NO,
//         SWT.CURSOR_APPSTARTING, SWT.CURSOR_UPARROW

// Create custom cursor from image
Cursor(Device device, ImageData source, int hotspotX, int hotspotY)

// Create custom cursor with mask
Cursor(Device device, ImageData source, ImageData mask,
       int hotspotX, int hotspotY)
```

#### Methods

```java
// Check disposal
boolean isDisposed()

// Dispose (REQUIRED)
void dispose()

// Get device
Device getDevice()

// Equality
boolean equals(Object object)
int hashCode()

// String representation
String toString()
```

---

### 10. Region

**Purpose**: Represents an area as a collection of polygons.

**Device-Specific**: Regions are allocated on a specific device.

#### Constructors

```java
// Create empty region
Region(Device device)

// Create from array of points (polygon)
Region(Device device, int[] pointArray)

// Create from rectangle
Region(Device device, Rectangle rectangle)
```

#### Methods

```java
// Add/subtract regions
void add(Rectangle rect)
void add(Region region)
void subtract(Rectangle rect)
void subtract(Region region)
void intersect(Rectangle rect)
void intersect(Region region)

// Test containment
boolean contains(int x, int y)
boolean contains(Point pt)

// Test intersection
boolean intersects(int x, int y, int width, int height)
boolean intersects(Rectangle rect)

// Get bounds
Rectangle getBounds()

// Check if empty
boolean isEmpty()

// Get rectangles making up region
int[] getRectangles()

// Equality
boolean equals(Object object)
int hashCode()

// Check disposal
boolean isDisposed()

// Dispose (REQUIRED)
void dispose()
```

---

### 11. Path

**Purpose**: Represents a geometric path for advanced drawing.

**Device-Specific**: Paths are allocated on a specific device.

#### Constructors

```java
// Create empty path
Path(Device device)

// Create from PathData
Path(Device device, PathData data)
```

#### Methods

```java
// Move to point (start new subpath)
void moveTo(float x, float y)

// Line to point
void lineTo(float x, float y)

// Cubic bezier curve
void cubicTo(float cx1, float cy1, float cx2, float cy2,
             float x, float y)

// Quadratic bezier curve
void quadTo(float cx, float cy, float x, float y)

// Add arc
void addArc(float x, float y, float width, float height,
            float startAngle, float arcAngle)

// Add rectangle
void addRectangle(float x, float y, float width, float height)

// Add string (as outline)
void addString(String string, float x, float y, Font font)

// Add path
void addPath(Path path)

// Close current subpath
void close()

// Get current point
void getCurrentPoint(float[] point)

// Get path data
PathData getPathData()

// Get bounds
void getBounds(float[] bounds)

// Test containment
boolean contains(float x, float y, GC gc, boolean outline)

// Check disposal
boolean isDisposed()

// Dispose (REQUIRED)
void dispose()

// Get device
Device getDevice()
```

---

### 12. Transform

**Purpose**: Represents a 2D affine transformation matrix.

**Device-Specific**: Transforms are allocated on a specific device.

#### Constructors

```java
// Create identity transform
Transform(Device device)

// Create from matrix elements
Transform(Device device, float[] elements)

// Create from individual matrix values
Transform(Device device, float m11, float m12, float m21,
          float m22, float dx, float dy)
```

#### Methods

```java
// Get/set matrix elements
void getElements(float[] elements)
void setElements(float m11, float m12, float m21, float m22,
                 float dx, float dy)

// Transform operations
void translate(float offsetX, float offsetY)
void scale(float scaleX, float scaleY)
void rotate(float angle)  // In degrees
void shear(float shearX, float shearY)

// Multiply by another transform
void multiply(Transform matrix)

// Invert transform
void invert()

// Check if identity
boolean isIdentity()

// Transform points
void transform(float[] pointArray)

// Check disposal
boolean isDisposed()

// Dispose (REQUIRED)
void dispose()

// Get device
Device getDevice()

// String representation
String toString()
```

---

### 13. Pattern

**Purpose**: Represents a pattern for filling shapes.

**Device-Specific**: Patterns are allocated on a specific device.

#### Constructors

```java
// Create gradient pattern
Pattern(Device device, float x1, float y1, float x2, float y2,
        Color color1, Color color2)

// Create gradient with alpha
Pattern(Device device, float x1, float y1, float x2, float y2,
        Color color1, int alpha1, Color color2, int alpha2)

// Create image pattern
Pattern(Device device, Image image)
```

#### Methods

```java
// Check disposal
boolean isDisposed()

// Dispose (REQUIRED)
void dispose()

// Get device
Device getDevice()

// String representation
String toString()
```

---

## Resource Management

### The Fundamental Rule

**"If you created it, you dispose it"**

This is the most important principle in SWT resource management.

### Resource Lifecycle

1. **Allocation**: Resources are allocated in constructors
2. **Usage**: Resources are used in drawing operations
3. **Disposal**: Resources MUST be explicitly disposed when no longer needed

### Why Explicit Disposal?

Java's garbage collection is insufficient for managing operating system resources because:

1. **Finalization is unreliable**: No guaranteed timing
2. **OS resources are limited**: Not infinite like heap memory
3. **Platform differences**: Each OS manages resources differently
4. **Performance**: Waiting for GC can cause resource exhaustion

### Consequences of Not Disposing

- Resource leaks
- Potential system-wide resource exhaustion
- Performance degradation
- May require user to reboot OS in extreme cases

### Disposal Patterns

#### Pattern 1: Immediate Disposal

```java
// Create, use, dispose immediately
GC gc = new GC(control);
gc.drawLine(0, 0, 100, 100);
gc.dispose();
```

#### Pattern 2: Try-Finally

```java
// Ensure disposal even if exception occurs
GC gc = new GC(control);
try {
    gc.drawLine(0, 0, 100, 100);
    gc.drawText("Hello", 10, 10);
} finally {
    gc.dispose();
}
```

#### Pattern 3: Paint Listener

```java
// GC provided by paint event - do NOT dispose
control.addPaintListener(event -> {
    GC gc = event.gc;  // Don't dispose this GC
    gc.drawLine(0, 0, 100, 100);
});
```

#### Pattern 4: Widget Disposal Listener

```java
// Dispose resources when widget is disposed
Font font = new Font(display, "Arial", 12, SWT.NORMAL);
label.setFont(font);
label.addDisposeListener(e -> font.dispose());
```

#### Pattern 5: System Resources (Do NOT Dispose)

```java
// System colors - never dispose these
Color systemColor = display.getSystemColor(SWT.COLOR_BLUE);
// Don't call systemColor.dispose()

// System fonts - never dispose these
Font systemFont = control.getFont();
// Don't call systemFont.dispose()
```

### Parent Disposal Rule

**"Disposing the parent disposes the children"**

When you dispose a widget (like a Shell), all its child widgets are automatically disposed. However, custom-created resources (Color, Font, Image) are NOT automatically disposed.

---

## Device-Specific vs Device-Independent

### Device-Independent Classes

These represent pure data and don't allocate OS resources:

- **RGB / RGBA**: Color values
- **FontData**: Font description
- **ImageData**: Image pixels and metadata
- **PathData**: Path commands and coordinates
- **Rectangle / Point**: Geometry

**No disposal required** - these are plain data objects.

### Device-Specific Classes

These allocate OS resources and MUST be disposed:

- **GC**: Graphics context
- **Image**: Display-ready bitmap
- **Color**: OS color handle
- **Font**: OS font handle
- **Cursor**: OS cursor handle
- **Region**: OS region handle
- **Path**: OS path handle
- **Transform**: OS transform matrix
- **Pattern**: OS pattern brush

**Disposal required** - call `dispose()` when done.

### Conversion Between Forms

```java
// Device-independent → Device-specific
ImageData imageData = new ImageData("file.png");
Image image = new Image(display, imageData);

// Device-specific → Device-independent
Image image = new Image(display, "file.png");
ImageData imageData = image.getImageData();

// Similar for fonts
FontData fontData = new FontData("Arial", 12, SWT.BOLD);
Font font = new Font(display, fontData);
FontData[] retrievedData = font.getFontData();
```

---

## Platform-Specific Handles

### Handle Management

SWT maintains platform-specific handles internally:

- **Windows**: HBRUSH, HPEN, HFONT, HBITMAP, HDC handles
- **macOS**: NSColor, NSFont, NSImage, NSGraphicsContext
- **Linux**: GdkPixbuf, PangoFont, GdkGC, cairo_t

These handles are:
- Allocated in constructors
- Used internally by platform-specific code
- Released in `dispose()` method
- **Never exposed** in public API

### Platform Detection

```java
// Check platform at runtime
String platform = SWT.getPlatform();
// Returns: "win32", "cocoa", "gtk", "motif"

// Use SWT constants
if (SWT.getPlatform().equals("win32")) {
    // Windows-specific code
}
```

---

## Best Practices

### 1. Resource Creation

```java
// ✓ Good: Create on device
Color color = new Color(display, 255, 0, 0);

// ✗ Bad: Creating without device
// Color has no default constructor
```

### 2. Resource Disposal

```java
// ✓ Good: Explicit disposal
Image image = new Image(display, "file.png");
// ... use image ...
image.dispose();

// ✗ Bad: No disposal (resource leak)
Image image = new Image(display, "file.png");
// ... use image ...
// Missing: image.dispose();
```

### 3. GC Usage

```java
// ✓ Good: Dispose GC created by you
GC gc = new GC(control);
try {
    gc.drawLine(0, 0, 100, 100);
} finally {
    gc.dispose();
}

// ✓ Good: Don't dispose GC from event
control.addPaintListener(event -> {
    GC gc = event.gc;  // Managed by SWT
    gc.drawLine(0, 0, 100, 100);
    // No dispose() call
});
```

### 4. System Resources

```java
// ✓ Good: Use system colors without disposal
Color fg = display.getSystemColor(SWT.COLOR_WIDGET_FOREGROUND);
gc.setForeground(fg);
// No fg.dispose()

// ✓ Good: Use control's font without disposal
Font font = control.getFont();
gc.setFont(font);
// No font.dispose()
```

### 5. Resource Sharing

```java
// ✓ Good: Share resources, dispose once
Font headerFont = new Font(display, "Arial", 16, SWT.BOLD);
label1.setFont(headerFont);
label2.setFont(headerFont);
label3.setFont(headerFont);

// Dispose when all widgets using it are disposed
shell.addDisposeListener(e -> headerFont.dispose());
```

### 6. Double Buffering

```java
// ✓ Good: Use offscreen image to reduce flicker
control.addPaintListener(event -> {
    Rectangle bounds = control.getBounds();
    Image offscreen = new Image(display, bounds.width, bounds.height);
    GC gc = new GC(offscreen);
    try {
        // Draw to offscreen image
        gc.setBackground(display.getSystemColor(SWT.COLOR_WHITE));
        gc.fillRectangle(0, 0, bounds.width, bounds.height);
        gc.drawLine(0, 0, bounds.width, bounds.height);

        // Copy offscreen to screen
        event.gc.drawImage(offscreen, 0, 0);
    } finally {
        gc.dispose();
        offscreen.dispose();
    }
});
```

### 7. Clipping for Performance

```java
// ✓ Good: Use clipping to limit drawing
control.addPaintListener(event -> {
    GC gc = event.gc;
    Rectangle clip = gc.getClipping();

    // Only draw what's visible
    if (clip.intersects(drawableArea)) {
        gc.drawImage(image, x, y);
    }
});
```

---

## Common Patterns

### Pattern: Canvas for Custom Drawing

```java
Canvas canvas = new Canvas(shell, SWT.NONE);
canvas.addPaintListener(event -> {
    GC gc = event.gc;

    // Set colors
    gc.setBackground(display.getSystemColor(SWT.COLOR_WHITE));
    gc.setForeground(display.getSystemColor(SWT.COLOR_BLACK));

    // Clear background
    Rectangle bounds = canvas.getBounds();
    gc.fillRectangle(0, 0, bounds.width, bounds.height);

    // Draw content
    gc.drawLine(10, 10, 100, 100);
    gc.drawText("Hello", 10, 120);
});
```

### Pattern: Image Button

```java
Button button = new Button(shell, SWT.PUSH);
Image icon = new Image(display, "icon.png");
button.setImage(icon);

button.addDisposeListener(e -> icon.dispose());
```

### Pattern: Custom Font for Label

```java
Label label = new Label(shell, SWT.NONE);
Font font = new Font(display, "Arial", 14, SWT.BOLD);
label.setFont(font);
label.setText("Custom Font");

label.addDisposeListener(e -> font.dispose());
```

### Pattern: Color Gradient

```java
canvas.addPaintListener(event -> {
    GC gc = event.gc;
    Rectangle bounds = canvas.getBounds();

    Color color1 = new Color(display, 255, 0, 0);    // Red
    Color color2 = new Color(display, 0, 0, 255);    // Blue

    try {
        gc.setForeground(color1);
        gc.setBackground(color2);
        gc.fillGradientRectangle(0, 0, bounds.width, bounds.height, true);
    } finally {
        color1.dispose();
        color2.dispose();
    }
});
```

### Pattern: Image Scaling

```java
Image original = new Image(display, "large.png");
Rectangle bounds = original.getBounds();

// Scale to half size
Image scaled = new Image(display, bounds.width / 2, bounds.height / 2);
GC gc = new GC(scaled);
try {
    gc.drawImage(original,
                 0, 0, bounds.width, bounds.height,      // source
                 0, 0, bounds.width/2, bounds.height/2); // dest
} finally {
    gc.dispose();
}

original.dispose();
// Use scaled image...
scaled.dispose();
```

---

## Implementation Priorities for SWTSharp

### Phase 1: Core Classes (High Priority)

1. **Resource** (abstract base class)
2. **Device** (abstract base class)
3. **RGB / RGBA** (color values)
4. **Color** (device-specific colors)
5. **Rectangle / Point** (geometry)

### Phase 2: Basic Drawing (High Priority)

1. **GC** (graphics context)
   - Basic drawing: drawLine, drawRectangle, drawOval
   - Basic filling: fillRectangle, fillOval
   - Text drawing: drawString, drawText
   - Color management: setForeground, setBackground

### Phase 3: Font Support (Medium Priority)

1. **FontData** (font description)
2. **Font** (device-specific font)
3. **FontMetrics** (font measurements)

### Phase 4: Image Support (Medium Priority)

1. **ImageData** (device-independent image)
2. **PaletteData** (color palette)
3. **Image** (device-specific image)
4. **Image loading** (PNG, JPEG, BMP support)

### Phase 5: Advanced Drawing (Lower Priority)

1. **Path** (vector paths)
2. **Transform** (transformations)
3. **Pattern** (fill patterns)
4. **Region** (clipping regions)
5. **Cursor** (mouse cursors)

### Phase 6: Advanced Features (Future)

1. Alpha blending
2. Antialiasing
3. Advanced text (Unicode, bidirectional)
4. Image filters and effects
5. Printing support

---

## Platform Implementation Notes

### Windows (Win32 GDI/GDI+)

- Use GDI for basic drawing
- Use GDI+ for advanced features (alpha, transforms, antialiasing)
- Handle HDC, HBITMAP, HBRUSH, HPEN
- Manage device contexts carefully (SelectObject, DeleteObject)

### macOS (Cocoa/Quartz)

- Use Quartz 2D (Core Graphics) for drawing
- NSGraphicsContext for GC
- NSImage for images
- NSColor for colors
- NSFont for fonts
- Handle coordinate system differences (macOS origin is bottom-left)

### Linux (GTK/Cairo)

- Use Cairo for drawing
- GdkPixbuf for images
- GdkColor for colors
- PangoFont for fonts
- Handle X11 or Wayland specifics

---

## Testing Strategy

### Unit Tests

1. Resource lifecycle (create, use, dispose)
2. Color construction and conversion (RGB, RGBA, system colors)
3. Font creation and metrics
4. Image loading from various formats
5. ImageData manipulation
6. GC state management (colors, fonts, line styles)

### Integration Tests

1. Drawing primitives on Canvas
2. Image display and scaling
3. Text rendering with different fonts
4. Color gradients
5. Clipping and transformations

### Platform Tests

1. Verify platform-specific handles are created
2. Test resource disposal releases OS resources
3. Verify coordinate system consistency
4. Test platform-specific features

---

## API Design for C#/.NET

### Naming Conventions

```csharp
// Class names: PascalCase
public class GraphicsContext { }  // Not "GC" (conflicts with System.GC)

// Properties instead of getters/setters
public Color Foreground { get; set; }  // Not getForeground()/setForeground()

// Events instead of listeners
public event EventHandler<PaintEventArgs> Paint;

// IDisposable pattern
public class Image : IDisposable
{
    public void Dispose() { /* ... */ }
}
```

### Platform P/Invoke

```csharp
// Windows
[DllImport("gdi32.dll")]
static extern IntPtr CreateSolidBrush(int color);

// macOS (via Objective-C runtime)
[DllImport("/usr/lib/libobjc.dylib")]
static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

// Linux (via GTK)
[DllImport("libgtk-3.so")]
static extern IntPtr gtk_widget_get_window(IntPtr widget);
```

### Resource Management

```csharp
// Use IDisposable
public class Color : IDisposable
{
    private IntPtr handle;
    private bool disposed;

    public void Dispose()
    {
        if (!disposed)
        {
            // Release platform-specific handle
            ReleaseHandle(handle);
            disposed = true;
        }
    }

    ~Color()
    {
        Dispose();
    }
}

// Usage with using statement
using (var color = new Color(display, 255, 0, 0))
{
    gc.Foreground = color;
    gc.DrawLine(0, 0, 100, 100);
}  // Automatically disposed
```

---

## References

### Official Documentation

- [Eclipse SWT Graphics Package](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/graphics/package-summary.html)
- [Introduction to SWT Graphics](https://www.eclipse.org/articles/Article-SWT-graphics/SWT_graphics.html)
- [Managing Operating System Resources](https://www.eclipse.org/articles/swt-design-2/swt-design-2.html)
- [Taking a Look at SWT Images](https://www.eclipse.org/articles/Article-SWT-images/graphics-resources.html)

### API Documentation

- [GC Class](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/graphics/GC.html)
- [Image Class](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/graphics/Image.html)
- [Color Class](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/graphics/Color.html)
- [Font Class](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/graphics/Font.html)

### Design Articles

- [SWT: The Standard Widget Toolkit](https://www.eclipse.org/articles/Article-SWT-Design-1/SWT-Design-1.html)
- [FAQ: Why do I have to dispose of colors, fonts, and images?](https://wiki.eclipse.org/FAQ_Why_do_I_have_to_dispose_of_colors,_fonts,_and_images%3F)

---

## Appendix A: Complete GC Method List

### Drawing Operations

```java
void drawArc(int x, int y, int width, int height, int startAngle, int arcAngle)
void drawFocus(int x, int y, int width, int height)
void drawImage(Image image, int x, int y)
void drawImage(Image image, int srcX, int srcY, int srcWidth, int srcHeight, int destX, int destY, int destWidth, int destHeight)
void drawLine(int x1, int y1, int x2, int y2)
void drawOval(int x, int y, int width, int height)
void drawPath(Path path)
void drawPoint(int x, int y)
void drawPolygon(int[] pointArray)
void drawPolyline(int[] pointArray)
void drawRectangle(int x, int y, int width, int height)
void drawRectangle(Rectangle rect)
void drawRoundRectangle(int x, int y, int width, int height, int arcWidth, int arcHeight)
void drawString(String string, int x, int y)
void drawString(String string, int x, int y, boolean isTransparent)
void drawText(String string, int x, int y)
void drawText(String string, int x, int y, boolean isTransparent)
void drawText(String string, int x, int y, int flags)
void fillArc(int x, int y, int width, int height, int startAngle, int arcAngle)
void fillGradientRectangle(int x, int y, int width, int height, boolean vertical)
void fillOval(int x, int y, int width, int height)
void fillPath(Path path)
void fillPolygon(int[] pointArray)
void fillRectangle(int x, int y, int width, int height)
void fillRectangle(Rectangle rect)
void fillRoundRectangle(int x, int y, int width, int height, int arcWidth, int arcHeight)
```

### State Management

```java
int getAdvanced()
int getAlpha()
int getAntialias()
Color getBackground()
Pattern getBackgroundPattern()
int getCharWidth(char ch)
Rectangle getClipping()
void getClipping(Region region)
int getFillRule()
Font getFont()
FontMetrics getFontMetrics()
Color getForeground()
Pattern getForegroundPattern()
int getInterpolation()
int getLineCap()
int[] getLineDash()
int getLineJoin()
int getLineStyle()
int getLineWidth()
int getStyle()
int getTextAntialias()
Transform getTransform()
boolean getXORMode()

void setAdvanced(boolean advanced)
void setAlpha(int alpha)
void setAntialias(int antialias)
void setBackground(Color color)
void setBackgroundPattern(Pattern pattern)
void setClipping(int x, int y, int width, int height)
void setClipping(Path path)
void setClipping(Rectangle rect)
void setClipping(Region region)
void setFillRule(int rule)
void setFont(Font font)
void setForeground(Color color)
void setForegroundPattern(Pattern pattern)
void setInterpolation(int interpolation)
void setLineCap(int cap)
void setLineDash(int[] dashes)
void setLineJoin(int join)
void setLineStyle(int lineStyle)
void setLineWidth(int lineWidth)
void setTextAntialias(int antialias)
void setTransform(Transform transform)
void setXORMode(boolean xor)
```

### Utility Methods

```java
void copyArea(int srcX, int srcY, int width, int height, int destX, int destY)
void copyArea(int srcX, int srcY, int width, int height, int destX, int destY, boolean paint)
void dispose()
Device getDevice()
GCData getGCData()
boolean isDisposed()
Point stringExtent(String string)
Point textExtent(String string)
Point textExtent(String string, int flags)
```

---

## Appendix B: SWT Constants for Graphics

### Line Styles

```java
SWT.LINE_SOLID
SWT.LINE_DASH
SWT.LINE_DOT
SWT.LINE_DASHDOT
SWT.LINE_DASHDOTDOT
SWT.LINE_CUSTOM
```

### Line Caps

```java
SWT.CAP_FLAT
SWT.CAP_ROUND
SWT.CAP_SQUARE
```

### Line Joins

```java
SWT.JOIN_MITER
SWT.JOIN_ROUND
SWT.JOIN_BEVEL
```

### Fill Rules

```java
SWT.FILL_EVEN_ODD
SWT.FILL_WINDING
```

### Font Styles

```java
SWT.NORMAL
SWT.BOLD
SWT.ITALIC
```

### Draw Text Flags

```java
SWT.DRAW_DELIMITER
SWT.DRAW_TAB
SWT.DRAW_MNEMONIC
SWT.DRAW_TRANSPARENT
```

### Antialiasing

```java
SWT.DEFAULT
SWT.OFF
SWT.ON
```

### Interpolation

```java
SWT.DEFAULT
SWT.NONE
SWT.LOW
SWT.HIGH
```

### Image Copy Flags

```java
SWT.IMAGE_COPY
SWT.IMAGE_DISABLE
SWT.IMAGE_GRAY
```

### Cursor Styles

```java
SWT.CURSOR_ARROW
SWT.CURSOR_WAIT
SWT.CURSOR_CROSS
SWT.CURSOR_APPSTARTING
SWT.CURSOR_HELP
SWT.CURSOR_SIZEALL
SWT.CURSOR_SIZENESW
SWT.CURSOR_SIZENS
SWT.CURSOR_SIZENWSE
SWT.CURSOR_SIZEWE
SWT.CURSOR_SIZEN
SWT.CURSOR_SIZES
SWT.CURSOR_SIZEE
SWT.CURSOR_SIZEW
SWT.CURSOR_SIZENE
SWT.CURSOR_SIZESE
SWT.CURSOR_SIZESW
SWT.CURSOR_SIZENW
SWT.CURSOR_UPARROW
SWT.CURSOR_IBEAM
SWT.CURSOR_NO
SWT.CURSOR_HAND
```

---

*End of Graphics System Implementation Guide*
