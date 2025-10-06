namespace SWTSharp;

/// <summary>
/// SWT-specific exception class.
/// This exception is thrown when an error occurs in SWT operations.
/// </summary>
public class SWTException : Exception
{
    /// <summary>
    /// The SWT error code.
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Creates a new SWTException with the specified error code.
    /// </summary>
    public SWTException(int code) : base(SWT.GetErrorMessage(code))
    {
        Code = code;
    }

    /// <summary>
    /// Creates a new SWTException with the specified error code and message.
    /// </summary>
    public SWTException(int code, string message) : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// Creates a new SWTException with the specified error code, message, and inner exception.
    /// </summary>
    public SWTException(int code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}

/// <summary>
/// Exception thrown when a widget or resource has been disposed.
/// </summary>
public class SWTDisposedException : SWTException
{
    public SWTDisposedException() : base(SWT.ERROR_WIDGET_DISPOSED)
    {
    }

    public SWTDisposedException(string message) : base(SWT.ERROR_WIDGET_DISPOSED, message)
    {
    }
}

/// <summary>
/// Exception thrown when a method is called from the wrong thread.
/// </summary>
public class SWTInvalidThreadException : SWTException
{
    public SWTInvalidThreadException() : base(SWT.ERROR_THREAD_INVALID_ACCESS)
    {
    }

    public SWTInvalidThreadException(string message) : base(SWT.ERROR_THREAD_INVALID_ACCESS, message)
    {
    }
}
