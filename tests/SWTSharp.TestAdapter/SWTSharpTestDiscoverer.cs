using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SWTSharp.TestAdapter;

/// <summary>
/// Custom VSTest discoverer that finds xUnit tests and marks them for separate-process execution.
/// This adapter delegates discovery to xUnit but prepares tests to run in a macOS-compatible host.
/// </summary>
[FileExtension(".dll")]
[FileExtension(".exe")]
[DefaultExecutorUri(SWTSharpTestExecutor.ExecutorUri)]
public class SWTSharpTestDiscoverer : ITestDiscoverer
{
    public void DiscoverTests(
        IEnumerable<string> sources,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        logger.SendMessage(TestMessageLevel.Informational, "SWTSharp TestAdapter: Starting test discovery...");

        foreach (var source in sources)
        {
            try
            {
                // Only process assemblies that contain SWTSharp tests
                if (!IsSWTSharpTestAssembly(source))
                {
                    logger.SendMessage(TestMessageLevel.Informational,
                        $"SWTSharp TestAdapter: Skipping {Path.GetFileName(source)} (not a SWTSharp test assembly)");
                    continue;
                }

                logger.SendMessage(TestMessageLevel.Informational,
                    $"SWTSharp TestAdapter: Discovering tests in {Path.GetFileName(source)}");

                DiscoverTestsInSource(source, logger, discoverySink);
            }
            catch (Exception ex)
            {
                logger.SendMessage(TestMessageLevel.Error,
                    $"SWTSharp TestAdapter: Error discovering tests in {source}: {ex.Message}");
            }
        }

        logger.SendMessage(TestMessageLevel.Informational, "SWTSharp TestAdapter: Test discovery completed");
    }

    private bool IsSWTSharpTestAssembly(string source)
    {
        // Check if this is a SWTSharp test assembly
        var fileName = Path.GetFileName(source);
        return fileName.IndexOf("SWTSharp", StringComparison.OrdinalIgnoreCase) >= 0 &&
               fileName.IndexOf("Tests", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void DiscoverTestsInSource(string source, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        // Use xUnit's discovery mechanism
        using var framework = new Xunit2(
            AppDomainSupport.Denied,
            new NullSourceInformationProvider(),
            source,
            configFileName: null,
            shadowCopy: false,
            diagnosticMessageSink: new DiagnosticMessageSink(logger));

        var visitor = new TestDiscoveryVisitor();
        framework.Find(includeSourceInformation: true, visitor, TestFrameworkOptions.ForDiscovery());
        visitor.Finished.WaitOne();

        foreach (var testCase in visitor.TestCases)
        {
            var vstestCase = CreateVSTestCase(source, testCase, logger);
            discoverySink.SendTestCase(vstestCase);
        }

        logger.SendMessage(TestMessageLevel.Informational,
            $"SWTSharp TestAdapter: Discovered {visitor.TestCases.Count} tests in {Path.GetFileName(source)}");
    }

    private TestCase CreateVSTestCase(string source, ITestCase xunitTestCase, IMessageLogger logger)
    {
        var testCase = new TestCase(
            fullyQualifiedName: xunitTestCase.DisplayName,
            executorUri: new Uri(SWTSharpTestExecutor.ExecutorUri),
            source: source)
        {
            DisplayName = xunitTestCase.DisplayName,
            Id = Guid.Parse(xunitTestCase.UniqueID)
        };

        // Add metadata for executor
        testCase.SetPropertyValue(TestCaseProperties.XunitTestCase, xunitTestCase.UniqueID);
        testCase.SetPropertyValue(TestCaseProperties.FullyQualifiedName, xunitTestCase.DisplayName);

        // Add source location if available
        if (xunitTestCase.SourceInformation != null)
        {
            testCase.CodeFilePath = xunitTestCase.SourceInformation.FileName;
            testCase.LineNumber = xunitTestCase.SourceInformation.LineNumber ?? 0;
        }

        return testCase;
    }

    private class TestDiscoveryVisitor : IMessageSink
    {
        public List<ITestCase> TestCases { get; } = new();
        public System.Threading.ManualResetEvent Finished { get; } = new(false);

        public bool OnMessage(IMessageSinkMessage message)
        {
            if (message is ITestCaseDiscoveryMessage discoveryMessage)
            {
                TestCases.Add(discoveryMessage.TestCase);
            }
            else if (message is IDiscoveryCompleteMessage)
            {
                Finished.Set();
            }

            return true;
        }

        public void Dispose()
        {
            Finished?.Dispose();
        }
    }

    private class DiagnosticMessageSink : IMessageSink
    {
        private readonly IMessageLogger _logger;

        public DiagnosticMessageSink(IMessageLogger logger)
        {
            _logger = logger;
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            if (message is IDiagnosticMessage diagnostic)
            {
                _logger.SendMessage(TestMessageLevel.Informational,
                    $"xUnit: {diagnostic.Message}");
            }

            return true;
        }

        public void Dispose() { }
    }

    private class NullSourceInformationProvider : ISourceInformationProvider
    {
        public ISourceInformation GetSourceInformation(ITestCase testCase) => null!;
        public void Dispose() { }
    }
}

internal static class TestCaseProperties
{
    public static readonly TestProperty XunitTestCase =
        TestProperty.Register("SWTSharp.XunitTestCase", "xUnit Test Case ID", typeof(string), typeof(SWTSharpTestDiscoverer));

    public static readonly TestProperty FullyQualifiedName =
        TestProperty.Register("SWTSharp.FullyQualifiedName", "Fully Qualified Name", typeof(string), typeof(SWTSharpTestDiscoverer));
}
