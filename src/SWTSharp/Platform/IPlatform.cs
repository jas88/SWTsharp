namespace SWTSharp.Platform;

/// <summary>
/// Interface for platform-specific implementations.
/// </summary>
public partial interface IPlatform
{
    /// <summary>
    /// Initializes the platform.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Processes a single event from the event queue.
    /// </summary>
    bool ProcessEvent();

    /// <summary>
    /// Waits for the next event.
    /// </summary>
    void WaitForEvent();

    /// <summary>
    /// Wakes up the event loop.
    /// </summary>
    void WakeEventLoop();

    /// <summary>
    /// Executes an action on the platform's main thread (macOS only - uses GCD main queue).
    /// On other platforms, this may execute on the UI thread.
    /// </summary>
    void ExecuteOnMainThread(Action action);

    // New platform widget methods (return objects, not handles!)
    IPlatformWindow CreateWindowWidget(int style, string title);
    IPlatformWidget CreateButtonWidget(IPlatformWidget? parent, int style);
    IPlatformWidget CreateLabelWidget(IPlatformWidget? parent, int style);
    IPlatformTextInput CreateTextWidget(IPlatformWidget? parent, int style);
    IPlatformComposite CreateCompositeWidget(IPlatformWidget? parent, int style);
    IPlatformToolBar CreateToolBarWidget(IPlatformWindow parent, int style);

    // Advanced widget factory methods
    IPlatformCombo CreateComboWidget(IPlatformWidget? parent, int style);
    IPlatformList CreateListWidget(IPlatformWidget? parent, int style);
    IPlatformProgressBar CreateProgressBarWidget(IPlatformWidget? parent, int style);
    IPlatformSlider CreateSliderWidget(IPlatformWidget? parent, int style);
    IPlatformScale CreateScaleWidget(IPlatformWidget? parent, int style);
    IPlatformSpinner CreateSpinnerWidget(IPlatformWidget? parent, int style);
    IPlatformTabFolder CreateTabFolderWidget(IPlatformWidget? parent, int style);
    IPlatformTable CreateTableWidget(IPlatformWidget? parent, int style);
    IPlatformComposite CreateTreeWidget(IPlatformWidget? parent, int style);
    IPlatformComposite CreateCanvasWidget(IPlatformWidget? parent, int style);
    IPlatformComposite CreateGroupWidget(IPlatformWidget? parent, int style, string text);

    // Additional widgets
    IPlatformLink CreateLinkWidget(IPlatformWidget? parent, int style);
    IPlatformSash CreateSashWidget(IPlatformWidget? parent, int style);
    IPlatformScrollBar CreateScrollBarWidget(IPlatformWidget? parent, int style);
    IPlatformStyledText CreateStyledTextWidget(IPlatformWidget? parent, int style);
    IPlatformTracker CreateTracker(IPlatformWidget? parent, int style);
    IPlatformDateTime CreateDateTimeWidget(IPlatformWidget? parent, int style);
    IPlatformExpandBar CreateExpandBarWidget(IPlatformWidget? parent, int style);

    // Menu widgets
    IPlatformMenu CreateMenuWidget(int style);
}

/// <summary>
/// Result structure for file dialog.
/// </summary>
public struct FileDialogResult
{
    /// <summary>
    /// Selected file paths (can be multiple for MULTI style).
    /// </summary>
    public string[]? SelectedFiles { get; set; }

    /// <summary>
    /// Selected filter path (directory).
    /// </summary>
    public string? FilterPath { get; set; }

    /// <summary>
    /// Selected filter index (0-based).
    /// </summary>
    public int FilterIndex { get; set; }
}

/// <summary>
/// Result structure for font dialog.
/// </summary>
public struct FontDialogResult
{
    /// <summary>
    /// Selected font data.
    /// </summary>
    public Graphics.FontData? FontData { get; set; }

    /// <summary>
    /// Selected font color.
    /// </summary>
    public Graphics.RGB? Color { get; set; }
}

// CoolBar widget factory (Phase 5.9)
public partial interface IPlatform
{
    IPlatformCoolBar CreateCoolBarWidget(IPlatformWidget? parent, int style);
}

// Browser widget factory
public partial interface IPlatform
{
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Browser widget may use WebView2 on Windows which requires dynamic code generation")]
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Browser widget may use WebView2 on Windows which uses reflection and COM interop")]
#endif
    IPlatformBrowser CreateBrowserWidget(IPlatformWidget? parent, int style);
}

// Dialog methods (Phase 2, Plan 03)
public partial interface IPlatform
{
    /// <summary>
    /// Shows a message box dialog.
    /// </summary>
    /// <param name="parent">Parent window handle</param>
    /// <param name="message">Message to display</param>
    /// <param name="title">Dialog title</param>
    /// <param name="style">Button and icon style flags</param>
    /// <returns>SWT button constant (SWT.OK, SWT.CANCEL, SWT.YES, SWT.NO, etc.)</returns>
    int ShowMessageBox(IntPtr parent, string message, string title, int style);

    /// <summary>
    /// Shows a file dialog (open or save).
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="title">Dialog title</param>
    /// <param name="filterPath">Initial directory path</param>
    /// <param name="fileName">Initial file name</param>
    /// <param name="filterNames">Filter display names</param>
    /// <param name="filterExtensions">Filter extension patterns</param>
    /// <param name="style">Dialog style (SWT.OPEN, SWT.SAVE, SWT.MULTI)</param>
    /// <param name="overwrite">Whether to prompt on overwrite (for save)</param>
    /// <returns>FileDialogResult with selected files, or null files if cancelled</returns>
    FileDialogResult ShowFileDialog(IntPtr parentHandle, string title, string filterPath, string fileName, string[] filterNames, string[] filterExtensions, int style, bool overwrite);

    /// <summary>
    /// Shows a directory selection dialog.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Dialog message</param>
    /// <param name="filterPath">Initial directory path</param>
    /// <returns>Selected directory path, or null if cancelled</returns>
    string? ShowDirectoryDialog(IntPtr parentHandle, string title, string message, string filterPath);

    /// <summary>
    /// Shows a color selection dialog.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="title">Dialog title</param>
    /// <param name="initialColor">Initial color selection</param>
    /// <param name="customColors">Custom color palette (platform-specific)</param>
    /// <returns>Selected color, or null if cancelled</returns>
    Graphics.RGB? ShowColorDialog(IntPtr parentHandle, string title, Graphics.RGB initialColor, Graphics.RGB[]? customColors);

    /// <summary>
    /// Shows a font selection dialog.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="title">Dialog title</param>
    /// <param name="initialFont">Initial font selection</param>
    /// <param name="initialColor">Initial font color</param>
    /// <returns>FontDialogResult with selected font and color, or null FontData if cancelled</returns>
    FontDialogResult ShowFontDialog(IntPtr parentHandle, string title, Graphics.FontData? initialFont, Graphics.RGB? initialColor);
}
