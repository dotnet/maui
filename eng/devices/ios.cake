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
var USE_DOTNET = Argument("dotnet", true);
var DOTNET_ROOT = Argument("dotnet-root", EnvironmentVariable("DOTNET_ROOT"));
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? (USE_DOTNET ? $"{dotnetVersion}-ios" : ""));
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("IOS_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("IOS_TEST_APP") ?? "");
FilePath TEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("IOS_TEST_APP_PROJECT") ?? DEFAULT_APP_PROJECT);
var TEST_RESULTS = Argument("results", EnvironmentVariable("IOS_TEST_RESULTS") ?? "");

string TEST_WHERE = Argument("where", EnvironmentVariable("NUNIT_TEST_WHERE") ?? $"");

//these are for appium iOS UITests
var udid = Argument("udid", EnvironmentVariable("IOS_SIMULATOR_UDID") ?? "");
var iosVersion = Argument("apiversion", EnvironmentVariable("IOS_PLATFORM_VERSION") ?? defaultVersion);

// other
string PLATFORM = TEST_DEVICE.ToLower().Contains("simulator") ? "iPhoneSimulator" : "iPhone";
string DOTNET_PLATFORM = TEST_DEVICE.ToLower().Contains("simulator") ? "iossimulator-x64" : "ios-x64";
string CONFIGURATION = Argument("configuration", "Debug");
bool DEVICE_CLEANUP = Argument("cleanup", true);
string TEST_FRAMEWORK = "net472";

Information("Project File: {0}", PROJECT);
Information("Build Binary Log (binlog): {0}", BINLOG_DIR);
Information("Build Platform: {0}", PLATFORM);
Information("Build Configuration: {0}", CONFIGURATION);

Setup(context =>
{
	Cleanup();

	// only install when an explicit version is specified
	if (TEST_DEVICE.IndexOf("_") != -1) {
		var settings = new DotNetToolSettings {
			ToolPath = DOTNET_PATH,
			DiagnosticOutput = true,
			ArgumentCustomization = args => args.Append("run xharness apple simulators install " +
				$"\"{TEST_DEVICE}\" " +
				$"--verbosity=\"Debug\" ")
		};

		DotNetTool("tool", settings);
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
	var xharness = sims.Where(s => s.Name.Contains("XHarness")).ToArray();
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

	if (USE_DOTNET)
	{
		Information($"Build target dotnet root: {DOTNET_ROOT}");
		Information($"Build target set dotnet tool path: {DOTNET_PATH}");
		
		var localDotnetRoot = MakeAbsolute(Directory("../../bin/dotnet/"));
		Information("new dotnet root: {0}", localDotnetRoot);

		DOTNET_ROOT = localDotnetRoot.ToString();

		SetDotNetEnvironmentVariables(DOTNET_ROOT);
		
		DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
			Configuration = CONFIGURATION,
			Framework = TARGET_FRAMEWORK,
			MSBuildSettings = new DotNetMSBuildSettings {
				MaxCpuCount = 0
			},
			ToolPath = DOTNET_PATH,
			ArgumentCustomization = args => args
				.Append("/p:BuildIpa=true")
				.Append("/bl:" + binlog)
				//.Append("/tl")
			
		});
	}
	else
	{
		MSBuild(PROJECT.FullPath, c => {
			c.Configuration = CONFIGURATION;
			c.MaxCpuCount = 0;
			c.Restore = true;
			c.Properties["Platform"] = new List<string> { PLATFORM };
			c.Properties["BuildIpa"] = new List<string> { "true" };
			c.Properties["ContinuousIntegrationBuild"] = new List<string> { "false" };
			if (!string.IsNullOrEmpty(TARGET_FRAMEWORK))
				c.Properties["TargetFramework"] = new List<string> { TARGET_FRAMEWORK };
			c.Targets.Clear();
			c.Targets.Add("Build");
			c.BinaryLogger = new MSBuildBinaryLogSettings {
				Enabled = true,
				FileName = binlog,
			};
		});
	}
});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
{
	if (string.IsNullOrEmpty(TEST_APP)) {
		if (string.IsNullOrEmpty(PROJECT.FullPath))
			throw new Exception("If no app was specified, an app must be provided.");
		var binDir = USE_DOTNET
			? PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TARGET_FRAMEWORK).Combine(DOTNET_PLATFORM).FullPath
			: PROJECT.GetDirectory().Combine("bin").Combine(PLATFORM).Combine(CONFIGURATION).FullPath;
		var apps = GetDirectories(binDir + "/*.app");
		if (apps.Any()) {
			TEST_APP = apps.First().FullPath;
		}
		else {
			Error("Error: Couldn't find .app file");
			throw new Exception("Error: Couldn't find .app file");
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
		DiagnosticOutput = true,
		ArgumentCustomization = args => args.Append("run xharness apple test " +
		$"--app=\"{TEST_APP}\" " +
		$"--targets=\"{TEST_DEVICE}\" " +
		$"--output-directory=\"{TEST_RESULTS}\" " +
		xcode_args +
		$"--verbosity=\"Debug\" ")
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
			Configuration = CONFIGURATION,
			ToolPath = DOTNET_PATH,
			ArgumentCustomization = args => args
				.Append("/p:ExtraDefineConstants=IOSUITEST")
				.Append("/bl:" + binlog)
				//.Append("/tl")
			
	});

	SetEnvironmentVariable("APPIUM_LOG_FILE", $"{BINLOG_DIR}/appium_ios.log");

	Information("Run UITests project {0}",PROJECT.FullPath);
	RunTestWithLocalDotNet(PROJECT.FullPath, CONFIGURATION, noBuild: true, resultsFileNameWithoutExtension: $"{name}-{CONFIGURATION}-ios");
});

Task("cg-uitest")
	.Does(() =>
{
	SetupAppPackageNameAndResult();
	
	CleanDirectories(TEST_RESULTS);

	InstallIpa(TEST_APP, "com.microsoft.mauicompatibilitygallery", TEST_DEVICE, $"{TEST_RESULTS}/ios", iosVersion);

	//set env var for the app path for Xamarin.UITest setup
	SetEnvironmentVariable("iOS_APP", $"{TEST_APP}");

	// build the test library
	var binDir = PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TEST_FRAMEWORK).FullPath;
	Information("BinDir: {0}", binDir);
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{binDir}/{name}-{CONFIGURATION}-ios.binlog";
	Information("Build UITests project {0}", PROJECT.FullPath);
	DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
			Configuration = CONFIGURATION,
			ArgumentCustomization = args => args
				.Append("/bl:" + binlog),
			ToolPath = DOTNET_PATH,
	});
	
	var testLibDllPath = $"{binDir}/Microsoft.Maui.Controls.iOS.UITests.dll";
	Information("Run UITests lib {0}", testLibDllPath);
	var nunitSettings = new NUnit3Settings { 
		Configuration = CONFIGURATION,
		OutputFile = $"{TEST_RESULTS}/ios/run_uitests_output.log",
		Work = $"{TEST_RESULTS}/ios"
	};

	Information("Outputfile {0}", nunitSettings.OutputFile);

	if(!string.IsNullOrEmpty(TEST_WHERE))
	{
		Information("Add Where filter to NUnit {0}", TEST_WHERE);
		nunitSettings.Where = TEST_WHERE;
	}
	RunTestsNunit(testLibDllPath, nunitSettings);
});

RunTarget(TARGET);

void SetupAppPackageNameAndResult()
{
   if (string.IsNullOrEmpty(TEST_APP) ) {
		if (string.IsNullOrEmpty(TEST_APP_PROJECT.FullPath))
			throw new Exception("If no app was specified, an app must be provided.");
		var binDir = USE_DOTNET
			? TEST_APP_PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TARGET_FRAMEWORK).Combine(DOTNET_PLATFORM).FullPath
			: TEST_APP_PROJECT.GetDirectory().Combine("bin").Combine(PLATFORM).Combine(CONFIGURATION).FullPath;
		Information("BinDir: {0}", binDir);
		var apps = GetDirectories(binDir + "/*.app");
		if(apps.Count == 0)
			throw new Exception("No app was found in the bin directory.");
		
		TEST_APP = apps.First().FullPath;
	}
	if (string.IsNullOrEmpty(TEST_RESULTS)) {
		TEST_RESULTS =  GetTestResultsDirectory().FullPath;
	}

	Information($"Build target dotnet root: {DOTNET_ROOT}");
	Information($"Build target set dotnet tool path: {DOTNET_PATH}");
		
	var localDotnetRoot = MakeAbsolute(Directory("../../bin/dotnet/"));
	Information("new dotnet root: {0}", localDotnetRoot);

	DOTNET_ROOT = localDotnetRoot.ToString();

	Information("Test Device: {0}", TEST_DEVICE);
	Information("Test App: {0}", TEST_APP);
	Information("Test Results Directory: {0}", TEST_RESULTS);
}

void InstallIpa(string testApp, string testAppPackageName, string testDevice, string testResultsDirectory, string version)
{
	Information("Install with xharness: {0}",testApp);
	var settings = new DotNetToolSettings {
		DiagnosticOutput = true,
		ArgumentCustomization = args => args.Append("run xharness apple install " +
		$"--app=\"{testApp}\" " +
		$"--targets=\"{testDevice}\" " +
		$"--output-directory=\"{testResultsDirectory}\" " +
		$"--verbosity=\"Debug\" ")
	};

	try {
		DotNetTool("tool", settings);
	} finally {

		var sims = ListAppleSimulators();
	 	var xharness = sims.Where(s => s.Name.Contains("XHarness")).ToArray();
		var simXH = xharness.First();
		Information("The emulator to run tests: {0} {1}", simXH.Name, simXH.UDID);
		Information("The platform version to run tests: {0}", version);
		SetEnvironmentVariable("IOS_SIMULATOR_UDID",simXH.UDID);
		SetEnvironmentVariable("IOS_PLATFORM_VERSION", version);
	}
}
