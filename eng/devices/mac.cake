#addin nuget:?package=Cake.AppleSimulator&version=0.2.0
#load "../cake/helpers.cake"

string TARGET = Argument("target", "Test");

const string defaultVersion = "14.4";

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("MAC_TEST_PROJECT") ?? "");
string TEST_DEVICE = Argument("device", EnvironmentVariable("MAC_TEST_DEVICE") ?? $"ios-simulator-64_{defaultVersion}"); // comma separated in the form <platform>-<device|simulator>[-<32|64>][_<version>] (eg: ios-simulator-64_13.4,[...])

// optional
var localDotnet = GetBuildVariable("workloads", "local") == "local";
var USE_DOTNET = Argument("dotnet", true);
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? (USE_DOTNET ? "net7.0-maccatalyst" : ""));
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("IOS_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("MAC_TEST_APP") ?? "");
FilePath TEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("MAC_TEST_APP_PROJECT") ?? "");
var TEST_RESULTS = Argument("results", EnvironmentVariable("MAC_TEST_RESULTS") ?? "");


// other
string PLATFORM = "mac";
string DOTNET_PLATFORM = "maccatalyst-x64";
string CONFIGURATION = "Release";
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
		var binDir = USE_DOTNET
			? TEST_APP_PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TARGET_FRAMEWORK).Combine(DOTNET_PLATFORM).FullPath
			: TEST_APP_PROJECT.GetDirectory().Combine("bin").Combine(PLATFORM).Combine(CONFIGURATION).FullPath;
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

	// Information("Install with xharness: {0}",TEST_APP);
	// var settings = new DotNetCoreToolSettings {
	// 	DiagnosticOutput = true,
	// 	ArgumentCustomization = args => args.Append("run xharness apple install " +
	// 	$"--app=\"{TEST_APP}\" " +
	// 	$"--targets=\"{TEST_DEVICE}\" " +
	// 	$"--output-directory=\"{TEST_RESULTS}\" " +
	// 	$"--verbosity=\"Debug\" ")
	// };

	// try {
	// 	DotNetCoreTool("tool", settings);
	// } finally {
		
	// }

	Information("Run UITests {0}",PROJECT.FullPath);

	var properties = new Dictionary<string,string>
	{
		["ExtraDefineConstants"] = "MACUITEST"
	};
	RunTestWithLocalDotNet(PROJECT.FullPath, CONFIGURATION, argsExtra : properties);
});

RunTarget(TARGET);
