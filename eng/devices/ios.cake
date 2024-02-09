#addin nuget:?package=Cake.AppleSimulator&version=0.2.0
#load "../cake/helpers.cake"
#load "../cake/dotnet.cake"
#load "./devices-shared.cake"

#tool nuget:?package=NUnit.ConsoleRunner&version=3.16.3

const string defaultVersion = "16.4";
const string dotnetVersion = "net8.0";
// required
FilePath PROJECT = Argument("project", EnvironmentVariable("IOS_TEST_PROJECT") ?? DEFAULT_PROJECT);
string TEST_DEVICE = Argument("device", EnvironmentVariable("IOS_TEST_DEVICE") ?? $"ios-simulator-64_{defaultVersion}"); // comma separated in the form <platform>-<device|simulator>[-<32|64>][_<version>] (eg: ios-simulator-64_13.4,[...])

// optional
var DOTNET_ROOT = Argument("dotnet-root", EnvironmentVariable("DOTNET_ROOT"));
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{dotnetVersion}-ios");
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("IOS_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("IOS_TEST_APP") ?? "");
FilePath TEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("IOS_TEST_APP_PROJECT") ?? DEFAULT_APP_PROJECT);
var TEST_RESULTS = Argument("results", EnvironmentVariable("IOS_TEST_RESULTS") ?? "");

string TEST_WHERE = Argument("where", EnvironmentVariable("NUNIT_TEST_WHERE") ?? $"");

//these are for appium iOS UITests
var udid = Argument("udid", EnvironmentVariable("IOS_SIMULATOR_UDID") ?? "");
var iosVersion = Argument("apiversion", EnvironmentVariable("IOS_PLATFORM_VERSION") ?? defaultVersion);

string DEVICE_UDID = "";
string DEVICE_VERSION = "";
string DEVICE_NAME = "";
string DEVICE_OS = "";

// other
string PLATFORM = TEST_DEVICE.ToLower().Contains("simulator") ? "iPhoneSimulator" : "iPhone";
string DOTNET_PLATFORM = TEST_DEVICE.ToLower().Contains("simulator") ? 
	$"iossimulator-{System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString().ToLower()}"
  : $"ios-arm64";
string CONFIGURATION = Argument("configuration", "Debug");
bool DEVICE_CLEANUP = Argument("cleanup", true);
string TEST_FRAMEWORK = "net472";

Information("Project File: {0}", PROJECT);
Information("Build Binary Log (binlog): {0}", BINLOG_DIR);
Information("Build Platform: {0}", PLATFORM);
Information("Build Configuration: {0}", CONFIGURATION);

string DOTNET_TOOL_PATH = "/usr/local/share/dotnet/dotnet";

var localDotnetiOS = GetBuildVariable("workloads", "local") == "local";
if (localDotnetiOS)
{
	Information("Using local dotnet");
	DOTNET_TOOL_PATH = $"{MakeAbsolute(Directory("../../bin/dotnet/")).ToString()}/dotnet";
}
else
{
	Information("Using system dotnet");
}

Setup(context =>
{
	Cleanup();

	Information($"DOTNET_TOOL_PATH {DOTNET_TOOL_PATH}");
	
	Information("Host OS System Arch: {0}", System.Runtime.InteropServices.RuntimeInformation.OSArchitecture);
	Information("Host Processor System Arch: {0}", System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture);

	// only grab attached iOS devices if we are trying to test on device
	if (TEST_DEVICE.Contains("device")) 
	{ 
		GetDevices(iosVersion);
	}
	// only install simulator when an explicit version is specified
	if (TEST_DEVICE.IndexOf("_") != -1) 
	{
		GetSimulators(TEST_DEVICE);
	}
});

Teardown(context =>
{
	Cleanup();
});

void Cleanup()
{
	if (!DEVICE_CLEANUP)
		return;

	// delete the XHarness simulators first, if it exists
	Information("Deleting XHarness simulator if exists...");
	var sims = ListAppleSimulators();
	if(sims.Count == 0)
	{
		Information("No simulators found to delete.");
		return;
	}
	var simulatorName = "XHarness";
	Information("Looking for simulator: {0} ios version {1}", simulatorName, iosVersion);
	var xharness = sims.Where(s => s.Name.Contains(simulatorName))?.ToArray();
	if(xharness == null || xharness.Length == 0)
	{
		Information("No simulators with {0} found to delete.", simulatorName);
		return;
	}
	foreach (var sim in xharness) {
		Information("Deleting XHarness simulator {0} ({1})...", sim.Name, sim.UDID);
		StartProcess("xcrun", "simctl shutdown " + sim.UDID);
		var retries = 3;
		while (retries > 0) {
			var exitCode = StartProcess("xcrun", "simctl delete " + sim.UDID);
			if (exitCode == 0) {
				retries = 0;
			} else {
				retries--;
				System.Threading.Thread.Sleep(1000);
			}
		}
	}
}

Task("Cleanup");

Task("Build")
	.WithCriteria(!string.IsNullOrEmpty(PROJECT.FullPath))
	.Does(() =>
{
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-ios.binlog";
	
	DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
			ToolPath = DOTNET_TOOL_PATH,
			Configuration = CONFIGURATION,
			Framework = TARGET_FRAMEWORK,
			MSBuildSettings = new DotNetMSBuildSettings {
				MaxCpuCount = 0
			},	
			ArgumentCustomization = args =>
			{ 	
				args
				.Append("/p:BuildIpa=true")
				.Append("/bl:" + binlog)
				.Append("/tl");
				
				// if we building for a device
				if(TEST_DEVICE.ToLower().Contains("device"))
				{
					args.Append("/p:RuntimeIdentifier=ios-arm64");
				}
				return args;
			}
		});
});

Task("uitest-build")
	.Does(() =>
{
	var name = System.IO.Path.GetFileNameWithoutExtension(DEFAULT_APP_PROJECT);
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-ios.binlog";

	Information("app" +DEFAULT_APP_PROJECT);
	DotNetBuild(DEFAULT_APP_PROJECT, new DotNetBuildSettings {
		Configuration = CONFIGURATION,
		Framework = TARGET_FRAMEWORK,
		ToolPath = DOTNET_PATH,
		ArgumentCustomization = args =>
		{ 	
			args
			.Append("/p:BuildIpa=true")
			.Append("/bl:" + binlog)
			.Append("/tl");
			
			// if we building for a device
			if(TEST_DEVICE.ToLower().Contains("device"))
			{
				args.Append("/p:RuntimeIdentifier=ios-arm64");
			}

			return args;
		}
	});
});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
{
	if (string.IsNullOrEmpty(TEST_APP)) {
		if (string.IsNullOrEmpty(PROJECT.FullPath))
			throw new Exception("If no app was specified, an app must be provided.");
		var binDir = PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TARGET_FRAMEWORK).Combine(DOTNET_PLATFORM).FullPath;
		var apps = GetDirectories(binDir + "/*.app");
		if (apps.Any()) {
			TEST_APP = apps.First().FullPath;
		}
		else {
			Error($"Error: Couldn't find *.app file in {binDir}");
			throw new Exception($"Error: Couldn't find *.app file in {binDir}");
		}
	}
	if (string.IsNullOrEmpty(TEST_RESULTS)) {
		TEST_RESULTS = TEST_APP + "-results";
	}

	Information("Test Device: {0}", TEST_DEVICE);
	Information("Test App: {0}", TEST_APP);
	Information("Test Results Directory: {0}", TEST_RESULTS);

	if (!IsCIBuild())
		CleanDirectories(TEST_RESULTS);
	else
	{
		// Because we retry on CI we don't want to delete the previous failures
		// We want to publish those files for reference
		DeleteFiles(Directory(TEST_RESULTS).Path.Combine("*.*").FullPath);
	}

	var XCODE_PATH =  Argument("xcode_path", "");
		
	string xcode_args = "";
	if (!String.IsNullOrEmpty(XCODE_PATH))
	{
		xcode_args = $"--xcode=\"{XCODE_PATH}\" ";
	}

	Information("XCODE PATH: {0}", XCODE_PATH);

	var settings = new DotNetToolSettings {
		ToolPath = DOTNET_TOOL_PATH,
		DiagnosticOutput = true,
		ArgumentCustomization = args => 
		{
			args.Append("run xharness apple test " +
				$"--app=\"{TEST_APP}\" " +
				$"--targets=\"{TEST_DEVICE}\" " +
				$"--output-directory=\"{TEST_RESULTS}\" " +
				$"--timeout=01:15:00 " +
				$"--launch-timeout=00:06:00 " +
				xcode_args +
				$"--verbosity=\"Debug\" ");
			
			if (TEST_DEVICE.Contains("device"))
			{
				if(string.IsNullOrEmpty(DEVICE_UDID))
				{
					throw new Exception("No device was found to install the app on. See the Setup method for more details.");
				}
				args.Append($"--device=\"{DEVICE_UDID}\" ");
			}
			return args;	
		}
	};

	bool testsFailed = true;
	try {
		DotNetTool("tool", settings);
		testsFailed = false;
	} finally {
		// ios test result files are weirdly named, so fix it up
		var resultsFile = GetFiles($"{TEST_RESULTS}/xunit-test-*.xml").FirstOrDefault();
		if (FileExists(resultsFile)) {
			CopyFile(resultsFile, resultsFile.GetDirectory().CombineWithFilePath("TestResults.xml"));
		}

		if (testsFailed && IsCIBuild())
		{
			var failurePath = $"{TEST_RESULTS}/TestResultsFailures/{Guid.NewGuid()}";
			EnsureDirectoryExists(failurePath);
			// The tasks will retry the tests and overwrite the failed results each retry
			// we want to retain the failed results for diagnostic purposes
			CopyFiles($"{TEST_RESULTS}/*.*", failurePath);
			
			// We don't want these to upload
			MoveFile($"{failurePath}/TestResults.xml", $"{failurePath}/Results.xml");
		}
	}

	// this _may_ not be needed, but just in case
	var failed = XmlPeek($"{TEST_RESULTS}/TestResults.xml", "/assemblies/assembly[@failed > 0 or @errors > 0]/@failed");
	if (!string.IsNullOrEmpty(failed)) {
		throw new Exception($"At least {failed} test(s) failed.");
	}
});

Task("uitest")
	.IsDependentOn("uitest-build")
	.Does(() =>
{
	SetupAppPackageNameAndResult();

	CleanDirectories(TEST_RESULTS);

	InstallIpa(TEST_APP, "", TEST_DEVICE, TEST_RESULTS, iosVersion);

	//we need to build tests first to pass ExtraDefineConstants
	Information("Build UITests project {0}", PROJECT.FullPath);
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-ios.binlog";
	DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
			ToolPath = DOTNET_TOOL_PATH,
			Configuration = CONFIGURATION,
			ArgumentCustomization = args => args
				.Append("/p:ExtraDefineConstants=IOSUITEST")
				.Append("/bl:" + binlog)
				.Append("/tl")
			
	});

	SetEnvironmentVariable("APPIUM_LOG_FILE", $"{BINLOG_DIR}/appium_ios.log");

	Information("Run UITests project {0}",PROJECT.FullPath);
	RunTestWithLocalDotNet(PROJECT.FullPath, CONFIGURATION, pathDotnet: DOTNET_TOOL_PATH, noBuild: true, resultsFileNameWithoutExtension: $"{name}-{CONFIGURATION}-ios");
});

Task("cg-uitest")
	.Does(() =>
{
	SetupAppPackageNameAndResult();
	
	CleanDirectories(TEST_RESULTS);

	InstallIpa(TEST_APP, "com.microsoft.mauicompatibilitygallery", TEST_DEVICE, $"{TEST_RESULTS}/ios", iosVersion);

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
	SetEnvironmentVariable("iOS_APP", $"{TEST_APP}");

	// build the test library
	var binDir = PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TEST_FRAMEWORK).FullPath;
	Information("BinDir: {0}", binDir);
	Information("PROJECT: {0}", PROJECT);
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{binDir}/{name}-{CONFIGURATION}-ios.binlog";
	Information("Build UITests project {0}", PROJECT.FullPath);
	DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
			ToolPath = DOTNET_TOOL_PATH,
			Configuration = CONFIGURATION,
			ArgumentCustomization = args => args
				.Append("/bl:" + binlog)
				.Append("/tl"),
	});
	
	var testLibDllPath = $"{binDir}/Microsoft.Maui.Controls.iOS.UITests.dll";
	Information("Run UITests lib {0}", testLibDllPath);
	var nunitSettings = new NUnit3Settings { 
		Configuration = CONFIGURATION,
		OutputFile = $"{TEST_RESULTS}/ios/run_uitests_output.log",
		Work = $"{TEST_RESULTS}/ios",
		TraceLevel = NUnitInternalTraceLevel.Verbose
	};

	Information("Outputfile {0}", nunitSettings.OutputFile);

	if(!string.IsNullOrEmpty(TEST_WHERE))
	{
		Information("Add Where filter to NUnit {0}", TEST_WHERE);
		nunitSettings.Where = TEST_WHERE;
	}
	RunTestsNunit(testLibDllPath, nunitSettings);

	// When all tests are inconclusive the run does not fail, check if this is the case and fail the pipeline so we get notified	
	FailRunOnOnlyInconclusiveTests(System.IO.Path.Combine(nunitSettings.Work.FullPath, "TestResult.xml"));
});

RunTarget(TARGET);

void SetupAppPackageNameAndResult()
{
   if (string.IsNullOrEmpty(TEST_APP) ) {
		if (string.IsNullOrEmpty(TEST_APP_PROJECT.FullPath))
			throw new Exception("If no app was specified, an app must be provided.");
		var binDir =  MakeAbsolute(TEST_APP_PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION).Combine(TARGET_FRAMEWORK).Combine(DOTNET_PLATFORM));
		Information("BinDir: {0}", binDir);
		var apps = GetDirectories(binDir + "/*.app");
		if(apps.Count == 0)
			throw new Exception("No app was found in the bin directory.");
		
		TEST_APP = apps.First().FullPath;
	}
	if (string.IsNullOrEmpty(TEST_RESULTS)) {
		TEST_RESULTS =  GetTestResultsDirectory().FullPath;
	}
}

void InstallIpa(string testApp, string testAppPackageName, string testDevice, string testResultsDirectory, string version)
{
	Information("Install with xharness: {0}",testApp);
	var settings = new DotNetToolSettings {
		ToolPath = DOTNET_TOOL_PATH,
		DiagnosticOutput = true,	
		ArgumentCustomization = args => { 
			args.Append("run xharness apple install " +
							$"--app=\"{testApp}\" " +
							$"--targets=\"{testDevice}\" " +
							$"--output-directory=\"{testResultsDirectory}\" " +
							$"--verbosity=\"Debug\" ");
			if (testDevice.Contains("device"))
			{
				if(string.IsNullOrEmpty(DEVICE_UDID))
				{
					throw new Exception("No device was found to install the app on. See the Setup method for more details.");
				}
				args.Append($"--device=\"{DEVICE_UDID}\" ");
			}
			return args;
		}
	};

	try {
		DotNetTool("tool", settings);
	} finally {
		string iosVersionToRun = version;
		string deviceToRun = "";	
		
		if (testDevice.Contains("device"))
		{	
			if(!string.IsNullOrEmpty(DEVICE_UDID))
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
			if(simXH == null)
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

void GetSimulators(string version)
{
	DotNetTool("tool", new DotNetToolSettings {
			ToolPath = DOTNET_TOOL_PATH,
			DiagnosticOutput = true,
			ArgumentCustomization = args => args.Append("run xharness apple simulators install " +
				$"\"{version}\" " +
				$"--verbosity=\"Debug\" ")
		});
}

void GetDevices(string version)
{
	var deviceUdid = "";
	var deviceName = "";
	var deviceVersion = "";
	var deviceOS = "";
	
	var list = new List<string>();
	bool isDevice = false;
	// print the apple state of the machine
	// this will print the connected devices
	DotNetTool("tool", new DotNetToolSettings {
			ToolPath = DOTNET_TOOL_PATH,
			DiagnosticOutput = true,
			ArgumentCustomization = args => args.Append("run xharness apple state --verbosity=\"Debug\" "),
			SetupProcessSettings = processSettings =>
				{
					processSettings.RedirectStandardOutput = true;
					processSettings.RedirectStandardError = true;
					processSettings.RedirectedStandardOutputHandler = (output) => {
							Information("Apple State: {0}", output);
							if (output == "Connected Devices:" )
							{
								isDevice = true;
							}
							else if(isDevice)
							{
								list.Add(output);
							}
							return output;
						};
				}
	});
	Information("List count: {0}", list.Count);	
	//this removes the extra lines from output
	if(list.Count == 0)
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
		if(match.Success)
		{
			deviceName = match.Groups[1].Value;
			deviceUdid = match.Groups[2].Value;
			deviceVersion = match.Groups[3].Value;
			deviceOS = match.Groups[4].Value;
			Information("DeviceName:{0} udid:{1} version:{2} os:{3}", deviceName, deviceUdid, deviceVersion, deviceOS);
			if(version.Contains(deviceVersion.Split(".")[0]))
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
	if(string.IsNullOrEmpty(DEVICE_UDID))
	{
		throw new Exception($"No devices found for version {version}");
	}
}
