//This assumes that this is always running from a mac with global workloads
const string DotnetToolPathDefault = "/usr/local/share/dotnet/dotnet";
const string DotnetVersion = "net8.0";
const string TestFramework = "net472";

// Map project types to specific subdirectories under artifacts
var projectMappings = new Dictionary<string, string>
{
    ["Controls.DeviceTests"] = "Controls.DeviceTests",
    ["Core.DeviceTests"] = "Core.DeviceTests",
    ["Graphics.DeviceTests"] = "Graphics.DeviceTests",
    ["MauiBlazorWebView.DeviceTests"] = "MauiBlazorWebView.DeviceTests",
    ["Essentials.DeviceTests"] = "Essentials.DeviceTests",
    ["Controls.TestCases.HostApp"] = "Controls.TestCases.HostApp",
    ["Compatibility.ControlGallery.iOS"] = "Compatibility.ControlGallery.iOS",
    ["Compatibility.ControlGallery.Android"] = "Compatibility.ControlGallery.Android",
};

string TARGET = Argument("target", "Test");

string DEFAULT_PROJECT = "";
string DEFAULT_APP_PROJECT = "";


// "uitest", "uitest-build", and "uitest-prepare" all trigger this case
if (TARGET.StartsWith("uitest", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Controls/tests/TestCases.Shared.Tests/Controls.TestCases.Shared.Tests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj";
}

if (string.Equals(TARGET, "cg-uitest", StringComparison.OrdinalIgnoreCase))
{
    DEFAULT_PROJECT = "../../src/Compatibility/ControlGallery/test/iOS.UITests/Compatibility.ControlGallery.iOS.UITests.csproj";
    DEFAULT_APP_PROJECT = "../../src/Compatibility/ControlGallery/src/iOS/Compatibility.ControlGallery.iOS.csproj";
}

IEnumerable<string> GetTestApplications(string project, string device, string config, string tfm, string rid)
{
    const string binDirBase = "bin";
    const string artifactsDir = "../../artifacts/bin/";
    bool isAndroid = tfm.Contains("android");

    var binDir = new DirectoryPath(project).Combine($"{config}/{tfm}/{rid}");
    IEnumerable<string> applications;

    if (isAndroid)
    {
        applications = FindApksInDirectory(binDir);
    }
    else
    {
        applications = FindAppsInDirectory(binDir);
    }

    // If no applications found, check in specific artifact directories
    if (!applications.Any())
    {
        applications = SearchInArtifacts(binDir, project, config, tfm, rid, isAndroid, artifactsDir);
    }

    if (!applications.Any())
    {
        throw new Exception($"No application was found in the specified directories.");
    }

    return applications;
}

IEnumerable<string> SearchInArtifacts(DirectoryPath originalBinDir, string project, string config, string tfm, string rid, bool isAndroid, string artifactsDir)
{
    foreach (var entry in projectMappings)
    {
        if (project.Contains(entry.Key))
        {
            var binDir = MakeAbsolute(new DirectoryPath($"{artifactsDir}{entry.Value}/{config}/{tfm}/{rid}"));
            IEnumerable<string> applications;

            if (isAndroid)
            {
                applications = FindApksInDirectory(binDir);
            }
            else
            {
                applications = FindAppsInDirectory(binDir);
            }

            if (applications.Any())
            {
                return applications;
            }
        }
    }

    // Return empty if no applications found in any artifact directories
    return Enumerable.Empty<string>();
}

IEnumerable<string> FindAppsInDirectory(DirectoryPath directory)
{
    Information($"Looking for .app in {directory}");
    return GetDirectories($"{directory}/*.app").Select(dir => dir.FullPath);
}

IEnumerable<string> FindApksInDirectory(DirectoryPath directory)
{
    Information($"Looking for .apk files in {directory}");
    return GetFiles($"{directory}/*-Signed.apk").Select(file => file.FullPath);
}


void FailRunOnOnlyInconclusiveTests(string testResultsFile)
{
    // When all tests are inconclusive the run does not fail, check if this is the case and fail the pipeline so we get notified
    var totalTestCount = XmlPeek(testResultsFile, "/assemblies/assembly/@total");
    var inconclusiveTestCount = XmlPeek(testResultsFile, "/assemblies/assembly/@inconclusive");

    if (totalTestCount == null)
    {
        throw new Exception("Could not find total or inconclusive test count in test results.");
    }
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

void HandleTestResults(string resultsDir, bool testsFailed, bool needsNameFix)
{
    Information($"Handling test results: {resultsDir}");

    // catalyst and ios test result files are weirdly named, so fix it up
    if(needsNameFix)
    {
        var resultsFile = GetFiles($"{resultsDir}/xunit-test-*.xml").FirstOrDefault();
        if (resultsFile == null)
        {
            throw new Exception("No test results found.");
        }
        if (FileExists(resultsFile))
        {
            Information($"Test results found on {resultsDir}");
            CopyFile(resultsFile, resultsFile.GetDirectory().CombineWithFilePath("TestResults.xml"));
        }
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
        Warning("No project path or binlog directory specified, using current directory.");
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
    string toolPath;
    
    
    if(IsRunningOnWindows())
    {
        toolPath = isLocalDotnet ? $"{MakeAbsolute(Directory("../../bin/dotnet/")).ToString()}/dotnet.exe" : null;
    }
    else
    {
        toolPath = isLocalDotnet ? $"{MakeAbsolute(Directory("../../bin/dotnet/")).ToString()}/dotnet" : DotnetToolPathDefault;
    }

    Information(isLocalDotnet ? "Using local dotnet" : "Using system dotnet");
    return toolPath;
}

void ExecuteWithRetries(Func<int> action, int retries)
{
    Information($"Retrying {retries} times");
    while (retries > 0)
    {
        if (action() == 0) break;
        retries--;
        System.Threading.Thread.Sleep(1000);
    }
}

string SanitizeTestResultsFilename(string input)
{
    string resultFilename = input.Replace("|", "_").Replace("TestCategory=", "");

    return resultFilename;
}
