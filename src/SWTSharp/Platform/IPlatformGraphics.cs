namespace SWTSharp.Platform;

/// <summary>
/// Interface for platform-specific graphics operations.
/// </summary>
internal interface IPlatformGraphics : IPlatform
{
    // Color operations
    IntPtr CreateColor(int red, int green, int blue);
    void DestroyColor(IntPtr handle);

    // Font operations
    IntPtr CreateFont(string name, int height, int style);
    void DestroyFont(IntPtr handle);

    // Image operations
    IntPtr CreateImage(int width, int height);
    (IntPtr Handle, int Width, int Height) LoadImage(string filename);
    void DestroyImage(IntPtr handle);
}
