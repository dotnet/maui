#load "./uitests-shared.cake"

// Argument handling
string DEFAULT_MAC_PROJECT = "../../src/Controls/tests/TestCases.Mac.Tests/Controls.TestCases.Mac.Tests.csproj";
var projectPath = Argument("project", EnvironmentVariable("MAC_TEST_PROJECT") ?? DEFAULT_MAC_PROJECT);
var testDevice = Argument("device", EnvironmentVariable("MAC_TEST_DEVICE") ?? "maccatalyst");
var dotnetRoot = Argument("dotnet-root", EnvironmentVariable("DOTNET_ROOT"));
var targetFramework = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{DotnetVersion}-maccatalyst");
var binlogArg = Argument("binlog", EnvironmentVariable("MAC_TEST_BINLOG") ?? "");
var testApp = Argument("app", EnvironmentVariable("MAC_TEST_APP") ?? "");
var testAppProjectPath = Argument("appproject", EnvironmentVariable("MAC_TEST_APP_PROJECT") ?? DEFAULT_APP_PROJECT);
var testResultsPath = Argument("results", EnvironmentVariable("MAC_TEST_RESULTS") ?? GetTestResultsDirectory().FullPath);
var runtimeIdentifier = Argument("rid", EnvironmentVariable("MAC_RUNTIME_IDENTIFIER") ?? GetDefaultRuntimeIdentifier());
var deviceCleanupEnabled = Argument("cleanup", true);
var useCoreClr = Argument("coreclr", false);

// Directory setup
var binlogDirectory = DetermineBinlogDirectory(projectPath, binlogArg).FullPath;
var dotnetToolPath = GetDotnetToolPath();
LogSetupInfo(dotnetToolPath);

Information($"Project File: {projectPath}");
Information($"Build Binary Log (binlog): {binlogDirectory}");
Information($"Build Configuration: {configuration}");
Information($"Build Runtime Identifier: {runtimeIdentifier}");
Information($"Build Target Framework: {targetFramework}");
Information($"Test Device: {testDevice}");
Information($"Test Results Path: {testResultsPath}");
Information("Use CoreCLR: {0}", useCoreClr);

Task("buildOnly")
	.WithCriteria(!string.IsNullOrEmpty(projectPath))
	.Does(() =>
	{
		ExecuteBuild(projectPath, binlogDirectory, configuration, runtimeIdentifier, targetFramework, dotnetToolPath, useCoreClr);
	});

Task("testOnly")
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
	.Does(() =>
	{
		ExecuteBuildUITestApp(testAppProjectPath, binlogDirectory, configuration, targetFramework, runtimeIdentifier, dotnetToolPath);
	});

Task("uitest")
	.Does(() =>
	{
		ExecuteUITests(projectPath, testAppProjectPath, testDevice, testResultsPath, binlogDirectory, configuration, targetFramework, runtimeIdentifier, dotnetToolPath);
	});

RunTarget(TARGET);

void ExecuteBuild(string project, string binDir, string config, string rid, string tfm, string toolPath, bool useCoreClr)
{
	var projectName = System.IO.Path.GetFileNameWithoutExtension(project);
	var monoRuntime = useCoreClr ? "coreclr" : "mono";
	var binlog = $"{binDir}/{projectName}-{config}-{monoRuntime}-catalyst.binlog";
	DotNetBuild(project, new DotNetBuildSettings
	{
		Configuration = config,
		Framework = tfm,
		MSBuildSettings = new DotNetMSBuildSettings
		{
			MaxCpuCount = 0
		},
		ToolPath = toolPath,
		ArgumentCustomization = args =>
		{
			args
				.Append("/p:BuildIpa=true")
				.Append($"/p:RuntimeIdentifier={rid}")
				.Append($"/bl:{binlog}");

			if (isUsingCoreClr)
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

	var testApp = GetTestApplications(project, device, config, tfm, rid).FirstOrDefault();
	Information($"Testing App: {testApp}");

	RunMacAndiOSTests(project, device, resultsDir, config, tfm, rid, toolPath, projectPath, (category) =>
	{
		return new DotNetToolSettings
		{
			DiagnosticOutput = true,
			ToolPath = toolPath,
			ArgumentCustomization = args => 
				args.Append($"run xharness apple test --app=\"{testApp}\" --targets=\"{device}\" --output-directory=\"{resultsDir}\" " + 
					" --verbosity=\"Debug\" " + 
					$"--set-env=\"TestFilter={category}\" ")
		};
	});
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

	// Launch the app so it can be found by the test runner
	StartProcess("chmod", $"+x {testApp}/Contents/MacOS/Controls.TestCases.HostApp");

	var p = new System.Diagnostics.Process();
	p.StartInfo.UseShellExecute = true;
	p.StartInfo.FileName = "open";
	p.StartInfo.Arguments = testApp;
	p.Start();

	Information("Build UITests project {0}", project);

	var name = System.IO.Path.GetFileNameWithoutExtension(project);
	var binlog = $"{binDir}/{name}-{config}-mac.binlog";
	var resultsFileName = SanitizeTestResultsFilename($"{name}-{config}-catalyst-{testFilter}");
	var appiumLog = $"{binDir}/appium_mac_{resultsFileName}.log";

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

void ExecuteBuildUITestApp(string appProject, string binDir, string config, string tfm, string rid, string toolPath)
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
			.Append($"/bl:{binlog}")
	});

	Information("UI Test app build completed.");
}

// Helper methods

string GetDefaultRuntimeIdentifier()
{
	var architecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;
	return architecture == System.Runtime.InteropServices.Architecture.Arm64 ? "maccatalyst-arm64" : "maccatalyst-x64";
}