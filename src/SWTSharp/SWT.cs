namespace SWTSharp;

/// <summary>
/// SWT constants and utility methods.
/// This class provides platform-independent constants used throughout the SWT API.
/// </summary>
public static class SWT
{
    // Error codes
    public const int ERROR_UNSPECIFIED = 1;
    public const int ERROR_NO_HANDLES = 2;
    public const int ERROR_NO_MORE_CALLBACKS = 3;
    public const int ERROR_NULL_ARGUMENT = 4;
    public const int ERROR_INVALID_ARGUMENT = 5;
    public const int ERROR_INVALID_RANGE = 6;
    public const int ERROR_CANNOT_BE_ZERO = 7;
    public const int ERROR_CANNOT_GET_ITEM = 8;
    public const int ERROR_CANNOT_GET_SELECTION = 9;
    public const int ERROR_CANNOT_GET_ITEM_HEIGHT = 11;
    public const int ERROR_CANNOT_GET_TEXT = 12;
    public const int ERROR_CANNOT_SET_TEXT = 13;
    public const int ERROR_ITEM_NOT_ADDED = 14;
    public const int ERROR_ITEM_NOT_REMOVED = 15;
    public const int ERROR_NOT_IMPLEMENTED = 20;
    public const int ERROR_MENU_NOT_DROP_DOWN = 21;
    public const int ERROR_THREAD_INVALID_ACCESS = 22;
    public const int ERROR_WIDGET_DISPOSED = 24;
    public const int ERROR_MENUITEM_NOT_CASCADE = 27;
    public const int ERROR_CANNOT_SET_SELECTION = 28;
    public const int ERROR_CANNOT_SET_MENU = 29;
    public const int ERROR_CANNOT_SET_ENABLED = 30;
    public const int ERROR_CANNOT_GET_ENABLED = 31;
    public const int ERROR_INVALID_PARENT = 32;
    public const int ERROR_MENU_NOT_BAR = 33;
    public const int ERROR_CANNOT_GET_COUNT = 36;
    public const int ERROR_MENU_NOT_POP_UP = 37;
    public const int ERROR_UNSUPPORTED_DEPTH = 38;
    public const int ERROR_IO = 39;
    public const int ERROR_INVALID_IMAGE = 40;
    public const int ERROR_UNSUPPORTED_FORMAT = 42;
    public const int ERROR_INVALID_SUBCLASS = 43;
    public const int ERROR_GRAPHIC_DISPOSED = 44;
    public const int ERROR_DEVICE_DISPOSED = 45;
    public const int ERROR_FAILED_EXEC = 46;
    public const int ERROR_FAILED_LOAD_LIBRARY = 47;
    public const int ERROR_INVALID_FONT = 48;

    // Widget styles
    public const int NONE = 0;
    public const int BAR = 1 << 1;
    public const int DROP_DOWN = 1 << 2;
    public const int POP_UP = 1 << 3;
    public const int SEPARATOR = 1 << 1;
    public const int TOGGLE = 1 << 1;
    public const int ARROW = 1 << 2;
    public const int PUSH = 1 << 3;
    public const int RADIO = 1 << 4;
    public const int CHECK = 1 << 5;
    public const int CASCADE = 1 << 6;
    public const int MULTI = 1 << 1;
    public const int SINGLE = 1 << 2;
    public const int READ_ONLY = 1 << 3;
    public const int WRAP = 1 << 6;
    public const int SEARCH = 1 << 7;
    public const int SIMPLE = 1 << 6;
    public const int PASSWORD = 1 << 22;
    public const int SHADOW_IN = 1 << 2;
    public const int SHADOW_OUT = 1 << 3;
    public const int SHADOW_ETCHED_IN = 1 << 4;
    public const int SHADOW_ETCHED_OUT = 1 << 6;
    public const int SHADOW_NONE = 1 << 5;
    public const int BORDER = 1 << 11;
    public const int FLAT = 1 << 22;
    public const int CLIP_CHILDREN = 1 << 22;
    public const int CLIP_SIBLINGS = 1 << 23;
    public const int ON_TOP = 1 << 24;
    public const int LEAD = 1 << 14;
    public const int LEFT = 1 << 14;
    public const int TRAIL = 1 << 17;
    public const int RIGHT = 1 << 17;
    public const int CENTER = 1 << 24;
    public const int HORIZONTAL = 1 << 8;
    public const int VERTICAL = 1 << 9;
    public const int TOP = 1 << 10;
    public const int BOTTOM = 1 << 11;
    public const int DATE = 1 << 5;
    public const int TIME = 1 << 7;
    public const int CALENDAR = 1 << 10;
    public const int SHORT = 1 << 15;
    public const int MEDIUM = 1 << 16;
    public const int LONG = 1 << 28;
    public const int MOZILLA = 1 << 15;
    public const int WEBKIT = 1 << 16;
    public const int FULL_SELECTION = 1 << 16;
    public const int HIDE_SELECTION = 1 << 15;

    // Dialog styles
    public const int OPEN = 1 << 0;
    public const int SAVE = 1 << 1;

    // MessageBox icons
    public const int ICON_ERROR = 1 << 0;
    public const int ICON_INFORMATION = 1 << 1;
    public const int ICON_QUESTION = 1 << 2;
    public const int ICON_WARNING = 1 << 3;
    public const int ICON_WORKING = 1 << 4;

    // MessageBox buttons
    public const int OK = 1 << 5;
    public const int CANCEL = 1 << 8;
    public const int YES = 1 << 6;
    public const int NO = 1 << 7;
    public const int RETRY = 1 << 12;
    public const int ABORT = 1 << 9;
    public const int IGNORE = 1 << 10;

    // Layout constants
    public const int DEFAULT = -1;
    public const int FILL = 1 << 4;

    // Shell styles
    public const int SHELL_TRIM = CLOSE | TITLE | MIN | MAX | RESIZE;

    // System styles
    public const int APPLICATION_MODAL = 1 << 16;
    public const int MODELESS = 0;
    public const int PRIMARY_MODAL = 1 << 15;
    public const int SYSTEM_MODAL = 1 << 17;
    public const int SHEET = 1 << 28;
    public const int DIALOG_TRIM = CLOSE | TITLE | BORDER;
    public const int RESIZE = 1 << 4;
    public const int MIN = 1 << 7;
    public const int MAX = 1 << 10;
    public const int NO_TRIM = 1 << 3;
    public const int TITLE = 1 << 5;
    public const int CLOSE = 1 << 6;
    public const int TOOL = 1 << 2;

    // Layout flags
    public const int CHANGED = 1 << 0;
    public const int ALL = 1 << 1;
    public const int Resize = 1 << 11;

    // Background mode constants
    public const int INHERIT_NONE = 0;
    public const int INHERIT_DEFAULT = 1;
    public const int INHERIT_FORCE = 2;

    // Font styles
    public const int NORMAL = 0;
    public const int BOLD = 1 << 0;
    public const int ITALIC = 1 << 1;

    // Event types
    public const int KeyDown = 1;
    public const int KeyUp = 2;
    public const int MouseDown = 3;
    public const int MouseUp = 4;
    public const int MouseMove = 5;
    public const int MouseEnter = 6;
    public const int MouseExit = 7;
    public const int MouseDoubleClick = 8;
    public const int MouseHover = 9;
    public const int Paint = 10;
    public const int Move = 11;
    public const int Dispose = 13;
    public const int Selection = 14;
    public const int DefaultSelection = 15;
    public const int FocusIn = 16;
    public const int FocusOut = 17;
    public const int Expand = 18;
    public const int Collapse = 19;
    public const int Iconify = 20;
    public const int Deiconify = 21;
    public const int Show = 22;
    public const int Hide = 23;
    public const int Modify = 24;
    public const int Verify = 25;
    public const int Activate = 26;
    public const int Deactivate = 27;
    public const int Help = 28;
    public const int DragDetect = 29;
    public const int Arm = 30;
    public const int Traverse = 31;
    public const int MouseWheel = 32;
    public const int Settings = 33;
    public const int EraseItem = 34;
    public const int MeasureItem = 35;
    public const int PaintItem = 36;

    // Key state masks
    public const int SHIFT = 1 << 0;
    public const int CTRL = 1 << 1;
    public const int ALT = 1 << 2;
    public const int COMMAND = 1 << 3;
    public const int MODIFIER_MASK = SHIFT | CTRL | ALT | COMMAND;

    // Mouse button state masks
    public const int BUTTON1 = 1 << 19;
    public const int BUTTON2 = 1 << 20;
    public const int BUTTON3 = 1 << 21;
    public const int BUTTON4 = 1 << 23;
    public const int BUTTON5 = 1 << 25;
    public const int BUTTON_MASK = BUTTON1 | BUTTON2 | BUTTON3 | BUTTON4 | BUTTON5;

    // Platform constants
    public const string PLATFORM_WIN32 = "win32";
    public const string PLATFORM_MACOSX = "macosx";
    public const string PLATFORM_LINUX = "linux";

    /// <summary>
    /// Returns an error message for the given error code.
    /// </summary>
    public static string GetErrorMessage(int code)
    {
        return code switch
        {
            ERROR_UNSPECIFIED => "Unspecified error",
            ERROR_NO_HANDLES => "No more handles",
            ERROR_NO_MORE_CALLBACKS => "No more callbacks",
            ERROR_NULL_ARGUMENT => "Argument cannot be null",
            ERROR_INVALID_ARGUMENT => "Argument not valid",
            ERROR_INVALID_RANGE => "Index out of bounds",
            ERROR_CANNOT_BE_ZERO => "Argument cannot be zero",
            ERROR_CANNOT_GET_ITEM => "Cannot get item",
            ERROR_CANNOT_GET_SELECTION => "Cannot get selection",
            ERROR_CANNOT_GET_TEXT => "Cannot get text",
            ERROR_CANNOT_SET_TEXT => "Cannot set text",
            ERROR_ITEM_NOT_ADDED => "Item not added",
            ERROR_ITEM_NOT_REMOVED => "Item not removed",
            ERROR_NOT_IMPLEMENTED => "Not implemented",
            ERROR_THREAD_INVALID_ACCESS => "Invalid thread access",
            ERROR_WIDGET_DISPOSED => "Widget is disposed",
            ERROR_CANNOT_SET_SELECTION => "Cannot set selection",
            ERROR_IO => "I/O error",
            ERROR_INVALID_IMAGE => "Invalid image",
            ERROR_GRAPHIC_DISPOSED => "Graphic is disposed",
            ERROR_DEVICE_DISPOSED => "Device is disposed",
            _ => $"Unknown error code: {code}"
        };
    }

    // Value-based widget styles
    public const int SMOOTH = 1 << 16;
    public const int INDETERMINATE = 1 << 1;
}
