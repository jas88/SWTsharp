using System.Runtime.CompilerServices;

namespace SWTSharp;

/// <summary>
/// String utility extensions for efficient string slicing across different .NET versions.
/// </summary>
internal static class StringExtensions
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Efficiently slices a string using Span&lt;T&gt; (zero-allocation on .NET 8+).
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="start">The starting index.</param>
    /// <param name="length">The number of characters to extract.</param>
    /// <returns>A new string containing the specified slice.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string SliceToString(this string str, int start, int length)
    {
        return str.AsSpan(start, length).ToString();
    }

    /// <summary>
    /// Efficiently slices a string from start to end using Span&lt;T&gt; (zero-allocation on .NET 8+).
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="start">The starting index.</param>
    /// <returns>A new string containing the slice from start to end.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string SliceToString(this string str, int start)
    {
        return str.AsSpan(start).ToString();
    }

    /// <summary>
    /// Converts a ReadOnlySpan&lt;char&gt; slice to a string efficiently.
    /// </summary>
    /// <param name="span">The character span.</param>
    /// <param name="start">The starting index.</param>
    /// <param name="length">The number of characters to extract.</param>
    /// <returns>A new string containing the specified slice.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string SliceToString(this ReadOnlySpan<char> span, int start, int length)
    {
        return span.Slice(start, length).ToString();
    }

    /// <summary>
    /// Converts a ReadOnlySpan&lt;char&gt; slice to a string efficiently.
    /// </summary>
    /// <param name="span">The character span.</param>
    /// <param name="start">The starting index.</param>
    /// <returns>A new string containing the slice from start to end.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string SliceToString(this ReadOnlySpan<char> span, int start)
    {
        return span.Slice(start).ToString();
    }
#else
    /// <summary>
    /// Slices a string using Substring (allocates new string on .NET Standard 2.0).
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="start">The starting index.</param>
    /// <param name="length">The number of characters to extract.</param>
    /// <returns>A new string containing the specified slice.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string SliceToString(this string str, int start, int length)
    {
        return str.Substring(start, length);
    }

    /// <summary>
    /// Slices a string from start to end using Substring (allocates new string on .NET Standard 2.0).
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="start">The starting index.</param>
    /// <returns>A new string containing the slice from start to end.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string SliceToString(this string str, int start)
    {
        return str.Substring(start);
    }
#endif
}
