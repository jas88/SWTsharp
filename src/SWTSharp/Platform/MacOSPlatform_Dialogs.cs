using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Dialog methods (MessageBox, FileDialog, ColorDialog, FontDialog).
/// </summary>
internal partial class MacOSPlatform
{
    // NSAlert button return values
    private const long NSAlertFirstButtonReturn = 1000;
    private const long NSAlertSecondButtonReturn = 1001;
    private const long NSAlertThirdButtonReturn = 1002;

    // NSSavePanel/NSOpenPanel response codes
    private const long NSModalResponseOK = 1;
    private const long NSModalResponseCancel = 0;

    public int ShowMessageBox(IntPtr parent, string message, string title, int style)
    {
        // Create NSAlert
        IntPtr nsAlertClass = objc_getClass("NSAlert");
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr selInit = sel_registerName("init");
        IntPtr selSetMessageText = sel_registerName("setMessageText:");
        IntPtr selSetInformativeText = sel_registerName("setInformativeText:");
        IntPtr selAddButtonWithTitle = sel_registerName("addButtonWithTitle:");
        IntPtr selSetAlertStyle = sel_registerName("setAlertStyle:");
        IntPtr selRunModal = sel_registerName("runModal");
        IntPtr selRelease = sel_registerName("release");

        IntPtr alert = objc_msgSend(nsAlertClass, selAlloc);
        alert = objc_msgSend(alert, selInit);

        try
        {
            // Set title and message
            IntPtr nsTitle = CreateNSString(title ?? string.Empty);
            IntPtr nsMessage = CreateNSString(message ?? string.Empty);

            objc_msgSend(alert, selSetMessageText, nsTitle);
            objc_msgSend(alert, selSetInformativeText, nsMessage);

            // Set alert style based on icon flags
            // NSAlertStyle: 0=Warning, 1=Informational, 2=Critical
            long alertStyle = 1; // Informational by default
            if ((style & SWT.ICON_ERROR) != 0)
                alertStyle = 2; // Critical
            else if ((style & SWT.ICON_WARNING) != 0)
                alertStyle = 0; // Warning
            else if ((style & SWT.ICON_QUESTION) != 0)
                alertStyle = 1; // Informational (no native question style)
            else if ((style & SWT.ICON_INFORMATION) != 0)
                alertStyle = 1; // Informational

            objc_msgSend_IntPtr_long(alert, selSetAlertStyle, alertStyle);

            // Add buttons based on style flags
            // Buttons are added in order: first button is default (rightmost on macOS)
            int firstButton = 0;
            int secondButton = 0;
            int thirdButton = 0;

            if ((style & SWT.OK) != 0 && (style & SWT.CANCEL) != 0)
            {
                // OK/Cancel - OK is default (first button)
                IntPtr okButton = CreateNSString("OK");
                IntPtr cancelButton = CreateNSString("Cancel");
                objc_msgSend(alert, selAddButtonWithTitle, okButton);
                objc_msgSend(alert, selAddButtonWithTitle, cancelButton);
                firstButton = SWT.OK;
                secondButton = SWT.CANCEL;
            }
            else if ((style & SWT.YES) != 0 && (style & SWT.NO) != 0 && (style & SWT.CANCEL) != 0)
            {
                // Yes/No/Cancel
                IntPtr yesButton = CreateNSString("Yes");
                IntPtr noButton = CreateNSString("No");
                IntPtr cancelButton = CreateNSString("Cancel");
                objc_msgSend(alert, selAddButtonWithTitle, yesButton);
                objc_msgSend(alert, selAddButtonWithTitle, noButton);
                objc_msgSend(alert, selAddButtonWithTitle, cancelButton);
                firstButton = SWT.YES;
                secondButton = SWT.NO;
                thirdButton = SWT.CANCEL;
            }
            else if ((style & SWT.YES) != 0 && (style & SWT.NO) != 0)
            {
                // Yes/No
                IntPtr yesButton = CreateNSString("Yes");
                IntPtr noButton = CreateNSString("No");
                objc_msgSend(alert, selAddButtonWithTitle, yesButton);
                objc_msgSend(alert, selAddButtonWithTitle, noButton);
                firstButton = SWT.YES;
                secondButton = SWT.NO;
            }
            else if ((style & SWT.RETRY) != 0 && (style & SWT.CANCEL) != 0)
            {
                // Retry/Cancel
                IntPtr retryButton = CreateNSString("Retry");
                IntPtr cancelButton = CreateNSString("Cancel");
                objc_msgSend(alert, selAddButtonWithTitle, retryButton);
                objc_msgSend(alert, selAddButtonWithTitle, cancelButton);
                firstButton = SWT.RETRY;
                secondButton = SWT.CANCEL;
            }
            else if ((style & SWT.ABORT) != 0 && (style & SWT.RETRY) != 0 && (style & SWT.IGNORE) != 0)
            {
                // Abort/Retry/Ignore
                IntPtr abortButton = CreateNSString("Abort");
                IntPtr retryButton = CreateNSString("Retry");
                IntPtr ignoreButton = CreateNSString("Ignore");
                objc_msgSend(alert, selAddButtonWithTitle, abortButton);
                objc_msgSend(alert, selAddButtonWithTitle, retryButton);
                objc_msgSend(alert, selAddButtonWithTitle, ignoreButton);
                firstButton = SWT.ABORT;
                secondButton = SWT.RETRY;
                thirdButton = SWT.IGNORE;
            }
            else
            {
                // Default: OK only
                IntPtr okButton = CreateNSString("OK");
                objc_msgSend(alert, selAddButtonWithTitle, okButton);
                firstButton = SWT.OK;
            }

            // Run modal dialog
            long response = objc_msgSend_long(alert, selRunModal);

            // Map response to SWT constant
            if (response == NSAlertFirstButtonReturn)
                return firstButton;
            else if (response == NSAlertSecondButtonReturn)
                return secondButton;
            else if (response == NSAlertThirdButtonReturn)
                return thirdButton;

            // Default to CANCEL for unknown responses
            return SWT.CANCEL;
        }
        finally
        {
            objc_msgSend(alert, selRelease);
        }
    }

    public FileDialogResult ShowFileDialog(IntPtr parentHandle, string title, string filterPath, string fileName, string[] filterNames, string[] filterExtensions, int style, bool overwrite)
    {
        bool isSave = (style & SWT.SAVE) != 0;
        bool isMulti = (style & SWT.MULTI) != 0;

        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr selInit = sel_registerName("init");
        IntPtr selSetTitle = sel_registerName("setTitle:");
        IntPtr selSetDirectoryURL = sel_registerName("setDirectoryURL:");
        IntPtr selSetNameFieldStringValue = sel_registerName("setNameFieldStringValue:");
        IntPtr selSetAllowsMultipleSelection = sel_registerName("setAllowsMultipleSelection:");
        IntPtr selSetAllowedFileTypes = sel_registerName("setAllowedFileTypes:");
        IntPtr selRunModal = sel_registerName("runModal");
        IntPtr selURL = sel_registerName("URL");
        IntPtr selURLs = sel_registerName("URLs");
        IntPtr selPath = sel_registerName("path");
        IntPtr selRelease = sel_registerName("release");
        IntPtr selCount = sel_registerName("count");
        IntPtr selObjectAtIndex = sel_registerName("objectAtIndex:");

        // Create appropriate panel
        IntPtr panelClass;
        if (isSave)
        {
            panelClass = objc_getClass("NSSavePanel");
        }
        else
        {
            panelClass = objc_getClass("NSOpenPanel");
        }

        IntPtr panel = objc_msgSend(panelClass, selAlloc);
        panel = objc_msgSend(panel, selInit);

        try
        {
            // Set title
            if (!string.IsNullOrEmpty(title))
            {
                IntPtr nsTitle = CreateNSString(title);
                objc_msgSend(panel, selSetTitle, nsTitle);
            }

            // Set initial directory
            if (!string.IsNullOrEmpty(filterPath) && Directory.Exists(filterPath))
            {
                IntPtr nsUrlClass = objc_getClass("NSURL");
                IntPtr selFileURLWithPath = sel_registerName("fileURLWithPath:");
                IntPtr nsPath = CreateNSString(filterPath);
                IntPtr nsURL = objc_msgSend(nsUrlClass, selFileURLWithPath, nsPath);
                objc_msgSend(panel, selSetDirectoryURL, nsURL);
            }

            // Set initial file name (for save dialogs)
            if (isSave && !string.IsNullOrEmpty(fileName))
            {
                IntPtr nsFileName = CreateNSString(fileName);
                objc_msgSend(panel, selSetNameFieldStringValue, nsFileName);
            }

            // Set multi-select for open dialogs
            if (!isSave && isMulti)
            {
                objc_msgSend_void(panel, selSetAllowsMultipleSelection, true);
            }

            // Set file type filters
            if (filterExtensions != null && filterExtensions.Length > 0)
            {
                string[] extensions = ExtractFileExtensions(filterExtensions);
                if (extensions.Length > 0)
                {
                    IntPtr nsArray = CreateNSArray(extensions);
                    objc_msgSend(panel, selSetAllowedFileTypes, nsArray);
                }
            }

            // Run modal dialog
            long response = objc_msgSend_long(panel, selRunModal);

            if (response != NSModalResponseOK)
            {
                // User cancelled
                return new FileDialogResult
                {
                    SelectedFiles = null,
                    FilterPath = null,
                    FilterIndex = 0
                };
            }

            // Get selected files
            string[] selectedFiles;
            string? resultFilterPath = null;

            if (!isSave && isMulti)
            {
                // Multiple files from open panel
                IntPtr urls = objc_msgSend(panel, selURLs);
                long count = objc_msgSend_long(urls, selCount);

                if (count > 0)
                {
                    selectedFiles = new string[count];
                    for (long i = 0; i < count; i++)
                    {
                        IntPtr url = objc_msgSend_IntPtr_long(urls, selObjectAtIndex, i);
                        IntPtr path = objc_msgSend(url, selPath);
                        selectedFiles[i] = GetNSStringValue(path) ?? string.Empty;
                    }

                    if (selectedFiles.Length > 0 && !string.IsNullOrEmpty(selectedFiles[0]))
                    {
                        resultFilterPath = Path.GetDirectoryName(selectedFiles[0]);
                    }
                }
                else
                {
                    selectedFiles = Array.Empty<string>();
                }
            }
            else
            {
                // Single file
                IntPtr url = objc_msgSend(panel, selURL);
                IntPtr path = objc_msgSend(url, selPath);
                string? filePath = GetNSStringValue(path);

                if (!string.IsNullOrEmpty(filePath))
                {
                    selectedFiles = new[] { filePath };
                    resultFilterPath = Path.GetDirectoryName(filePath);
                }
                else
                {
                    selectedFiles = Array.Empty<string>();
                }
            }

            return new FileDialogResult
            {
                SelectedFiles = selectedFiles,
                FilterPath = resultFilterPath,
                FilterIndex = 0 // macOS doesn't track filter index
            };
        }
        finally
        {
            objc_msgSend(panel, selRelease);
        }
    }

    public string? ShowDirectoryDialog(IntPtr parentHandle, string title, string message, string filterPath)
    {
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr selInit = sel_registerName("init");
        IntPtr selSetTitle = sel_registerName("setTitle:");
        IntPtr selSetMessage = sel_registerName("setMessage:");
        IntPtr selSetDirectoryURL = sel_registerName("setDirectoryURL:");
        IntPtr selSetCanChooseFiles = sel_registerName("setCanChooseFiles:");
        IntPtr selSetCanChooseDirectories = sel_registerName("setCanChooseDirectories:");
        IntPtr selSetAllowsMultipleSelection = sel_registerName("setAllowsMultipleSelection:");
        IntPtr selRunModal = sel_registerName("runModal");
        IntPtr selURL = sel_registerName("URL");
        IntPtr selPath = sel_registerName("path");
        IntPtr selRelease = sel_registerName("release");

        // Create NSOpenPanel configured for directory selection
        IntPtr panelClass = objc_getClass("NSOpenPanel");
        IntPtr panel = objc_msgSend(panelClass, selAlloc);
        panel = objc_msgSend(panel, selInit);

        try
        {
            // Configure for directory selection only
            objc_msgSend_void(panel, selSetCanChooseFiles, false);
            objc_msgSend_void(panel, selSetCanChooseDirectories, true);
            objc_msgSend_void(panel, selSetAllowsMultipleSelection, false);

            // Set title
            if (!string.IsNullOrEmpty(title))
            {
                IntPtr nsTitle = CreateNSString(title);
                objc_msgSend(panel, selSetTitle, nsTitle);
            }

            // Set message
            if (!string.IsNullOrEmpty(message))
            {
                IntPtr nsMessage = CreateNSString(message);
                objc_msgSend(panel, selSetMessage, nsMessage);
            }

            // Set initial directory
            if (!string.IsNullOrEmpty(filterPath) && Directory.Exists(filterPath))
            {
                IntPtr nsUrlClass = objc_getClass("NSURL");
                IntPtr selFileURLWithPath = sel_registerName("fileURLWithPath:");
                IntPtr nsPath = CreateNSString(filterPath);
                IntPtr nsURL = objc_msgSend(nsUrlClass, selFileURLWithPath, nsPath);
                objc_msgSend(panel, selSetDirectoryURL, nsURL);
            }

            // Run modal dialog
            long response = objc_msgSend_long(panel, selRunModal);

            if (response != NSModalResponseOK)
            {
                // User cancelled
                return null;
            }

            // Get selected directory
            IntPtr url = objc_msgSend(panel, selURL);
            IntPtr path = objc_msgSend(url, selPath);
            return GetNSStringValue(path);
        }
        finally
        {
            objc_msgSend(panel, selRelease);
        }
    }

    public Graphics.RGB? ShowColorDialog(IntPtr parentHandle, string title, Graphics.RGB initialColor, Graphics.RGB[]? customColors)
    {
        // macOS uses a shared color panel (NSColorPanel) with runModalForTypes approach
        // or we can use NSColorPanel as a modal panel
        IntPtr nsColorPanelClass = objc_getClass("NSColorPanel");
        IntPtr selSharedColorPanel = sel_registerName("sharedColorPanel");
        IntPtr selSetColor = sel_registerName("setColor:");
        IntPtr selColor = sel_registerName("color");
        IntPtr selOrderFront = sel_registerName("orderFront:");
        IntPtr selSetTitle = sel_registerName("setTitle:");

        IntPtr colorPanel = objc_msgSend(nsColorPanelClass, selSharedColorPanel);

        // Set title if provided
        if (!string.IsNullOrEmpty(title))
        {
            IntPtr nsTitle = CreateNSString(title);
            objc_msgSend(colorPanel, selSetTitle, nsTitle);
        }

        // Set initial color
        IntPtr nsColor = CreateNSColor(
            initialColor.Red / 255.0,
            initialColor.Green / 255.0,
            initialColor.Blue / 255.0,
            1.0);
        objc_msgSend(colorPanel, selSetColor, nsColor);

        // Show color panel and run modal
        // For a true modal dialog, we need to use NSApplication runModalForWindow:
        IntPtr nsAppClass = objc_getClass("NSApplication");
        IntPtr selSharedApplication = sel_registerName("sharedApplication");
        IntPtr selRunModalForWindow = sel_registerName("runModalForWindow:");
        IntPtr selStopModal = sel_registerName("stopModal");

        IntPtr app = objc_msgSend(nsAppClass, selSharedApplication);

        // Order front to show the panel
        objc_msgSend(colorPanel, selOrderFront, IntPtr.Zero);

        // Run modal session
        long response = objc_msgSend_long_IntPtr(app, selRunModalForWindow, colorPanel);

        // NSColorPanel doesn't have a standard OK/Cancel - it's modeless by design
        // For SWT compatibility, we'll get the current color after the panel closes
        // Users close it by clicking the window close button

        // Get selected color
        IntPtr selectedColor = objc_msgSend(colorPanel, selColor);

        if (selectedColor == IntPtr.Zero)
        {
            return null;
        }

        // Get RGB components from NSColor
        // Need to convert to calibrated RGB color space first
        IntPtr selColorUsingColorSpace = sel_registerName("colorUsingColorSpace:");
        IntPtr nsColorSpaceClass = objc_getClass("NSColorSpace");
        IntPtr selSRGBColorSpace = sel_registerName("sRGBColorSpace");
        IntPtr srgbColorSpace = objc_msgSend(nsColorSpaceClass, selSRGBColorSpace);

        IntPtr rgbColor = objc_msgSend(selectedColor, selColorUsingColorSpace, srgbColorSpace);
        if (rgbColor == IntPtr.Zero)
        {
            rgbColor = selectedColor; // Use original if conversion fails
        }

        IntPtr selRedComponent = sel_registerName("redComponent");
        IntPtr selGreenComponent = sel_registerName("greenComponent");
        IntPtr selBlueComponent = sel_registerName("blueComponent");

        double red = objc_msgSend_double(rgbColor, selRedComponent);
        double green = objc_msgSend_double(rgbColor, selGreenComponent);
        double blue = objc_msgSend_double(rgbColor, selBlueComponent);

        return new Graphics.RGB(
            (int)(red * 255),
            (int)(green * 255),
            (int)(blue * 255));
    }

    public FontDialogResult ShowFontDialog(IntPtr parentHandle, string title, Graphics.FontData? initialFont, Graphics.RGB? initialColor)
    {
        // macOS uses NSFontPanel which is typically modeless
        // For modal behavior, we use NSFontManager with a font panel
        IntPtr nsFontManagerClass = objc_getClass("NSFontManager");
        IntPtr selSharedFontManager = sel_registerName("sharedFontManager");
        IntPtr selFontPanel = sel_registerName("fontPanel:");
        IntPtr selSetSelectedFont = sel_registerName("setSelectedFont:isMultiple:");
        IntPtr selSelectedFont = sel_registerName("selectedFont");
        IntPtr selOrderFront = sel_registerName("orderFront:");
        IntPtr selSetTitle = sel_registerName("setTitle:");

        IntPtr fontManager = objc_msgSend(nsFontManagerClass, selSharedFontManager);
        IntPtr fontPanel = objc_msgSend_bool_IntPtr(fontManager, selFontPanel, true);

        // Set title if provided
        if (!string.IsNullOrEmpty(title))
        {
            IntPtr nsTitle = CreateNSString(title);
            objc_msgSend(fontPanel, selSetTitle, nsTitle);
        }

        // Set initial font if provided
        if (initialFont != null)
        {
            IntPtr nsFont = CreateNSFont(initialFont.Name, initialFont.Height, initialFont.Style);
            if (nsFont != IntPtr.Zero)
            {
                objc_msgSend_void_IntPtr_bool(fontManager, selSetSelectedFont, nsFont, false);
            }
        }

        // Show font panel and run modal
        IntPtr nsAppClass = objc_getClass("NSApplication");
        IntPtr selSharedApplication = sel_registerName("sharedApplication");
        IntPtr selRunModalForWindow = sel_registerName("runModalForWindow:");

        IntPtr app = objc_msgSend(nsAppClass, selSharedApplication);

        // Order front to show the panel
        objc_msgSend(fontPanel, selOrderFront, IntPtr.Zero);

        // Run modal session
        long response = objc_msgSend_long_IntPtr(app, selRunModalForWindow, fontPanel);

        // Get selected font
        IntPtr selectedFont = objc_msgSend(fontManager, selSelectedFont);

        if (selectedFont == IntPtr.Zero)
        {
            return new FontDialogResult
            {
                FontData = null,
                Color = null
            };
        }

        // Extract font information
        IntPtr selFontName = sel_registerName("fontName");
        IntPtr selPointSize = sel_registerName("pointSize");

        IntPtr fontNamePtr = objc_msgSend(selectedFont, selFontName);
        string? fontName = GetNSStringValue(fontNamePtr);

        double pointSize = objc_msgSend_double(selectedFont, selPointSize);

        // Determine style from font traits
        IntPtr selTraitsOfFont = sel_registerName("traitsOfFont:");
        long traits = objc_msgSend_long_IntPtr(fontManager, selTraitsOfFont, selectedFont);

        int fontStyle = SWT.NORMAL;
        const long NSBoldFontMask = 0x00000002;
        const long NSItalicFontMask = 0x00000001;

        if ((traits & NSBoldFontMask) != 0)
            fontStyle |= SWT.BOLD;
        if ((traits & NSItalicFontMask) != 0)
            fontStyle |= SWT.ITALIC;

        var fontData = new Graphics.FontData(
            fontName ?? "System",
            (int)pointSize,
            fontStyle);

        return new FontDialogResult
        {
            FontData = fontData,
            Color = initialColor // macOS font panel doesn't typically have color selection
        };
    }

    // Helper method to create NSFont
    private IntPtr CreateNSFont(string name, int size, int style)
    {
        IntPtr nsFontClass = objc_getClass("NSFont");
        IntPtr selFontWithName = sel_registerName("fontWithName:size:");
        IntPtr nsName = CreateNSString(name);

        IntPtr font = objc_msgSend_IntPtr_double(nsFontClass, selFontWithName, nsName, (double)size);

        if (font == IntPtr.Zero)
        {
            // Fall back to system font
            IntPtr selSystemFontOfSize = sel_registerName("systemFontOfSize:");
            font = objc_msgSend_IntPtr_double(nsFontClass, selSystemFontOfSize, IntPtr.Zero, (double)size);
        }

        // Apply bold/italic traits if needed
        if (font != IntPtr.Zero && style != SWT.NORMAL)
        {
            IntPtr nsFontManagerClass = objc_getClass("NSFontManager");
            IntPtr selSharedFontManager = sel_registerName("sharedFontManager");
            IntPtr fontManager = objc_msgSend(nsFontManagerClass, selSharedFontManager);

            if ((style & SWT.BOLD) != 0)
            {
                IntPtr selConvertFontToHaveTrait = sel_registerName("convertFont:toHaveTrait:");
                font = objc_msgSend_IntPtr_long(fontManager, selConvertFontToHaveTrait, font, 0x00000002); // NSBoldFontMask
            }

            if ((style & SWT.ITALIC) != 0)
            {
                IntPtr selConvertFontToHaveTrait = sel_registerName("convertFont:toHaveTrait:");
                font = objc_msgSend_IntPtr_long(fontManager, selConvertFontToHaveTrait, font, 0x00000001); // NSItalicFontMask
            }
        }

        return font;
    }

    // Helper methods for dialog implementations
    private string[] ExtractFileExtensions(string[] filterExtensions)
    {
        var extensions = new List<string>();

        foreach (var filter in filterExtensions)
        {
            if (string.IsNullOrEmpty(filter) || filter == "*.*" || filter == "*")
                continue;

            // Parse patterns like "*.txt", "*.jpg;*.png"
            string[] patterns = filter.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pattern in patterns)
            {
                string ext = pattern.Trim();
                if (ext.StartsWith("*."))
                {
                    ext = ext.SliceToString(2); // Remove "*."
                }
                else if (ext.StartsWith("."))
                {
                    ext = ext.SliceToString(1); // Remove "."
                }

                if (!string.IsNullOrEmpty(ext))
                {
                    extensions.Add(ext);
                }
            }
        }

        return extensions.ToArray();
    }

    private IntPtr CreateNSArray(string[] items)
    {
        IntPtr nsArrayClass = objc_getClass("NSArray");
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr selInitWithObjects = sel_registerName("initWithObjects:count:");

        // Create array of NSString objects
        IntPtr[] nsStrings = new IntPtr[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            nsStrings[i] = CreateNSString(items[i]);
        }

        // Allocate memory for object array
        IntPtr objectsPtr = Marshal.AllocHGlobal(IntPtr.Size * items.Length);
        try
        {
            Marshal.Copy(nsStrings, 0, objectsPtr, items.Length);

            // Create NSArray
            IntPtr array = objc_msgSend(nsArrayClass, selAlloc);
            return objc_msgSend(array, selInitWithObjects, objectsPtr, new IntPtr(items.Length));
        }
        finally
        {
            Marshal.FreeHGlobal(objectsPtr);
        }
    }

    private IntPtr CreateNSColor(double red, double green, double blue, double alpha)
    {
        IntPtr nsColorClass = objc_getClass("NSColor");
        IntPtr selColorWithDeviceRed = sel_registerName("colorWithDeviceRed:green:blue:alpha:");

        // Call colorWithDeviceRed:green:blue:alpha: with four double parameters
        return objc_msgSend_NSColor(nsColorClass, selColorWithDeviceRed, red, green, blue, alpha);
    }

    // Additional P/Invoke declarations for dialog methods (not already in MacOSPlatform.cs)
    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_NSColor(IntPtr receiver, IntPtr selector, double red, double green, double blue, double alpha);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr_long(IntPtr receiver, IntPtr selector, long arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr_long(IntPtr receiver, IntPtr selector, IntPtr arg1, long arg2);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr_double(IntPtr receiver, IntPtr selector, IntPtr arg1, double arg2);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_bool_IntPtr(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void_IntPtr_bool(IntPtr receiver, IntPtr selector, IntPtr arg1, bool arg2);

    // Overload for runModalForWindow: and traitsOfFont: which take an IntPtr argument
    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern long objc_msgSend_long_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg);
}
