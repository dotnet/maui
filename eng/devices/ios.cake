#addin nuget:?package=Cake.AppleSimulator&version=0.2.0
#load "../cake/helpers.cake"
#load "../cake/dotnet.cake"
#load "./devices-shared.cake"

const string DefaultVersion = "17.2";

// Required arguments
var projectPath = Argument("project", EnvironmentVariable("IOS_TEST_PROJECT") ?? DEFAULT_PROJECT);
var testDevice = Argument("device", EnvironmentVariable("IOS_TEST_DEVICE") ?? $"ios-simulator-64_{DefaultVersion}");
var targetFramework = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{DotnetVersion}-ios");
var binlogArg = Argument("binlog", EnvironmentVariable("IOS_TEST_BINLOG") ?? "");
var testApp = Argument("app", EnvironmentVariable("IOS_TEST_APP") ?? "");
var testAppProjectPath = Argument("appproject", EnvironmentVariable("IOS_TEST_APP_PROJECT") ?? DEFAULT_APP_PROJECT);
var testResultsPath = Argument("results", EnvironmentVariable("IOS_TEST_RESULTS") ?? GetTestResultsDirectory().FullPath);
var platform = testDevice.ToLower().Contains("simulator") ? "iPhoneSimulator" : "iPhone";
var runtimeIdentifier = Argument("rid", EnvironmentVariable("IOS_RUNTIME_IDENTIFIER") ?? GetDefaultRuntimeIdentifier(testDevice));
var deviceCleanupEnabled = Argument("cleanup", true);

// Test where clause
string testWhere = Argument("where", EnvironmentVariable("NUNIT_TEST_WHERE") ?? "");

// Device details
var udid = Argument("udid", EnvironmentVariable("IOS_SIMULATOR_UDID") ?? "");
var iosVersion = Argument("apiversion", EnvironmentVariable("IOS_PLATFORM_VERSION") ?? DefaultVersion);

// Directory setup
var binlogDirectory = DetermineBinlogDirectory(projectPath, binlogArg).FullPath;

string DEVICE_UDID = "";
string DEVICE_VERSION = "";
string DEVICE_NAME = "";
string DEVICE_OS = "";

Information($"Project File: {projectPath}");
Information($"Build Binary Log (binlog): {binlogDirectory}");
Information($"Build Platform: {platform}");
Information($"Build Configuration: {configuration}");
Information($"Build Runtime Identifier: {runtimeIdentifier}");
Information($"Build Target Framework: {targetFramework}");
Information($"Test Device: {testDevice}");
Information($"Test Results Path: {testResultsPath}");

var dotnetToolPath = GetDotnetToolPath();

Setup(context =>
{
	LogSetupInfo(dotnetToolPath);
	PerformCleanupIfNeeded(deviceCleanupEnabled);

	// Device or simulator setup
	if (testDevice.Contains("device"))
	{
		GetDevices(iosVersion, dotnetToolPath);
	}
	else if (testDevice.IndexOf("_") != -1)
	{
		GetSimulators(testDevice, dotnetToolPath);
		ResetSimulators(testDevice, dotnetToolPath);
	}
});

Teardown(context => PerformCleanupIfNeeded(deviceCleanupEnabled));

Task("Cleanup");

Task("Build")
	.WithCriteria(!string.IsNullOrEmpty(projectPath))
	.Does(() =>
	{
		ExecuteBuild(projectPath, testDevice, binlogDirectory, configuration, runtimeIdentifier, targetFramework, dotnetToolPath);
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
	{
		ExecuteTests(projectPath, testDevice, testResultsPath, configuration, targetFramework, runtimeIdentifier, dotnetToolPath);
	});

Task("uitest-build")
	.Does(() =>
	{
		BuildUITestApp(testAppProjectPath, testDevice, binlogDirectory, configuration, targetFramework, runtimeIdentifier, dotnetToolPath);

	});

Task("uitest")
	.IsDependentOn("uitest-build")
	.Does(() =>
	{
		ExecuteUITests(projectPath, testAppProjectPath, testDevice, testResultsPath, binlogDirectory, configuration, targetFramework, runtimeIdentifier, iosVersion, dotnetToolPath);

	});

RunTarget(TARGET);

void ExecuteBuild(string project, string device, string binDir, string config, string rid, string tfm, string toolPath)
{
	var projectName = System.IO.Path.GetFileNameWithoutExtension(project);
	var binlog = $"{binDir}/{projectName}-{config}-catalyst.binlog";

	DotNetBuild(project, new DotNetBuildSettings
	{
		ToolPath = toolPath,
		Configuration = config,
		Framework = tfm,
		MSBuildSettings = new DotNetMSBuildSettings
		{
			MaxCpuCount = 0
		},
		ArgumentCustomization = args =>
		{
			args
				.Append("/p:BuildIpa=true")
				.Append("/bl:" + binlog)
				.Append("/tl");

			if (device.ToLower().Contains("device"))
			{
				args.Append("/p:RuntimeIdentifier=ios-arm64");
			}
			return args;
		}
	});
}

void ExecuteTests(string project, string device, string resultsDir, string config, string tfm, string rid, string toolPath)
{
	CleanResults(resultsDir);

	var testApp = GetTestApplications(project, device, config, tfm, rid).FirstOrDefault();

	var XCODE_PATH = Argument("xcode_path", "");

	string xcode_args = "";
	if (!String.IsNullOrEmpty(XCODE_PATH))
	{
		Information($"Setting XCODE_PATH: {XCODE_PATH}");
		xcode_args = $"--xcode=\"{XCODE_PATH}\" ";
	}

	Information($"Testing App: {testApp}");

	var settings = new DotNetToolSettings
	{
		ToolPath = toolPath,
		DiagnosticOutput = true,
		ArgumentCustomization = args =>
		{
			args.Append("run xharness apple test " +
				$"--app=\"{testApp}\" " +
				$"--targets=\"{device}\" " +
				$"--output-directory=\"{resultsDir}\" " +
				$"--timeout=01:15:00 " +
				$"--launch-timeout=00:06:00 " +
				xcode_args +
				$"--verbosity=\"Debug\" ");

			if (device.Contains("device"))
			{
				if (string.IsNullOrEmpty(DEVICE_UDID))
				{
					throw new Exception("No device was found to install the app on. See the Setup method for more details.");
				}
				args.Append($"--device=\"{DEVICE_UDID}\" ");
			}
			return args;
		}
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

void ExecuteUITests(string project, string app, string device, string resultsDir, string binDir, string config, string tfm, string rid, string ver, string toolPath)
{
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

	InstallIpa(testApp, "", device, resultsDir, ver, toolPath);

	Information("Build UITests project {0}", project);

	var name = System.IO.Path.GetFileNameWithoutExtension(project);
	var binlog = $"{binDir}/{name}-{config}-ios.binlog";
	var appiumLog = $"{binDir}/appium_ios.log";
	var resultsFileName = $"{name}-{config}-ios";

	DotNetBuild(project, new DotNetBuildSettings
	{
		Configuration = config,
		ToolPath = toolPath,
		ArgumentCustomization = args => args
			.Append("/p:ExtraDefineConstants=IOSUITEST")
			.Append("/bl:" + binlog)
	});

	SetEnvironmentVariable("APPIUM_LOG_FILE", appiumLog);

	Information("Run UITests project {0}", project);
	RunTestWithLocalDotNet(project, config, pathDotnet: toolPath, noBuild: true, resultsFileNameWithoutExtension: resultsFileName);
	Information("UI Tests completed.");
}

void BuildUITestApp(string appProject, string device, string binDir, string config, string tfm, string rid, string toolPath)
{
	Information($"Building UI Test app: {appProject}");
	var projectName = System.IO.Path.GetFileNameWithoutExtension(appProject);
	var binlog = $"{binDir}/{projectName}-{config}-catalyst.binlog";

	DotNetBuild(appProject, new DotNetBuildSettings
	{
		Configuration = config,
		Framework = tfm,
		ToolPath = toolPath,
		ArgumentCustomization = args =>
		{
			args
			.Append("/p:BuildIpa=true")
			.Append("/bl:" + binlog)
			.Append("/tl");

			// if we building for a device
			if (device.ToLower().Contains("device"))
			{
				args.Append("/p:RuntimeIdentifier=ios-arm64");
			}

			return args;
		}
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
		Information("Deleting XHarness simulator if exists...");
		var sims = ListAppleSimulators().Where(s => s.Name.Contains("XHarness")).ToArray();
		foreach (var sim in sims)
		{
			Information($"Deleting XHarness simulator {sim.Name} ({sim.UDID})...");
			StartProcess("xcrun", $"simctl shutdown {sim.UDID}");
			ExecuteWithRetries(() => StartProcess("xcrun", $"simctl delete {sim.UDID}"), 3);
		}

	}
}

string GetDefaultRuntimeIdentifier(string testDeviceIdentifier)
{
	return testDeviceIdentifier.ToLower().Contains("simulator") ?
	 $"iossimulator-{System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString().ToLower()}"
   : $"ios-arm64";
}



// #addin nuget:?package=Cake.AppleSimulator&version=0.2.0
// #load "../cake/helpers.cake"
// #load "../cake/dotnet.cake"
// #load "./devices-shared.cake"

// const string defaultVersion = "16.4";
// const string dotnetVersion = "net8.0";
// // required
// FilePath PROJECT = Argument("project", EnvironmentVariable("IOS_TEST_PROJECT") ?? DEFAULT_PROJECT);
// string TEST_DEVICE = Argument("device", EnvironmentVariable("IOS_TEST_DEVICE") ?? $"ios-simulator-64_{defaultVersion}"); // comma separated in the form <platform>-<device|simulator>[-<32|64>][_<version>] (eg: ios-simulator-64_13.4,[...])

// // optional
// var DOTNET_ROOT = Argument("dotnet-root", EnvironmentVariable("DOTNET_ROOT"));
// var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
// var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{dotnetVersion}-ios");
// var BINLOG_ARG = Argument("binlog", EnvironmentVariable("IOS_TEST_BINLOG") ?? "");
// DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
// var TEST_APP = Argument("app", EnvironmentVariable("IOS_TEST_APP") ?? "");
// FilePath TEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("IOS_TEST_APP_PROJECT") ?? DEFAULT_APP_PROJECT);
// var TEST_RESULTS = Argument("results", EnvironmentVariable("IOS_TEST_RESULTS") ?? "");

// string TEST_WHERE = Argument("where", EnvironmentVariable("NUNIT_TEST_WHERE") ?? $"");

// //these are for appium iOS UITests
// var udid = Argument("udid", EnvironmentVariable("IOS_SIMULATOR_UDID") ?? "");
// var iosVersion = Argument("apiversion", EnvironmentVariable("IOS_PLATFORM_VERSION") ?? defaultVersion);

// string DEVICE_UDID = "";
// string DEVICE_VERSION = "";
// string DEVICE_NAME = "";
// string DEVICE_OS = "";

// // other
// string PLATFORM = TEST_DEVICE.ToLower().Contains("simulator") ? "iPhoneSimulator" : "iPhone";
// string RUNTIME_IDENTIFIER = TEST_DEVICE.ToLower().Contains("simulator") ? 
// 	$"iossimulator-{System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString().ToLower()}"
//   : $"ios-arm64";
// string CONFIGURATION = Argument("configuration", "Debug");
// bool DEVICE_CLEANUP = Argument("cleanup", !IsCIBuild());
// string TEST_FRAMEWORK = "net472";

// Information("Project File: {0}", PROJECT);
// Information("Build Binary Log (binlog): {0}", BINLOG_DIR);
// Information("Build Platform: {0}", PLATFORM);
// Information("Build Configuration: {0}", CONFIGURATION);

// string DOTNET_TOOL_PATH = "/usr/local/share/dotnet/dotnet";

// var localDotnetiOS = GetBuildVariable("workloads", "local") == "local";
// if (localDotnetiOS)
// {
// 	Information("Using local dotnet");
// 	DOTNET_TOOL_PATH = $"{MakeAbsolute(Directory("../../bin/dotnet/")).ToString()}/dotnet";
// }
// else
// {
// 	Information("Using system dotnet");
// }

// Setup(context =>
// {
// 	Cleanup();

// 	Information($"DOTNET_TOOL_PATH {DOTNET_TOOL_PATH}");

// 	Information("Host OS System Arch: {0}", System.Runtime.InteropServices.RuntimeInformation.OSArchitecture);
// 	Information("Host Processor System Arch: {0}", System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture);

// 	// only grab attached iOS devices if we are trying to test on device
// 	if (TEST_DEVICE.Contains("device")) 
// 	{ 
// 		GetDevices(iosVersion);
// 	}
// 	// only install simulator when an explicit version is specified
// 	if (TEST_DEVICE.IndexOf("_") != -1) 
// 	{
// 		GetSimulators(TEST_DEVICE);
// 		ResetSimulators(TEST_DEVICE);
// 	}
// });

// Teardown(context =>
// {
// 	Cleanup();
// });

// void Cleanup()
// {
// 	if (!DEVICE_CLEANUP)
// 		return;

// 	// delete the XHarness simulators first, if it exists
// 	Information("Deleting XHarness simulator if exists...");
// 	var sims = ListAppleSimulators();
// 	if(sims.Count == 0)
// 	{
// 		Information("No simulators found to delete.");
// 		return;
// 	}
// 	var simulatorName = "XHarness";
// 	Information("Looking for simulator: {0} ios version {1}", simulatorName, iosVersion);
// 	var xharness = sims.Where(s => s.Name.Contains(simulatorName))?.ToArray();
// 	if(xharness == null || xharness.Length == 0)
// 	{
// 		Information("No simulators with {0} found to delete.", simulatorName);
// 		return;
// 	}
// 	foreach (var sim in xharness) {
// 		Information("Deleting XHarness simulator {0} ({1})...", sim.Name, sim.UDID);
// 		StartProcess("xcrun", "simctl shutdown " + sim.UDID);
// 		var retries = 3;
// 		while (retries > 0) {
// 			var exitCode = StartProcess("xcrun", "simctl delete " + sim.UDID);
// 			if (exitCode == 0) {
// 				retries = 0;
// 			} else {
// 				retries--;
// 				System.Threading.Thread.Sleep(1000);
// 			}
// 		}
// 	}
// }

// Task("Cleanup");

// Task("Build")
// 	.WithCriteria(!string.IsNullOrEmpty(PROJECT.FullPath))
// 	.Does(() =>
// {
// 	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
// 	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-ios.binlog";

// 	DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
// 			ToolPath = DOTNET_TOOL_PATH,
// 			Configuration = CONFIGURATION,
// 			Framework = TARGET_FRAMEWORK,
// 			MSBuildSettings = new DotNetMSBuildSettings {
// 				MaxCpuCount = 0
// 			},	
// 			ArgumentCustomization = args =>
// 			{ 	
// 				args
// 				.Append("/p:BuildIpa=true")
// 				.Append("/bl:" + binlog)
// 				.Append("/tl");

// 				// if we building for a device
// 				if(TEST_DEVICE.ToLower().Contains("device"))
// 				{
// 					args.Append("/p:RuntimeIdentifier=ios-arm64");
// 				}
// 				return args;
// 			}
// 		});
// });

// Task("uitest-build")
// 	.Does(() =>
// {
// 	var name = System.IO.Path.GetFileNameWithoutExtension(DEFAULT_APP_PROJECT);
// 	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-ios.binlog";

// 	Information("app" + DEFAULT_APP_PROJECT);
// 	DotNetBuild(DEFAULT_APP_PROJECT, new DotNetBuildSettings {
// 		Configuration = CONFIGURATION,
// 		Framework = TARGET_FRAMEWORK,
// 		ToolPath = DOTNET_PATH,
// 		ArgumentCustomization = args =>
// 		{ 	
// 			args
// 			.Append("/p:BuildIpa=true")
// 			.Append("/bl:" + binlog)
// 			.Append("/tl");

// 			// if we building for a device
// 			if(TEST_DEVICE.ToLower().Contains("device"))
// 			{
// 				args.Append("/p:RuntimeIdentifier=ios-arm64");
// 			}

// 			return args;
// 		}
// 	});
// });

// Task("Test")
// 	.IsDependentOn("Build")
// 	.Does(() =>
// {
// 	if (string.IsNullOrEmpty(TEST_APP)) {
// 		if (string.IsNullOrEmpty(PROJECT.FullPath))
// 			throw new Exception("If no app was specified, an app must be provided.");
// 		var binDir = PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TARGET_FRAMEWORK).Combine(RUNTIME_IDENTIFIER);
// 		var apps = GetDirectories(binDir + "/*.app");
// 		if (apps.Count() == 0)
// 		{
// 			var arcadeBin = new DirectoryPath("../../artifacts/bin/");
// 			if(PROJECT.FullPath.Contains("Controls.DeviceTests"))
// 			{
// 				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Controls.DeviceTests/" + CONFIGURATION + "/" + TARGET_FRAMEWORK + "/" + RUNTIME_IDENTIFIER + "/"));
// 			}
// 			if(PROJECT.FullPath.Contains("Core.DeviceTests"))
// 			{
// 				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Core.DeviceTests/" + CONFIGURATION + "/" + TARGET_FRAMEWORK + "/" + RUNTIME_IDENTIFIER + "/"));
// 			}
// 			if(PROJECT.FullPath.Contains("Graphics.DeviceTests"))
// 			{
// 				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Graphics.DeviceTests/" + CONFIGURATION + "/" + TARGET_FRAMEWORK + "/" + RUNTIME_IDENTIFIER + "/"));
// 			}
// 			if(PROJECT.FullPath.Contains("MauiBlazorWebView.DeviceTests"))
// 			{
// 				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/MauiBlazorWebView.DeviceTests/" + CONFIGURATION + "/" + TARGET_FRAMEWORK + "/" + RUNTIME_IDENTIFIER + "/"));
// 			}
// 			if(PROJECT.FullPath.Contains("Essentials.DeviceTests"))
// 			{
// 				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Essentials.DeviceTests/" + CONFIGURATION + "/" + TARGET_FRAMEWORK + "/" + RUNTIME_IDENTIFIER + "/"));
// 			}
// 			Information("Looking for .app in arcade binDir {0}", binDir);
// 			apps = GetDirectories(binDir + "/*.app");
// 			if(apps.Count == 0)
// 			{
// 				throw new Exception("No app was found in the arcade bin directory.");
// 			}
// 		}
// 		TEST_APP = apps.First().FullPath;
// 	}

// 	if (string.IsNullOrEmpty(TEST_RESULTS)) {
// 		TEST_RESULTS = TEST_APP + "-results";
// 	}

// 	Information("Test Device: {0}", TEST_DEVICE);
// 	Information("Test App: {0}", TEST_APP);
// 	Information("Test Results Directory: {0}", TEST_RESULTS);

// 	if (!IsCIBuild())
// 		CleanDirectories(TEST_RESULTS);
// 	else
// 	{
// 		// Because we retry on CI we don't want to delete the previous failures
// 		// We want to publish those files for reference
// 		DeleteFiles(Directory(TEST_RESULTS).Path.Combine("*.*").FullPath);
// 	}

// 	var XCODE_PATH =  Argument("xcode_path", "");

// 	string xcode_args = "";
// 	if (!String.IsNullOrEmpty(XCODE_PATH))
// 	{
// 		xcode_args = $"--xcode=\"{XCODE_PATH}\" ";
// 	}

// 	Information("XCODE PATH: {0}", XCODE_PATH);

// 	var settings = new DotNetToolSettings {
// 		ToolPath = DOTNET_TOOL_PATH,
// 		DiagnosticOutput = true,
// 		ArgumentCustomization = args => 
// 		{
// 			args.Append("run xharness apple test " +
// 				$"--app=\"{TEST_APP}\" " +
// 				$"--targets=\"{TEST_DEVICE}\" " +
// 				$"--output-directory=\"{TEST_RESULTS}\" " +
// 				$"--timeout=01:15:00 " +
// 				$"--launch-timeout=00:06:00 " +
// 				xcode_args +
// 				$"--verbosity=\"Debug\" ");

// 			if (TEST_DEVICE.Contains("device"))
// 			{
// 				if(string.IsNullOrEmpty(DEVICE_UDID))
// 				{
// 					throw new Exception("No device was found to install the app on. See the Setup method for more details.");
// 				}
// 				args.Append($"--device=\"{DEVICE_UDID}\" ");
// 			}
// 			return args;	
// 		}
// 	};

// 	bool testsFailed = true;
// 	try {
// 		DotNetTool("tool", settings);
// 		testsFailed = false;
// 	} finally {
// 		// ios test result files are weirdly named, so fix it up
// 		var resultsFile = GetFiles($"{TEST_RESULTS}/xunit-test-*.xml").FirstOrDefault();
// 		if (FileExists(resultsFile)) {
// 			CopyFile(resultsFile, resultsFile.GetDirectory().CombineWithFilePath("TestResults.xml"));
// 		}

// 		if (testsFailed && IsCIBuild())
// 		{
// 			var failurePath = $"{TEST_RESULTS}/TestResultsFailures/{Guid.NewGuid()}";
// 			EnsureDirectoryExists(failurePath);
// 			// The tasks will retry the tests and overwrite the failed results each retry
// 			// we want to retain the failed results for diagnostic purposes
// 			CopyFiles($"{TEST_RESULTS}/*.*", failurePath);

// 			// We don't want these to upload
// 			MoveFile($"{failurePath}/TestResults.xml", $"{failurePath}/Results.xml");
// 		}
// 	}

// 	// this _may_ not be needed, but just in case
// 	var failed = XmlPeek($"{TEST_RESULTS}/TestResults.xml", "/assemblies/assembly[@failed > 0 or @errors > 0]/@failed");
// 	if (!string.IsNullOrEmpty(failed)) {
// 		throw new Exception($"At least {failed} test(s) failed.");
// 	}
// });

// Task("uitest")
// 	.IsDependentOn("uitest-build")
// 	.Does(() =>
// {
// 	SetupAppPackageNameAndResult();

// 	CleanDirectories(TEST_RESULTS);

// 	InstallIpa(TEST_APP, "", TEST_DEVICE, TEST_RESULTS, iosVersion);

// 	//we need to build tests first to pass ExtraDefineConstants
// 	Information("Build UITests project {0}", PROJECT.FullPath);
// 	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
// 	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-ios.binlog";
// 	DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
// 			ToolPath = DOTNET_TOOL_PATH,
// 			Configuration = CONFIGURATION,
// 			ArgumentCustomization = args => args
// 				.Append("/p:ExtraDefineConstants=IOSUITEST")
// 				.Append("/bl:" + binlog)
// 				.Append("/tl")

// 	});

// 	SetEnvironmentVariable("APPIUM_LOG_FILE", $"{BINLOG_DIR}/appium_ios.log");

// 	Information("Run UITests project {0}",PROJECT.FullPath);
// 	RunTestWithLocalDotNet(PROJECT.FullPath, CONFIGURATION, pathDotnet: DOTNET_TOOL_PATH, noBuild: true, resultsFileNameWithoutExtension: $"{name}-{CONFIGURATION}-ios");
// });

Task("cg-uitest")
	.Does(() =>
{
	//	SetupAppPackageNameAndResult();

	CleanDirectories(testResultsPath);

	Information("Starting UI Tests...");
	var testApp = GetTestApplications(testAppProjectPath, testDevice, configuration, targetFramework, runtimeIdentifier).FirstOrDefault();

	Information($"Testing Device: {testDevice}");
	Information($"Testing App Project: {testAppProjectPath}");
	Information($"Testing App: {testApp}");
	Information($"Results Directory: {testResultsPath}");

	InstallIpa(testApp, "com.microsoft.mauicompatibilitygallery", testDevice, $"{testResultsPath}/ios", iosVersion, dotnetToolPath);

	// For non-CI builds we assume that this is configured correctly on your machine
	if (IsCIBuild())
	{
		// Install IDB (and prerequisites)
		StartProcess("brew", "tap facebook/fb");
		StartProcess("brew", "install idb-companion");
		StartProcess("pip3", "install --user fb-idb");

		// Create a temporary script file to hold the inline Bash script
		var makeSymLinkScript = "./temp_script.sh";
		// Below is an attempt to somewhat dynamically determine the path to idb and make a symlink to /usr/local/bin this is needed in order for Xamarin.UITest to find it
		System.IO.File.AppendAllLines(makeSymLinkScript, new[] { "sudo ln -s $(find /Users/$(whoami)/Library/Python/?.*/bin -name idb | head -1) /usr/local/bin" });

		StartProcess("bash", makeSymLinkScript);
		System.IO.File.Delete(makeSymLinkScript);
	}

	//set env var for the app path for Xamarin.UITest setup
	SetEnvironmentVariable("iOS_APP", $"{testApp}");

	var resultName = $"{System.IO.Path.GetFileNameWithoutExtension(projectPath)}-{configuration}-{DateTime.UtcNow.ToFileTimeUtc()}";
	Information("Run UITests project {0}", resultName);
	RunTestWithLocalDotNet(
			projectPath,
			config: configuration,
			pathDotnet: dotnetToolPath,
			noBuild: false,
			resultsFileNameWithoutExtension: resultName,
			filter: Argument("filter", ""));
});

RunTarget(TARGET);

// void SetupAppPackageNameAndResult()
// {
//✗ dotnet cake -script eng/devices/catalyst.cake --target=uitest --apiversion="10.13" --device=mac --workloads=global 
//    if (string.IsNullOrEmpty(TEST_APP) ) {
// 		if (string.IsNullOrEmpty(TEST_APP_PROJECT.FullPath))
// 			throw new Exception("If no app was specified, an app must be provided.");
// 		var binDir =  MakeAbsolute(TEST_APP_PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION).Combine(TARGET_FRAMEWORK).Combine(RUNTIME_IDENTIFIER));
// 		Information("BinDir: {0}", binDir);
// 		var apps = GetDirectories(binDir + "/*.app");
// 		if(apps.Count == 0)
// 		{
// 			var arcadeBin = new DirectoryPath("../../artifacts/bin/");
// 			if(TEST_APP_PROJECT.FullPath.Contains("Controls.Sample.UITests"))
// 			{
// 				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Controls.Sample.UITests/" + CONFIGURATION + "/" + TARGET_FRAMEWORK + "/" + RUNTIME_IDENTIFIER + "/"));
// 			}
// 			if(TEST_APP_PROJECT.FullPath.Contains("Compatibility"))
// 			{
// 				//this is for the compatibility gallery
// 				RUNTIME_IDENTIFIER = "iossimulator-x64";
// 				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Compatibility.ControlGallery.iOS/" + CONFIGURATION + "/" + TARGET_FRAMEWORK + "/" + RUNTIME_IDENTIFIER + "/"));
// 			}
// 			Information("Looking for .app in arcade binDir {0}", binDir);
// 			apps = GetDirectories(binDir + "/*.app");
// 			if(apps.Count == 0)
// 			{
// 				throw new Exception("No app was found in the arcade bin directory.");
// 			}
// 		}

// 		TEST_APP = apps.First().FullPath;
// 	}
// 	if (string.IsNullOrEmpty(TEST_RESULTS)) {
// 		TEST_RESULTS =  GetTestResultsDirectory().FullPath;
// 	}
// }

void InstallIpa(string testApp, string testAppPackageName, string testDevice, string testResultsDirectory, string version, string toolPath)
{
	Information("Install with xharness: {0}", testApp);
	var settings = new DotNetToolSettings
	{
		ToolPath = toolPath,
		DiagnosticOutput = true,
		ArgumentCustomization = args =>
		{
			args.Append("run xharness apple install " +
							$"--app=\"{testApp}\" " +
							$"--targets=\"{testDevice}\" " +
							$"--output-directory=\"{testResultsDirectory}\" " +
							$"--verbosity=\"Debug\" ");
			if (testDevice.Contains("device"))
			{
				if (string.IsNullOrEmpty(DEVICE_UDID))
				{
					throw new Exception("No device was found to install the app on. See the Setup method for more details.");
				}
				args.Append($"--device=\"{DEVICE_UDID}\" ");
			}
			return args;
		}
	};

	try
	{
		DotNetTool("tool", settings);
	}
	finally
	{
		string iosVersionToRun = version;
		string deviceToRun = "";

		if (testDevice.Contains("device"))
		{
			if (!string.IsNullOrEmpty(DEVICE_UDID))
			{
				deviceToRun = DEVICE_UDID;
			}
			else
			{
				throw new Exception("No device was found to run tests on.");
			}

			iosVersionToRun = DEVICE_VERSION;

			Information("The device to run tests: {0} {1}", DEVICE_NAME, iosVersionToRun);
		}
		else
		{
			var simulatorName = "XHarness";
			Information("Looking for simulator: {0} iosversion {1}", simulatorName, iosVersionToRun);
			var sims = ListAppleSimulators();
			var simXH = sims.Where(s => s.Name.Contains(simulatorName) && s.Name.Contains(iosVersionToRun)).FirstOrDefault();
			if (simXH == null)
				throw new Exception("No simulator was found to run tests on.");

			deviceToRun = simXH.UDID;
			DEVICE_NAME = simXH.Name;
			Information("The emulator to run tests: {0} {1}", simXH.Name, simXH.UDID);
		}

		Information("The platform version to run tests: {0}", iosVersionToRun);
		SetEnvironmentVariable("DEVICE_UDID", deviceToRun);
		SetEnvironmentVariable("DEVICE_NAME", DEVICE_NAME);
		SetEnvironmentVariable("PLATFORM_VERSION", iosVersionToRun);
	}
}

void GetSimulators(string version, string tool)
{
	Information("Getting simulators for version {0}", version);
	DotNetTool("tool", new DotNetToolSettings
	{
		ToolPath = tool,
		DiagnosticOutput = true,
		ArgumentCustomization = args => args.Append("run xharness apple simulators install " +
			$"\"{version}\" " +
			$"--verbosity=\"Debug\" ")
	});
}

void ResetSimulators(string version, string tool)
{
	Information("Getting simulators for version {0}", version);
	var logDirectory = GetLogDirectory();
	DotNetTool("tool", new DotNetToolSettings
	{
		ToolPath = tool,
		DiagnosticOutput = true,
		ArgumentCustomization = args => args.Append("run xharness apple simulators reset-simulator " +
			$"--output-directory=\"{logDirectory}\" " +
			$"--target=\"{version}\" " +
			$"--verbosity=\"Debug\" ")
	});
}

void GetDevices(string version, string tool)
{
	var deviceUdid = "";
	var deviceName = "";
	var deviceVersion = "";
	var deviceOS = "";

	var list = new List<string>();
	bool isDevice = false;
	// print the apple state of the machine
	// this will print the connected devices
	DotNetTool("tool", new DotNetToolSettings
	{
		ToolPath = tool,
		DiagnosticOutput = true,
		ArgumentCustomization = args => args.Append("run xharness apple state --verbosity=\"Debug\" "),
		SetupProcessSettings = processSettings =>
			{
				processSettings.RedirectStandardOutput = true;
				processSettings.RedirectStandardError = true;
				processSettings.RedirectedStandardOutputHandler = (output) =>
				{
					Information("Apple State: {0}", output);
					if (output == "Connected Devices:")
					{
						isDevice = true;
					}
					else if (isDevice)
					{
						list.Add(output);
					}
					return output;
				};
			}
	});
	Information("List count: {0}", list.Count);
	//this removes the extra lines from output
	if (list.Count == 0)
	{
		throw new Exception($"No devices found");
		return;
	}
	list.Remove(list.Last());
	list.Remove(list.Last());
	foreach (var item in list)
	{
		var stringToTest = $"Device: {item.Trim()}";
		Information(stringToTest);
		var regex = new System.Text.RegularExpressions.Regex(@"Device:\s+((?:[^\s]+(?:\s+[^\s]+)*)?)\s+([0-9A-Fa-f-]+)\s+([\d.]+)\s+(iPhone|iPad)\s+iOS");
		var match = regex.Match(stringToTest);
		if (match.Success)
		{
			deviceName = match.Groups[1].Value;
			deviceUdid = match.Groups[2].Value;
			deviceVersion = match.Groups[3].Value;
			deviceOS = match.Groups[4].Value;
			Information("DeviceName:{0} udid:{1} version:{2} os:{3}", deviceName, deviceUdid, deviceVersion, deviceOS);
			if (version.Contains(deviceVersion.Split(".")[0]))
			{
				Information("We want this device: {0} {1} because it matches {2}", deviceName, deviceVersion, version);
				DEVICE_UDID = deviceUdid;
				DEVICE_VERSION = deviceVersion;
				DEVICE_NAME = deviceName;
				DEVICE_OS = deviceOS;
				break;
			}
		}
		else
		{
			Information("No match found for {0}", stringToTest);
		}
	}
	if (string.IsNullOrEmpty(DEVICE_UDID))
	{
		throw new Exception($"No devices found for version {version}");
	}
}
