//This assumes that this is always running from a mac with global workloads
const string DotnetToolPathDefault = "/usr/local/share/dotnet/dotnet";
string DotnetVersion = Argument("targetFrameworkVersion", EnvironmentVariable("TARGET_FRAMEWORK_VERSION") ?? "net9.0");
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

List<string> GetTestCategoriesToRunSeparately(string projectPath)
{

    if (!string.IsNullOrEmpty(testFilter))
    {
        return new List<string> { testFilter };
    }

    if (!projectPath.EndsWith("Controls.DeviceTests.csproj") && !projectPath.EndsWith("Core.DeviceTests.csproj"))
    {
        return new List<string>
        {
            ""
        };
    }

	var file = Context.GetCallerInfo().SourceFilePath;
	var directoryPath = file.GetDirectory().FullPath;
	Information($"Directory: {directoryPath}");
	Information(directoryPath);

	// Search for files that match the pattern
	List<FilePath> dllFilePath = null;
    
    if (projectPath.EndsWith("Controls.DeviceTests.csproj"))
        dllFilePath = GetFiles($"{directoryPath}/../../**/Microsoft.Maui.Controls.DeviceTests.dll").ToList();

    if (projectPath.EndsWith("Core.DeviceTests.csproj"))
        dllFilePath = GetFiles($"{directoryPath}/../../**/Microsoft.Maui.Core.DeviceTests.dll").ToList();

    System.Reflection.Assembly loadedAssembly = null;

    foreach (var filePath in dllFilePath)
    {
        try
        {
            loadedAssembly = System.Reflection.Assembly.LoadFrom(filePath.FullPath);
            Information($"Loaded assembly from {filePath}: {loadedAssembly.FullName}");
            break; // Exit the loop if the assembly is loaded successfully
        }
        catch (Exception ex)
        {
            Warning($"Failed to load assembly from {filePath}: {ex.Message}");
        }
    }

    if (loadedAssembly == null)
    {
        throw new Exception("No test assembly found.");
    }
	var testCategoryType = loadedAssembly.GetType("Microsoft.Maui.DeviceTests.TestCategory");

	var values = new List<string>();

	foreach (var field in testCategoryType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
	{
		if (field.FieldType == typeof(string))
		{
			values.Add($"Category={(string)field.GetValue(null)}");
		}
	}
	
	return values.ToList();
}

void HandleTestResults(string resultsDir, bool testsFailed, bool needsNameFix, string suffix = null)
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
            MoveFile(resultsFile, resultsFile.GetDirectory().CombineWithFilePath($"testResults{suffix}.xml"));
            var logFiles = GetFiles($"{resultsDir}/*.log");

            foreach (var logFile in logFiles)
            {
                if (logFile.GetFilename().ToString().StartsWith("testResults"))
                {
                    // These are log files that have already been renamed
                    continue;
                }

                Information($"Log file found: {logFile.GetFilename().ToString()}");
                MoveFile(logFile, resultsFile.GetDirectory().CombineWithFilePath($"testResults{suffix}-{logFile.GetFilename()}"));
            }
        }
    }

    if (testsFailed && IsCIBuild())
    {
        var failurePath = $"{resultsDir}/TestResultsFailures/{Guid.NewGuid()}";
        EnsureDirectoryExists(failurePath);
        // The tasks will retry the tests and overwrite the failed results each retry
        // we want to retain the failed results for diagnostic purposes

        var searchQuery = "*.*";

        if (!string.IsNullOrWhiteSpace(suffix))
        {
            searchQuery = $"*{suffix}*.*";
        }

        // Only copy files from this suffix set of failures
        CopyFiles($"{resultsDir}/{searchQuery}", failurePath);

        // We don't want these to upload
        MoveFile($"{failurePath}/testResults{suffix}.xml", $"{failurePath}/Results{suffix}.xml");
    }
    FailRunOnOnlyInconclusiveTests($"{resultsDir}/testResults{suffix}.xml");
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

void RunMacAndiOSTests(
    string project, string device, string resultsDir, string config, string tfm, string rid, string toolPath, string projectPath,
    Func<string, DotNetToolSettings> getSettings)
{
    Exception exception = null;
	foreach (var category in GetTestCategoriesToRunSeparately(projectPath))
	{
	    bool testsFailed = true;
		Information($"Running tests for category: {category}");
		var settings = getSettings(category);
        var suffix = category.Split('=').Skip(1).FirstOrDefault();

		try
		{
			for(int i = 0; i < 2; i++)
			{
				Information($"Running test attempt {i}");
				try
				{
					DotNetTool("tool", settings);
					testsFailed = false;
					break;
				}
				catch (Exception ex)
				{
					Information($"Test attempt {i} failed: {ex.Message}");
					bool isLaunchFailure = IsSimulatorLaunchFailure(ex);
					
					if (isLaunchFailure)
					{
						Information("Detected simulator launch failure (exit code 4). This may be a transient issue.");
					}
					
					if (i == 1)
                    {
						throw;
                    }
                    else
                    {
                        // delete any log files created so it's fresh for the rerun
			            HandleTestResults(resultsDir, false, true, "-" + suffix);
                        var logFiles = GetFiles($"{resultsDir}/*-{suffix}*");

                        foreach (var logFile in logFiles)
                        {
                            DeleteFile(logFile);
                        }
                        
                        // For launch failures, add a small delay to let the simulator settle
                        if (isLaunchFailure)
                        {
                            Information("Adding delay before retry due to launch failure...");
                            System.Threading.Thread.Sleep(5000); // 5 second delay
                        }
                    }
				}
			}
		}
		catch (Exception ex)
		{
			exception = ex;
		}
		finally
		{
			HandleTestResults(resultsDir, testsFailed, true, "-" + suffix);
		}
	}

	Information("Testing completed.");
	if (exception is not null)
	{
		throw exception;
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

    string toolPath = null;

    if (isLocalDotnet)
    {
        var ext = IsRunningOnWindows() ? ".exe" : "";
        var dir = MakeAbsolute(Directory("../../.dotnet/"));
        toolPath = dir.CombineWithFilePath($"dotnet{ext}").FullPath;
    }

    Information($"Using {(isLocalDotnet ? "local" : "system")} dotnet: {toolPath ?? "<null>"}");
    
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

bool IsSimulatorLaunchFailure(Exception ex)
{
    // Check if the exception message contains indicators of simulator launch failures
    var message = ex.Message;
    return message.Contains("simctl returned exit code 4") || 
           message.Contains("HE0042") ||
           message.Contains("Could not launch the app") ||
           message.Contains("FBSOpenApplicationServiceErrorDomain") ||
           message.Contains("Simulator device failed to launch");
}
