#addin nuget:?package=Cake.AppleSimulator&version=0.2.0
#load "./uitests-shared.cake"

const string DefaultVersion = "18.4";
const string DefaultTestDevice = $"ios-simulator-64";

// Required arguments
string DEFAULT_IOS_PROJECT = "../../src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj";
var projectPath = Argument("project", EnvironmentVariable("IOS_TEST_PROJECT") ?? DEFAULT_IOS_PROJECT);
var testDevice = Argument("device", EnvironmentVariable("IOS_TEST_DEVICE") ?? DefaultTestDevice);
var targetFramework = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{DotnetVersion}-ios");
var binlogArg = Argument("binlog", EnvironmentVariable("IOS_TEST_BINLOG") ?? "");
var testApp = Argument("app", EnvironmentVariable("IOS_TEST_APP") ?? "");
var testAppProjectPath = Argument("appproject", EnvironmentVariable("IOS_TEST_APP_PROJECT") ?? DEFAULT_APP_PROJECT);
var testResultsPath = Argument("results", EnvironmentVariable("IOS_TEST_RESULTS") ?? GetTestResultsDirectory()?.FullPath);
var platform = testDevice.ToLower().Contains("simulator") ? "iPhoneSimulator" : "iPhone";
var runtimeIdentifier = Argument("rid", EnvironmentVariable("IOS_RUNTIME_IDENTIFIER") ?? GetDefaultRuntimeIdentifier(testDevice));
var deviceCleanupEnabled = Argument("cleanup", true);
var useCoreClr = Argument("coreclr", false);

// Device details
var udid = Argument("udid", EnvironmentVariable("IOS_SIMULATOR_UDID") ?? "");
var iosVersion = Argument("apiversion", EnvironmentVariable("IOS_PLATFORM_VERSION") ?? DefaultVersion);
var headless = Argument<bool>("headless", EnvironmentVariable<bool>("HEADLESS", false));

// Directory setup
var binlogDirectory = DetermineBinlogDirectory(projectPath, binlogArg)?.FullPath;

string DEVICE_UDID = "";
string DEVICE_VERSION = "";
string DEVICE_NAME = "";
string DEVICE_OS = "";
string ownedSimulatorUdid = "";

Information($"Project File: {projectPath}");
Information($"Build Binary Log (binlog): {binlogDirectory}");
Information($"Build Platform: {platform}");
Information($"Build Configuration: {configuration}");
Information($"Build Runtime Identifier: {runtimeIdentifier}");
Information($"Build Target Framework: {targetFramework}");
Information($"Test Device: {testDevice}");
Information($"Test Results Path: {testResultsPath}");
Information("Use CoreCLR: {0}", useCoreClr);
Information("Runtime Variant: {0}", RUNTIME_VARIANT);

var dotnetToolPath = GetDotnetToolPath();
LogSetupInfo(dotnetToolPath);

Teardown(context => 
{
	if (!deviceBoot || targetBoot || string.Equals(TARGET, "uitest-prepare", StringComparison.OrdinalIgnoreCase))
	{
		return;
	}

	PerformCleanupIfNeeded(deviceCleanupEnabled, true);
});

Task("connectToDevice")
	.Does(() => RunUITestSetupWithRetry(() =>
	{
		if (!deviceBoot)
		{
			return;
		}

		PerformCleanupIfNeeded(deviceCleanupEnabled, false);

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
	}));

Task("Cleanup")
	.Does(() =>
	{
		PerformCleanupIfNeeded(deviceCleanupEnabled, true);
	});

Task("buildOnly")
	.WithCriteria(!string.IsNullOrEmpty(projectPath))
	.Does(() =>
	{
		ExecuteBuild(projectPath, testDevice, binlogDirectory, configuration, runtimeIdentifier, targetFramework, dotnetToolPath, useCoreClr);
	});

Task("testOnly")
	.IsDependentOn("connectToDevice")
	.WithCriteria(!string.IsNullOrEmpty(projectPath))
	.Does(() =>
	{
		ExecuteTests(projectPath, testDevice, testResultsPath, configuration, targetFramework, runtimeIdentifier, dotnetToolPath);
	});

Task("build")
	.IsDependentOn("buildOnly");

Task("test")
	.IsDependentOn("buildOnly")
	.IsDependentOn("testOnly");

Task("buildAndTest")
	.IsDependentOn("buildOnly")
	.IsDependentOn("testOnly");

Task("uitest-build")
	.IsDependentOn("dotnet-buildtasks")
	.Does(() =>
	{
		ExecuteBuildUITestApp(testAppProjectPath, testDevice, binlogDirectory, configuration, targetFramework, runtimeIdentifier, dotnetToolPath);
	});

Task("uitest-prepare")
	.IsDependentOn("connectToDevice")
	.Does(() => RunUITestSetupWithRetry(() =>
	{
		ExecutePrepareUITests(projectPath, testAppProjectPath, testDevice, testResultsPath, binlogDirectory, configuration, targetFramework, runtimeIdentifier, iosVersion, dotnetToolPath, headless);
	}));

Task("uitest")
	.IsDependentOn("uitest-prepare")
	.Does(() =>
	{
		ExecuteUITests(projectPath, testAppProjectPath, testDevice, testResultsPath, binlogDirectory, configuration, targetFramework, runtimeIdentifier, iosVersion, dotnetToolPath);
	});

RunTarget(TARGET);

void ExecuteBuild(string project, string device, string binDir, string config, string rid, string tfm, string toolPath, bool useCoreClr)
{
	var projectName = System.IO.Path.GetFileNameWithoutExtension(project);
	var monoRuntime = useCoreClr ? "coreclr" : "mono";
	var binlog = $"{binDir}/{projectName}-{config}-{monoRuntime}-ios.binlog";

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
				.Append("/p:CodesignRequireProvisioningProfile=false")
				.Append($"/p:RuntimeIdentifier={rid}")
				.Append("/bl:" + binlog)
				.Append("/tl");

			if (useCoreClr)
			{
				args.Append("/p:UseMonoRuntime=false");
			}

			return args;
		}
	});
}

void ExecuteTests(string project, string device, string resultsDir, string config, string tfm, string rid, string toolPath)
{
	CleanResults(resultsDir);

	var selectedDevice = !string.IsNullOrEmpty(DEVICE_UDID) ? DEVICE_UDID :
		!string.IsNullOrEmpty(udid) ? udid : EnvironmentVariable("DEVICE_UDID");
	if (string.IsNullOrEmpty(selectedDevice) && !device.Contains("device"))
		selectedDevice = GetUDID(device, toolPath);
	RunNativeTestWithLocalDotNet(project, config, tfm, rid, "ios", testFilter, selectedDevice,
		resultsDir, toolPath, useCoreClr, Argument("xcode_path", ""));
}

void ExecutePrepareUITests(string project, string app, string device, string resultsDir, string binDir, string config, string tfm, string rid, string ver, string toolPath, bool headless)
{
	Information("Preparing UI Tests...");
	Information($"Testing Device: {device}");
	Information($"Testing App Project: {app}");
	Information($"USE_NATIVE_AOT: {USE_NATIVE_AOT}");
	
	var testApp = GetTestApplications(app, device, config, tfm, rid).FirstOrDefault();

	Information($"Testing App found: {testApp}");
	
	if (string.IsNullOrEmpty(testApp))
	{
		throw new Exception("UI Test application path not specified.");
	}

	InstallIpa(testApp, "", device, resultsDir, ver, toolPath, headless);
}

void ExecuteUITests(string project, string app, string device, string resultsDir, string binDir, string config, string tfm, string rid, string ver, string toolPath)
{
	Information($"Results Directory: {resultsDir}");

	Information("Build UITests project {0}", project);

	var name = System.IO.Path.GetFileNameWithoutExtension(project);
	var binlog = $"{binDir}/{name}-{config}-ios.binlog";
	var resultsFileName = SanitizeTestResultsFilename($"{name}-{config}-ios-{testFilter}");
	var appiumLog = $"{binDir}/appium_ios_{resultsFileName}.log";

	RunUITestSetupWithRetry(() => DotNetBuild(project, new DotNetBuildSettings
	{
		Configuration = config,
		ToolPath = toolPath,
		ArgumentCustomization = args => args
			.Append("/p:ExtraDefineConstants=IOSUITEST")
			.Append($"/p:_UseNativeAot={USE_NATIVE_AOT}")
			.Append("/bl:" + binlog)
	}));

	var TEST_CONFIGURATION_ARGS = Argument("TEST_CONFIGURATION_ARGS", EnvironmentVariable("TEST_CONFIGURATION_ARGS") ?? "");

	Information("TEST_CONFIGURATION_ARGS {0}", TEST_CONFIGURATION_ARGS);
	SetEnvironmentVariable("TEST_CONFIGURATION_ARGS", TEST_CONFIGURATION_ARGS);
	SetEnvironmentVariable("APPIUM_LOG_FILE", appiumLog);

	Information("Run UITests project {0}", project);
	RunUITestsWithRetry(project, config, toolPath, resultsFileName);
	Information("UI Tests completed.");
}

void ExecuteBuildUITestApp(string appProject, string device, string binDir, string config, string tfm, string rid, string toolPath)
{
	Information($"Building UI Test app: {appProject}");
	Information($"USE_NATIVE_AOT: {USE_NATIVE_AOT}");

	var projectName = System.IO.Path.GetFileNameWithoutExtension(appProject);
	var binlog = $"{binDir}/{projectName}-{config}-ios.binlog";

	if (USE_NATIVE_AOT && config.Equals("Debug", StringComparison.OrdinalIgnoreCase))
	{
		var errMsg = $"Error: Running UI tests with NativeAOT is only supported in Release configuration";
		Error(errMsg);
		throw new Exception(errMsg);
	}


	DotNetBuild(appProject, new DotNetBuildSettings
	{
		Configuration = config,
		Framework = tfm,
		ToolPath = toolPath,
		ArgumentCustomization = args =>
		{
			args
			.Append("/p:BuildIpa=true")
			.Append($"/p:_UseNativeAot={USE_NATIVE_AOT}")
			.Append($"/p:RuntimeIdentifier={rid}")
			.Append("/bl:" + binlog)
			.Append("/tl");

			return args;
		}
	});

	Information("UI Test app build completed.");
}

// Helper methods
void PerformCleanupIfNeeded(bool cleanupEnabled, bool createDeviceLogs)
{
	try
	{
		if (cleanupEnabled)
		{
			var logDirectory = GetLogDirectory();
			Information("Cleaning up...");
			Information("Deleting XHarness simulator if exists...");
			var sims = ListAppleSimulators().Where(s => s.Name.Contains("XHarness") || s.UDID == ownedSimulatorUdid).ToArray();
			foreach (var sim in sims)
			{
				var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
				if(createDeviceLogs)
				{
					try
					{
						var homeDirectory = Environment.GetEnvironmentVariable("HOME");
						Information("Diagnostics Reports");
						StartProcess("zip", new ProcessSettings {
							Arguments = new ProcessArgumentBuilder()
								.Append("-9r")
								.AppendQuoted($"{logDirectory}/DiagnosticReports_{sim.UDID}_{timestamp}.zip")
								.AppendQuoted($"{homeDirectory}/Library/Logs/DiagnosticReports/"),
							RedirectStandardOutput = false
						});

						Information("CoreSimulator");
						StartProcess("zip", new ProcessSettings {
							Arguments = new ProcessArgumentBuilder()
								.Append("-9r")
								.AppendQuoted($"{logDirectory}/CoreSimulator_{sim.UDID}_{timestamp}.zip")
								.AppendQuoted($"{homeDirectory}/Library/Logs/CoreSimulator/{sim.UDID}"),
							RedirectStandardOutput = false
						});

						StartProcess("xcrun", $"simctl spawn {sim.UDID} log collect --output {homeDirectory}/{sim.UDID}_{timestamp}_log.logarchive");

						StartProcess("zip", new ProcessSettings {
							Arguments = new ProcessArgumentBuilder()
								.Append("-9r")
								.AppendQuoted($"{logDirectory}/{sim.UDID}_{timestamp}_log.logarchive.zip")
								.AppendQuoted($"{homeDirectory}/{sim.UDID}_{timestamp}_log.logarchive"),
							RedirectStandardOutput = false
						});

						var screenshotPath = $"{testResultsPath}/{sim.UDID}_{timestamp}_screenshot.png";
						StartProcess("xcrun", $"simctl io {sim.UDID} screenshot {screenshotPath}");
					}
					catch(Exception ex)
					{
						Information($"Failed to collect logs for simulator {sim.Name} ({sim.UDID}): {ex.Message}");
						Information($"Command Executed: simctl spawn {sim.UDID} log collect --output {logDirectory}/{sim.UDID}_{timestamp}_log.logarchive");
					}
				}

				Information($"Deleting XHarness simulator {sim.Name} ({sim.UDID})...");
				DeleteSimulator(sim.UDID);
			}
		}
	}
	catch (Exception ex)
	{
		if (IsCIBuild())
		{
			Information($"Error during cleanup: {ex}");
		}
		else
		{
			throw;
		}
	}
}

void DeleteSimulator(string simulator)
{
	StartProcess("xcrun", $"simctl shutdown {simulator}");
	var deleted = false;
	ExecuteWithRetries(() =>
	{
		var exitCode = StartProcess("xcrun", $"simctl delete {simulator}");
		deleted = exitCode == 0;
		return exitCode;
	}, 3);
	if (!deleted)
		throw new InvalidOperationException($"Failed to delete simulator '{simulator}'.");
	if (simulator == ownedSimulatorUdid)
		ownedSimulatorUdid = "";
}

string GetDefaultRuntimeIdentifier(string testDeviceIdentifier)
{
	return testDeviceIdentifier.ToLower().Contains("simulator") ?
	 $"iossimulator-{System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString().ToLower()}"
   : $"ios-arm64";
}

void InstallIpa(string testApp, string testAppPackageName, string testDevice, string testResultsDirectory, string version, string toolPath, bool headless)
{
	Information("Install app: {0} testDevice:{1}", testApp, testDevice);

	try
	{
		if (testDevice.Contains("device"))
		{
			if (string.IsNullOrEmpty(DEVICE_UDID))
				throw new InvalidOperationException("No device was found to install the app on.");
			RunAppleCommand("devicectl", "device", "install", "app", "--device", DEVICE_UDID, testApp);
		}
		else
		{
			var simulator = GetUDID(testDevice, toolPath);
			RunAppleCommand("simctl", "bootstatus", simulator, "-b");
			RunAppleCommand("simctl", "install", simulator, testApp);
		}
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
			var UDID = GetUDID(testDevice, dotnetToolPath);
			var sims = ListAppleSimulators();

			var simXH = sims.Where(s => s.UDID == UDID).FirstOrDefault();
			if (simXH == null)
			{
				throw new Exception("No simulator was found to run tests on.");
			}

			deviceToRun = simXH.UDID;
			DEVICE_NAME = simXH.Name;
			Information("The emulator to run tests: {0} {1}", simXH.Name, simXH.UDID);
		}

		Information("The platform version to run tests: {0}", iosVersionToRun);
		SetEnvironmentVariable("DEVICE_UDID", deviceToRun);
		SetEnvironmentVariable("DEVICE_NAME", DEVICE_NAME);
		SetEnvironmentVariable("PLATFORM_VERSION", iosVersionToRun);
        SetEnvironmentVariable("HEADLESS", headless.ToString());
	}
}

string GetUDID(string testDevice, string tool)
{
	Information("Looking for simulator: {0}", testDevice);
	var selected = !string.IsNullOrEmpty(udid) ? udid : EnvironmentVariable("DEVICE_UDID");
	if (string.IsNullOrEmpty(selected))
		selected = ownedSimulatorUdid;
	using (var devices = System.Text.Json.JsonDocument.Parse(RunAppleCommand("simctl", "list", "devices", "available", "--json")))
	{
		var allDevices = devices.RootElement.GetProperty("devices").EnumerateObject()
			.SelectMany(runtime => runtime.Value.EnumerateArray()).ToArray();
		if (!string.IsNullOrEmpty(selected))
		{
			var match = allDevices.FirstOrDefault(simulator =>
				string.Equals(simulator.GetProperty("udid").GetString(), selected, StringComparison.OrdinalIgnoreCase) ||
				string.Equals(simulator.GetProperty("name").GetString(), selected, StringComparison.OrdinalIgnoreCase));
			if (match.ValueKind == System.Text.Json.JsonValueKind.Undefined)
				throw new InvalidOperationException($"Selected simulator '{selected}' is unavailable.");
			return match.GetProperty("udid").GetString();
		}
		var requestedVersion = testDevice.Contains("_") ? testDevice.Split('_').Last() : iosVersion;
		var runtimeId = FindSimulatorRuntime(requestedVersion);
		if (runtimeId == null)
			throw new InvalidOperationException($"No installed iOS simulator runtime matches {testDevice}.");
		// Preserve the former XHarness profile; arbitrary installed/booted phones change snapshot geometry.
		var deviceType = "iPhone-11-Pro";
		if (requestedVersion == "min")
		{
			var runtimeVersion = Version.Parse(runtimeId.Substring(runtimeId.LastIndexOf("iOS-", StringComparison.Ordinal) + 4).Replace('-', '.'));
			deviceType = runtimeVersion.Major >= 16 ? "iPhone-SE-3rd-generation" : "iPhone-6s";
		}
		var typeId = "com.apple.CoreSimulator.SimDeviceType." + deviceType;
		ownedSimulatorUdid = RunAppleCommand("simctl", "create", $"MauiUITest-{Guid.NewGuid():N}", typeId, runtimeId).Trim();
		return ownedSimulatorUdid;
	}
}

void GetSimulators(string version, string tool)
{
	var requestedVersion = version.Contains("_") ? version.Split('_').Last() : version;
	if (FindSimulatorRuntime(requestedVersion) == null)
	{
		if (requestedVersion == "latest")
			RunAppleCommand("xcodebuild", "-downloadPlatform", "iOS");
		else
			RunAppleCommand("xcodebuild", "-downloadPlatform", "iOS", "-buildVersion", requestedVersion);
	}
}

void ResetSimulators(string version, string tool)
{
	var simulator = GetUDID(version, tool);
	if (simulator == ownedSimulatorUdid)
	{
		using (var devices = System.Text.Json.JsonDocument.Parse(RunAppleCommand("simctl", "list", "devices", "available", "--json")))
		{
			if (devices.RootElement.GetProperty("devices").EnumerateObject()
				.SelectMany(runtime => runtime.Value.EnumerateArray()).Any(candidate =>
					candidate.GetProperty("udid").GetString() == simulator && candidate.GetProperty("state").GetString() == "Booted"))
				RunAppleCommand("simctl", "shutdown", simulator);
		}
		RunAppleCommand("simctl", "erase", simulator);
	}
	RunAppleCommand("simctl", "bootstatus", simulator, "-b");
}

void GetDevices(string version, string tool)
{
	EnsureDirectoryExists(GetLogDirectory());
	var statePath = MakeAbsolute(GetLogDirectory()).CombineWithFilePath($"apple-devices-{Guid.NewGuid():N}.json").FullPath;
	RunAppleCommand("devicectl", "list", "devices", "--json-output", statePath);
	using (var state = System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(statePath)))
	{
		foreach (var candidate in state.RootElement.GetProperty("result").GetProperty("devices").EnumerateArray())
		{
			if (!candidate.TryGetProperty("deviceProperties", out var properties) ||
				!properties.TryGetProperty("osVersionNumber", out var osVersion))
				continue;
			var deviceVersion = osVersion.GetString();
			if (version.Contains(deviceVersion.Split('.')[0]))
			{
				DEVICE_UDID = candidate.GetProperty("hardwareProperties").GetProperty("udid").GetString();
				DEVICE_VERSION = deviceVersion;
				DEVICE_NAME = properties.GetProperty("name").GetString();
				DEVICE_OS = "iOS";
				break;
			}
		}
	}
	if (string.IsNullOrEmpty(DEVICE_UDID))
		throw new InvalidOperationException($"No devices found for version {version}");
}

string FindSimulatorRuntime(string version)
{
	using (var runtimes = System.Text.Json.JsonDocument.Parse(RunAppleCommand("simctl", "list", "runtimes", "--json")))
		return runtimes.RootElement.GetProperty("runtimes").EnumerateArray()
			.Where(runtime => runtime.GetProperty("isAvailable").GetBoolean() &&
				runtime.GetProperty("identifier").GetString().Contains(".iOS-") &&
				(version == "latest" || runtime.GetProperty("version").GetString().StartsWith(version, StringComparison.Ordinal)))
			.OrderBy(runtime => new Version(runtime.GetProperty("version").GetString()))
			.Select(runtime => runtime.GetProperty("identifier").GetString()).LastOrDefault();
}

string RunAppleCommand(params string[] arguments)
{
	var args = new ProcessArgumentBuilder();
	foreach (var argument in arguments)
		args.AppendQuoted(argument);
	var output = new System.Text.StringBuilder();
	var exitCode = StartProcess("xcrun", new ProcessSettings
	{
		Arguments = args,
		RedirectStandardOutput = true,
		RedirectedStandardOutputHandler = line => { output.AppendLine(line); return line; }
	});
	if (exitCode != 0)
		throw new InvalidOperationException($"xcrun {arguments[0]} failed with exit code {exitCode}.");
	return output.ToString();
}
