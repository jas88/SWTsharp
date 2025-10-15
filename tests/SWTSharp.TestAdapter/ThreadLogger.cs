using System;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace SWTSharp.TestAdapter;

public static class ThreadLogger
{
    private static readonly string LogFile = "/tmp/test-thread-log.txt";

    static ThreadLogger()
    {
        File.AppendAllText(LogFile, $"\n=== NEW TEST RUN at {DateTime.Now} ===\n");
        Log("ThreadLogger static constructor");
    }

    public static void Log(string location)
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        var threadName = Thread.CurrentThread.Name ?? "(unnamed)";
        var stackTrace = new StackTrace(1, true);
        var frames = stackTrace.GetFrames();

        var msg = $"[Thread {threadId}:{threadName}] {location}\n";

        if (frames != null && frames.Length > 0)
        {
            for (int i = 0; i < Math.Min(5, frames.Length); i++)
            {
                var method = frames[i].GetMethod();
                msg += $"  [{i}] {method?.DeclaringType?.Name}.{method?.Name}\n";
            }
        }

        File.AppendAllText(LogFile, msg);
        Console.WriteLine(msg);
    }
}
