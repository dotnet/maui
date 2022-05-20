#addin nuget:?package=Cake.AppleSimulator&version=0.2.0
#load "../cake/helpers.cake"

string TARGET = Argument("target", "Test");

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("IOS_TEST_PROJECT") ?? "");
string TEST_DEVICE = Argument("device", EnvironmentVariable("IOS_TEST_DEVICE") ?? "ios-simulator-64_14.4"); // comma separated in the form <platform>-<device|simulator>[-<32|64>][_<version>] (eg: ios-simulator-64_13.4,[...])

// optional
var USE_DOTNET = Argument("dotnet", true);
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? (USE_DOTNET ? "net7.0-ios" : ""));
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("IOS_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("IOS_TEST_APP") ?? "");
var TEST_RESULTS = Argument("results", EnvironmentVariable("IOS_TEST_RESULTS") ?? "");

// other
string PLATFORM = TEST_DEVICE.ToLower().Contains("simulator") ? "iPhoneSimulator" : "iPhone";
string DOTNET_PLATFORM = TEST_DEVICE.ToLower().Contains("simulator") ? "iossimulator-x64" : "ios-x64";
string CONFIGURATION = "Release";
bool DEVICE_CLEANUP = Argument("cleanup", true);

Information("Project File: {0}", PROJECT);
Information("Build Binary Log (binlog): {0}", BINLOG_DIR);
Information("Build Platform: {0}", PLATFORM);
Information("Build Configuration: {0}", CONFIGURATION);

Setup(context =>
{
	Cleanup();

	// only install when an explicit version is specified
	if (TEST_DEVICE.IndexOf("_") != -1) {
		var settings = new DotNetCoreToolSettings {
			ToolPath = DOTNET_PATH,
			DiagnosticOutput = true,
			ArgumentCustomization = args => args.Append("run xharness apple simulators install " +
				$"\"{TEST_DEVICE}\" " +
				$"--verbosity=\"Debug\" ")
		};

		DotNetCoreTool("tool", settings);
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
		SetDotNetEnvironmentVariables(DOTNET_PATH);

		DotNetCoreBuild(PROJECT.FullPath, new DotNetCoreBuildSettings {
			Configuration = CONFIGURATION,
			Framework = TARGET_FRAMEWORK,
			MSBuildSettings = new DotNetCoreMSBuildSettings {
				MaxCpuCount = 0
			},
			ArgumentCustomization = args => args
				.Append("/p:BuildIpa=true")
				.Append("/bl:" + binlog),
			ToolPath = DOTNET_PATH,
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
		TEST_APP = apps.First().FullPath;
	}
	if (string.IsNullOrEmpty(TEST_RESULTS)) {
		TEST_RESULTS = TEST_APP + "-results";
	}

	Information("Test Device: {0}", TEST_DEVICE);
	Information("Test App: {0}", TEST_APP);
	Information("Test Results Directory: {0}", TEST_RESULTS);

	CleanDirectories(TEST_RESULTS);

	var settings = new DotNetCoreToolSettings {
		DiagnosticOutput = true,
		ArgumentCustomization = args => args.Append("run xharness apple test " +
		$"--app=\"{TEST_APP}\" " +
		$"--targets=\"{TEST_DEVICE}\" " +
		$"--output-directory=\"{TEST_RESULTS}\" " +
		$"--verbosity=\"Debug\" ")
	};

	try {
		DotNetCoreTool("tool", settings);
	} finally {
		// ios test result files are weirdly named, so fix it up
		var resultsFile = GetFiles($"{TEST_RESULTS}/xunit-test-*.xml").FirstOrDefault();
		if (FileExists(resultsFile)) {
			CopyFile(resultsFile, resultsFile.GetDirectory().CombineWithFilePath("TestResults.xml"));
		}
	}

	// this _may_ not be needed, but just in case
	var failed = XmlPeek($"{TEST_RESULTS}/TestResults.xml", "/assemblies/assembly[@failed > 0 or @errors > 0]/@failed");
	if (!string.IsNullOrEmpty(failed)) {
		throw new Exception($"At least {failed} test(s) failed.");
	}
});

RunTarget(TARGET);
