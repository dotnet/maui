#load "../cake/helpers.cake"
#load "../cake/dotnet.cake"
#load "./devices-shared.cake"

// Argument handling
var projectPath = Argument("project", EnvironmentVariable("MAC_TEST_PROJECT") ?? DEFAULT_PROJECT);
var testDevice = Argument("device", EnvironmentVariable("MAC_TEST_DEVICE") ?? "maccatalyst");
var dotnetRoot = Argument("dotnet-root", EnvironmentVariable("DOTNET_ROOT"));
var targetFramework = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{DotnetVersion}-maccatalyst");
var binlogArg = Argument("binlog", EnvironmentVariable("MAC_TEST_BINLOG") ?? "");
var testApp = Argument("app", EnvironmentVariable("MAC_TEST_APP") ?? "");
var testAppProjectPath = Argument("appproject", EnvironmentVariable("MAC_TEST_APP_PROJECT") ?? DEFAULT_APP_PROJECT);
var testResultsPath = Argument("results", EnvironmentVariable("MAC_TEST_RESULTS") ?? GetTestResultsDirectory().FullPath);
var runtimeIdentifier = Argument("rid", EnvironmentVariable("MAC_RUNTIME_IDENTIFIER") ?? GetDefaultRuntimeIdentifier());
var deviceCleanupEnabled = Argument("cleanup", true);

// Directory setup
var binlogDirectory = DetermineBinlogDirectory(projectPath, binlogArg).FullPath;
var dotnetToolPath = GetDotnetToolPath();

Information($"Project File: {projectPath}");
Information($"Build Binary Log (binlog): {binlogDirectory}");
Information($"Build Configuration: {configuration}");
Information($"Build Runtime Identifier: {runtimeIdentifier}");
Information($"Build Target Framework: {targetFramework}");
Information($"Test Device: {testDevice}");
Information($"Test Results Path: {testResultsPath}");

Setup(context =>
{
	LogSetupInfo(dotnetToolPath);
	PerformCleanupIfNeeded(deviceCleanupEnabled);
});

Teardown(context =>
{
	PerformCleanupIfNeeded(deviceCleanupEnabled);
});

Task("Cleanup");

Task("Build")
	.WithCriteria(!string.IsNullOrEmpty(projectPath))
	.Does(() =>
	{
		ExecuteBuild(projectPath, binlogDirectory, configuration, runtimeIdentifier, targetFramework, dotnetToolPath);
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
	{
		ExecuteTests(projectPath, testDevice, testResultsPath, configuration, targetFramework, runtimeIdentifier, dotnetToolPath);
	});

Task("uitest")
	.IsDependentOn("uitest-build")
	.Does(() =>
	{
		ExecuteUITests(projectPath, testAppProjectPath, testDevice, testResultsPath, binlogDirectory, configuration, targetFramework, runtimeIdentifier, dotnetToolPath);
	});

Task("uitest-build")
	.Does(() =>
	{
		BuildUITestApp(testAppProjectPath, binlogDirectory, configuration, targetFramework, runtimeIdentifier, dotnetToolPath);
	});

RunTarget(TARGET);


void ExecuteBuild(string project, string binDir, string config, string rid, string tfm, string toolPath)
{
	var projectName = System.IO.Path.GetFileNameWithoutExtension(project);
	var binlog = $"{binDir}/{projectName}-{config}-catalyst.binlog";
	DotNetBuild(project, new DotNetBuildSettings
	{
		Configuration = config,
		Framework = tfm,
		MSBuildSettings = new DotNetMSBuildSettings
		{
			MaxCpuCount = 0
		},
		ToolPath = toolPath,
		ArgumentCustomization = args => args
			.Append("/p:BuildIpa=true")
			.Append($"/p:RuntimeIdentifier={rid}")
			.Append($"/bl:{binlog}")
	});
}

void ExecuteTests(string project, string device, string resultsDir, string config, string tfm, string rid, string toolPath)
{
	CleanResults(resultsDir);

	var testApp = GetTestApplications(project, device, config, tfm, rid).FirstOrDefault();

	Information($"Testing App: {testApp}");
	var settings = new DotNetToolSettings
	{
		DiagnosticOutput = true,
		ToolPath = toolPath,
		ArgumentCustomization = args => args.Append($"run xharness apple test --app=\"{testApp}\" --targets=\"{device}\" --output-directory=\"{resultsDir}\" --verbosity=\"Debug\" ")
	};

	bool testsFailed = true;
	try
	{
		DotNetTool("tool", settings);
		testsFailed = false;
	}
	finally
	{
		HandleTestResults(resultsDir, testsFailed);
	}

	Information("Testing completed.");
}

void ExecuteUITests(string project, string app, string device, string resultsDir, string binDir, string config, string tfm, string rid, string toolPath)
{
	// Setup environment for UI tests
	Information("Starting UI Tests...");
	var testApp = GetTestApplications(app, device, config, tfm, rid).FirstOrDefault();

	Information($"Testing Device: {device}");
	Information($"Testing App Project: {app}");
	Information($"Testing App: {testApp}");
	Information($"Results Directory: {resultsDir}");

	if (string.IsNullOrEmpty(testApp))
	{
		throw new Exception("UI Test application path not specified.");
	}

	Information("Build UITests project {0}", project);

	var name = System.IO.Path.GetFileNameWithoutExtension(project);
	var binlog = $"{binDir}/{name}-{config}-mac.binlog";
	var appiumLog = $"{binDir}/appium_mac.log";
	var resultsFileName = $"{name}-{config}-catalyst";

	DotNetBuild(project, new DotNetBuildSettings
	{
		Configuration = config,
		ToolPath = toolPath,
		ArgumentCustomization = args => args
			.Append("/p:ExtraDefineConstants=MACUITEST")
			.Append("/bl:" + binlog)
	});

	SetEnvironmentVariable("APPIUM_LOG_FILE", appiumLog);

	Information("Run UITests project {0}", project);
	RunTestWithLocalDotNet(project, config, pathDotnet: toolPath, noBuild: true, resultsFileNameWithoutExtension: resultsFileName);
	Information("UI Tests completed.");
}

void BuildUITestApp(string appProject, string binDir, string config, string tfm, string rid, string toolPath)
{
	Information($"Building UI Test app: {appProject}");
	var projectName = System.IO.Path.GetFileNameWithoutExtension(appProject);
	var binlog = $"{binDir}/{projectName}-{config}-catalyst.binlog";

	DotNetBuild(appProject, new DotNetBuildSettings
	{
		Configuration = config,
		Framework = tfm,
		ToolPath = toolPath,
		ArgumentCustomization = args => args
			.Append("/t:Restore;Build")
			.Append($"/bl:{binlog}")
	});

	Information("UI Test app build completed.");
}

IEnumerable<string> GetTestApplications(string project, string device, string config, string tfm, string rid)
{ // Define common directory segments
    const string binDirBase = "bin";
    const string artifactsDir = "../../artifacts/bin/";
    
    // Map project types to specific subdirectories under artifacts
    var projectMappings = new Dictionary<string, string>
    {
        ["Controls.DeviceTests"] = "Controls.DeviceTests",
        ["Core.DeviceTests"] = "Core.DeviceTests",
        ["Graphics.DeviceTests"] = "Graphics.DeviceTests",
        ["MauiBlazorWebView.DeviceTests"] = "MauiBlazorWebView.DeviceTests",
        ["Essentials.DeviceTests"] = "Essentials.DeviceTests",
        ["Controls.Sample.UITests"] = "Controls.Sample.UITests"
    };

    // First try to find apps in the normal bin directory
    var binDir = new DirectoryPath(project).Combine($"{binDirBase}/{config}/{tfm}/{rid}");
    var apps = FindAppsInDirectory(binDir);
    
    // If no apps found, check in specific artifact directories
    if (!apps.Any())
    {
        foreach (var entry in projectMappings)
        {
            if (project.Contains(entry.Key))
            {
                binDir = MakeAbsolute(new DirectoryPath($"{artifactsDir}{entry.Value}/{config}/{tfm}/{rid}/"));
                apps = FindAppsInDirectory(binDir);
                if (apps.Any()) break;
            }
        }

        if (!apps.Any())
        {
            throw new Exception($"No app was found in the arcade {binDir} directory.");
        }
    }

    return apps.Select(a => a.FullPath);
}

// Helper method to encapsulate the directory search logic
IEnumerable<DirectoryPath> FindAppsInDirectory(DirectoryPath directory)
{
    Information($"Looking for .app in {directory}");
    return GetDirectories($"{directory}/*.app");
}

// Helper methods
void PerformCleanupIfNeeded(bool cleanupEnabled)
{
	if (cleanupEnabled)
	{
		// Add cleanup logic, possibly deleting temporary files, directories, etc.
		Information("Cleaning up...");
	}
}

string GetDefaultRuntimeIdentifier()
{
	var architecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;
	return architecture == System.Runtime.InteropServices.Architecture.Arm64 ? "maccatalyst-arm64" : "maccatalyst-x64";
}