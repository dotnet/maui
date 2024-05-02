//This assumes that this alwasys running from a mac with global workloads
const string DotnetToolPathDefault = "/usr/local/share/dotnet/dotnet";
const string DotnetVersion = "net8.0";

string TARGET = Argument("target", "Test");

string DEFAULT_PROJECT = "";
string DEFAULT_APP_PROJECT = "";

if (string.Equals(TARGET, "uitest", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Controls/tests/UITests/Controls.AppiumTests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Controls/samples/Controls.Sample.UITests/Controls.Sample.UITests.csproj";
}

if (string.Equals(TARGET, "uitest-build", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Controls/tests/UITests/Controls.AppiumTests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Controls/samples/Controls.Sample.UITests/Controls.Sample.UITests.csproj";
}

if (string.Equals(TARGET, "cg-uitest", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Compatibility/ControlGallery/test/iOS.UITests/Compatibility.ControlGallery.iOS.UITests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Compatibility/ControlGallery/src/iOS/Compatibility.ControlGallery.iOS.csproj";
}

void FailRunOnOnlyInconclusiveTests(string testResultsFile)
{
    // When all tests are inconclusive the run does not fail, check if this is the case and fail the pipeline so we get notified
    var totalTestCount = XmlPeek(testResultsFile, "/test-run/@total");
    var inconclusiveTestCount = XmlPeek(testResultsFile, "/test-run/@inconclusive");

    if (totalTestCount.Equals(inconclusiveTestCount))
    {
        throw new Exception("All tests are marked inconclusive, no tests ran. There is probably something wrong with running the tests.");
    }
}

void CleanResults(string resultsDir)
{
    Information($"Cleaning test results: {resultsDir}");

    if (!IsCIBuild())
    {
        CleanDirectories(resultsDir);
    }
    else
    {
        // Because we retry on CI we don't want to delete the previous failures
        // We want to publish those files for reference
        DeleteFiles(Directory(resultsDir).Path.Combine("*.*").FullPath);
    }
}

void HandleTestResults(string resultsDir, bool testsFailed)
{
    // catalyst test result files are weirdly named, so fix it up
    var resultsFile = GetFiles($"{resultsDir}/xunit-test-*.xml").FirstOrDefault();
    if (FileExists(resultsFile))
    {
        CopyFile(resultsFile, resultsFile.GetDirectory().CombineWithFilePath("TestResults.xml"));
    }

    if (testsFailed && IsCIBuild())
    {
        var failurePath = $"{resultsDir}/TestResultsFailures/{Guid.NewGuid()}";
        EnsureDirectoryExists(failurePath);
        // The tasks will retry the tests and overwrite the failed results each retry
        // we want to retain the failed results for diagnostic purposes
        CopyFiles($"{resultsDir}/*.*", failurePath);

        // We don't want these to upload
        MoveFile($"{failurePath}/TestResults.xml", $"{failurePath}/Results.xml");
    }
    FailRunOnOnlyInconclusiveTests($"{resultsDir}/TestResults.xml");
}

DirectoryPath DetermineBinlogDirectory(string projectPath, string binlogArg)
{
    if (!string.IsNullOrEmpty(binlogArg))
    {
        return new DirectoryPath(binlogArg);
    }
    else if (!string.IsNullOrEmpty(projectPath))
    {
        // Assumes projectPath might be a file path, gets the directory
        var filePath = new FilePath(projectPath);
        return filePath.GetDirectory();
    }
    else
    {
        return null;
    }
}

void LogSetupInfo(string toolPath)
{
    Information($"DOTNET_TOOL_PATH: {toolPath}");
    Information($"Host OS System Arch: {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}");
    Information($"Host Processor System Arch: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
}

string GetDotnetToolPath()
{
    var isLocalDotnet = GetBuildVariable("workloads", "local") == "local";
    var toolPath = isLocalDotnet ? $"{MakeAbsolute(Directory("../../bin/dotnet/")).ToString()}/dotnet" : DotnetToolPathDefault;
    Information(isLocalDotnet ? "Using local dotnet" : "Using system dotnet");
    return toolPath;
}
