#load "../cake/helpers.cake"
#load "../cake/dotnet.cake"

string TARGET = Argument("target", "Test");

const string defaultVersion = "14.4";
const string dotnetVersion = "net7.0";

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("MAC_TEST_PROJECT") ?? "");
string TEST_DEVICE = Argument("device", EnvironmentVariable("MAC_TEST_DEVICE") ?? $"ios-simulator-64_{defaultVersion}"); // comma separated in the form <platform>-<device|simulator>[-<32|64>][_<version>] (eg: ios-simulator-64_13.4,[...])

// optional
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{dotnetVersion}-maccatalyst");
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("IOS_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("MAC_TEST_APP") ?? "");
FilePath TEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("MAC_TEST_APP_PROJECT") ?? "");
var TEST_RESULTS = Argument("results", EnvironmentVariable("MAC_TEST_RESULTS") ?? "");

// other
string PLATFORM = "mac";
string DOTNET_PLATFORM = "maccatalyst-x64";
string CONFIGURATION = Argument("configuration", "Release");
bool DEVICE_CLEANUP = Argument("cleanup", true);

Information("Project File: {0}", PROJECT);
Information("Build Binary Log (binlog): {0}", BINLOG_DIR);
Information("Build Platform: {0}", PLATFORM);
Information("Build Configuration: {0}", CONFIGURATION);

Setup(context =>
{
	Cleanup();
});

Teardown(context =>
{
	Cleanup();
});

void Cleanup()
{
	if (!DEVICE_CLEANUP)
		return;
}

Task("Cleanup");

Task("uitest")
	.Does(() =>
{
	if (string.IsNullOrEmpty(TEST_APP) ) {
		if (string.IsNullOrEmpty(TEST_APP_PROJECT.FullPath))
			throw new Exception("If no app was specified, an app must be provided.");
		var binDir = TEST_APP_PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TARGET_FRAMEWORK).Combine(DOTNET_PLATFORM).FullPath;
		Information("BinDir: {0}", binDir);
		var apps = GetDirectories(binDir + "/*.app");
		TEST_APP = apps.First().FullPath;
	}
	if (string.IsNullOrEmpty(TEST_RESULTS)) {
		TEST_RESULTS = TEST_APP + "-results";
	}

	Information("Test Device: {0}", TEST_DEVICE);
	Information("Test App Project: {0}", TEST_APP_PROJECT);
	Information("Test App: {0}", TEST_APP);
	Information("Test Results Directory: {0}", TEST_RESULTS);

	CleanDirectories(TEST_RESULTS);

	Information("Run App project {0}",TEST_APP_PROJECT.FullPath);
	DotNetBuild(TEST_APP_PROJECT.FullPath, new DotNetBuildSettings {
			Configuration = CONFIGURATION,
			ArgumentCustomization = args => args
				.Append($"-f {TARGET_FRAMEWORK}")
				.Append("-t:Run")
				//.Append("/tl")
	});

	Information("Build UITests project {0}",PROJECT.FullPath);
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-mac.binlog";
	DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
			Configuration = CONFIGURATION,
			ArgumentCustomization = args => args
				.Append("/p:ExtraDefineConstants=MACUITEST")
				.Append("/bl:" + binlog)
				//.Append("/tl")
	});

	SetEnvironmentVariable("APPIUM_LOG_FILE", $"{BINLOG_DIR}/appium_mac.log");

	Information("Run UITests project {0}",PROJECT.FullPath);
	RunTestWithLocalDotNet(PROJECT.FullPath, CONFIGURATION, noBuild: true);
});

RunTarget(TARGET);
