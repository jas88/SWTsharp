using System.Reflection;
using Xunit.Sdk;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Logs UTC timestamps to stderr before/after each test, so we can correlate
/// with GTK-CRITICAL timestamps to identify which test triggers the crash.
/// </summary>
public class TimestampTestLoggerAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest)
    {
        Console.Error.WriteLine(
            $"[TEST-START {System.DateTime.UtcNow:HH:mm:ss.fff}] {methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name}");
    }

    public override void After(MethodInfo methodUnderTest)
    {
        Console.Error.WriteLine(
            $"[TEST-END   {System.DateTime.UtcNow:HH:mm:ss.fff}] {methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name}");
    }
}
