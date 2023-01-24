#addin nuget:?package=Cake.AppleSimulator&version=0.2.0
#load "../cake/helpers.cake"

string TARGET = Argument("target", "Test");

const string defaultVersion = "10.0.19041";

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("WINDOWS_TEST_PROJECT") ?? "");
string TEST_DEVICE = Argument("device", EnvironmentVariable("WINDOWS_TEST_DEVICE") ?? $"");

// optional
var localDotnet = GetBuildVariable("workloads", "local") == "local";
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"net7.0-windows{defaultVersion}");
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("WINDOWS_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("WINDOWS_TEST_APP") ?? "");
FilePath TEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("WINDOWS_TEST_APP_PROJECT") ?? "");
var TEST_RESULTS = Argument("results", EnvironmentVariable("MAC_TEST_RESULTS") ?? "");
string CONFIGURATION = Argument("configuration", "Release");

var windowsVersion = Argument("apiversion", EnvironmentVariable("WINDOWS_PLATFORM_VERSION") ?? defaultVersion);

// other
string PLATFORM = "windows";
string DOTNET_PLATFORM = $"win10-x64";
bool DEVICE_CLEANUP = Argument("cleanup", true);

Information("Project File: {0}", PROJECT);
Information("Build Binary Log (binlog): {0}", BINLOG_DIR);
Information("Build Platform: {0}", PLATFORM);
Information("Build Configuration: {0}", CONFIGURATION);

Information("Windows version: {0}", windowsVersion);

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
		var apps = GetFiles(binDir + "/*.exe").Where(c => !c.FullPath.EndsWith("createdump.exe"));
		Information("Apps: {0}", apps);
		TEST_APP = apps.First().FullPath;
	}
	if (string.IsNullOrEmpty(TEST_RESULTS)) {
		TEST_RESULTS = TEST_APP + "-results";
	}

	Information("Test Device: {0}", TEST_DEVICE);
	Information("Test App: {0}", TEST_APP);
	Information("Test Results Directory: {0}", TEST_RESULTS);

	CleanDirectories(TEST_RESULTS);

	Information("Run UITests {0}",PROJECT.FullPath);

	var properties = new Dictionary<string,string>
	{
		["ExtraDefineConstants"] = "WINTEST"
	};
	SetEnvironmentVariable("WINDOWS_APP_PATH", TEST_APP);
	SetEnvironmentVariable("APPIUM_LOG_FILE", $"{BINLOG_ARG}/appium_windows.log");
	
	RunTestWithLocalDotNet(PROJECT.FullPath, CONFIGURATION, argsExtra : properties);
});

RunTarget(TARGET);
