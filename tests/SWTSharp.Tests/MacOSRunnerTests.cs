using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Xunit;
using SWTSharp.TestHost;

namespace SWTSharp.Tests;

/// <summary>
/// Dummy test for macOS that ensures tests run through the custom test runner.
/// When invoked by 'dotnet test', it launches the custom runner as an external process.
/// When invoked by the custom runner, it's a no-op that passes.
/// </summary>
public class MacOSRunnerTests
{
    /// <summary>
    /// This test ensures all macOS tests run through the custom test runner
    /// which provides proper Thread 1 dispatch support.
    /// </summary>
    [Fact]
    public void MacOS_Tests_Should_Run_Through_Custom_Runner()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        // Skip in CI environment - CI already uses 'dotnet run --project'
        var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) ||
                   !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

        if (isCI)
        {
            // Running in CI which already uses custom runner via 'dotnet run --project'
            Assert.True(true, "Skipping in CI - already using custom runner");
            return;
        }

        // Check if we're running under the custom test runner
        // The custom runner initializes MainThreadDispatcher
        if (IsRunningUnderCustomRunner())
        {
            // We're running under the custom runner - this is correct
            // Just pass as a no-op
            Assert.True(true, "Running under custom test runner (correct)");
            return;
        }

        // We're running under dotnet test directly - need to launch custom runner
        Console.WriteLine("Detected 'dotnet test' invocation on macOS");
        Console.WriteLine("Launching custom test runner as external process...");
        Console.WriteLine();

        var testAssemblyPath = typeof(MacOSRunnerTests).Assembly.Location;
        var testAssemblyDir = Path.GetDirectoryName(testAssemblyPath)!;
        var testDllName = Path.GetFileName(testAssemblyPath);

        // The test assembly is executable (OutputType=Exe in csproj)
        // We can run it directly: dotnet SWTSharp.Tests.dll
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = testDllName,
            WorkingDirectory = testAssemblyDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Console.WriteLine($"Working directory: {testAssemblyDir}");
        Console.WriteLine($"Command: dotnet {testDllName}");
        Console.WriteLine();

        using var process = new Process { StartInfo = startInfo };

        var output = new System.Text.StringBuilder();
        var error = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
                output.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.Error.WriteLine(e.Data);
                error.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Wait for test runner to complete (with timeout)
        var timeout = TimeSpan.FromMinutes(5);
        if (!process.WaitForExit((int)timeout.TotalMilliseconds))
        {
            process.Kill();
            Assert.Fail($"Custom test runner timed out after {timeout.TotalMinutes} minutes");
        }

        // Parse the test results from output
        var outputText = output.ToString();
        var testResults = ParseTestResults(outputText);

        // Assert the test runner executed successfully
        Assert.True(
            process.ExitCode == 0,
            $"Custom test runner exited with code {process.ExitCode}\n" +
            $"Failed: {testResults.Failed}, Passed: {testResults.Passed}, Total: {testResults.Total}\n" +
            $"Error output:\n{error}"
        );

        // Verify tests actually ran
        Assert.True(
            testResults.Total > 0,
            $"No tests were executed by custom runner\nOutput:\n{outputText}"
        );

        // Verify no test failures
        Assert.True(
            testResults.Failed == 0,
            $"{testResults.Failed} test(s) failed when running through custom runner\n" +
            $"Failed: {testResults.Failed}, Passed: {testResults.Passed}, Total: {testResults.Total}\n" +
            $"Check output above for details"
        );

        Console.WriteLine();
        Console.WriteLine("✅ All tests passed through custom test runner");
        Console.WriteLine($"   Passed: {testResults.Passed}, Total: {testResults.Total}");
    }

    /// <summary>
    /// Detects if we're running under the custom test runner by checking
    /// if MainThreadDispatcher has been initialized.
    /// </summary>
    private static bool IsRunningUnderCustomRunner()
    {
        try
        {
            // Try to check if MainThreadDispatcher is initialized
            // If it throws "not initialized", we're not under custom runner
            // This is a bit hacky but works for detection
            var isInitialized = MainThreadDispatcher.IsInitialized;
            return isInitialized;
        }
        catch (InvalidOperationException)
        {
            // MainThreadDispatcher.IsInitialized threw "not initialized"
            return false;
        }
        catch
        {
            // Any other exception means we can't determine state
            // Assume not running under custom runner
            return false;
        }
    }

    /// <summary>
    /// Parses test results from custom runner output.
    /// Expected format:
    /// === Test Results ===
    /// Passed:  42
    /// Failed:  0
    /// Skipped: 1
    /// Total:   43
    /// </summary>
    private static (int Passed, int Failed, int Skipped, int Total) ParseTestResults(string output)
    {
        var passed = 0;
        var failed = 0;
        var skipped = 0;
        var total = 0;

        // Look for the results section
        var resultsMatch = Regex.Match(output, @"=== Test Results ===", RegexOptions.Multiline);
        if (!resultsMatch.Success)
        {
            return (passed, failed, skipped, total);
        }

        // Extract numbers after the results header
        var resultsSection = output.Substring(resultsMatch.Index);

        var passedMatch = Regex.Match(resultsSection, @"Passed:\s+(\d+)");
        if (passedMatch.Success)
        {
            passed = int.Parse(passedMatch.Groups[1].Value);
        }

        var failedMatch = Regex.Match(resultsSection, @"Failed:\s+(\d+)");
        if (failedMatch.Success)
        {
            failed = int.Parse(failedMatch.Groups[1].Value);
        }

        var skippedMatch = Regex.Match(resultsSection, @"Skipped:\s+(\d+)");
        if (skippedMatch.Success)
        {
            skipped = int.Parse(skippedMatch.Groups[1].Value);
        }

        var totalMatch = Regex.Match(resultsSection, @"Total:\s+(\d+)");
        if (totalMatch.Success)
        {
            total = int.Parse(totalMatch.Groups[1].Value);
        }

        return (passed, failed, skipped, total);
    }
}
