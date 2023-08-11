#load "../cake/helpers.cake"
#load "../cake/dotnet.cake"

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

string TARGET = Argument("target", "Test");

const string defaultVersion = "10.0.19041";
const string dotnetVersion = "net7.0";

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("WINDOWS_TEST_PROJECT") ?? "");
// Not used for Windows. TODO Use this for packaged vs unpackaged?
string TEST_DEVICE = Argument("device", EnvironmentVariable("WINDOWS_TEST_DEVICE") ?? $"");
// Package ID of the WinUI Application
var PACKAGEID = Argument("packageid", EnvironmentVariable("WINDOWS_TEST_PACKAGE_ID") ?? $"");

// optional
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{dotnetVersion}-windows{defaultVersion}");
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("WINDOWS_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("WINDOWS_TEST_APP") ?? "");
var DEVICETEST_APP = Argument("devicetestapp", EnvironmentVariable("WINDOWS_DEVICETEST_APP") ?? "");
FilePath TEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("WINDOWS_TEST_APP_PROJECT") ?? PROJECT);
FilePath DEVICETEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("WINDOWS_DEVICETEST_APP_PROJECT") ?? PROJECT);
var TEST_RESULTS = Argument("results", EnvironmentVariable("WINDOWS_TEST_RESULTS") ?? "");
string CONFIGURATION = Argument("configuration", "Debug");

var windowsVersion = Argument("apiversion", EnvironmentVariable("WINDOWS_PLATFORM_VERSION") ?? defaultVersion);

// other
string PLATFORM = "windows";
string DOTNET_PLATFORM = $"win10-x64";
bool DEVICE_CLEANUP = Argument("cleanup", true);
string certificateThumbprint = "";

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

Task("GenerateMsixCert")
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
		var req = new CertificateRequest("CN=" + certCN, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

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

Task("Build")
	.IsDependentOn("GenerateMsixCert")
	.WithCriteria(!string.IsNullOrEmpty(PROJECT.FullPath))
	.WithCriteria(!string.IsNullOrEmpty(PACKAGEID))
	.Does(() =>
{
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-windows.binlog";

	SetDotNetEnvironmentVariables(DOTNET_PATH);

	var dd = MakeAbsolute(Directory("../../bin/dotnet/"));
	Information("DOTNET_PATH: {0}", dd);

	var toolPath = $"{dd}/dotnet.exe";

	Information("toolPath: {0}", toolPath);

	Information("Building and publishing device test app");

	// Build the app in publish mode
	// Using the certificate thumbprint for the cert we just created
	var s = new DotNetPublishSettings();
	s.ToolPath = toolPath;
	s.Configuration = CONFIGURATION;
	s.Framework = TARGET_FRAMEWORK;
	s.MSBuildSettings = new DotNetMSBuildSettings();
	s.MSBuildSettings.Properties.Add("RuntimeIdentifierOverride", new List<string> { "win10-x64" });
	s.MSBuildSettings.Properties.Add("PackageCertificateThumbprint", new List<string> { certificateThumbprint });
	s.MSBuildSettings.Properties.Add("AppxPackageSigningEnabled", new List<string> { "True" });
	s.MSBuildSettings.Properties.Add("SelfContained", new List<string> { "True" });

	DotNetPublish(PROJECT.FullPath, s);
});

Task("Test")
	.IsDependentOn("Build")
	.IsDependentOn("SetupTestPaths")
	.Does(() =>
{
	CleanDirectories(TEST_RESULTS);

	Information("Cleaned directories");

	// Try to uninstall the app if it exists from before
	uninstallPS();

	Information("Uninstalled previously deployed app");
	var projectDir = PROJECT.GetDirectory();
	var cerPath = GetFiles(projectDir.FullPath + "/**/AppPackages/*/*.cer").First();
	var msixPath = GetFiles(projectDir.FullPath + "/**/AppPackages/*/*.msix").First();

	var testResultsFile = MakeAbsolute((DirectoryPath)TEST_RESULTS).FullPath.Replace("/", "\\") + $"\\TestResults-{PACKAGEID.Replace(".", "_")}.xml";

	Information($"Found MSIX, installing: {msixPath}");
	Information($"Test Results File: {testResultsFile}");

	if (FileExists(testResultsFile))
	{
		DeleteFile(testResultsFile);
	}

	// Install dependencies
	var dependencies = GetFiles(projectDir.FullPath + "/**/AppPackages/**/Dependencies/x64/*.msix");
	foreach (var dep in dependencies) {
		Information("Installing Dependency MSIX: {0}", dep);
		StartProcess("powershell", "Add-AppxPackage -Path \"" + MakeAbsolute(dep).FullPath + "\"");
	}

	// Install the DeviceTests app
	StartProcess("powershell", "Add-AppxPackage -Path \"" + MakeAbsolute(msixPath).FullPath + "\"");

	var startArgs = "Start-Process shell:AppsFolder\\$((Get-AppxPackage -Name \"" + PACKAGEID + "\").PackageFamilyName)!App -Args \"" + testResultsFile + "\"";

	Information(startArgs);

	// Start the DeviceTests app
	StartProcess("powershell", startArgs);

	var waited = 0;
	while (!FileExists(testResultsFile)) {
		System.Threading.Thread.Sleep(1000);
		waited++;

		Information($"Waiting {waited} second(s) for tests to finish...");
		if (waited >= 120)
			break;
	}

	if(!FileExists(testResultsFile))
	{
		throw new Exception($"Test results file not found after {waited} seconds, process might have crashed or not completed yet.");
	}

	Information($"Tests Finished");
});


Task("SetupTestPaths")
	.Does(() => {

	// UI Tests
	if (string.IsNullOrEmpty(TEST_APP) )
	{
		if (string.IsNullOrEmpty(TEST_APP_PROJECT.FullPath))
		{
			throw new Exception("If no app was specified, an app must be provided.");
		}

		var binDir = TEST_APP_PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + $"{dotnetVersion}-windows{windowsVersion}").Combine(DOTNET_PLATFORM).FullPath;
		Information("BinDir: {0}", binDir);
		var apps = GetFiles(binDir + "/*.exe").Where(c => !c.FullPath.EndsWith("createdump.exe"));
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

	var dd = MakeAbsolute(Directory("../../bin/dotnet/"));
	Information("DOTNET_PATH: {0}", dd);

	var toolPath = $"{dd}/dotnet.exe";

	Information("toolPath: {0}", toolPath);

	SetDotNetEnvironmentVariables(dd.FullPath);

	DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
			Configuration = CONFIGURATION,
			ToolPath = toolPath,
			ArgumentCustomization = args => args
				.Append("/p:ExtraDefineConstants=WINTEST")
				.Append("/bl:" + binlog)
				.Append("/maxcpucount:1")
				//.Append("/tl")
	});

	SetEnvironmentVariable("WINDOWS_APP_PATH", TEST_APP);
	SetEnvironmentVariable("APPIUM_LOG_FILE", $"{BINLOG_DIR}/appium_windows.log");

	Information("Run UITests project {0}",PROJECT.FullPath);
	RunTestWithLocalDotNet(PROJECT.FullPath, CONFIGURATION, toolPath, noBuild: true);
});

RunTarget(TARGET);
