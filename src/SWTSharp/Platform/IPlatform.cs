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
    IPlatformBrowser CreateBrowserWidget(IPlatformWidget? parent, int style);
}
