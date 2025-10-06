using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Dialog methods (MessageBox, FileDialog, ColorDialog, FontDialog).
/// </summary>
internal partial class MacOSPlatform
{
    public int ShowMessageBox(IntPtr parent, string message, string title, int style)
    {
        // Initialize NSAlert
        IntPtr nsAlertClass = objc_getClass("NSAlert");
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr selInit = sel_registerName("init");
        IntPtr selSetMessageText = sel_registerName("setMessageText:");
        IntPtr selSetInformativeText = sel_registerName("setInformativeText:");
        IntPtr selAddButtonWithTitle = sel_registerName("addButtonWithTitle:");
        IntPtr selSetAlertStyle = sel_registerName("setAlertStyle:");
        IntPtr selRunModal = sel_registerName("runModal");

        // Create alert
        IntPtr alert = objc_msgSend(nsAlertClass, selAlloc);
        alert = objc_msgSend(alert, selInit);

        // Set title (used as message text in NSAlert)
        if (!string.IsNullOrEmpty(title))
        {
            IntPtr nsTitle = CreateNSString(title);
            objc_msgSend(alert, selSetMessageText, nsTitle);
        }

        // Set message (used as informative text in NSAlert)
        if (!string.IsNullOrEmpty(message))
        {
            IntPtr nsMessage = CreateNSString(message);
            objc_msgSend(alert, selSetInformativeText, nsMessage);
        }

        // Set alert style based on icon
        long alertStyle = 1; // NSAlertStyleInformational (default)
        if ((style & SWT.ICON_ERROR) != 0)
        {
            alertStyle = 2; // NSAlertStyleCritical
        }
        else if ((style & SWT.ICON_WARNING) != 0)
        {
            alertStyle = 0; // NSAlertStyleWarning
        }
        objc_msgSend(alert, selSetAlertStyle, new IntPtr(alertStyle));

        // Add buttons based on style
        bool hasYes = (style & SWT.YES) != 0;
        bool hasNo = (style & SWT.NO) != 0;
        bool hasCancel = (style & SWT.CANCEL) != 0;
        bool hasOk = (style & SWT.OK) != 0;

        if (hasYes)
        {
            IntPtr yesButton = CreateNSString("Yes");
            objc_msgSend(alert, selAddButtonWithTitle, yesButton);
        }
        if (hasNo)
        {
            IntPtr noButton = CreateNSString("No");
            objc_msgSend(alert, selAddButtonWithTitle, noButton);
        }
        if (hasCancel)
        {
            IntPtr cancelButton = CreateNSString("Cancel");
            objc_msgSend(alert, selAddButtonWithTitle, cancelButton);
        }
        if (hasOk || (!hasYes && !hasNo && !hasCancel))
        {
            // Default to OK button if no buttons specified
            IntPtr okButton = CreateNSString("OK");
            objc_msgSend(alert, selAddButtonWithTitle, okButton);
        }

        // Run modal and get response
        long response = objc_msgSend_long(alert, selRunModal);

        // NSAlert button responses: first button = 1000, second = 1001, third = 1002, etc.
        const long NSAlertFirstButtonReturn = 1000;

        int buttonIndex = (int)(response - NSAlertFirstButtonReturn);

        // Map button index to SWT constant
        if (hasYes)
        {
            if (buttonIndex == 0) return SWT.YES;
            buttonIndex--;
        }
        if (hasNo)
        {
            if (buttonIndex == 0) return SWT.NO;
            buttonIndex--;
        }
        if (hasCancel)
        {
            if (buttonIndex == 0) return SWT.CANCEL;
            buttonIndex--;
        }
        if (hasOk || (!hasYes && !hasNo && !hasCancel))
        {
            if (buttonIndex == 0) return SWT.OK;
        }

        return SWT.CANCEL; // Default fallback
    }

    public FileDialogResult ShowFileDialog(IntPtr parentHandle, string title, string filterPath, string fileName, string[] filterNames, string[] filterExtensions, int style, bool overwrite)
    {
        // Initialize selectors for file dialogs
        IntPtr nsOpenPanelClass = objc_getClass("NSOpenPanel");
        IntPtr nsSavePanelClass = objc_getClass("NSSavePanel");
        IntPtr nsUrlClass = objc_getClass("NSURL");
        IntPtr nsArrayClass = objc_getClass("NSArray");

        IntPtr selOpenPanel = sel_registerName("openPanel");
        IntPtr selSavePanel = sel_registerName("savePanel");
        IntPtr selRunModal = sel_registerName("runModal");
        IntPtr selSetTitle = sel_registerName("setTitle:");
        IntPtr selSetMessage = sel_registerName("setMessage:");
        IntPtr selSetCanChooseFiles = sel_registerName("setCanChooseFiles:");
        IntPtr selSetCanChooseDirectories = sel_registerName("setCanChooseDirectories:");
        IntPtr selSetAllowsMultipleSelection = sel_registerName("setAllowsMultipleSelection:");
        IntPtr selSetCanCreateDirectories = sel_registerName("setCanCreateDirectories:");
        IntPtr selSetDirectoryURL = sel_registerName("setDirectoryURL:");
        IntPtr selSetNameFieldStringValue = sel_registerName("setNameFieldStringValue:");
        IntPtr selSetAllowedFileTypes = sel_registerName("setAllowedFileTypes:");
        IntPtr selURLs = sel_registerName("URLs");
        IntPtr selURL = sel_registerName("URL");
        IntPtr selPath = sel_registerName("path");
        IntPtr selCount = sel_registerName("count");
        IntPtr selObjectAtIndex = sel_registerName("objectAtIndex:");
        IntPtr selFileURLWithPath = sel_registerName("fileURLWithPath:");

        // Modal response codes
        const long NSModalResponseOK = 1;

        bool isSave = (style & SWT.SAVE) != 0;
        bool isMulti = (style & SWT.MULTI) != 0;

        IntPtr panel;
        if (isSave)
        {
            panel = objc_msgSend(nsSavePanelClass, selSavePanel);
        }
        else
        {
            panel = objc_msgSend(nsOpenPanelClass, selOpenPanel);

            // Configure for open panel
            objc_msgSend_void(panel, selSetCanChooseFiles, true);
            objc_msgSend_void(panel, selSetCanChooseDirectories, false);
            objc_msgSend_void(panel, selSetAllowsMultipleSelection, isMulti);
        }

        // Set title
        if (!string.IsNullOrEmpty(title))
        {
            IntPtr nsTitle = CreateNSString(title);
            objc_msgSend(panel, selSetTitle, nsTitle);
        }

        // Set initial directory
        if (!string.IsNullOrEmpty(filterPath) && Directory.Exists(filterPath))
        {
            IntPtr pathString = CreateNSString(filterPath);
            IntPtr directoryURL = objc_msgSend(nsUrlClass, selFileURLWithPath, pathString);
            objc_msgSend(panel, selSetDirectoryURL, directoryURL);
        }

        // Set initial file name
        if (!string.IsNullOrEmpty(fileName))
        {
            IntPtr nsFileName = CreateNSString(fileName);
            objc_msgSend(panel, selSetNameFieldStringValue, nsFileName);
        }

        // Set allowed file types (filters)
        if (filterExtensions != null && filterExtensions.Length > 0)
        {
            IntPtr nsArray = CreateNSArray(ExtractFileExtensions(filterExtensions));
            objc_msgSend(panel, selSetAllowedFileTypes, nsArray);
        }

        // Run modal dialog
        long response = (long)objc_msgSend(panel, selRunModal);

        if (response != NSModalResponseOK)
        {
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

        if (isSave)
        {
            // Save panel - single file
            IntPtr url = objc_msgSend(panel, selURL);
            IntPtr pathPtr = objc_msgSend(url, selPath);
            string? path = GetNSStringValue(pathPtr);

            if (!string.IsNullOrEmpty(path))
            {
                selectedFiles = new[] { path };
                resultFilterPath = Path.GetDirectoryName(path);
            }
            else
            {
                selectedFiles = Array.Empty<string>();
            }
        }
        else
        {
            // Open panel - possibly multiple files
            IntPtr urls = objc_msgSend(panel, selURLs);
            int count = (int)(long)objc_msgSend(urls, selCount);

            if (count > 0)
            {
                selectedFiles = new string[count];
                for (int i = 0; i < count; i++)
                {
                    IntPtr url = objc_msgSend(urls, selObjectAtIndex, new IntPtr(i));
                    IntPtr pathPtr = objc_msgSend(url, selPath);
                    string? path = GetNSStringValue(pathPtr);
                    selectedFiles[i] = path ?? string.Empty;
                }

                // Get directory from first file
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

        return new FileDialogResult
        {
            SelectedFiles = selectedFiles,
            FilterPath = resultFilterPath,
            FilterIndex = 0 // macOS doesn't provide selected filter index
        };
    }

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
                    ext = ext.Substring(2); // Remove "*."
                }
                else if (ext.StartsWith("."))
                {
                    ext = ext.Substring(1); // Remove "."
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

    public string? ShowDirectoryDialog(IntPtr parentHandle, string title, string message, string filterPath)
    {
        // Initialize NSOpenPanel for directory selection
        IntPtr nsOpenPanelClass = objc_getClass("NSOpenPanel");
        IntPtr nsUrlClass = objc_getClass("NSURL");

        IntPtr selOpenPanel = sel_registerName("openPanel");
        IntPtr selRunModal = sel_registerName("runModal");
        IntPtr selSetTitle = sel_registerName("setTitle:");
        IntPtr selSetMessage = sel_registerName("setMessage:");
        IntPtr selSetCanChooseFiles = sel_registerName("setCanChooseFiles:");
        IntPtr selSetCanChooseDirectories = sel_registerName("setCanChooseDirectories:");
        IntPtr selSetAllowsMultipleSelection = sel_registerName("setAllowsMultipleSelection:");
        IntPtr selSetDirectoryURL = sel_registerName("setDirectoryURL:");
        IntPtr selURL = sel_registerName("URL");
        IntPtr selPath = sel_registerName("path");
        IntPtr selFileURLWithPath = sel_registerName("fileURLWithPath:");

        // Modal response codes
        const long NSModalResponseOK = 1;

        // Create open panel
        IntPtr panel = objc_msgSend(nsOpenPanelClass, selOpenPanel);

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

        // Set message (prompt)
        if (!string.IsNullOrEmpty(message))
        {
            IntPtr nsMessage = CreateNSString(message);
            objc_msgSend(panel, selSetMessage, nsMessage);
        }

        // Set initial directory
        if (!string.IsNullOrEmpty(filterPath) && Directory.Exists(filterPath))
        {
            IntPtr pathString = CreateNSString(filterPath);
            IntPtr directoryURL = objc_msgSend(nsUrlClass, selFileURLWithPath, pathString);
            objc_msgSend(panel, selSetDirectoryURL, directoryURL);
        }

        // Run modal dialog
        long response = objc_msgSend_long(panel, selRunModal);

        if (response != NSModalResponseOK)
        {
            return null;
        }

        // Get selected directory
        IntPtr url = objc_msgSend(panel, selURL);
        if (url == IntPtr.Zero)
        {
            return null;
        }

        IntPtr pathPtr = objc_msgSend(url, selPath);
        return GetNSStringValue(pathPtr);
    }

    public Graphics.RGB? ShowColorDialog(IntPtr parentHandle, string title, Graphics.RGB initialColor, Graphics.RGB[]? customColors)
    {
        // Initialize NSColorPanel
        IntPtr nsColorPanelClass = objc_getClass("NSColorPanel");
        IntPtr nsColorClass = objc_getClass("NSColor");

        IntPtr selSharedColorPanel = sel_registerName("sharedColorPanel");
        IntPtr selSetColor = sel_registerName("setColor:");
        IntPtr selRunModal = sel_registerName("runModal");
        IntPtr selColor = sel_registerName("color");
        IntPtr selColorWithDeviceRed = sel_registerName("colorWithDeviceRed:green:blue:alpha:");
        IntPtr selRedComponent = sel_registerName("redComponent");
        IntPtr selGreenComponent = sel_registerName("greenComponent");
        IntPtr selBlueComponent = sel_registerName("blueComponent");
        IntPtr selOrderOut = sel_registerName("orderOut:");
        IntPtr selMakeKeyAndOrderFront = sel_registerName("makeKeyAndOrderFront:");

        // Modal response codes
        const long NSModalResponseOK = 1;

        // Get shared color panel
        IntPtr panel = objc_msgSend(nsColorPanelClass, selSharedColorPanel);

        // Create NSColor from initial RGB (converting 0-255 to 0.0-1.0)
        double red = initialColor.Red / 255.0;
        double green = initialColor.Green / 255.0;
        double blue = initialColor.Blue / 255.0;
        double alpha = 1.0;

        IntPtr initialNSColor = CreateNSColor(red, green, blue, alpha);
        objc_msgSend(panel, selSetColor, initialNSColor);

        // Show the panel as modal
        objc_msgSend(panel, selMakeKeyAndOrderFront, IntPtr.Zero);
        long response = objc_msgSend_long(panel, selRunModal);

        // Hide the panel
        objc_msgSend(panel, selOrderOut, IntPtr.Zero);

        if (response != NSModalResponseOK)
        {
            return null;
        }

        // Get selected color
        IntPtr selectedColor = objc_msgSend(panel, selColor);
        if (selectedColor == IntPtr.Zero)
        {
            return null;
        }

        // Extract RGB components (returns 0.0-1.0, convert to 0-255)
        double selectedRed = objc_msgSend_double(selectedColor, selRedComponent);
        double selectedGreen = objc_msgSend_double(selectedColor, selGreenComponent);
        double selectedBlue = objc_msgSend_double(selectedColor, selBlueComponent);

        return new Graphics.RGB(
            (int)(selectedRed * 255),
            (int)(selectedGreen * 255),
            (int)(selectedBlue * 255)
        );
    }

    private IntPtr CreateNSColor(double red, double green, double blue, double alpha)
    {
        IntPtr nsColorClass = objc_getClass("NSColor");
        IntPtr selColorWithDeviceRed = sel_registerName("colorWithDeviceRed:green:blue:alpha:");

        // Call colorWithDeviceRed:green:blue:alpha: with four double parameters
        return objc_msgSend_NSColor(nsColorClass, selColorWithDeviceRed, red, green, blue, alpha);
    }

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_NSColor(IntPtr receiver, IntPtr selector, double red, double green, double blue, double alpha);

    public FontDialogResult ShowFontDialog(IntPtr parentHandle, string title, Graphics.FontData? initialFont, Graphics.RGB? initialColor)
    {
        // Initialize NSFontPanel and NSFontManager
        IntPtr nsFontPanelClass = objc_getClass("NSFontPanel");
        IntPtr nsFontManagerClass = objc_getClass("NSFontManager");
        IntPtr nsFontClass = objc_getClass("NSFont");

        IntPtr selSharedFontPanel = sel_registerName("sharedFontPanel");
        IntPtr selSharedFontManager = sel_registerName("sharedFontManager");
        IntPtr selSetPanelFont = sel_registerName("setPanelFont:isMultiple:");
        IntPtr selRunModal = sel_registerName("runModal");
        IntPtr selPanelConvertFont = sel_registerName("panelConvertFont:");
        IntPtr selFontWithName = sel_registerName("fontWithName:size:");
        IntPtr selSystemFontOfSize = sel_registerName("systemFontOfSize:");
        IntPtr selFamilyName = sel_registerName("familyName");
        IntPtr selPointSize = sel_registerName("pointSize");
        IntPtr selFontDescriptor = sel_registerName("fontDescriptor");
        IntPtr selSymbolicTraits = sel_registerName("symbolicTraits");
        IntPtr selOrderOut = sel_registerName("orderOut:");
        IntPtr selMakeKeyAndOrderFront = sel_registerName("makeKeyAndOrderFront:");

        // Modal response codes
        const long NSModalResponseOK = 1;

        // Get shared font panel and manager
        IntPtr panel = objc_msgSend(nsFontPanelClass, selSharedFontPanel);
        IntPtr fontManager = objc_msgSend(nsFontManagerClass, selSharedFontManager);

        // Create initial font
        IntPtr initialNSFont;
        if (initialFont != null && !string.IsNullOrEmpty(initialFont.Name))
        {
            IntPtr initialFontName = CreateNSString(initialFont.Name);
            double initialFontSize = initialFont.Height > 0 ? initialFont.Height : 12.0;
            initialNSFont = objc_msgSend_NSFont(nsFontClass, selFontWithName, initialFontName, initialFontSize);

            // If font creation failed, use system font
            if (initialNSFont == IntPtr.Zero)
            {
                initialNSFont = objc_msgSend_NSFont(nsFontClass, selSystemFontOfSize, IntPtr.Zero, initialFontSize);
            }
        }
        else
        {
            // Default to system font
            initialNSFont = objc_msgSend_NSFont(nsFontClass, selSystemFontOfSize, IntPtr.Zero, 12.0);
        }

        // Set the panel font
        objc_msgSend_void_bool(panel, selSetPanelFont, initialNSFont, false);

        // Show the panel as modal
        objc_msgSend(panel, selMakeKeyAndOrderFront, IntPtr.Zero);
        long response = objc_msgSend_long(panel, selRunModal);

        // Hide the panel
        objc_msgSend(panel, selOrderOut, IntPtr.Zero);

        if (response != NSModalResponseOK)
        {
            return new FontDialogResult
            {
                FontData = null,
                Color = null
            };
        }

        // Get the selected font
        IntPtr selectedFont = objc_msgSend(fontManager, selPanelConvertFont, initialNSFont);
        if (selectedFont == IntPtr.Zero)
        {
            return new FontDialogResult
            {
                FontData = null,
                Color = null
            };
        }

        // Extract font properties
        IntPtr familyNamePtr = objc_msgSend(selectedFont, selFamilyName);
        string fontName = GetNSStringValue(familyNamePtr);

        double pointSize = objc_msgSend_double(selectedFont, selPointSize);
        int fontSize = (int)Math.Round(pointSize);

        // Get font traits (bold/italic)
        IntPtr fontDescriptor = objc_msgSend(selectedFont, selFontDescriptor);
        int symbolicTraits = 0;
        if (fontDescriptor != IntPtr.Zero)
        {
            IntPtr traitsPtr = objc_msgSend(fontDescriptor, selSymbolicTraits);
            symbolicTraits = (int)(long)traitsPtr;
        }

        // NSFontDescriptorSymbolicTraits: Bold = 1 << 1, Italic = 1 << 0
        int style = SWT.NORMAL;
        if ((symbolicTraits & (1 << 1)) != 0) // Bold
        {
            style |= SWT.BOLD;
        }
        if ((symbolicTraits & (1 << 0)) != 0) // Italic
        {
            style |= SWT.ITALIC;
        }

        var resultFontData = new Graphics.FontData(fontName, fontSize, style);

        return new FontDialogResult
        {
            FontData = resultFontData,
            Color = initialColor // macOS NSFontPanel doesn't support color selection
        };
    }

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_NSFont(IntPtr receiver, IntPtr selector, IntPtr arg1, double size);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void_bool(IntPtr receiver, IntPtr selector, IntPtr arg1, bool arg2);
}
