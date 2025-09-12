#load "./uitests-shared.cake"

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

const string defaultVersion = "10.0.19041.0";

// Test category filtering:
// Use --test-filter to run only specific categories. Examples:
//   --test-filter="Button"              # Run only Button category
//   --test-filter="Button,Label"        # Run Button and Label categories
//   --test-filter="Button,Layout"       # Run any category containing "Button" or "Layout"
// Categories are matched case-insensitively using substring matching

// required
string DEFAULT_WINDOWS_PROJECT = "../../src/Controls/tests/TestCases.WinUI.Tests/Controls.TestCases.WinUI.Tests.csproj";
FilePath PROJECT = Argument("project", EnvironmentVariable("WINDOWS_TEST_PROJECT") ?? DEFAULT_WINDOWS_PROJECT);
// Used for Windows to differentiate between packaged and unpackaged
string TEST_DEVICE = Argument("device", EnvironmentVariable("WINDOWS_TEST_DEVICE") ?? $"");
// Package ID of the WinUI Application
var PACKAGEID = Argument("packageid", EnvironmentVariable("WINDOWS_TEST_PACKAGE_ID") ?? $"");

// optional
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var DOTNET_ROOT = Argument("dotnet-root", EnvironmentVariable("DOTNET_ROOT"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{DotnetVersion}-windows{defaultVersion}");
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("WINDOWS_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("WINDOWS_TEST_APP") ?? "");
var DEVICETEST_APP = Argument("devicetestapp", EnvironmentVariable("WINDOWS_DEVICETEST_APP") ?? "");
FilePath TEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("WINDOWS_TEST_APP_PROJECT") ?? (!string.IsNullOrEmpty(DEFAULT_APP_PROJECT) ? DEFAULT_APP_PROJECT : PROJECT));
FilePath DEVICETEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("WINDOWS_DEVICETEST_APP_PROJECT") ?? PROJECT);
var TEST_RESULTS = Argument("results", EnvironmentVariable("WINDOWS_TEST_RESULTS") ?? "");
string CONFIGURATION = Argument("configuration", "Debug");

var windowsVersion = Argument("apiversion", EnvironmentVariable("WINDOWS_PLATFORM_VERSION") ?? defaultVersion);

var dotnetToolPath = GetDotnetToolPath();
LogSetupInfo(dotnetToolPath);

// other
string PLATFORM = "windows";
string DOTNET_PLATFORM = $"win10-x64";
bool DEVICE_CLEANUP = Argument("cleanup", true);
string certificateThumbprint = "";
bool isPackagedTestRun = TEST_DEVICE.ToLower().Equals("packaged");
bool isControlsProjectTestRun = PROJECT.FullPath.EndsWith("Controls.DeviceTests.csproj");

// Certificate Common Name to use/generate (eg: CN=DotNetMauiTests)
var certCN = Argument("commonname", "DotNetMAUITests");

// Uninstall the deployed app
var uninstallPS = new Action(() =>
{
	try {
		StartProcess("powershell",
			"$app = Get-AppxPackage -Name " + PACKAGEID + "; if ($app) { Remove-AppxPackage -Package $app.PackageFullName }");
	} catch { }
});

Information("Project File: {0}", PROJECT);
Information("Application ID: {0}", PACKAGEID);
Information("Build Binary Log (binlog): {0}", BINLOG_DIR);
Information("Build Platform: {0}", PLATFORM);
Information("Build Configuration: {0}", CONFIGURATION);

Information("Target Framework: {0}", TARGET_FRAMEWORK);
Information("Windows version: {0}", windowsVersion);

Task("GenerateMsixCert")
	.WithCriteria(isPackagedTestRun)
	.Does(() =>
{
	// We need the key to be in LocalMachine -> TrustedPeople to install the msix signed with the key
	var localTrustedPeopleStore = new X509Store("TrustedPeople", StoreLocation.LocalMachine);
	localTrustedPeopleStore.Open(OpenFlags.ReadWrite);

	// We need to have the key also in CurrentUser -> My so that the msix can be built and signed
	// with the key by passing the key's thumbprint to the build
	var currentUserMyStore = new X509Store("My", StoreLocation.CurrentUser);
	currentUserMyStore.Open(OpenFlags.ReadWrite);
	certificateThumbprint = localTrustedPeopleStore.Certificates.FirstOrDefault(c => c.Subject.Contains(certCN))?.Thumbprint;
	Information("Cert thumbprint: " + certificateThumbprint ?? "null");

	if (string.IsNullOrEmpty(certificateThumbprint))
	{
		Information("Generating cert");
		var rsa = RSA.Create();
		var req = new CertificateRequest("CN=" + certCN, rsa, System.Security.Cryptography.HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

		req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
		{
			new Oid
			{
				Value = "1.3.6.1.5.5.7.3.3",
				FriendlyName = "Code Signing"
			}
		}, false));

		req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
		req.CertificateExtensions.Add(
			new X509KeyUsageExtension(
				X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation,
				false));

		req.CertificateExtensions.Add(
						new X509SubjectKeyIdentifierExtension(req.PublicKey, false));
		var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

		if (OperatingSystem.IsWindows())
		{
			cert.FriendlyName = certCN;
		}

		var tmpCert = new X509Certificate2(cert.Export(X509ContentType.Pfx), "", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
		certificateThumbprint = tmpCert.Thumbprint;
		localTrustedPeopleStore.Add(tmpCert);
		currentUserMyStore.Add(tmpCert);
	}

	localTrustedPeopleStore.Close();
	currentUserMyStore.Close();
});

Task("buildOnly")
	.IsDependentOn("GenerateMsixCert")
	.WithCriteria(!string.IsNullOrEmpty(PROJECT.FullPath))
	.Does(() =>
{
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-windows.binlog";

    var localDotnetRoot = MakeAbsolute(Directory("../../.dotnet/"));
	Information("new dotnet root: {0}", localDotnetRoot);
	DOTNET_ROOT = localDotnetRoot.ToString();
	SetDotNetEnvironmentVariables(DOTNET_ROOT);
	
    Information("toolPath: {0}", dotnetToolPath);

	Information("Building and publishing device test app");

	// Build the app in publish mode
	// Using the certificate thumbprint for the cert we just created
	var s = new DotNetPublishSettings();
	s.ToolPath = dotnetToolPath;
	s.Configuration = CONFIGURATION;
	s.Framework = TARGET_FRAMEWORK;
	s.MSBuildSettings = new DotNetMSBuildSettings()
	{
		ArgumentCustomization = args => args.Append("/bl:" + binlog),
	};
	s.MSBuildSettings.Properties.Add("RuntimeIdentifierOverride", new List<string> { "win10-x64" });
	
	var launchSettingsNeedle = "Project";
	var launchSettingsReplacement = "MsixPackage";

	if (!isPackagedTestRun)
	{
		launchSettingsNeedle = "MsixPackage";
		launchSettingsReplacement = "Project";
	}

	if (isPackagedTestRun)
	{
		// Apply correct build properties for packaged builds
		s.MSBuildSettings.Properties.Add("PackageCertificateThumbprint", new List<string> { certificateThumbprint });
		s.MSBuildSettings.Properties.Add("AppxPackageSigningEnabled", new List<string> { "True" });
		s.MSBuildSettings.Properties.Add("SelfContained", new List<string> { "True" });
		s.MSBuildSettings.Properties.Add("ExtraDefineConstants", new List<string> { "PACKAGED" });
	}
	else
	{
		// Apply correct build properties for unpackaged builds
		s.MSBuildSettings.Properties.Add("WindowsPackageType", new List<string> { "None" });
		s.MSBuildSettings.Properties.Add("ExtraDefineConstants", new List<string> { "UNPACKAGED" });
	}

	// Set correct launchSettings.json setting for packaged/unpackaged
	// Get launchSettings.json Path
	var launchSettingsPath = PROJECT.GetDirectory();
	launchSettingsPath = launchSettingsPath.Combine("Properties").Combine("launchSettings.json");

	// Replace value in launchSettings.json
	var launchSettingsContents = System.IO.File.ReadAllText(launchSettingsPath.FullPath);
	launchSettingsContents = launchSettingsContents.Replace($"\"commandName\": \"{launchSettingsNeedle}\",", $"\"commandName\": \"{launchSettingsReplacement}\",");
	System.IO.File.WriteAllText(launchSettingsPath.FullPath, launchSettingsContents);

	DotNetPublish(PROJECT.FullPath, s);
});

// Helper function to wait for a specific test result file with timeout
Func<string, string, bool> WaitForCategoryTestResult = (string expectedFile, string categoryName) => {
	var timeoutInSeconds = 480; // 8 minutes per category
	var waited = 0;
	
	Information($"Waiting for test results file: {expectedFile}");
	
	while (!FileExists(expectedFile) && waited < timeoutInSeconds) {
		System.Threading.Thread.Sleep(1000);
		waited++;
		
		if (waited % 10 == 0) { // Log every 10 seconds
			Information($"Still waiting for {categoryName} test results... ({waited}s)");
		}
	}
	
	if (FileExists(expectedFile)) {
		Information($"✓ Found test results for {categoryName} after {waited} seconds");
		return true;
	} else {
		Warning($"✗ Timeout waiting for {categoryName} test results after {waited} seconds");
		return false;
	}
};

// Helper function to filter categories based on testFilter parameter
Func<string[], string[]> FilterCategories = (string[] allCategories) => {
	if (string.IsNullOrWhiteSpace(testFilter)) {
		Information("No test filter specified, running all categories");
		return allCategories;
	}
	
	var filters = testFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
		.Select(f => f.Trim())
		.ToArray();
	
	var filteredCategories = allCategories.Where(category => 
		filters.Any(filter => 
			category.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
		)
	).ToArray();
	
	Information($"Test filter '{testFilter}' applied:");
	Information($"  - Total categories available: {allCategories.Length}");
	Information($"  - Filtered categories to run: {filteredCategories.Length}");
	
	if (filteredCategories.Length > 0) {
		Information("  - Categories that will run:");
		foreach (var category in filteredCategories) {
			Information($"    * {category}");
		}
	} else {
		Warning($"No categories matched the filter '{testFilter}'. Available categories:");
		foreach (var category in allCategories) {
			Information($"    * {category}");
		}
	}
	
	return filteredCategories;
};

Task("testOnly")
	.IsDependentOn("SetupTestPaths")
	.Does(() =>
{
	CleanDirectories(TEST_RESULTS);
	Information("Cleaned directories");

	var testResultsPath = MakeAbsolute((DirectoryPath)TEST_RESULTS).FullPath.Replace("/", "\\");
	var testResultsFile = testResultsPath + $"\\TestResults-{PACKAGEID.Replace(".", "_")}";

	// For Controls project tests, each category gets its own result file, so don't append testFilter here
	// For non-Controls tests, append testFilter to the single result file
	if (!isControlsProjectTestRun && !string.IsNullOrWhiteSpace(testFilter))
	{
		testResultsFile += SanitizeTestResultsFilename($"-{testFilter}");
	}

	testResultsFile += ".xml";

	var testsToRunFile = MakeAbsolute((DirectoryPath)TEST_RESULTS).FullPath.Replace("/", "\\") + $"\\devicetestcategories.txt";

	Information($"Test Results File: {testResultsFile}");
	Information($"Tests To Run File: {testsToRunFile}");

	if (FileExists(testResultsFile))
	{
		DeleteFile(testResultsFile);
	}

	if (FileExists(testsToRunFile))
	{
		DeleteFile(testsToRunFile);
	}

	// Track completed and failed test categories
	var completedCategories = new List<string>();
	var failedCategories = new List<string>();

	if (isPackagedTestRun)
	{
		Information("isPackagedTestRuns");
		// Try to uninstall the app if it exists from before
		uninstallPS();

		Information("Uninstalled previously deployed app");

		var projectDir = PROJECT.GetDirectory();
		Information($"Get Cert {projectDir}");

		var cerPaths = GetFiles(projectDir.FullPath + "/**/AppPackages/*/*.cer");
		var msixPaths = GetFiles(projectDir.FullPath + "/**/AppPackages/*/*.msix");
		if(cerPaths.Count() == 0 || msixPaths.Count() == 0)
		{
			var arcadeBin = new DirectoryPath("../../artifacts/bin/");

			if(PROJECT.FullPath.Contains("Controls.DeviceTests"))
			{
				projectDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Controls.DeviceTests/" + CONFIGURATION + "/"));
			}
			if(PROJECT.FullPath.Contains("Core.DeviceTests"))
			{
				projectDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Core.DeviceTests/" + CONFIGURATION + "/"));
			}
			if(PROJECT.FullPath.Contains("Graphics.DeviceTests"))
			{
				projectDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Graphics.DeviceTests/" + CONFIGURATION + "/"));
			}
			if(PROJECT.FullPath.Contains("MauiBlazorWebView.DeviceTests"))
			{
				projectDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/MauiBlazorWebView.DeviceTests/" + CONFIGURATION + "/"));
			}	
			if(PROJECT.FullPath.Contains("Essentials.DeviceTests"))
			{
				projectDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Essentials.DeviceTests/" + CONFIGURATION + "/"));
			}
			cerPaths = GetFiles(projectDir.FullPath + "/**/AppPackages/*/*.cer");
		    msixPaths = GetFiles(projectDir.FullPath + "/**/AppPackages/*/*.msix");
			if(cerPaths.Count() == 0 || msixPaths.Count() == 0)
			{
				Error("Error: Couldn't find .cer or .msix file");
				throw new Exception("Error: Couldn't find .cer or .msix file");
			}
		}

		var msixPath = msixPaths.First();
		var cerPath = cerPaths.First();
		Information($"Found MSIX, installing: {msixPath}");

		// Install dependencies
		var dependencies = GetFiles(projectDir.FullPath + "/**/AppPackages/**/Dependencies/x64/*.msix");
		foreach (var dep in dependencies) {
			Information("Installing Dependency MSIX: {0}", dep);
			try {
				StartProcess("powershell", "Add-AppxPackage -Path \"" + MakeAbsolute(dep).FullPath + "\"");
			} catch { 
				Warning($"Failed to install dependency: {dep}");
			}
		}

		// Install the DeviceTests app
		StartProcess("powershell", "Add-AppxPackage -Path \"" + MakeAbsolute(msixPath).FullPath + "\"");

		if (isControlsProjectTestRun)
		{
			// Start the app once, this will trigger the discovery of the test categories
			var startArgsInitial = "Start-Process shell:AppsFolder\\$((Get-AppxPackage -Name \"" + PACKAGEID + "\").PackageFamilyName)!App -ArgumentList \"" + testResultsFile + "\", \"-1\"";
			StartProcess("powershell", startArgsInitial);

			Information($"Waiting 10 seconds for category discovery to finish...");
			System.Threading.Thread.Sleep(10000);

			if (!FileExists(testsToRunFile)) {
				throw new Exception("Test categories file was not created during discovery phase");
			}

			var expectedCategories = System.IO.File.ReadAllLines(testsToRunFile);
			var filteredCategories = FilterCategories(expectedCategories);
			
			if (filteredCategories.Length == 0) {
				Information("No categories to run after applying filter. Skipping test execution.");
				return;
			}
			
			Information($"Will run {filteredCategories.Length} filtered categories out of {expectedCategories.Length} total");

			for (int i = 0; i < filteredCategories.Length; i++)
			{
				var categoryName = filteredCategories[i];
				// Find the original index of this category in the expected categories
				var originalIndex = Array.IndexOf(expectedCategories, categoryName);
				var expectedResultFile = testResultsPath + $"\\TestResults-{PACKAGEID.Replace(".", "_")}_{categoryName}.xml";
				
				Information($"Running category {originalIndex}: {categoryName} (filtered {i + 1}/{filteredCategories.Length})");
				
				var startArgs = "Start-Process shell:AppsFolder\\$((Get-AppxPackage -Name \"" + PACKAGEID + "\").PackageFamilyName)!App -ArgumentList \"" + testResultsFile + "\", \"" + originalIndex + "\"";
				Information(startArgs);

				// Start the DeviceTests app for packaged
				StartProcess("powershell", startArgs);

				// Wait for this specific category's results
				if (WaitForCategoryTestResult(expectedResultFile, categoryName)) {
					completedCategories.Add(categoryName);
					Information($"✓ Category {categoryName} completed successfully");
				} else {
					failedCategories.Add(categoryName);
					Error($"✗ Category {categoryName} failed or timed out");
				}
			}
		}
		else
		{
			var startArgs = "Start-Process shell:AppsFolder\\$((Get-AppxPackage -Name \"" + PACKAGEID + "\").PackageFamilyName)!App -ArgumentList \"" + testResultsFile + "\"";

			Information(startArgs);

			// Start the DeviceTests app for packaged
			StartProcess("powershell", startArgs);
			
			// Wait for the single test result file
			if (WaitForCategoryTestResult(testResultsFile, "All Tests")) {
				completedCategories.Add("All Tests");
			} else {
				failedCategories.Add("All Tests");
			}
		}
	}
	else
	{
		if (isControlsProjectTestRun)
		{
			// Start the app once, this will trigger the discovery of the test categories
			StartProcess(TEST_APP, testResultsFile + " -1");
			
			Information($"Waiting 10 seconds for category discovery to finish...");
			System.Threading.Thread.Sleep(10000);

			if (!FileExists(testsToRunFile)) {
				throw new Exception("Test categories file was not created during discovery phase");
			}

			var expectedCategories = System.IO.File.ReadAllLines(testsToRunFile);
			var filteredCategories = FilterCategories(expectedCategories);
			
			if (filteredCategories.Length == 0) {
				Information("No categories to run after applying filter. Skipping test execution.");
				return;
			}

			Information($"Will run {filteredCategories.Length} filtered categories out of {expectedCategories.Length} total");

			for (int i = 0; i < filteredCategories.Length; i++)
			{
				var categoryName = filteredCategories[i];
				// Find the original index of this category in the expected categories
				var originalIndex = Array.IndexOf(expectedCategories, categoryName);
				var expectedResultFile = testResultsPath + $"\\TestResults-{PACKAGEID.Replace(".", "_")}_{categoryName}.xml";
				
				Information($"Running category {originalIndex}: {categoryName} (filtered {i + 1}/{filteredCategories.Length})");
				
				// Start the DeviceTests app for unpackaged
				StartProcess(TEST_APP, testResultsFile + " " + originalIndex);

				// Wait for this specific category's results
				if (WaitForCategoryTestResult(expectedResultFile, categoryName)) {
					completedCategories.Add(categoryName);
					Information($"✓ Category {categoryName} completed successfully");
				} else {
					failedCategories.Add(categoryName);
					Error($"✗ Category {categoryName} failed or timed out");
				}
			}
		}
		else
		{
			StartProcess(TEST_APP, testResultsFile);
			
			// Wait for the single test result file
			if (WaitForCategoryTestResult(testResultsFile, "All Tests")) {
				completedCategories.Add("All Tests");
			} else {
				failedCategories.Add("All Tests");
			}
		}
	}

	// Final validation and reporting
	Information($"=== Test Execution Summary ===");
	Information($"Completed categories: {completedCategories.Count}");
	Information($"Failed categories: {failedCategories.Count}");

	if (completedCategories.Any()) {
		Information("✓ Successfully completed categories:");
		foreach (var category in completedCategories) {
			Information($"  - {category}");
		}
	}

	if (failedCategories.Any()) {
		Error("✗ Failed or timed out categories:");
		foreach (var category in failedCategories) {
			Error($"  - {category}");
		}
	}

	// Clean up the test categories file
	if (FileExists(testsToRunFile))
	{
		DeleteFile(testsToRunFile);
	}

	// Check if we have any test result files at all
	var actualResultFiles = System.IO.Directory.GetFiles(testResultsPath, "TestResults-*.xml");
	if (actualResultFiles.Length == 0)
	{
		throw new Exception($"No test result files found. All test processes may have crashed or failed to start.");
	}

	// If we're running Controls tests, validate we have results for expected categories
	if (isControlsProjectTestRun && failedCategories.Any())
	{
		throw new Exception($"Some test categories failed to complete: {string.Join(", ", failedCategories)}. Expected {completedCategories.Count + failedCategories.Count} categories, but {failedCategories.Count} failed.");
	}

	// Check for test failures in the result files
	foreach(var file in actualResultFiles)
	{
		var failed = XmlPeek(file, "/assemblies/assembly[@failed > 0 or @errors > 0]/@failed");
		if (!string.IsNullOrEmpty(failed)) {
			throw new Exception($"At least {failed} test(s) failed in {System.IO.Path.GetFileName(file)}.");
		}
	}

	Information($"✓ All test executions completed successfully with {actualResultFiles.Length} result file(s)");
});

Task("build")
	.IsDependentOn("buildOnly");

Task("test")
	.IsDependentOn("buildOnly")
	.IsDependentOn("testOnly");

Task("buildAndTest")
	.IsDependentOn("buildOnly")
	.IsDependentOn("testOnly");

Task("SetupTestPaths")
	.Does(() => {

	// UI Tests
	if (string.IsNullOrEmpty(TEST_APP) )
	{
		if (string.IsNullOrEmpty(TEST_APP_PROJECT.FullPath))
		{
			throw new Exception("If no app was specified, an app must be provided.");
		}

		var winVersion = $"{DotnetVersion}-windows{windowsVersion}";
		var binDir = TEST_APP_PROJECT.GetDirectory().Combine("Controls.TestCases.HostApp").Combine(CONFIGURATION + "/" + winVersion).Combine(DOTNET_PLATFORM);
		Information("BinDir: {0}", binDir);
		var apps = GetFiles(binDir + "/*.exe").Where(c => !c.FullPath.EndsWith("createdump.exe"));
		if (apps.Count() == 0) 
		{
			var arcadeBin = new DirectoryPath("../../artifacts/bin/");
			if(TEST_APP_PROJECT.FullPath.Contains("Controls.TestCases.HostApp"))
			{
				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Controls.TestCases.HostApp/" + CONFIGURATION + "/" + winVersion).Combine(DOTNET_PLATFORM));
			}

			if(PROJECT.FullPath.Contains("Controls.DeviceTests"))
			{
				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Controls.DeviceTests/" + CONFIGURATION + "/" + winVersion).Combine(DOTNET_PLATFORM));
			}
			if(PROJECT.FullPath.Contains("Core.DeviceTests"))
			{
				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Core.DeviceTests/" + CONFIGURATION + "/" + winVersion).Combine(DOTNET_PLATFORM));
			}
			if(PROJECT.FullPath.Contains("Graphics.DeviceTests"))
			{
				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Graphics.DeviceTests/" + CONFIGURATION + "/" + winVersion).Combine(DOTNET_PLATFORM));
			}
			if(PROJECT.FullPath.Contains("MauiBlazorWebView.DeviceTests"))
			{
				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/MauiBlazorWebView.DeviceTests/" + CONFIGURATION + "/" + winVersion).Combine(DOTNET_PLATFORM));
			}
			if(PROJECT.FullPath.Contains("Essentials.DeviceTests"))
			{
				binDir = MakeAbsolute(new DirectoryPath(arcadeBin + "/Essentials.DeviceTests/" + CONFIGURATION + "/" + winVersion).Combine(DOTNET_PLATFORM));
			}

			Information("Looking for .exe in arcade binDir {0}", binDir);
			apps = GetFiles(binDir + "/*.exe").Where(c => !c.FullPath.EndsWith("createdump.exe"));
			if(apps.Count() == 0 )
			{
				Error("Error: Couldn't find .exe file");
				throw new Exception("Error: Couldn't find .exe file");
			}
		}
		TEST_APP = apps.First().FullPath;
	}

	if (string.IsNullOrEmpty(TEST_RESULTS))
	{
		TEST_RESULTS = TEST_APP + "-results";
	}

	// Device Tests
	if (string.IsNullOrEmpty(DEVICETEST_APP) )
	{
		if (string.IsNullOrEmpty(DEVICETEST_APP_PROJECT.FullPath))
		{
			throw new Exception("If no app was specified, an app must be provided.");
		}

		DEVICETEST_APP = DEVICETEST_APP_PROJECT.GetFilenameWithoutExtension().ToString();
	}

	if (string.IsNullOrEmpty(TEST_RESULTS))
	{
		TEST_RESULTS = DEVICETEST_APP + "-results";
	}

	CreateDirectory(TEST_RESULTS);

	Information("Test Device: {0}", TEST_DEVICE);
	Information("UITest App: {0}", TEST_APP);
	Information("DeviceTest App: {0}", DEVICETEST_APP);
	Information("Test Results Directory: {0}", TEST_RESULTS);
});

Task("uitest")
	.IsDependentOn("SetupTestPaths")
	.Does(() =>
{
	CleanDirectories(TEST_RESULTS);

	Information("Build UITests project {0}",PROJECT.FullPath);
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-windows.binlog";

	Information("old dotnet root: {0}", DOTNET_ROOT);
	Information("old dotnet path: {0}", DOTNET_PATH);

	var buildSettings = new DotNetBuildSettings {
			Configuration = CONFIGURATION,
			ArgumentCustomization = args => args
				.Append("/p:ExtraDefineConstants=WINTEST")
				.Append("/bl:" + binlog)
				.Append("/maxcpucount:1")
				//.Append("/tl")
	};

	string? localToolPath = null;

	if (localDotnet)
	{
		var localDotnetRoot = MakeAbsolute(Directory("../../.dotnet/"));
		Information("new dotnet root: {0}", localDotnetRoot);

		DOTNET_ROOT = localDotnetRoot.ToString();

		localToolPath = $"{localDotnetRoot}/dotnet.exe";

		Information("new dotnet toolPath: {0}", localToolPath);

		SetDotNetEnvironmentVariables(DOTNET_ROOT);

		buildSettings.ToolPath = localToolPath;
	}

	DotNetBuild(PROJECT.FullPath, buildSettings);

	SetEnvironmentVariable("WINDOWS_APP_PATH", TEST_APP);
	SetEnvironmentVariable("APPIUM_LOG_FILE", $"{BINLOG_DIR}/appium_windows_{name}.log");

	Information("Run UITests project {0}",PROJECT.FullPath);
	RunTestWithLocalDotNet(PROJECT.FullPath, CONFIGURATION, localToolPath, noBuild: true, resultsFileNameWithoutExtension: $"{name}-{CONFIGURATION}-windows");
});

RunTarget(TARGET);
