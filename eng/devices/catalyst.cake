#load "../cake/helpers.cake"
#load "../cake/dotnet.cake"
#load "./devices-shared.cake"

const string dotnetVersion = "net8.0";

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("MAC_TEST_PROJECT") ?? DEFAULT_PROJECT);
string TEST_DEVICE = Argument("device", EnvironmentVariable("MAC_TEST_DEVICE") ?? "maccatalyst");

// optional
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var DOTNET_ROOT = Argument("dotnet-root", EnvironmentVariable("DOTNET_ROOT"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{dotnetVersion}-maccatalyst");
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("MAC_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("MAC_TEST_APP") ?? "");
FilePath TEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("MAC_TEST_APP_PROJECT") ?? DEFAULT_APP_PROJECT);
var TEST_RESULTS = Argument("results", EnvironmentVariable("MAC_TEST_RESULTS") ?? "");

// other
string RUNTIME_IDENTIFIER = Argument("rid", EnvironmentVariable("MAC_RUNTIME_IDENTIFIER") ?? "maccatalyst-x64");
string CONFIGURATION = Argument("configuration", "Release");
bool DEVICE_CLEANUP = Argument("cleanup", true);

Information("Project File: {0}", PROJECT);
Information("Build Binary Log (binlog): {0}", BINLOG_DIR);
Information("Build Configuration: {0}", CONFIGURATION);
Information("Build Runtime Identifier: {0}", RUNTIME_IDENTIFIER);
Information("Build Target Framework: {0}", TARGET_FRAMEWORK);

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

Task("Build")
	.WithCriteria(!string.IsNullOrEmpty(PROJECT.FullPath))
	.Does(() =>
{
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-catalyst.binlog";

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
			.Append("/p:RuntimeIdentifier=" + RUNTIME_IDENTIFIER)
			.Append("/bl:" + binlog)
	});
});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
{
	if (string.IsNullOrEmpty(TEST_APP)) {
		if (string.IsNullOrEmpty(PROJECT.FullPath))
			throw new Exception("If no app was specified, an app must be provided.");
		var binDir = PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TARGET_FRAMEWORK).Combine(RUNTIME_IDENTIFIER).FullPath;
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

		SetDotNetEnvironmentVariables("/Users/runner/hostedtoolcache/dotnet");
	}


	var settings = new DotNetToolSettings {
		DiagnosticOutput = true,
		ArgumentCustomization = args => args.Append("run xharness apple test " +
		$"--app=\"{TEST_APP}\" " +
		$"--targets=\"{TEST_DEVICE}\" " +
		$"--output-directory=\"{TEST_RESULTS}\" " +
		$"--verbosity=\"Debug\" ")
	};

	bool testsFailed = true;
	try {
		DotNetTool("tool", settings);
		testsFailed = false;
	} finally {
		// catalyst test result files are weirdly named, so fix it up
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
	if (string.IsNullOrEmpty(TEST_APP) ) {
		if (string.IsNullOrEmpty(TEST_APP_PROJECT.FullPath))
			throw new Exception("If no app was specified, an app must be provided.");
		var binDir = TEST_APP_PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TARGET_FRAMEWORK).Combine(RUNTIME_IDENTIFIER).FullPath;
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
	RunTestWithLocalDotNet(PROJECT.FullPath, CONFIGURATION, noBuild: true, resultsFileNameWithoutExtension: $"{name}-{CONFIGURATION}-catalyst");
});

RunTarget(TARGET);
