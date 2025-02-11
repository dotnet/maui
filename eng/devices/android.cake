#addin nuget:?package=Cake.Android.Adb&version=3.2.0
#addin nuget:?package=Cake.Android.AvdManager&version=2.2.0
#load "./uitests-shared.cake"

const int DefaultApiLevel = 30;

Information("Local Dotnet: {0}", localDotnet);

if (EnvironmentVariable("JAVA_HOME") == null)
{
	throw new Exception("JAVA_HOME environment variable isn't set. Set it to your JDK installation (e.g. \"C:\\Program Files (x86)\\Android\\openjdk\\jdk-17.0.8.101-hotspot\\bin\").");
}

string DEFAULT_ANDROID_PROJECT = "../../src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj";
var projectPath = Argument("project", EnvironmentVariable("ANDROID_TEST_PROJECT") ?? DEFAULT_ANDROID_PROJECT);
var testDevice = Argument("device", EnvironmentVariable("ANDROID_TEST_DEVICE") ?? $"android-emulator-64_{DefaultApiLevel}");
var targetFramework = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? $"{DotnetVersion}-android");
var binlogArg = Argument("binlog", EnvironmentVariable("ANDROID_TEST_BINLOG") ?? "");
var testApp = Argument("app", EnvironmentVariable("ANDROID_TEST_APP") ?? "");
var testAppProjectPath = Argument("appproject", EnvironmentVariable("ANDROID_TEST_APP_PROJECT") ?? DEFAULT_APP_PROJECT);
var testAppPackageName = Argument("package", EnvironmentVariable("ANDROID_TEST_APP_PACKAGE_NAME") ?? "");
var testAppInstrumentation = Argument("instrumentation", EnvironmentVariable("ANDROID_TEST_APP_INSTRUMENTATION") ?? "");
var testResultsPath = Argument("results", EnvironmentVariable("ANDROID_TEST_RESULTS") ?? GetTestResultsDirectory()?.FullPath);
var deviceCleanupEnabled = Argument("cleanup", true);

// Device details
var deviceSkin = Argument("skin", EnvironmentVariable("ANDROID_TEST_SKIN") ?? "Nexus 5X");
var androidAvd = "DEVICE_TESTS_EMULATOR";
var androidAvdImage = "";
var deviceArch = "";
var androidVersion = Argument("apiversion", EnvironmentVariable("ANDROID_PLATFORM_VERSION") ?? DefaultApiLevel.ToString());

// Directory setup
var binlogDirectory = DetermineBinlogDirectory(projectPath, binlogArg)?.FullPath;

string DEVICE_UDID = "";
string DEVICE_VERSION = "";
string DEVICE_NAME = "";
string DEVICE_OS = "";

// Android SDK setup
Information("ANDROID_SDK_ROOT: {0}", EnvironmentVariable("ANDROID_SDK_ROOT"));
Information("ANDROID_HOME: {0}", EnvironmentVariable("ANDROID_HOME"));

var androidSdkRoot = GetAndroidSDKPath();

SetAndroidEnvironmentVariables(androidSdkRoot);

Information("Android SDK Root: {0}", androidSdkRoot);
Information("Project File: {0}", projectPath);
Information("Build Binary Log (binlog): {0}", binlogDirectory);
Information("Build Configuration: {0}", configuration);
Information("Build Target Framework: {0}", targetFramework);

var avdSettings = new AndroidAvdManagerToolSettings { SdkRoot = androidSdkRoot };
var adbSettings = new AdbToolSettings { SdkRoot = androidSdkRoot };
var emuSettings = new AndroidEmulatorToolSettings { SdkRoot = androidSdkRoot };
emuSettings = AdjustEmulatorSettingsForCI(emuSettings);

AndroidEmulatorProcess emulatorProcess = null;

var dotnetToolPath = GetDotnetToolPath();

Teardown(context =>
{
	// For the uitest-prepare target, just leave the virtual device running
	if (! string.Equals(TARGET, "uitest-prepare", StringComparison.OrdinalIgnoreCase))
	{
		CleanUpVirtualDevice(emulatorProcess, avdSettings);
	}

});

Task("Setup")
	.Does(async () =>
	{
		LogSetupInfo(dotnetToolPath);

		PerformCleanupIfNeeded(deviceCleanupEnabled);

		DetermineDeviceCharacteristics(testDevice, DefaultApiLevel);

		// The Emulator Start command seems to hang sometimes so let's only give it two minutes to complete
		await HandleVirtualDevice(emuSettings, avdSettings, androidAvd, androidAvdImage, deviceSkin, deviceBoot);
	});

Task("boot")
	.IsDependentOn("Setup");

Task("build")
	.IsDependentOn("Setup")
	.WithCriteria(!string.IsNullOrEmpty(projectPath))
	.Does(() =>
	{
		ExecuteBuild(projectPath, testDevice, binlogDirectory, configuration, targetFramework, dotnetToolPath);
	});

Task("test")
	.IsDependentOn("Build")
	.Does(() =>
	{
		ExecuteTests(projectPath, testDevice, testApp, testAppPackageName, testResultsPath, configuration, targetFramework, adbSettings, dotnetToolPath, deviceBootWait, testAppInstrumentation);
	});

Task("uitest-build")
	.IsDependentOn("Setup")
	.IsDependentOn("dotnet-buildtasks")
	.Does(() =>
	{
		ExecuteBuildUITestApp(testAppProjectPath, testDevice, binlogDirectory, configuration, targetFramework, "", dotnetToolPath);
	});

Task("uitest-prepare")
	.IsDependentOn("Setup")
	.Does(() =>
	{
		ExecutePrepareUITests(projectPath, testAppProjectPath, testAppPackageName, testDevice, testResultsPath, binlogDirectory, configuration, targetFramework, "", androidVersion, dotnetToolPath, testAppInstrumentation);
	});

Task("uitest")
	.IsDependentOn("uitest-prepare")
	.Does(() =>
	{
		ExecuteUITests(projectPath, testAppProjectPath, testAppPackageName, testDevice, testResultsPath, binlogDirectory, configuration, targetFramework, "", androidVersion, dotnetToolPath, testAppInstrumentation);
	});

Task("logcat")
	.IsDependentOn("Setup")
	.Does(() =>
{
	WriteLogCat();
});

RunTarget(TARGET);

void ExecuteBuild(string project, string device, string binDir, string config, string tfm, string toolPath)
{
	var projectName = System.IO.Path.GetFileNameWithoutExtension(project);
	var binlog = $"{binDir}/{projectName}-{config}-ios.binlog";

	DotNetBuild(project, new DotNetBuildSettings
	{
		Configuration = config,
		Framework = tfm,
		MSBuildSettings = new DotNetMSBuildSettings
		{
			MaxCpuCount = 0
		},
		ToolPath = toolPath,
		ArgumentCustomization = args => args
			.Append("/p:EmbedAssembliesIntoApk=true")
			.Append("/bl:" + binlog)
	});
}

void ExecuteTests(string project, string device, string appPath, string appPackageName, string resultsDir, string config, string tfm, AdbToolSettings adbSettings, string toolPath, bool waitDevice, string instrumentation)
{
	CleanResults(resultsDir);

	var testApp = GetTestApplications(project, device, config, tfm, "").FirstOrDefault();

	if (string.IsNullOrEmpty(appPackageName))
	{
		var appFile = new FilePath(testApp);
		appFile = appFile.GetFilenameWithoutExtension();
		appPackageName = appFile.FullPath.Replace("-Signed", "");
	}
	if (string.IsNullOrEmpty(instrumentation))
	{
		instrumentation = appPackageName + ".TestInstrumentation";
	}

	Information("Test App: {0}", testApp);
	Information("Test App Package Name: {0}", appPackageName);
	Information("Test Results Directory: {0}", resultsDir);

	if (waitDevice)
	{
		Information("Waiting for the emulator to finish booting...");

		// wait for it to finish booting (10 mins)
		var waited = 0;
		var total = 60 * 10;
		while (AdbShell("getprop sys.boot_completed", adbSettings).FirstOrDefault() != "1")
		{
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

	var settings = new DotNetToolSettings
	{
		DiagnosticOutput = true,
		ArgumentCustomization = args => args.Append("run xharness android test " +
			$"--app=\"{testApp}\" " +
			$"--package-name=\"{appPackageName}\" " +
			$"--instrumentation=\"{instrumentation}\" " +
			$"--device-arch=\"{deviceArch}\" " +
			$"--output-directory=\"{resultsDir}\" " +
			$"--verbosity=\"Debug\" ")
	};

	bool testsFailed = true;
	try
	{
		DotNetTool("tool", settings);
		testsFailed = false;
	}
	finally
	{
		if (testsFailed)
		{
			// uncomment if you want to copy the test app to the results directory for any reason
			// CopyFile(testApp, new DirectoryPath(resultsDir).CombineWithFilePath(new FilePath(testApp).GetFilename()));
		}

		HandleTestResults(resultsDir, testsFailed, false);
	}
	
	Information("Testing completed.");
}

void ExecuteBuildUITestApp(string appProject, string device, string binDir, string config, string tfm, string rid, string toolPath)
{
	Information($"Building UI Test app: {appProject}");
	var projectName = System.IO.Path.GetFileNameWithoutExtension(appProject);
	var binlog = $"{binDir}/{projectName}-{config}-ios.binlog";

	DotNetBuild(appProject, new DotNetBuildSettings
	{
		Configuration = config,
		Framework = tfm,
		ToolPath = toolPath,
		ArgumentCustomization = args =>
		{
			args
			.Append("/p:EmbedAssembliesIntoApk=true")
			.Append("/bl:" + binlog)
			.Append("/tl");

			return args;
		}
	});

	Information("UI Test app build completed.");
}

void ExecutePrepareUITests(string project, string app, string appPackageName, string device, string resultsDir, string binDir, string config, string tfm, string rid, string ver, string toolPath, string instrumentation)
{
	string platform = "android";
	Information("Preparing UI Tests...");

	var testApp = GetTestApplications(app, device, config, tfm, "").FirstOrDefault();

	if (string.IsNullOrEmpty(testApp))
	{
		throw new Exception("UI Test application path not specified.");
	}
	if (string.IsNullOrEmpty(appPackageName))
	{
		var appFile = new FilePath(testApp);
		appFile = appFile.GetFilenameWithoutExtension();
		appPackageName = appFile.FullPath.Replace("-Signed", "");
	}
	if (string.IsNullOrEmpty(instrumentation))
	{
		instrumentation = appPackageName + ".TestInstrumentation";
	}

	Information("Test App: {0}", testApp);
	Information("Test App Package Name: {0}", appPackageName);
	Information("Test Results Directory: {0}", resultsDir);
	Information($"Testing Device: {device}");
	Information($"Testing App Project: {app}");
	Information($"Testing App: {testApp}");
	Information($"Results Directory: {resultsDir}");

	InstallApk(testApp, appPackageName, resultsDir, deviceSkin);
}

void ExecuteUITests(string project, string app, string appPackageName, string device, string resultsDir, string binDir, string config, string tfm, string rid, string ver, string toolPath, string instrumentation)
{
	string platform = "android";
	Information("Build UITests project {0}", project);

	var name = System.IO.Path.GetFileNameWithoutExtension(project);
	var binlog = $"{binDir}/{name}-{config}-{platform}.binlog";
	var resultsFileName = SanitizeTestResultsFilename($"{name}-{config}-{platform}-{testFilter}");
	var appiumLog = $"{binDir}/appium_{platform}_{resultsFileName}.log";

	DotNetBuild(project, new DotNetBuildSettings
	{
		Configuration = config,
		ToolPath = toolPath,
		ArgumentCustomization = args => args
			.Append("/p:ExtraDefineConstants=ANDROID")
			.Append("/bl:" + binlog)
	});

	SetEnvironmentVariable("APPIUM_LOG_FILE", appiumLog);

	int numOfRetries = 0;

	if (IsCIBuild())
		numOfRetries = 1;

	Information("Run UITests  project {0}", project);
	for(int retryCount = 0; retryCount <= numOfRetries; retryCount++)
	{
		try
		{
			Information("Retry UITests run Count: {0}", retryCount);
			RunTestWithLocalDotNet(project, config, pathDotnet: toolPath, noBuild: true, resultsFileNameWithoutExtension: resultsFileName);
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
	Information("UI Tests completed.");
}

// Helper methods

void PerformCleanupIfNeeded(bool cleanupEnabled)
{
	if (cleanupEnabled)
	{


	}
}

void SetAndroidEnvironmentVariables(string sdkRoot)
{
	// Set up Android SDK environment variables and paths
	string[] paths = { 
		$"{sdkRoot}/cmdline-tools/latest/bin",
		$"{sdkRoot}/cmdline-tools/17.0/bin",
        $"{sdkRoot}/platform-tools", 
		$"{sdkRoot}/emulator" };
		
	foreach (var path in paths)
	{
		SetEnvironmentVariable("PATH", path, prepend: true);
	}

	foreach (var folder in GetDirectories($"{sdkRoot}/cmdline-tools/*"))
	{
		Information("Found cmdline-tools folders: {0}", folder.FullPath);
	}
}

AndroidEmulatorToolSettings AdjustEmulatorSettingsForCI(AndroidEmulatorToolSettings settings)
{
	if (IsCIBuild())
	{
		settings.ArgumentCustomization = args => args.Append("-no-window");
	}
	return settings;
}

void DetermineDeviceCharacteristics(string deviceDescriptor, int defaultApiLevel)
{
	var working = deviceDescriptor.Trim().ToLower();
	var emulator = true;
	var api = defaultApiLevel;
	// version
	if (working.IndexOf("_") is int idx && idx > 0)
	{
		api = int.Parse(working.Substring(idx + 1));
		working = working.Substring(0, idx);
	}
	var parts = working.Split('-');
	// os
	if (parts[0] != "android")
		throw new Exception("Unexpected platform (expected: android) in device: " + deviceDescriptor);
	// device/emulator
	Information("Create for: {0}", parts[1]);
	if (parts[1] == "device")
		emulator = false;
	else if (parts[1] != "emulator" && parts[1] != "simulator")
		throw new Exception("Unexpected device type (expected: device|emulator) in device: " + deviceDescriptor);
	// arch/bits
	Information("Host OS System Arch: {0}", System.Runtime.InteropServices.RuntimeInformation.OSArchitecture);
	Information("Host Processor System Arch: {0}", System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture);
	if (parts[2] == "32")
	{
		if (emulator)
			deviceArch = "x86";
		else
			deviceArch = "armeabi-v7a";
	}
	else if (parts[2] == "64")
	{
		if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
			deviceArch = "arm64-v8a";
		else if (emulator)
			deviceArch = "x86_64";
		else
			deviceArch = "arm64-v8a";
	}
	var sdk = api >= 27 ? "google_apis_playstore" : "google_apis";
	if (api == 27 && deviceArch == "x86_64")
		sdk = "default";

	androidAvdImage = $"system-images;android-{api};{sdk};{deviceArch}";

	Information("Going to run image: {0}", androidAvdImage);
	// we are not using a virtual device, so quit
	if (!emulator)
	{
		Information("Not using a virtual device, skipping... and getting devices ");

		GetDevices(api.ToString(), dotnetToolPath);

		return;
	}
}

async Task HandleVirtualDevice(AndroidEmulatorToolSettings emuSettings, AndroidAvdManagerToolSettings avdSettings, string avdName, string avdImage, string avdSkin, bool boot)
{
	try
	{
		// The Emulator Start command seems to hang sometimes so let's only give it two minutes to complete
		await System.Threading.Tasks.Task.Run(() =>
		{
			Information("Test Device ID: {0}", avdImage);

			if (boot)
			{
				Information("Trying to boot the emulator...");

				// delete the AVD first, if it exists
				Information("Deleting AVD if exists: {0}...", avdName);
				try { AndroidAvdDelete(avdName, avdSettings); }
				catch { }

				// create the new AVD
				Information("Creating AVD: {0} ({1})...", avdName, avdImage);
				AndroidAvdCreate(avdName, avdImage, avdSkin, force: true, settings: avdSettings);

				// start the emulator
				Information("Starting Emulator: {0}...", avdName);
				emulatorProcess = AndroidEmulatorStart(avdName, emuSettings);
			}
		}).WaitAsync(TimeSpan.FromMinutes(2));
	}
	catch(TimeoutException)
	{
		Error("Failed to start the Android Emulator.");
		throw;
	}

	try
	{
		await System.Threading.Tasks.Task.Run(() =>
		{
			if (IsCIBuild() && emulatorProcess is not null)
			{
				Information("Setting Logcat Values");
				AdbLogcat(new AdbLogcatOptions() { Clear = true });
				AdbShell("logcat -G 16M");
				Information("Finished Setting Logcat Values");
			}
		}).WaitAsync(TimeSpan.FromMinutes(1));
	}
	catch(TimeoutException)
	{
		Warning("Failed to Issue Logcat Commands to the Android Emulator.");
	}
}

void CleanUpVirtualDevice(AndroidEmulatorProcess emulatorProcess, AndroidAvdManagerToolSettings avdSettings)
{
	// no virtual device was used
	if (emulatorProcess == null || !deviceBoot || targetBoot)
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
	try { AndroidAvdDelete(androidAvd, avdSettings); }
	catch { }
}

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
	var adb = $"{androidSdkRoot}/platform-tools/adb";

	Information("Running: {0} logcat -d", adb);
	processSettings.Arguments = $"logcat -d";
	using (var fs = new System.IO.FileStream(location, System.IO.FileMode.Create))
	using (var sw = new StreamWriter(fs))
	{
		processSettings.RedirectedStandardOutputHandler = (output) =>
		{
			sw.WriteLine(output);
			return output;
		};

		var process = StartProcess($"{adb}", processSettings);
		Information("exit code {0}", process);
	}

	Information("Logcat written to {0}", location);
}

void InstallApk(string testApp, string testAppPackageName, string testResultsDirectory, string skin)
{
	var installadbSettings = new AdbToolSettings { SdkRoot = androidSdkRoot };
	if (!string.IsNullOrEmpty(DEVICE_UDID))
	{
		installadbSettings.Serial = DEVICE_UDID;
	}
	if (deviceBootWait)
	{
		Information("Waiting for the emulator to finish booting...");

		// wait for it to finish booting (10 mins)
		var waited = 0;
		var total = 60 * 10;
		while (AdbShell("getprop sys.boot_completed", installadbSettings).FirstOrDefault() != "1")
		{
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
	var settings = new DotNetToolSettings
	{
		DiagnosticOutput = true,
		ArgumentCustomization = args =>
						{
							args.Append("run xharness android install " +
										$"--app=\"{testApp}\" " +
										$"--package-name=\"{testAppPackageName}\" " +
										$"--output-directory=\"{testResultsDirectory}\" " +
										$"--verbosity=\"Debug\" ");

							//if we specify a device we need to pass it to xharness
							if (!string.IsNullOrEmpty(DEVICE_UDID))
							{
								args.Append($"--device-id=\"{DEVICE_UDID}\" ");
							}

							return args;
						}
	};

	Information("The platform version to run tests:");
	SetEnvironmentVariable("DEVICE_SKIN", skin);

	if (!string.IsNullOrEmpty(DEVICE_UDID))
	{
		SetEnvironmentVariable("DEVICE_UDID", DEVICE_UDID);
		//this needs to be translated to android 10/11 for appium
		var realApi = "";
		if (DEVICE_VERSION == "34ß")
		{
			realApi = "14";
		}
		if (DEVICE_VERSION == "33")
		{
			realApi = "13";
		}
		if (DEVICE_VERSION == "32" || DEVICE_VERSION == "31")
		{
			realApi = "12";
		}
		else if (DEVICE_VERSION == "30")
		{
			realApi = "11";
		}
		SetEnvironmentVariable("PLATFORM_VERSION", realApi);
	}

	DotNetTool("tool", settings);
}

void GetDevices(string version, string toolPath)
{
	var deviceUdid = "";
	var deviceName = "";
	var deviceVersion = "";
	var deviceOS = "";

	var devices = AdbDevices(adbSettings);
	foreach (var device in devices)
	{
		deviceUdid = device.Serial;
		deviceName = device.Model;
		deviceOS = device.Product;

		deviceVersion = AdbShell($"getprop ro.build.version.sdk ", new AdbToolSettings { SdkRoot = androidSdkRoot, Serial = deviceUdid }).FirstOrDefault();
		Information("DeviceName:{0} udid:{1} version:{2} os:{3}", deviceName, deviceUdid, deviceVersion, deviceOS);

		if (version.Contains(deviceVersion.Split(".")[0]))
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
	var settings = new DotNetToolSettings
	{
		DiagnosticOutput = true,
		ToolPath = toolPath,
		ArgumentCustomization = args => args.Append("run xharness android device " +
		$"--api-version=\"{version}\" ")
	};
	DotNetTool("tool", settings);
}
