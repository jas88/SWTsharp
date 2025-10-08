#!/bin/bash
# Script to run tests on macOS with proper threading
# This uses the custom TestRunner that ensures Thread 1 is the UI thread

echo "Running SWTSharp tests on macOS with MainThreadDispatcher..."
dotnet build tests/SWTSharp.Tests/SWTSharp.Tests.csproj -c Debug -p:RunTestRunner=true
dotnet run --project tests/SWTSharp.Tests/SWTSharp.Tests.csproj --no-build -- "$@"
