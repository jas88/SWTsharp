using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - Dialog methods (MessageBox, FileDialog, DirectoryDialog, ColorDialog, FontDialog).
/// </summary>
internal partial class LinuxPlatform
{
    // GTK Dialog enums
    private enum GtkMessageType
    {
        Info = 0,
        Warning = 1,
        Question = 2,
        Error = 3
    }

    private enum GtkButtonsType
    {
        None = 0,
        OK = 1,
        Close = 2,
        Cancel = 3,
        YesNo = 4,
        OKCancel = 5
    }

    private enum GtkResponseType
    {
        None = -1,
        Reject = -2,
        Accept = -3,
        DeleteEvent = -4,
        OK = -5,
        Cancel = -6,
        Close = -7,
        Yes = -8,
        No = -9,
        Apply = -10,
        Help = -11
    }

    private enum GtkFileChooserAction
    {
        Open = 0,
        Save = 1,
        SelectFolder = 2,
        CreateFolder = 3
    }

    // GTK Dialog imports
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_message_dialog_new(IntPtr parent, int flags, GtkMessageType type, GtkButtonsType buttons, string message, IntPtr args);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_dialog_run(IntPtr dialog);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_file_chooser_dialog_new(string title, IntPtr parent, GtkFileChooserAction action, string first_button_text, GtkResponseType first_button_response, string second_button_text, GtkResponseType second_button_response, IntPtr terminator);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern bool gtk_file_chooser_set_current_folder(IntPtr chooser, string filename);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_file_chooser_set_current_name(IntPtr chooser, string name);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_file_chooser_set_select_multiple(IntPtr chooser, bool select_multiple);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_file_chooser_set_do_overwrite_confirmation(IntPtr chooser, bool do_overwrite_confirmation);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_file_chooser_get_filename(IntPtr chooser);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_file_chooser_get_filenames(IntPtr chooser);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_file_filter_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_file_filter_set_name(IntPtr filter, string name);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_file_filter_add_pattern(IntPtr filter, string pattern);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_file_chooser_add_filter(IntPtr chooser, IntPtr filter);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_slist_nth_data(IntPtr list, uint n);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint g_slist_length(IntPtr list);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_slist_free(IntPtr list);

    // GTK Color Chooser Dialog imports
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_color_chooser_dialog_new(string title, IntPtr parent);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_color_chooser_set_rgba(IntPtr chooser, ref GdkRGBA color);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_color_chooser_get_rgba(IntPtr chooser, out GdkRGBA color);

    // GTK Font Chooser Dialog imports
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_font_chooser_dialog_new(string title, IntPtr parent);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_font_chooser_set_font(IntPtr fontchooser, string fontname);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_font_chooser_get_font(IntPtr fontchooser);

    // Dialog operations
    public int ShowMessageBox(IntPtr parent, string message, string title, int style)
    {
        // Determine icon type
        GtkMessageType messageType = GtkMessageType.Info;
        if ((style & SWT.ICON_ERROR) != 0)
            messageType = GtkMessageType.Error;
        else if ((style & SWT.ICON_WARNING) != 0)
            messageType = GtkMessageType.Warning;
        else if ((style & SWT.ICON_QUESTION) != 0)
            messageType = GtkMessageType.Question;
        else if ((style & SWT.ICON_INFORMATION) != 0)
            messageType = GtkMessageType.Info;

        // Determine button type
        GtkButtonsType buttonsType = GtkButtonsType.OK;
        if ((style & SWT.OK) != 0 && (style & SWT.CANCEL) != 0)
            buttonsType = GtkButtonsType.OKCancel;
        else if ((style & SWT.YES) != 0 && (style & SWT.NO) != 0)
            buttonsType = GtkButtonsType.YesNo;
        else if ((style & SWT.OK) != 0)
            buttonsType = GtkButtonsType.OK;

        // Create message dialog
        IntPtr dialog = gtk_message_dialog_new(
            parent,
            0, // flags
            messageType,
            buttonsType,
            message ?? string.Empty,
            IntPtr.Zero);

        if (dialog == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK message dialog");
        }

        try
        {
            // Set title if provided
            if (!string.IsNullOrEmpty(title))
            {
                gtk_window_set_title(dialog, title);
            }

            // Run dialog
            int response = gtk_dialog_run(dialog);

            // Map GTK response to SWT constants
            switch ((GtkResponseType)response)
            {
                case GtkResponseType.OK:
                    return SWT.OK;
                case GtkResponseType.Cancel:
                    return SWT.CANCEL;
                case GtkResponseType.Yes:
                    return SWT.YES;
                case GtkResponseType.No:
                    return SWT.NO;
                default:
                    return SWT.CANCEL;
            }
        }
        finally
        {
            gtk_widget_destroy(dialog);
        }
    }

    public FileDialogResult ShowFileDialog(IntPtr parentHandle, string title, string filterPath, string fileName, string[] filterNames, string[] filterExtensions, int style, bool overwrite)
    {
        bool isSave = (style & SWT.SAVE) != 0;
        bool isMulti = (style & SWT.MULTI) != 0;

        GtkFileChooserAction action = isSave ? GtkFileChooserAction.Save : GtkFileChooserAction.Open;

        // Create file chooser dialog
        IntPtr dialog = gtk_file_chooser_dialog_new(
            title ?? "Select File",
            parentHandle,
            action,
            "_Cancel",
            GtkResponseType.Cancel,
            isSave ? "_Save" : "_Open",
            GtkResponseType.Accept,
            IntPtr.Zero);

        if (dialog == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK file chooser dialog");
        }

        try
        {
            // Set initial directory
            if (!string.IsNullOrEmpty(filterPath) && Directory.Exists(filterPath))
            {
                gtk_file_chooser_set_current_folder(dialog, filterPath);
            }

            // Set initial file name
            if (!string.IsNullOrEmpty(fileName))
            {
                if (isSave)
                {
                    gtk_file_chooser_set_current_name(dialog, fileName);
                }
            }

            // Set multi-select
            if (isMulti && !isSave)
            {
                gtk_file_chooser_set_select_multiple(dialog, true);
            }

            // Set overwrite confirmation for save dialogs
            if (isSave && overwrite)
            {
                gtk_file_chooser_set_do_overwrite_confirmation(dialog, true);
            }

            // Add file filters
            if (filterNames != null && filterExtensions != null)
            {
                int count = Math.Min(filterNames.Length, filterExtensions.Length);
                for (int i = 0; i < count; i++)
                {
                    IntPtr filter = gtk_file_filter_new();
                    gtk_file_filter_set_name(filter, filterNames[i]);

                    // Parse extension patterns (e.g., "*.txt", "*.jpg;*.png")
                    string[] patterns = filterExtensions[i].Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string pattern in patterns)
                    {
                        gtk_file_filter_add_pattern(filter, pattern.Trim());
                    }

                    gtk_file_chooser_add_filter(dialog, filter);
                }
            }

            // Run dialog
            int response = gtk_dialog_run(dialog);

            if (response != (int)GtkResponseType.Accept)
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

            if (isMulti)
            {
                // Get multiple files
                IntPtr filesList = gtk_file_chooser_get_filenames(dialog);
                uint length = g_slist_length(filesList);

                if (length > 0)
                {
                    selectedFiles = new string[length];
                    for (uint i = 0; i < length; i++)
                    {
                        IntPtr filePtr = g_slist_nth_data(filesList, i);
#if NETSTANDARD2_0
                        string? file = Marshal.PtrToStringAnsi(filePtr);
#else
                        string? file = Marshal.PtrToStringUTF8(filePtr);
#endif
                        selectedFiles[i] = file ?? string.Empty;

                        // Free the individual string
                        g_free(filePtr);
                    }

                    // Free the list
                    g_slist_free(filesList);

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
            else
            {
                // Get single file
                IntPtr filePtr = gtk_file_chooser_get_filename(dialog);
                if (filePtr != IntPtr.Zero)
                {
#if NETSTANDARD2_0
                    string? file = Marshal.PtrToStringAnsi(filePtr);
#else
                    string? file = Marshal.PtrToStringUTF8(filePtr);
#endif
                    if (!string.IsNullOrEmpty(file))
                    {
                        selectedFiles = new[] { file };
                        resultFilterPath = Path.GetDirectoryName(file);
                    }
                    else
                    {
                        selectedFiles = Array.Empty<string>();
                    }

                    g_free(filePtr);
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
                FilterIndex = 0 // GTK doesn't provide selected filter index easily
            };
        }
        finally
        {
            gtk_widget_destroy(dialog);
        }
    }

    public string? ShowDirectoryDialog(IntPtr parentHandle, string title, string message, string filterPath)
    {
        // Create directory chooser dialog
        IntPtr dialog = gtk_file_chooser_dialog_new(
            title ?? "Select Directory",
            parentHandle,
            GtkFileChooserAction.SelectFolder,
            "_Cancel",
            GtkResponseType.Cancel,
            "_Select",
            GtkResponseType.Accept,
            IntPtr.Zero);

        if (dialog == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK directory chooser dialog");
        }

        try
        {
            // Set initial directory
            if (!string.IsNullOrEmpty(filterPath) && Directory.Exists(filterPath))
            {
                gtk_file_chooser_set_current_folder(dialog, filterPath);
            }

            // Run dialog
            int response = gtk_dialog_run(dialog);

            if (response != (int)GtkResponseType.Accept)
            {
                return null;
            }

            // Get selected directory
            IntPtr dirPtr = gtk_file_chooser_get_filename(dialog);
            if (dirPtr != IntPtr.Zero)
            {
#if NETSTANDARD2_0
                string? directory = Marshal.PtrToStringAnsi(dirPtr);
#else
                string? directory = Marshal.PtrToStringUTF8(dirPtr);
#endif
                g_free(dirPtr);
                return directory;
            }

            return null;
        }
        finally
        {
            gtk_widget_destroy(dialog);
        }
    }

    public Graphics.RGB? ShowColorDialog(IntPtr parentHandle, string title, Graphics.RGB initialColor, Graphics.RGB[]? customColors)
    {
        // Create color chooser dialog
        IntPtr dialog = gtk_color_chooser_dialog_new(title ?? "Select Color", parentHandle);

        if (dialog == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK color chooser dialog");
        }

        try
        {
            // Set initial color
            var gtkColor = new GdkRGBA(initialColor);
            gtk_color_chooser_set_rgba(dialog, ref gtkColor);

            // Run dialog
            int response = gtk_dialog_run(dialog);

            if (response != (int)GtkResponseType.OK)
            {
                return null;
            }

            // Get selected color
            gtk_color_chooser_get_rgba(dialog, out GdkRGBA selectedColor);

            return new Graphics.RGB(
                (byte)(selectedColor.Red * 255),
                (byte)(selectedColor.Green * 255),
                (byte)(selectedColor.Blue * 255)
            );
        }
        finally
        {
            gtk_widget_destroy(dialog);
        }
    }

    public FontDialogResult ShowFontDialog(IntPtr parentHandle, string title, Graphics.FontData? initialFont, Graphics.RGB? initialColor)
    {
        // Create font chooser dialog
        IntPtr dialog = gtk_font_chooser_dialog_new(title ?? "Select Font", parentHandle);

        if (dialog == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK font chooser dialog");
        }

        try
        {
            // Set initial font if provided
            if (initialFont != null && !string.IsNullOrEmpty(initialFont.Name))
            {
                string initialFontDesc = $"{initialFont.Name} {initialFont.Height}";
                gtk_font_chooser_set_font(dialog, initialFontDesc);
            }

            // Run dialog
            int response = gtk_dialog_run(dialog);

            if (response != (int)GtkResponseType.OK)
            {
                return new FontDialogResult
                {
                    FontData = null,
                    Color = null
                };
            }

            // Get selected font
            IntPtr fontPtr = gtk_font_chooser_get_font(dialog);
            if (fontPtr == IntPtr.Zero)
            {
                return new FontDialogResult
                {
                    FontData = null,
                    Color = null
                };
            }

#if NETSTANDARD2_0
            string? fontDesc = Marshal.PtrToStringAnsi(fontPtr);
#else
            string? fontDesc = Marshal.PtrToStringUTF8(fontPtr);
#endif
            g_free(fontPtr);

            if (string.IsNullOrEmpty(fontDesc))
            {
                return new FontDialogResult
                {
                    FontData = null,
                    Color = null
                };
            }

            // Parse font description (e.g., "Sans Bold 12")
            string[] parts = fontDesc.Split(' ');
            if (parts.Length < 2)
            {
                return new FontDialogResult
                {
                    FontData = null,
                    Color = null
                };
            }

            string name = string.Join(" ", parts.Take(parts.Length - 1));
            if (!int.TryParse(parts[parts.Length - 1], out int size))
            {
                size = 10;
            }

            return new FontDialogResult
            {
                FontData = new Graphics.FontData
                {
                    Name = name,
                    Height = size,
                    Style = 0 // Would need to parse style from font description
                },
                Color = initialColor // GTK font dialog doesn't support color selection, return initial color
            };
        }
        finally
        {
            gtk_widget_destroy(dialog);
        }
    }
}
