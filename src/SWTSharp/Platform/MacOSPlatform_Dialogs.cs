using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Dialog methods (MessageBox, FileDialog, ColorDialog, FontDialog).
/// </summary>
internal partial class MacOSPlatform
{
    // REMOVED METHODS (moved to IPlatformDialogService interface):
    // - ShowMessageBox(IntPtr parent, string message, string title, int style)
    // - ShowFileDialog(IntPtr parentHandle, string title, string filterPath, string fileName, string[] filterNames, string[] filterExtensions, int style, bool overwrite)
    // - ShowDirectoryDialog(IntPtr parentHandle, string title, string message, string filterPath)
    // - ShowColorDialog(IntPtr parentHandle, string title, Graphics.RGB initialColor, Graphics.RGB[]? customColors)
    // - ShowFontDialog(IntPtr parentHandle, string title, Graphics.FontData? initialFont, Graphics.RGB? initialColor)
    // These methods are now implemented via the IPlatformDialogService interface using proper handles

    // Helper methods for dialog implementations (kept for potential future use)
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

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_NSColor(IntPtr receiver, IntPtr selector, double red, double green, double blue, double alpha);
}
