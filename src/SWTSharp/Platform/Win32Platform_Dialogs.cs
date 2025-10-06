using System.Runtime.InteropServices;
using System.Text;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Dialog methods.
/// </summary>
internal partial class Win32Platform
{
    // File Dialog structures and imports
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OPENFILENAME
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public IntPtr lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }

    // OFN flags
    private const int OFN_READONLY = 0x00000001;
    private const int OFN_OVERWRITEPROMPT = 0x00000002;
    private const int OFN_HIDEREADONLY = 0x00000004;
    private const int OFN_NOCHANGEDIR = 0x00000008;
    private const int OFN_SHOWHELP = 0x00000010;
    private const int OFN_ENABLEHOOK = 0x00000020;
    private const int OFN_NOVALIDATE = 0x00000100;
    private const int OFN_ALLOWMULTISELECT = 0x00000200;
    private const int OFN_EXTENSIONDIFFERENT = 0x00000400;
    private const int OFN_PATHMUSTEXIST = 0x00000800;
    private const int OFN_FILEMUSTEXIST = 0x00001000;
    private const int OFN_CREATEPROMPT = 0x00002000;
    private const int OFN_SHAREAWARE = 0x00004000;
    private const int OFN_NOREADONLYRETURN = 0x00008000;
    private const int OFN_NOTESTFILECREATE = 0x00010000;
    private const int OFN_NONETWORKBUTTON = 0x00020000;
    private const int OFN_EXPLORER = 0x00080000;
    private const int OFN_NODEREFERENCELINKS = 0x00100000;
    private const int OFN_LONGNAMES = 0x00200000;
    private const int OFN_ENABLEINCLUDENOTIFY = 0x00400000;
    private const int OFN_ENABLESIZING = 0x00800000;
    private const int OFN_DONTADDTORECENT = 0x02000000;
    private const int OFN_FORCESHOWHIDDEN = 0x10000000;

    private const string Comdlg32 = "comdlg32.dll";

    // LibraryImport doesn't support complex struct marshalling, use DllImport
    [DllImport(Comdlg32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetOpenFileName(ref OPENFILENAME ofn);

    [DllImport(Comdlg32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetSaveFileName(ref OPENFILENAME ofn);

    // MessageBox constants
    private const uint MB_OK = 0x00000000;
    private const uint MB_OKCANCEL = 0x00000001;
    private const uint MB_ABORTRETRYIGNORE = 0x00000002;
    private const uint MB_YESNOCANCEL = 0x00000003;
    private const uint MB_YESNO = 0x00000004;
    private const uint MB_RETRYCANCEL = 0x00000005;
    private const uint MB_ICONERROR = 0x00000010;
    private const uint MB_ICONQUESTION = 0x00000020;
    private const uint MB_ICONWARNING = 0x00000030;
    private const uint MB_ICONINFORMATION = 0x00000040;

    // MessageBox return values
    private const int IDOK = 1;
    private const int IDCANCEL = 2;
    private const int IDABORT = 3;
    private const int IDRETRY = 4;
    private const int IDIGNORE = 5;
    private const int IDYES = 6;
    private const int IDNO = 7;

#if NET8_0_OR_GREATER
    [LibraryImport(User32, StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
#else
    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
#endif

    public int ShowMessageBox(IntPtr parent, string message, string title, int style)
    {
        uint flags = 0;

        // Determine button style
        if ((style & (SWT.OK | SWT.CANCEL)) == (SWT.OK | SWT.CANCEL))
        {
            flags |= MB_OKCANCEL;
        }
        else if ((style & (SWT.YES | SWT.NO | SWT.CANCEL)) == (SWT.YES | SWT.NO | SWT.CANCEL))
        {
            flags |= MB_YESNOCANCEL;
        }
        else if ((style & (SWT.YES | SWT.NO)) == (SWT.YES | SWT.NO))
        {
            flags |= MB_YESNO;
        }
        else if ((style & (SWT.RETRY | SWT.CANCEL)) == (SWT.RETRY | SWT.CANCEL))
        {
            flags |= MB_RETRYCANCEL;
        }
        else if ((style & (SWT.ABORT | SWT.RETRY | SWT.IGNORE)) == (SWT.ABORT | SWT.RETRY | SWT.IGNORE))
        {
            flags |= MB_ABORTRETRYIGNORE;
        }
        else
        {
            flags |= MB_OK;
        }

        // Determine icon style
        if ((style & SWT.ICON_ERROR) != 0)
        {
            flags |= MB_ICONERROR;
        }
        else if ((style & SWT.ICON_INFORMATION) != 0)
        {
            flags |= MB_ICONINFORMATION;
        }
        else if ((style & SWT.ICON_QUESTION) != 0)
        {
            flags |= MB_ICONQUESTION;
        }
        else if ((style & SWT.ICON_WARNING) != 0)
        {
            flags |= MB_ICONWARNING;
        }

        int result = MessageBox(parent, message, title, flags);

        // Convert Win32 result to SWT constant
        return result switch
        {
            IDOK => SWT.OK,
            IDCANCEL => SWT.CANCEL,
            IDYES => SWT.YES,
            IDNO => SWT.NO,
            IDABORT => SWT.ABORT,
            IDRETRY => SWT.RETRY,
            IDIGNORE => SWT.IGNORE,
            _ => SWT.CANCEL
        };
    }

    public FileDialogResult ShowFileDialog(IntPtr parentHandle, string title, string filterPath, string fileName, string[] filterNames, string[] filterExtensions, int style, bool overwrite)
    {
        const int MAX_PATH = 260;
        const int MULTI_SELECT_BUFFER = 65536; // 64KB for multiple file selection

        // Determine buffer size based on MULTI style
        bool isMultiSelect = (style & SWT.MULTI) != 0;
        int bufferSize = isMultiSelect ? MULTI_SELECT_BUFFER : MAX_PATH;

        // Allocate file name buffer
        IntPtr fileBuffer = Marshal.AllocHGlobal(bufferSize * 2); // Unicode = 2 bytes per char
        try
        {
            // Initialize buffer with initial file name
            if (!string.IsNullOrEmpty(fileName))
            {
                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(fileName + "\0");
                Marshal.Copy(bytes, 0, fileBuffer, Math.Min(bytes.Length, bufferSize * 2));
            }
            else
            {
                Marshal.WriteInt16(fileBuffer, 0, 0); // Null terminator
            }

            // Build filter string: "Text Files\0*.txt\0All Files\0*.*\0\0"
            string filter = BuildFilterString(filterNames, filterExtensions);

            var ofn = new OPENFILENAME
            {
                lStructSize = Marshal.SizeOf<OPENFILENAME>(),
                hwndOwner = parentHandle,
                hInstance = _hInstance,
                lpstrFilter = filter,
                lpstrCustomFilter = null!,
                nMaxCustFilter = 0,
                nFilterIndex = 1, // 1-based index
                lpstrFile = fileBuffer,
                nMaxFile = bufferSize,
                lpstrFileTitle = IntPtr.Zero,
                nMaxFileTitle = 0,
                lpstrInitialDir = filterPath,
                lpstrTitle = title,
                Flags = OFN_EXPLORER | OFN_ENABLESIZING | OFN_NOCHANGEDIR,
                nFileOffset = 0,
                nFileExtension = 0,
                lpstrDefExt = null!,
                lCustData = IntPtr.Zero,
                lpfnHook = IntPtr.Zero,
                lpTemplateName = null!,
                pvReserved = IntPtr.Zero,
                dwReserved = 0,
                FlagsEx = 0
            };

            // Set flags based on style
            if ((style & SWT.SAVE) == 0) // OPEN dialog
            {
                ofn.Flags |= OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST;
            }

            if ((style & SWT.SAVE) != 0 && overwrite)
            {
                ofn.Flags |= OFN_OVERWRITEPROMPT;
            }

            if (isMultiSelect)
            {
                ofn.Flags |= OFN_ALLOWMULTISELECT;
            }

            // Show dialog
            bool result;
            if ((style & SWT.SAVE) != 0)
            {
                result = GetSaveFileName(ref ofn);
            }
            else
            {
                result = GetOpenFileName(ref ofn);
            }

            if (!result)
            {
                return new FileDialogResult
                {
                    SelectedFiles = null,
                    FilterPath = null,
                    FilterIndex = 0
                };
            }

            // Parse result
            string[] selectedFiles;
            string? resultFilterPath = null;

            if (isMultiSelect)
            {
                // Parse multi-select format: "directory\0file1\0file2\0\0"
                selectedFiles = ParseMultiSelectFiles(fileBuffer, bufferSize);

                // Extract directory from first file (or single item)
                if (selectedFiles.Length > 0)
                {
                    resultFilterPath = Path.GetDirectoryName(selectedFiles[0]);
                }
            }
            else
            {
                // Single file selection
                string selectedFile = Marshal.PtrToStringUni(fileBuffer) ?? string.Empty;
                if (!string.IsNullOrEmpty(selectedFile))
                {
                    selectedFiles = new[] { selectedFile };
                    resultFilterPath = Path.GetDirectoryName(selectedFile);
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
                FilterIndex = ofn.nFilterIndex - 1 // Convert from 1-based to 0-based
            };
        }
        finally
        {
            Marshal.FreeHGlobal(fileBuffer);
        }
    }

    private string BuildFilterString(string[] filterNames, string[] filterExtensions)
    {
        if (filterNames == null || filterExtensions == null || filterNames.Length == 0)
        {
            // Default filter
            return "All Files\0*.*\0\0";
        }

        var builder = new StringBuilder();
        int count = Math.Min(filterNames.Length, filterExtensions.Length);

        for (int i = 0; i < count; i++)
        {
            builder.Append(filterNames[i]);
            builder.Append('\0');
            builder.Append(filterExtensions[i]);
            builder.Append('\0');
        }

        builder.Append('\0'); // Double null terminator
        return builder.ToString();
    }

    private string[] ParseMultiSelectFiles(IntPtr buffer, int bufferSize)
    {
        var files = new List<string>();

        // Read directory path first
        string directory = string.Empty;
        int charIndex = 0;

        while (charIndex < bufferSize)
        {
            char c = (char)Marshal.ReadInt16(buffer, charIndex * 2);
            if (c == '\0')
                break;
            directory += c;
            charIndex++;
        }

        if (string.IsNullOrEmpty(directory))
        {
            return Array.Empty<string>();
        }

        // Check if there are multiple files
        charIndex++; // Skip first null terminator
        int nextCharIndex = charIndex;

        // Peek at next character
        if (nextCharIndex < bufferSize)
        {
            char nextChar = (char)Marshal.ReadInt16(buffer, nextCharIndex * 2);
            if (nextChar == '\0')
            {
                // Only one file selected (directory is actually the full file path)
                return new[] { directory };
            }
        }

        // Multiple files - parse each filename
        while (charIndex < bufferSize)
        {
            string filename = string.Empty;
            while (charIndex < bufferSize)
            {
                char c = (char)Marshal.ReadInt16(buffer, charIndex * 2);
                charIndex++;

                if (c == '\0')
                    break;

                filename += c;
            }

            if (string.IsNullOrEmpty(filename))
                break;

            // Combine directory and filename
            files.Add(Path.Combine(directory, filename));
        }

        return files.Count > 0 ? files.ToArray() : new[] { directory };
    }

    // Shell API for directory browsing
    private const string Shell32 = "shell32.dll";

    // BIF flags
    private const uint BIF_RETURNONLYFSDIRS = 0x00000001;
    private const uint BIF_DONTGOBELOWDOMAIN = 0x00000002;
    private const uint BIF_NEWDIALOGSTYLE = 0x00000040;
    private const uint BIF_EDITBOX = 0x00000010;
    private const uint BIF_USENEWUI = BIF_NEWDIALOGSTYLE | BIF_EDITBOX;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct BROWSEINFO
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        public IntPtr pszDisplayName;
        public string lpszTitle;
        public uint ulFlags;
        public IntPtr lpfn;
        public IntPtr lParam;
        public int iImage;
    }

    [DllImport(Shell32, CharSet = CharSet.Unicode)]
    private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

    [DllImport(Shell32, CharSet = CharSet.Unicode)]
    private static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

    [DllImport("ole32.dll")]
    private static extern void CoTaskMemFree(IntPtr ptr);

    public string? ShowDirectoryDialog(IntPtr parentHandle, string title, string message, string filterPath)
    {
        const int MAX_PATH = 260;

        // Allocate buffer for the display name
        IntPtr displayNameBuffer = Marshal.AllocHGlobal(MAX_PATH * 2); // Unicode
        IntPtr pathBuffer = Marshal.AllocHGlobal(MAX_PATH * 2);

        try
        {
            // Initialize the buffer
            Marshal.WriteInt16(displayNameBuffer, 0, 0);

            var browseInfo = new BROWSEINFO
            {
                hwndOwner = parentHandle,
                pidlRoot = IntPtr.Zero,
                pszDisplayName = displayNameBuffer,
                lpszTitle = string.IsNullOrEmpty(message) ? title : message,
                ulFlags = BIF_RETURNONLYFSDIRS | BIF_USENEWUI,
                lpfn = IntPtr.Zero,
                lParam = IntPtr.Zero,
                iImage = 0
            };

            IntPtr pidl = SHBrowseForFolder(ref browseInfo);

            if (pidl == IntPtr.Zero)
            {
                // User cancelled
                return null;
            }

            try
            {
                // Get the path from the PIDL
                Marshal.WriteInt16(pathBuffer, 0, 0);
                bool success = SHGetPathFromIDList(pidl, pathBuffer);

                if (!success)
                {
                    return null;
                }

                string? selectedPath = Marshal.PtrToStringUni(pathBuffer);
                return selectedPath;
            }
            finally
            {
                // Free the PIDL
                CoTaskMemFree(pidl);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(displayNameBuffer);
            Marshal.FreeHGlobal(pathBuffer);
        }
    }

    // ChooseColor structures and constants
    private const uint CC_RGBINIT = 0x00000001;
    private const uint CC_FULLOPEN = 0x00000002;
    private const uint CC_PREVENTFULLOPEN = 0x00000004;
    private const uint CC_SHOWHELP = 0x00000008;
    private const uint CC_ENABLEHOOK = 0x00000010;
    private const uint CC_SOLIDCOLOR = 0x00000080;
    private const uint CC_ANYCOLOR = 0x00000100;

    [StructLayout(LayoutKind.Sequential)]
    private struct CHOOSECOLOR
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public uint rgbResult;
        public IntPtr lpCustColors;
        public uint Flags;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public IntPtr lpTemplateName;
    }

    [DllImport(Comdlg32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool ChooseColor(ref CHOOSECOLOR cc);

    public Graphics.RGB? ShowColorDialog(IntPtr parentHandle, string title, Graphics.RGB initialColor, Graphics.RGB[]? customColors)
    {
        // Allocate custom colors array (16 colors required by Windows)
        const int CUSTOM_COLOR_COUNT = 16;
        IntPtr customColorsPtr = Marshal.AllocHGlobal(sizeof(uint) * CUSTOM_COLOR_COUNT);

        try
        {
            // Initialize custom colors array
            for (int i = 0; i < CUSTOM_COLOR_COUNT; i++)
            {
                uint color;
                if (customColors != null && i < customColors.Length)
                {
                    // Convert RGB to COLORREF (0x00BBGGRR format)
                    color = (uint)(customColors[i].Red | (customColors[i].Green << 8) | (customColors[i].Blue << 16));
                }
                else
                {
                    color = 0x00FFFFFF; // White
                }
                Marshal.WriteInt32(customColorsPtr, i * sizeof(uint), (int)color);
            }

            // Convert initial color to COLORREF
            uint initialColorRef = (uint)(initialColor.Red | (initialColor.Green << 8) | (initialColor.Blue << 16));

            var cc = new CHOOSECOLOR
            {
                lStructSize = Marshal.SizeOf<CHOOSECOLOR>(),
                hwndOwner = parentHandle,
                hInstance = IntPtr.Zero,
                rgbResult = initialColorRef,
                lpCustColors = customColorsPtr,
                Flags = CC_RGBINIT | CC_FULLOPEN | CC_ANYCOLOR,
                lCustData = IntPtr.Zero,
                lpfnHook = IntPtr.Zero,
                lpTemplateName = IntPtr.Zero
            };

            bool result = ChooseColor(ref cc);

            if (!result)
            {
                // User cancelled
                return null;
            }

            // Convert COLORREF back to RGB
            uint selectedColor = cc.rgbResult;
            int red = (int)(selectedColor & 0xFF);
            int green = (int)((selectedColor >> 8) & 0xFF);
            int blue = (int)((selectedColor >> 16) & 0xFF);

            // Update custom colors array if provided
            if (customColors != null)
            {
                for (int i = 0; i < Math.Min(customColors.Length, CUSTOM_COLOR_COUNT); i++)
                {
                    uint color = (uint)Marshal.ReadInt32(customColorsPtr, i * sizeof(uint));
                    customColors[i] = new Graphics.RGB(
                        (int)(color & 0xFF),
                        (int)((color >> 8) & 0xFF),
                        (int)((color >> 16) & 0xFF)
                    );
                }
            }

            return new Graphics.RGB(red, green, blue);
        }
        finally
        {
            Marshal.FreeHGlobal(customColorsPtr);
        }
    }

    // ChooseFont structures and constants
    private const uint CF_SCREENFONTS = 0x00000001;
    private const uint CF_PRINTERFONTS = 0x00000002;
    private const uint CF_BOTH = CF_SCREENFONTS | CF_PRINTERFONTS;
    private const uint CF_SHOWHELP = 0x00000004;
    private const uint CF_ENABLEHOOK = 0x00000008;
    private const uint CF_INITTOLOGFONTSTRUCT = 0x00000040;
    private const uint CF_EFFECTS = 0x00000100;
    private const uint CF_APPLY = 0x00000200;
    private const uint CF_FORCEFONTEXIST = 0x00010000;
    private const uint CF_FIXEDPITCHONLY = 0x00004000;
    private const uint CF_SCALABLEONLY = 0x00020000;
    private const uint CF_NOSCRIPTSEL = 0x00800000;

    // Font weights
    private const int FW_NORMAL = 400;
    private const int FW_BOLD = 700;

    // Font quality
    private const byte DEFAULT_QUALITY = 0;
    private const byte DRAFT_QUALITY = 1;
    private const byte PROOF_QUALITY = 2;
    private const byte NONANTIALIASED_QUALITY = 3;
    private const byte ANTIALIASED_QUALITY = 4;
    private const byte CLEARTYPE_QUALITY = 5;

    // Font pitch and family
    private const byte DEFAULT_PITCH = 0;
    private const byte FIXED_PITCH = 1;
    private const byte VARIABLE_PITCH = 2;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct LOGFONT
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CHOOSEFONT
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hDC;
        public IntPtr lpLogFont;
        public int iPointSize;
        public uint Flags;
        public uint rgbColors;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public IntPtr lpTemplateName;
        public IntPtr hInstance;
        public IntPtr lpszStyle;
        public ushort nFontType;
        public ushort ___MISSING_ALIGNMENT__;
        public int nSizeMin;
        public int nSizeMax;
    }

    [DllImport(Comdlg32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool ChooseFont(ref CHOOSEFONT cf);

    public FontDialogResult ShowFontDialog(IntPtr parentHandle, string title, Graphics.FontData? initialFont, Graphics.RGB? initialColor)
    {
        // Allocate LOGFONT structure
        IntPtr logFontPtr = Marshal.AllocHGlobal(Marshal.SizeOf<LOGFONT>());

        try
        {
            // Initialize LOGFONT with initial font or defaults
            var logFont = new LOGFONT();

            if (initialFont != null)
            {
                logFont.lfFaceName = initialFont.Name ?? "System";
                // Convert point size to logical units (negative for character height)
                logFont.lfHeight = -MulDiv(initialFont.Height, GetDeviceCaps(IntPtr.Zero, 90), 72); // 90 = LOGPIXELSY
                logFont.lfWeight = (initialFont.Style & SWT.BOLD) != 0 ? FW_BOLD : FW_NORMAL;
                logFont.lfItalic = (byte)((initialFont.Style & SWT.ITALIC) != 0 ? 1 : 0);
            }
            else
            {
                logFont.lfFaceName = "System";
                logFont.lfHeight = -MulDiv(12, GetDeviceCaps(IntPtr.Zero, 90), 72);
                logFont.lfWeight = FW_NORMAL;
                logFont.lfItalic = 0;
            }

            logFont.lfWidth = 0;
            logFont.lfEscapement = 0;
            logFont.lfOrientation = 0;
            logFont.lfUnderline = 0;
            logFont.lfStrikeOut = 0;
            logFont.lfCharSet = 1; // DEFAULT_CHARSET
            logFont.lfOutPrecision = 0; // OUT_DEFAULT_PRECIS
            logFont.lfClipPrecision = 0; // CLIP_DEFAULT_PRECIS
            logFont.lfQuality = CLEARTYPE_QUALITY;
            logFont.lfPitchAndFamily = DEFAULT_PITCH;

            Marshal.StructureToPtr(logFont, logFontPtr, false);

            // Convert initial color to COLORREF
            uint colorRef = 0x00000000; // Black
            if (initialColor.HasValue)
            {
                colorRef = (uint)(initialColor.Value.Red | (initialColor.Value.Green << 8) | (initialColor.Value.Blue << 16));
            }

            var cf = new CHOOSEFONT
            {
                lStructSize = Marshal.SizeOf<CHOOSEFONT>(),
                hwndOwner = parentHandle,
                hDC = IntPtr.Zero,
                lpLogFont = logFontPtr,
                iPointSize = 0,
                Flags = CF_SCREENFONTS | CF_EFFECTS | CF_INITTOLOGFONTSTRUCT | CF_FORCEFONTEXIST,
                rgbColors = colorRef,
                lCustData = IntPtr.Zero,
                lpfnHook = IntPtr.Zero,
                lpTemplateName = IntPtr.Zero,
                hInstance = IntPtr.Zero,
                lpszStyle = IntPtr.Zero,
                nFontType = 0,
                ___MISSING_ALIGNMENT__ = 0,
                nSizeMin = 0,
                nSizeMax = 0
            };

            bool result = ChooseFont(ref cf);

            if (!result)
            {
                // User cancelled
                return new FontDialogResult
                {
                    FontData = null,
                    Color = null
                };
            }

            // Read back the LOGFONT structure
            var selectedLogFont = Marshal.PtrToStructure<LOGFONT>(logFontPtr);

            // Convert LOGFONT to FontData
            int height = Math.Abs(selectedLogFont.lfHeight);
            // Convert logical units back to points
            int pointSize = MulDiv(height, 72, GetDeviceCaps(IntPtr.Zero, 90));

            int style = SWT.NORMAL;
            if (selectedLogFont.lfWeight >= FW_BOLD)
            {
                style |= SWT.BOLD;
            }
            if (selectedLogFont.lfItalic != 0)
            {
                style |= SWT.ITALIC;
            }

            var fontData = new Graphics.FontData(
                selectedLogFont.lfFaceName,
                pointSize,
                style
            );

            // Convert COLORREF back to RGB
            uint selectedColor = cf.rgbColors;
            var color = new Graphics.RGB(
                (int)(selectedColor & 0xFF),
                (int)((selectedColor >> 8) & 0xFF),
                (int)((selectedColor >> 16) & 0xFF)
            );

            return new FontDialogResult
            {
                FontData = fontData,
                Color = color
            };
        }
        finally
        {
            Marshal.FreeHGlobal(logFontPtr);
        }
    }

    // Helper function for font size conversion
    private static int MulDiv(int nNumber, int nNumerator, int nDenominator)
    {
        if (nDenominator == 0)
            return 0;
        return (int)((long)nNumber * nNumerator / nDenominator);
    }

    // Get device capabilities (for font size conversion)
    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
}
