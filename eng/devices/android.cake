#addin nuget:?package=Cake.Android.Adb&version=3.2.0
#addin nuget:?package=Cake.Android.AvdManager&version=2.2.0
#load "./uitests-shared.cake"

const int DefaultApiLevel = 30;

const int EmulatorStartProcessTimeoutSeconds = 1 * 60;
const int EmulatorBootTimeoutSeconds = 10 * 60;
const int EmulatorKillTimeoutSeconds = 1 * 60;
const int AdbCommandTimeoutSeconds = 30;

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
var useCoreClr = Argument("coreclr", false);

// Device details
var deviceSkin = Argument("skin", EnvironmentVariable("ANDROID_TEST_SKIN") ?? "Nexus 5X");
var androidAvd = "";
var androidAvdImage = "";
var deviceArch = "";
var androidVersion = Argument("apiversion", EnvironmentVariable("ANDROID_PLATFORM_VERSION") ?? DefaultApiLevel.ToString());
var headless = Argument<bool>("headless", EnvironmentVariable<bool>("HEADLESS", false));

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
Information("Use CoreCLR: {0}", useCoreClr);

var avdSettings = new AndroidAvdManagerToolSettings { SdkRoot = androidSdkRoot };
var adbSettings = new AdbToolSettings { SdkRoot = androidSdkRoot };
var emuSettings = new AndroidEmulatorToolSettings { SdkRoot = androidSdkRoot };
emuSettings = AdjustEmulatorSettingsForCI(emuSettings);

AndroidEmulatorProcess emulatorProcess = null;

var dotnetToolPath = GetDotnetToolPath();
LogSetupInfo(dotnetToolPath);

Teardown(context =>
{
	// For the uitest-prepare target, just leave the virtual device running
	if (!string.Equals(TARGET, "uitest-prepare", StringComparison.OrdinalIgnoreCase))
	{
		CleanUpVirtualDevice(emulatorProcess, avdSettings);
	}
});

Task("connectToDevice")
	.Does(async () =>
	{
		DetermineDeviceCharacteristics(testDevice, DefaultApiLevel);

		// The Emulator Start command seems to hang sometimes so let's only give it two minutes to complete
		await HandleVirtualDevice(emuSettings, avdSettings, androidAvd, androidAvdImage, deviceSkin, deviceBoot);
	});

Task("boot")
	.IsDependentOn("connectToDevice");

Task("buildOnly")
	.WithCriteria(!string.IsNullOrEmpty(projectPath))
	.Does(() =>
	{
		ExecuteBuild(projectPath, testDevice, binlogDirectory, configuration, targetFramework, dotnetToolPath, useCoreClr);
	});

Task("testOnly")
	.IsDependentOn("connectToDevice")
	.WithCriteria(!string.IsNullOrEmpty(projectPath))
	.Does(() =>
	{
		ExecuteTests(projectPath, testDevice, testAppPackageName, testResultsPath, configuration, targetFramework, adbSettings, dotnetToolPath, deviceBootWait, testAppInstrumentation);
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
	.IsDependentOn("dotnet-buildtasks")
	.Does(() =>
	{
		ExecuteBuildUITestApp(testAppProjectPath, testDevice, binlogDirectory, configuration, targetFramework, "", dotnetToolPath);
	});

Task("uitest-prepare")
	.IsDependentOn("connectToDevice")
	.Does(() =>
	{
		ExecutePrepareUITests(projectPath, testAppProjectPath, testAppPackageName, testDevice, testResultsPath, binlogDirectory, configuration, targetFramework, "", androidVersion, dotnetToolPath, testAppInstrumentation, headless);
	});

Task("uitest")
	.IsDependentOn("uitest-prepare")
	.Does(() =>
	{
		ExecuteUITests(projectPath, testAppProjectPath, testAppPackageName, testDevice, testResultsPath, binlogDirectory, configuration, targetFramework, "", androidVersion, dotnetToolPath, testAppInstrumentation);
	});

Task("logcat")
	.IsDependentOn("connectToDevice")
	.Does(() =>
{
	WriteLogCat();
});

RunTarget(TARGET);

void ExecuteBuild(string project, string device, string binDir, string config, string tfm, string toolPath, bool useCoreClr)
{
	var projectName = System.IO.Path.GetFileNameWithoutExtension(project);
	var monoRuntime = useCoreClr ? "coreclr" : "mono";
	var binlog = $"{binDir}/{projectName}-{config}-{monoRuntime}-android.binlog";

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
			args.Append("/p:EmbedAssembliesIntoApk=true")
				.Append("/bl:" + binlog);

			if (useCoreClr)
			{
				args.Append("/p:UseMonoRuntime=false");
			}
			return args;
		}
	});
}

void ExecuteTests(string project, string device, string appPackageName, string resultsDir, string config, string tfm, AdbToolSettings adbSettings, string toolPath, bool waitDevice, string instrumentation)
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

	PrepareDevice(waitDevice);

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

void ExecutePrepareUITests(string project, string app, string appPackageName, string device, string resultsDir, string binDir, string config, string tfm, string rid, string ver, string toolPath, string instrumentation, bool headless)
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

	InstallApk(testApp, appPackageName, resultsDir, deviceSkin, headless);
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

void ExecuteBuildUITestApp(string appProject, string device, string binDir, string config, string tfm, string rid, string toolPath)
{
	Information($"Building UI Test app: {appProject}");
	var projectName = System.IO.Path.GetFileNameWithoutExtension(appProject);
	var binlog = $"{binDir}/{projectName}-{config}-android.binlog";

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

// Helper methods

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
		var gpu = IsRunningOnLinux() ? "-gpu swiftshader_indirect" : "";
		settings.ArgumentCustomization = args => args
			.Append(gpu)
			.Append("-no-window")
			.Append("-no-snapshot")
			.Append("-no-audio")
			.Append("-no-boot-anim");
	}
	return settings;
}

void DetermineDeviceCharacteristics(string deviceDescriptor, int defaultApiLevel)
{
	var isArm64 = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64;
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
		if (isArm64)
			deviceArch = "arm64-v8a";
		else if (emulator)
			deviceArch = "x86_64";
		else
			deviceArch = "arm64-v8a";
	}
	var sdk = api >= 27 ? "google_apis_playstore" : "google_apis";
	if (api == 27 && deviceArch == "x86_64")
		sdk = "default";
	if (api == 27 && deviceArch == "arm64-v8a")
		sdk = "google_apis";

	androidAvd = $"Emulator_{api}";
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

				if (deviceCreate)
				{
					// delete the AVD first, if it exists
					Information("Deleting AVD if exists: {0}...", avdName);
					try { AndroidAvdDelete(avdName, avdSettings); }
					catch (Exception ex) { Warning("Failed to delete AVD: {0}", ex.Message); }

					// create the new AVD
					Information("Creating AVD: {0} ({1})...", avdName, avdImage);
					AndroidAvdCreate(avdName, avdImage, avdSkin, force: true, settings: avdSettings);
				}

				// Keep the same host identity before and throughout emulator boot.
				Information("Pre-authorizing ADB keys for emulator...");
				EnsureAdbKeys();
				// Copy the public key to the AVD directory so it's trusted from boot.
				var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				var adbKeyPubSource = System.IO.Path.Combine(homeDir, ".android", "adbkey.pub");
				var avdPath = System.IO.Path.Combine(homeDir, ".android", "avd", $"{avdName}.avd");
				var avdAdbKeysDest = System.IO.Path.Combine(avdPath, "adbkey.pub");

				if (System.IO.Directory.Exists(avdPath))
				{
					System.IO.File.Copy(adbKeyPubSource, avdAdbKeysDest, overwrite: true);
					Information($"Pre-authorized ADB key copied to: {avdAdbKeysDest}");
				}
				else
				{
					Warning($"Could not copy the ADB public key to the missing AVD directory: {avdPath}");
				}

				// start the emulator
				Information("Starting Emulator: {0}...", avdName);
				emulatorProcess = AndroidEmulatorStart(avdName, emuSettings);
			}
		}).WaitAsync(TimeSpan.FromSeconds(EmulatorStartProcessTimeoutSeconds));
	}
	catch (TimeoutException)
	{
		Error("Failed to start the Android Emulator.");
		throw;
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
	try 
	{ 
		// Wrap Kill() operation with timeout to prevent indefinite hanging
		var killTask = System.Threading.Tasks.Task.Run(() => emulatorProcess.Kill());
		if (killTask.Wait(TimeSpan.FromSeconds(EmulatorKillTimeoutSeconds)))
		{
			Information("Emulator process kill signal sent successfully.");
			
			// Now wait for the process to actually exit
			var waitTask = System.Threading.Tasks.Task.Run(() => emulatorProcess.WaitForExit());
			if (waitTask.Wait(TimeSpan.FromSeconds(EmulatorKillTimeoutSeconds)))
			{
				Information("Emulator process killed successfully.");
			}
			else
			{
				Warning("Emulator process did not exit within {0} seconds after kill signal.", EmulatorKillTimeoutSeconds);
			}
		}
		else
		{
			Warning("Emulator process kill operation timed out after {0} seconds. Attempting to restart ADB server...", EmulatorKillTimeoutSeconds);
			
			try
			{
				Information("Stopping ADB server...");
				AdbKillServer(adbSettings);
				System.Threading.Thread.Sleep(2000);
				
				Information("Starting ADB server...");
				AdbStartServer(adbSettings);
				System.Threading.Thread.Sleep(2000);
				
				Information("ADB server restart completed successfully.");
			}
			catch (Exception adbEx)
			{
				Error("Failed to restart ADB server after emulator kill timeout: {0}", adbEx.Message);
			}
		}
	}
	catch (Exception ex) 
	{ 
		Warning("Failed to kill emulator process: {0}", ex.Message);
	}

	if (deviceCreate)
	{
		Information("AndroidAvdDelete");
		// delete the AVD
		try { AndroidAvdDelete(androidAvd, avdSettings); }
		catch (Exception ex) { Warning("Failed to delete AVD during cleanup: {0}", ex.Message); }
	}
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

void InstallApk(string testApp, string testAppPackageName, string testResultsDirectory, string skin, bool headless)
{
	PrepareDevice(deviceBootWait);
	PrepareUITestEmulator();

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

    Information("Use the Android emulators in a headless mode:");
    SetEnvironmentVariable("HEADLESS", headless.ToString());

	Information("The platform version to run tests:");
	SetEnvironmentVariable("DEVICE_SKIN", skin);

	if (!string.IsNullOrEmpty(DEVICE_UDID))
	{
		SetEnvironmentVariable("DEVICE_UDID", DEVICE_UDID);
		//this needs to be translated to android 10/11 for appium
		var realApi = "";
		if (DEVICE_VERSION == "34")
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

void PrepareUITestEmulator()
{
	// Only configure the disposable emulator created by this CI UI-test run.
	if (!IsCIBuild() || !deviceCreate || emulatorProcess == null ||
		!(testDevice.StartsWith("android-emulator-", StringComparison.OrdinalIgnoreCase) ||
		  testDevice.StartsWith("android-simulator-", StringComparison.OrdinalIgnoreCase)))
		return;

	var emulators = AdbDevices(adbSettings)
		.Where(device => device.Serial.StartsWith("emulator-", StringComparison.Ordinal))
		.ToArray();
	if (emulators.Length != 1)
		throw new Exception($"Expected exactly one CI UI-test emulator, found {emulators.Length}. Refusing to configure an ambiguous device.");

	var exitCode = StartProcess("pwsh", new ProcessSettings
	{
		Arguments = new ProcessArgumentBuilder()
			.Append("-NoProfile")
			.Append("-File").AppendQuoted(MakeAbsolute(File("./Prepare-AndroidUITestEmulator.ps1")).FullPath)
			.Append("-AdbPath").AppendQuoted($"{androidSdkRoot}/platform-tools/adb")
			.Append("-Serial").AppendQuoted(emulators[0].Serial)
			.Append("-AvdName").AppendQuoted(androidAvd),
		Timeout = AdbCommandTimeoutSeconds * 1000
	});
	if (exitCode != 0)
		throw new Exception($"Android UI-test emulator preparation failed with exit code {exitCode}.");
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

IEnumerable<string> SafeAdbShell(string command, AdbToolSettings settings, int timeoutSeconds = AdbCommandTimeoutSeconds)
{
	try
	{
		var shellTask = System.Threading.Tasks.Task.Run(() => AdbShell(command, settings));
		if (shellTask.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
		{
			return shellTask.Result;
		}
		else
		{
			Warning("ADB shell command '{0}' timed out after {1} seconds", command, timeoutSeconds);
			return new string[0]; // Return empty array on timeout
		}
	}
	catch (Exception ex)
	{
		Warning("ADB shell command '{0}' failed: {1}", command, ex.Message);
		return new string[0]; // Return empty array on error
	}
}

void SafeAdbLogcat(AdbLogcatOptions options, int timeoutSeconds = AdbCommandTimeoutSeconds)
{
	try
	{
		var logcatTask = System.Threading.Tasks.Task.Run(() => AdbLogcat(options));
		if (!logcatTask.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
		{
			Warning("ADB logcat operation timed out after {0} seconds", timeoutSeconds);
		}
	}
	catch (Exception ex)
	{
		Warning("ADB logcat operation failed: {0}", ex.Message);
	}
}

void PrepareDevice(bool waitForBoot)
{
	var settings = new AdbToolSettings { SdkRoot = androidSdkRoot };
	if (!string.IsNullOrEmpty(DEVICE_UDID))
	{
		settings.Serial = DEVICE_UDID;
	}

	if (waitForBoot)
	{
		Information("Waiting for the emulator to finish booting...");

        // Wait for the emulator to finish booting
        var waited = 0;
        var total = EmulatorBootTimeoutSeconds;
        while (SafeAdbShell("getprop sys.boot_completed", settings).FirstOrDefault() != "1")
		{
		    System.Threading.Thread.Sleep(1000);

            Information("Waiting {0}/{1} seconds for the emulator to boot up.", waited, total);
            if (waited++ > total)
            {
                throw new Exception("The emulator did not finish booting in time.");
            }

            // At 90 seconds, restart ADB server to recover from authorization issues
            if (waited == 90 && IsCIBuild())
            {
                Information("Emulator boot taking longer than expected (90/{0} seconds). Restarting ADB server...", total);
                try
                {
                    Information("Stopping ADB server...");
                    AdbKillServer(settings);
                    System.Threading.Thread.Sleep(2000);
                    
                    Information("Starting ADB server...");
                    AdbStartServer(settings);
                    System.Threading.Thread.Sleep(2000);
                    
                    Information("ADB server restart completed. Continuing to wait for emulator boot...");
                }
                catch (Exception ex)
                {
                    Warning("Failed to restart ADB server during boot wait: {0}", ex.Message);
                    // Continue without throwing - this is a recovery attempt
                }
            }
		}

		Information("Waited {0} seconds for the emulator to boot up.", waited);
	}

	if (IsCIBuild())
	{
		Information("Setting Logcat properties...");

		try
		{
			SafeAdbLogcat(new AdbLogcatOptions() { Clear = true });
			
			SafeAdbShell("logcat -G 16M", settings);
			
			Information("Finished setting Logcat properties.");
		}
		catch (Exception ex)
		{
			Warning("Failed to set Logcat properties: {0}", ex.Message);
			// Continue without throwing - logcat setup is not critical for device function
		}
	}

	Information("Setting the ADB properties...");

	try
	{
		var lines = SafeAdbShell("setprop debug.mono.log default,mono_log_level=debug,mono_log_mask=all", settings);
		Information("{0}", string.Join("\n", lines));

		lines = SafeAdbShell("getprop debug.mono.log", settings);
		Information("{0}", string.Join("\n", lines));

		Information("Finished setting ADB properties.");
	}
	catch (Exception ex)
	{
		Warning("Failed to set ADB properties: {0}", ex.Message);
		// Continue without throwing - property setup failure should not stop the process
	}
}

void EnsureAdbKeys()
{
	var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	var adbKeyPath = System.IO.Path.Combine(homeDir, ".android");
	var adbKeyFile = System.IO.Path.Combine(adbKeyPath, "adbkey");
	var adbKeyPubFile = adbKeyFile + ".pub";
	var privateKeyExists = System.IO.File.Exists(adbKeyFile);
	var publicKeyExists = System.IO.File.Exists(adbKeyPubFile);

	if (!privateKeyExists && publicKeyExists)
		throw new Exception($"Incomplete ADB key pair in {adbKeyPath}. Restore the missing private key before starting the emulator.");

	if (!privateKeyExists)
	{
		System.IO.Directory.CreateDirectory(adbKeyPath);
		Information("Generating the host ADB key pair before emulator boot...");
		var exitCode = StartProcess("adb", new ProcessSettings
		{
			Arguments = new ProcessArgumentBuilder().Append("keygen").AppendQuoted(adbKeyFile),
			Timeout = 30000
		});

		if (exitCode != 0)
			throw new Exception($"ADB key generation failed with exit code {exitCode}.");
	}
	else if (!publicKeyExists)
	{
		IEnumerable<string> output;
		var exitCode = StartProcess("adb", new ProcessSettings
		{
			Arguments = new ProcessArgumentBuilder().Append("pubkey").AppendQuoted(adbKeyFile),
			RedirectStandardOutput = true,
			Timeout = 30000
		}, out output);
		var publicKey = string.Join("\n", output).Trim();

		if (exitCode != 0 || string.IsNullOrWhiteSpace(publicKey))
			throw new Exception($"ADB public key export failed with exit code {exitCode}; the private key was preserved.");

		System.IO.File.WriteAllText(adbKeyPubFile, publicKey);
	}

	if (!System.IO.File.Exists(adbKeyFile) || !System.IO.File.Exists(adbKeyPubFile) ||
		new System.IO.FileInfo(adbKeyFile).Length == 0 || new System.IO.FileInfo(adbKeyPubFile).Length == 0)
		throw new Exception("ADB keys were not created successfully.");

	// The emulator trusts this identity at boot; regenerating it while waiting revokes that trust.
	SetEnvironmentVariable("ADB_VENDOR_KEYS", adbKeyFile);
	Information("Using the existing host ADB identity; device authorization is checked during boot.");
}
