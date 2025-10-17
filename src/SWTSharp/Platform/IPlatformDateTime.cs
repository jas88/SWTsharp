namespace SWTSharp.Platform;

/// <summary>
/// Platform DateTime widget interface.
/// </summary>
public interface IPlatformDateTime : IPlatformWidget, IPlatformEventHandling
{
    /// <summary>
    /// Sets the date portion.
    /// </summary>
    /// <param name="year">Year (1752-9999)</param>
    /// <param name="month">Month (0-11)</param>
    /// <param name="day">Day (1-31)</param>
    void SetDate(int year, int month, int day);

    /// <summary>
    /// Gets the date portion.
    /// </summary>
    /// <returns>Tuple of (year, month, day)</returns>
    (int Year, int Month, int Day) GetDate();

    /// <summary>
    /// Sets the time portion.
    /// </summary>
    /// <param name="hours">Hours (0-23)</param>
    /// <param name="minutes">Minutes (0-59)</param>
    /// <param name="seconds">Seconds (0-59)</param>
    void SetTime(int hours, int minutes, int seconds);

    /// <summary>
    /// Gets the time portion.
    /// </summary>
    /// <returns>Tuple of (hours, minutes, seconds)</returns>
    (int Hours, int Minutes, int Seconds) GetTime();

    /// <summary>
    /// Occurs when the date/time selection changes.
    /// </summary>
    event EventHandler? SelectionChanged;
}
