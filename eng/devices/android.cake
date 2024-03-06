#addin nuget:?package=Cake.Android.Adb&version=3.2.0
#addin nuget:?package=Cake.Android.AvdManager&version=2.2.0
#load "../cake/helpers.cake"
#load "../cake/dotnet.cake"
#load "./devices-shared.cake"

#tool nuget:?package=NUnit.ConsoleRunner&version=3.16.3

const int defaultVersion = 30;
const string dotnetVersion = "net8.0";

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("ANDROID_TEST_PROJECT") ?? DEFAULT_PROJECT);
string TEST_DEVICE = Argument("device", EnvironmentVariable("ANDROID_TEST_DEVICE") ?? $"android-emulator-64_{defaultVersion}");
string DEVICE_SKIN = Argument("skin", EnvironmentVariable("ANDROID_TEST_SKIN") ?? "Nexus 5X");

// optional
var USE_DOTNET = Argument("dotnet", true);
var DOTNET_ROOT = Argument("dotnet-root", EnvironmentVariable("DOTNET_ROOT"));
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? (USE_DOTNET ? $"{dotnetVersion}-android" : ""));
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("ANDROID_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("ANDROID_TEST_APP") ?? "");
FilePath TEST_APP_PROJECT = Argument("appproject", EnvironmentVariable("ANDROID_TEST_APP_PROJECT") ?? DEFAULT_APP_PROJECT);
var TEST_APP_PACKAGE_NAME = Argument("package", EnvironmentVariable("ANDROID_TEST_APP_PACKAGE_NAME") ?? "");
var TEST_APP_INSTRUMENTATION = Argument("instrumentation", EnvironmentVariable("ANDROID_TEST_APP_INSTRUMENTATION") ?? "");
var TEST_RESULTS = Argument("results", EnvironmentVariable("ANDROID_TEST_RESULTS") ?? "");

string TEST_WHERE = Argument("where", EnvironmentVariable("NUNIT_TEST_WHERE") ?? $"");

string DEVICE_UDID = "";
string DEVICE_VERSION = "";
string DEVICE_NAME = "";
string DEVICE_OS = "";

// other
string CONFIGURATION = Argument("configuration", "Debug");
string TEST_FRAMEWORK = "net472";
string ANDROID_AVD = "DEVICE_TESTS_EMULATOR";
string ANDROID_AVD_IMAGE = "";
string DEVICE_ARCH = "";
bool DEVICE_BOOT = Argument("boot", true);
bool DEVICE_BOOT_WAIT = Argument("wait", true);

// set up env
var ANDROID_SDK_ROOT = GetAndroidSDKPath();

SetEnvironmentVariable("PATH", $"{ANDROID_SDK_ROOT}/tools/bin", prepend: true);
SetEnvironmentVariable("PATH", $"{ANDROID_SDK_ROOT}/cmdline-tools/5.0/bin", prepend: true);
SetEnvironmentVariable("PATH", $"{ANDROID_SDK_ROOT}/cmdline-tools/7.0/bin", prepend: true);
SetEnvironmentVariable("PATH", $"{ANDROID_SDK_ROOT}/cmdline-tools/latest/bin", prepend: true);

SetEnvironmentVariable("PATH", $"{ANDROID_SDK_ROOT}/platform-tools", prepend: true);
SetEnvironmentVariable("PATH", $"{ANDROID_SDK_ROOT}/emulator", prepend: true);

Information("Android SDK Root: {0}", ANDROID_SDK_ROOT);
Information("Project File: {0}", PROJECT);
Information("Build Binary Log (binlog): {0}", BINLOG_DIR);
Information("Build Configuration: {0}", CONFIGURATION);

var avdSettings = new AndroidAvdManagerToolSettings { SdkRoot = ANDROID_SDK_ROOT };
var adbSettings = new AdbToolSettings { SdkRoot = ANDROID_SDK_ROOT };
var emuSettings = new AndroidEmulatorToolSettings { SdkRoot = ANDROID_SDK_ROOT };

if (IsCIBuild())
	emuSettings.ArgumentCustomization = args => args.Append("-no-window");

AndroidEmulatorProcess emulatorProcess = null;

Setup(context =>
{
	Information("Test Device: {0}", TEST_DEVICE);

	// determine the device characteristics
	{
		var working = TEST_DEVICE.Trim().ToLower();
		var emulator = true;
		var api = defaultVersion;
		// version
		if (working.IndexOf("_") is int idx && idx > 0) {
			api = int.Parse(working.Substring(idx + 1));
			working = working.Substring(0, idx);
		}
		var parts = working.Split('-');
		// os
		if (parts[0] != "android")
			throw new Exception("Unexpected platform (expected: android) in device: " + TEST_DEVICE);
		// device/emulator
		Information("Create for: {0}", parts[1]);
		if (parts[1] == "device")
			emulator = false;
		else if (parts[1] != "emulator" && parts[1] != "simulator")
			throw new Exception("Unexpected device type (expected: device|emulator) in device: " + TEST_DEVICE);
		// arch/bits
		Information("Host OS System Arch: {0}", System.Runtime.InteropServices.RuntimeInformation.OSArchitecture);
		Information("Host Processor System Arch: {0}", System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture);
		if (parts[2] == "32") {
			if (emulator)
				DEVICE_ARCH = "x86";
			else
				DEVICE_ARCH = "armeabi-v7a";
		} else if (parts[2] == "64") {
			if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
				DEVICE_ARCH = "arm64-v8a";
			else if (emulator)
				DEVICE_ARCH = "x86_64";
			else
				DEVICE_ARCH = "arm64-v8a";
		}
		var sdk = api >= 27 ? "google_apis_playstore" : "google_apis";
		if (api == 27 && DEVICE_ARCH == "x86_64")
			sdk = "default";

		ANDROID_AVD_IMAGE = $"system-images;android-{api};{sdk};{DEVICE_ARCH}";

		Information("Going to run image: {0}", ANDROID_AVD_IMAGE);
		// we are not using a virtual device, so quit
		if (!emulator)
		{
			Information("Not using a virtual device, skipping... and getting devices ");
			
			GetDevices(api.ToString());

			return;
		}
	}

	Information("Test Device ID: {0}", ANDROID_AVD_IMAGE);

	if (DEVICE_BOOT) {
		Information("Trying to boot the emulator...");

		// delete the AVD first, if it exists
		Information("Deleting AVD if exists: {0}...", ANDROID_AVD);	
		try { AndroidAvdDelete(ANDROID_AVD, avdSettings); }
		catch { }

		// create the new AVD
		Information("Creating AVD: {0}...", ANDROID_AVD);
		AndroidAvdCreate(ANDROID_AVD, ANDROID_AVD_IMAGE, DEVICE_SKIN, force: true, settings: avdSettings);

		// start the emulator
		Information("Starting Emulator: {0}...", ANDROID_AVD);
		emulatorProcess = AndroidEmulatorStart(ANDROID_AVD, emuSettings);
	}

	if (IsCIBuild())
	{
		AdbLogcat(new AdbLogcatOptions() { Clear = true });
		AdbShell("logcat -G 16M");
	}
});

Teardown(context =>
{
	// no virtual device was used
	if (emulatorProcess == null || !DEVICE_BOOT || TARGET.ToLower() == "boot")
		return;

	//stop and cleanup the emulator
	Information("AdbEmuKill");
	AdbEmuKill(adbSettings);

	System.Threading.Thread.Sleep(5000);

	// kill the process if it has not already exited
	Information("emulatorProcess.Kill()");
	try { emulatorProcess.Kill(); }
	catch { }

	Information("AndroidAvdDelete");
	// delete the AVD
	try { AndroidAvdDelete(ANDROID_AVD, avdSettings); }
	catch { }
});

Task("Boot");

Task("Build")
	.WithCriteria(!string.IsNullOrEmpty(PROJECT.FullPath))
	.Does(() =>
{
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-android--{DateTime.UtcNow.ToFileTimeUtc()}.binlog";

	if (USE_DOTNET)
	{
		Information($"Build target dotnet root: {DOTNET_ROOT}");
		Information($"Build target set dotnet tool path: {DOTNET_PATH}");
		
		var localDotnetRoot = MakeAbsolute(Directory("../../bin/dotnet/"));
		Information("new dotnet root: {0}", localDotnetRoot);

		DOTNET_ROOT = localDotnetRoot.ToString();

		DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
			Configuration = CONFIGURATION,
			Framework = TARGET_FRAMEWORK,
			MSBuildSettings = new DotNetMSBuildSettings {
				MaxCpuCount = 0
			},
			ToolPath = DOTNET_PATH,
			ArgumentCustomization = args => args
				.Append("/p:EmbedAssembliesIntoApk=true")
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
			c.Properties["ContinuousIntegrationBuild"] = new List<string> { "false" };
			if (!string.IsNullOrEmpty(TARGET_FRAMEWORK))
				c.Properties["TargetFramework"] = new List<string> { TARGET_FRAMEWORK };
			c.Targets.Clear();
			c.Targets.Add("Build");
			c.Targets.Add("SignAndroidPackage");
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
		var binDir = PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TARGET_FRAMEWORK).FullPath;
		Information("BinDir: {0}", binDir);
		var apps = GetFiles(binDir + "/*-Signed.apk");
		if (apps.Any()) {
			TEST_APP = apps.FirstOrDefault().FullPath;
		} else {
			apps = GetFiles(binDir + "/*.apk");
			TEST_APP = apps.First().FullPath;
		}
	}
	if (string.IsNullOrEmpty(TEST_APP_PACKAGE_NAME)) {
		var appFile = (FilePath)TEST_APP;
		appFile = appFile.GetFilenameWithoutExtension();
		TEST_APP_PACKAGE_NAME = appFile.FullPath.Replace("-Signed", "");
	}
	if (string.IsNullOrEmpty(TEST_APP_INSTRUMENTATION)) {
		TEST_APP_INSTRUMENTATION = TEST_APP_PACKAGE_NAME + ".TestInstrumentation";
	}
	if (string.IsNullOrEmpty(TEST_RESULTS)) {
		TEST_RESULTS = TEST_APP + "-results";
	}

	Information("Test App: {0}", TEST_APP);
	Information("Test App Package Name: {0}", TEST_APP_PACKAGE_NAME);
	Information("Test App Instrumentation: {0}", TEST_APP_INSTRUMENTATION);
	Information("Test Results Directory: {0}", TEST_RESULTS);
	
	if (!IsCIBuild())
		CleanDirectories(TEST_RESULTS);
	else
	{
		// Because we retry on CI we don't want to delete the previous failures
		// We want to publish those files for reference
		DeleteFiles(Directory(TEST_RESULTS).Path.Combine("*.*").FullPath);
	}

	if (DEVICE_BOOT_WAIT) {
		Information("Waiting for the emulator to finish booting...");

		// wait for it to finish booting (10 mins)
		var waited = 0;
		var total = 60 * 10;
		while (AdbShell("getprop sys.boot_completed", adbSettings).FirstOrDefault() != "1") {
			System.Threading.Thread.Sleep(1000);
			Information("Wating {0}/{1} seconds for the emulator to boot up.", waited, total);
			if (waited++ > total)
				break;
		}
		Information("Waited {0} seconds for the emulator to boot up.", waited);
	}

	Information("Setting the ADB properties...");
	var lines = AdbShell("setprop debug.mono.log default,mono_log_level=debug,mono_log_mask=all", adbSettings);
	Information("{0}", string.Join("\n", lines));
	lines = AdbShell("getprop debug.mono.log", adbSettings);
	Information("{0}", string.Join("\n", lines));

	var settings = new DotNetToolSettings {
		DiagnosticOutput = true,
		ArgumentCustomization = args=>args.Append("run xharness android test " +
			$"--app=\"{TEST_APP}\" " +
			$"--package-name=\"{TEST_APP_PACKAGE_NAME}\" " +
			$"--instrumentation=\"{TEST_APP_INSTRUMENTATION}\" " +
			$"--device-arch=\"{DEVICE_ARCH}\" " +
			$"--output-directory=\"{TEST_RESULTS}\" " +
			$"--verbosity=\"Debug\" ")
	};

	bool testsFailed = true;
	try {
		DotNetTool("tool", settings);
		testsFailed = false;
	} finally {

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

	InstallApk(TEST_APP, TEST_APP_PACKAGE_NAME, TEST_RESULTS);
	
	//we need to build tests first to pass ExtraDefineConstants
	Information("Build UITests project {0}", PROJECT.FullPath);
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-android-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
	DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
			Configuration = CONFIGURATION,
			ArgumentCustomization = args => args
				.Append("/p:ExtraDefineConstants=ANDROID")
				.Append("/bl:" + binlog),
			ToolPath = DOTNET_PATH,
	});
	
	SetEnvironmentVariable("APPIUM_LOG_FILE", $"{BINLOG_DIR}/appium_android.log");

	Information("Run UITests project {0}", PROJECT.FullPath);	
	
	int numOfRetries = 0;

	if (IsCIBuild())
		numOfRetries = 1;

	for(int retryCount = 0; retryCount <= numOfRetries; retryCount++)
	{
		try
		{
			RunTestWithLocalDotNet(PROJECT.FullPath, CONFIGURATION,	noBuild: true, resultsFileNameWithoutExtension: $"{name}-{CONFIGURATION}-android");
			break;
		}
		catch(Exception)
		{
			if (retryCount == numOfRetries)
			{
				WriteLogCat();
				throw;
			}
		}
	}
});

Task("cg-uitest")
	.Does(() =>
{
	SetupAppPackageNameAndResult();
	
	CleanDirectories(TEST_RESULTS);

	InstallApk(TEST_APP, TEST_APP_PACKAGE_NAME, TEST_RESULTS);

	//set env var for the app path for Xamarin.UITest setup
	SetEnvironmentVariable("APP_APK", $"{TEST_APP}");

	// build the test library
	var binDir = PROJECT.GetDirectory().Combine("bin").Combine(CONFIGURATION + "/" + TEST_FRAMEWORK).FullPath;
	Information("BinDir: {0}", binDir);
	var name = System.IO.Path.GetFileNameWithoutExtension(PROJECT.FullPath);
	var binlog = $"{binDir}/{name}-{CONFIGURATION}-android-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
	Information("Build UITests project {0}", PROJECT.FullPath);
	DotNetBuild(PROJECT.FullPath, new DotNetBuildSettings {
			Configuration = CONFIGURATION,
			ArgumentCustomization = args => args
				.Append("/bl:" + binlog),
			ToolPath = DOTNET_PATH,
	});
	
	var testLibDllPath = $"{binDir}/Microsoft.Maui.Controls.Android.UITests.dll";
	Information("Run UITests lib {0}", testLibDllPath);
	var nunitSettings = new NUnit3Settings { 
		Configuration = CONFIGURATION,
		OutputFile = $"{TEST_RESULTS}/android/run_uitests_output-{DateTime.UtcNow.ToFileTimeUtc()}.log",
		Work = $"{TEST_RESULTS}/android/"
	};

	if(!string.IsNullOrEmpty(TEST_WHERE))
	{
		Information("Add Where filter to NUnit {0}", TEST_WHERE);
		nunitSettings.Where = TEST_WHERE;
	}

	RunTestsNunit(testLibDllPath, nunitSettings);

	// When all tests are inconclusive the run does not fail, check if this is the case and fail the pipeline so we get notified
	FailRunOnOnlyInconclusiveTests(System.IO.Path.Combine(nunitSettings.Work.FullPath, "TestResult.xml"));
});


Task("logcat")
	.Does(() =>
{
	WriteLogCat();
});

RunTarget(TARGET);

void WriteLogCat(string filename = null)
{
	if (string.IsNullOrWhiteSpace(filename))
	{
		var timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
		filename = $"logcat_{TARGET}_{timeStamp}.log";
	}
		
	EnsureDirectoryExists(GetLogDirectory());
	// I tried AdbLogcat here but the pipeline kept reporting "cannot create file"
	var location = $"{GetLogDirectory()}/{filename}";
	Information("Writing logcat to {0}", location);

	var processSettings = new ProcessSettings();
	processSettings.RedirectStandardOutput = true;
	processSettings.RedirectStandardError = true;
	var adb = $"{ANDROID_SDK_ROOT}/platform-tools/adb";

	Information("Running: {0} logcat -d", adb);
	processSettings.Arguments = $"logcat -d";
    using (var fs = new System.IO.FileStream(location, System.IO.FileMode.Create)) 
	using (var sw = new StreamWriter(fs))
	{
		processSettings.RedirectedStandardOutputHandler = (output) => {
			sw.WriteLine(output);
			return output;
		};

		var process = StartProcess($"{adb}", processSettings); 
		Information("exit code {0}", process);
	}

	Information("Logcat written to {0}", location);
}

void SetupAppPackageNameAndResult()
{
   if (string.IsNullOrEmpty(TEST_APP)) {
   		if (string.IsNullOrEmpty(TEST_APP_PROJECT.FullPath))
			throw new Exception("If no app was specified, an app must be provided.");
		
		var binFolder = TEST_APP_PROJECT.GetDirectory().Combine("bin");
		Information("Test app bin folder {0}", binFolder);
		var binDir = binFolder.Combine($"{CONFIGURATION}/{TARGET_FRAMEWORK}").FullPath;
		var apps = GetFiles(binDir + "/*-Signed.apk");
		if (apps.Any()) {
			TEST_APP = apps.FirstOrDefault().FullPath;
		} else {
			apps = GetFiles(binDir + "/*.apk");
			if (apps.Any()) {
				TEST_APP = apps.First().FullPath;
			}
			else {
				Error("Error: Couldn't find .apk file");
				throw new Exception("Error: Couldn't find .apk file");
			}
		}
	}
	if (string.IsNullOrEmpty(TEST_APP_PACKAGE_NAME)) {
		var appFile = (FilePath)TEST_APP;
		appFile = appFile.GetFilenameWithoutExtension();
		TEST_APP_PACKAGE_NAME = appFile.FullPath.Replace("-Signed", "");
	}
	if (string.IsNullOrEmpty(TEST_APP_INSTRUMENTATION)) {
		TEST_APP_INSTRUMENTATION = TEST_APP_PACKAGE_NAME + ".TestInstrumentation";
	}
	if (string.IsNullOrEmpty(TEST_RESULTS)) {
		TEST_RESULTS = TEST_APP + "-results";
	}

	Information($"Build target dotnet root: {DOTNET_ROOT}");
	Information($"Build target set dotnet tool path: {DOTNET_PATH}");
		
	var localDotnetRoot = MakeAbsolute(Directory("../../bin/dotnet/"));
	Information("new dotnet root: {0}", localDotnetRoot);

	DOTNET_ROOT = localDotnetRoot.ToString();

	Information("Test App: {0}", TEST_APP);
	Information("Test App Package Name: {0}", TEST_APP_PACKAGE_NAME);
	Information("Test App Instrumentation: {0}", TEST_APP_INSTRUMENTATION);
	Information("Test Results Directory: {0}", TEST_RESULTS);
	Information("Test project: {0}", PROJECT);
}

void InstallApk(string testApp, string testAppPackageName, string testResultsDirectory)
{
	var installadbSettings = new AdbToolSettings { SdkRoot = ANDROID_SDK_ROOT };
	if(!string.IsNullOrEmpty(DEVICE_UDID))
	{
		installadbSettings.Serial = DEVICE_UDID;
	}
	if (DEVICE_BOOT_WAIT) {
		Information("Waiting for the emulator to finish booting...");

		// wait for it to finish booting (10 mins)
		var waited = 0;
		var total = 60 * 10;
		while (AdbShell("getprop sys.boot_completed", installadbSettings).FirstOrDefault() != "1") {
			System.Threading.Thread.Sleep(1000);
			Information("Wating {0}/{1} seconds for the emulator to boot up.", waited, total);
			if (waited++ > total)
				break;
		}
		Information("Waited {0} seconds for the emulator to boot up.", waited);
	}

	Information("Setting the ADB properties...");
	var lines = AdbShell("setprop debug.mono.log default,mono_log_level=debug,mono_log_mask=all", installadbSettings);
	Information("{0}", string.Join("\n", lines));
	lines = AdbShell("getprop debug.mono.log", installadbSettings);
	Information("{0}", string.Join("\n", lines));

	//install apk on the emulator or device
	Information("Install with xharness: {0}", testApp);
	var settings = new DotNetToolSettings {
		DiagnosticOutput = true,
		ArgumentCustomization =  args =>
						{
							args.Append("run xharness android install " +
										$"--app=\"{testApp}\" " +
										$"--package-name=\"{testAppPackageName}\" " +
										$"--output-directory=\"{testResultsDirectory}\" " +
										$"--verbosity=\"Debug\" ");
							
							//if we specify a device we need to pass it to xharness
							if(!string.IsNullOrEmpty(DEVICE_UDID))
							{
								args.Append($"--device-id=\"{DEVICE_UDID}\" ");
							}

							return args;
						}
	};
	
	Information("The platform version to run tests:");
	SetEnvironmentVariable("DEVICE_SKIN", DEVICE_SKIN);

	if(!string.IsNullOrEmpty(DEVICE_UDID))
	{
		SetEnvironmentVariable("DEVICE_UDID", DEVICE_UDID);
		//this needs to be translated to android 10/11 for appium
		var realApi ="";
		if(DEVICE_VERSION == "33")
		{
			realApi = "13";
		}
		if(DEVICE_VERSION == "32" || DEVICE_VERSION == "31")
		{
			realApi = "12";
		}
		else if(DEVICE_VERSION == "30")
		{
			realApi = "11";
		}
		SetEnvironmentVariable("PLATFORM_VERSION", realApi);
	}

	DotNetTool("tool", settings);
}

void GetDevices(string version)
{
	var deviceUdid = "";
	var deviceName = "";
	var deviceVersion = "";
	var deviceOS = "";

	var devices = AdbDevices(adbSettings);
	foreach	(var device in devices)
	{
		deviceUdid = device.Serial;
		deviceName = device.Model;
		deviceOS = device.Product;

		deviceVersion = AdbShell($"getprop ro.build.version.sdk ", new AdbToolSettings { SdkRoot = ANDROID_SDK_ROOT, Serial = deviceUdid }).FirstOrDefault();
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

	//this will fail if there are no devices with this api attached
	var settings = new DotNetToolSettings {
			DiagnosticOutput = true,
			ArgumentCustomization = args=>args.Append("run xharness android device " +
			$"--api-version=\"{version}\" " )
	};
	DotNetTool("tool", settings);
}
