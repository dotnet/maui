#addin nuget:?package=Cake.Android.Adb&version=3.2.0
#addin nuget:?package=Cake.Android.AvdManager&version=2.2.0
#load "../cake/helpers.cake"

string TARGET = Argument("target", "Test");

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("ANDROID_TEST_PROJECT") ?? "");
string TEST_DEVICE = Argument("device", EnvironmentVariable("ANDROID_TEST_DEVICE") ?? "android-emulator-32_30");
string DEVICE_NAME = Argument("skin", EnvironmentVariable("ANDROID_TEST_SKIN") ?? "Nexus 5X");

// optional
var USE_DOTNET = Argument("dotnet", true);
var DOTNET_PATH = Argument("dotnet-path", EnvironmentVariable("DOTNET_PATH"));
var TARGET_FRAMEWORK = Argument("tfm", EnvironmentVariable("TARGET_FRAMEWORK") ?? (USE_DOTNET ? "net6.0-android" : ""));
var BINLOG_ARG = Argument("binlog", EnvironmentVariable("ANDROID_TEST_BINLOG") ?? "");
DirectoryPath BINLOG_DIR = string.IsNullOrEmpty(BINLOG_ARG) && !string.IsNullOrEmpty(PROJECT.FullPath) ? PROJECT.GetDirectory() : BINLOG_ARG;
var TEST_APP = Argument("app", EnvironmentVariable("ANDROID_TEST_APP") ?? "");
var TEST_APP_PACKAGE_NAME = Argument("package", EnvironmentVariable("ANDROID_TEST_APP_PACKAGE_NAME") ?? "");
var TEST_APP_INSTRUMENTATION = Argument("instrumentation", EnvironmentVariable("ANDROID_TEST_APP_INSTRUMENTATION") ?? "");
var TEST_RESULTS = Argument("results", EnvironmentVariable("ANDROID_TEST_RESULTS") ?? "");

// other
string CONFIGURATION = "Debug"; // needs to be debug so unit tests get discovered
string ANDROID_AVD = "DEVICE_TESTS_EMULATOR";
string DEVICE_ID = "";
string DEVICE_ARCH = "";
bool DEVICE_BOOT = Argument("boot", true);
bool DEVICE_BOOT_WAIT = Argument("wait", true);
string DataPartitionSizeMB  = "2048";
string RamSizeMB = "2048";

// set up env
var ANDROID_SDK_ROOT = GetAndroidSDKPath();
var ANDROID_EMULATOR_HOME = EnvironmentVariable("ANDROID_EMULATOR_HOME");
var ANDROID_AVD_HOME = EnvironmentVariable("ANDROID_AVD_HOME");
var ANDROID_USER_HOME = EnvironmentVariable("ANDROID_USER_HOME");

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
Information("ANDROID_EMULATOR_HOME: {0}", ANDROID_EMULATOR_HOME);
Information("ANDROID_AVD_HOME: {0}", ANDROID_AVD_HOME);
Information("ANDROID_USER_HOME: {0}", ANDROID_USER_HOME);

var avdSettings = new AndroidAvdManagerToolSettings { SdkRoot = ANDROID_SDK_ROOT };
var adbSettings = new AdbToolSettings { SdkRoot = ANDROID_SDK_ROOT };
var emuSettings = new AndroidEmulatorToolSettings { SdkRoot = ANDROID_SDK_ROOT, ArgumentCustomization = args => args.Append("-no-window") };

AndroidEmulatorProcess emulatorProcess = null;

Setup(context =>
{
	Information("Test Device: {0}", TEST_DEVICE);

	// determine the device characteristics
	{
		var working = TEST_DEVICE.Trim().ToLower();
		var emulator = true;
		var api = 30;
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
		if (parts[1] == "device")
			emulator = false;
		else if (parts[1] != "emulator" && parts[1] != "simulator")
			throw new Exception("Unexpected device type (expected: device|emulator) in device: " + TEST_DEVICE);
		// arch/bits
		if (parts[2] == "32") {
			if (emulator)
				DEVICE_ARCH = "x86";
			else
				DEVICE_ARCH = "armeabi-v7a";
		} else if (parts[2] == "64") {
			if (emulator)
				DEVICE_ARCH = "x86_64";
			else
				DEVICE_ARCH = "arm64-v8a";
		}
		var sdk = api >= 24 ? "google_apis_playstore" : "google_apis";
		DEVICE_ID = $"system-images;android-{api};{sdk};{DEVICE_ARCH}";

		// we are not using a virtual device, so quit
		if (!emulator)
			return;
	}

	Information("Test Device ID: {0}", DEVICE_ID);

	if (DEVICE_BOOT) {
		Information("Trying to boot the emulator...");

		// delete the AVD first, if it exists
		Information("Deleting AVD if exists: {0}...", ANDROID_AVD);
		try { AndroidAvdDelete(ANDROID_AVD, avdSettings); }
		catch { }

		// create the new AVD
		Information("Creating AVD: {0}...", ANDROID_AVD);
		AndroidAvdCreate(ANDROID_AVD, DEVICE_ID, DEVICE_NAME, force: true, settings: avdSettings);

		var configPath = System.IO.Path.Combine (ANDROID_AVD_HOME, $"{ANDROID_AVD}.avd", "config.ini");

		if (!FileExists(configPath))
			configPath = System.IO.Path.Combine (ANDROID_USER_HOME, "avd", $"{ANDROID_AVD}.avd", "config.ini");

		if (FileExists (configPath)) {
			Information ($"Config file for AVD '{ANDROID_AVD}' found at {configPath}");
			WriteConfigFile (configPath);
		}
		else{
			Information ($"Config file for AVD '{ANDROID_AVD}' not found at {configPath}");
		}


		// start the emulator
		Information("Starting Emulator: {0}...", ANDROID_AVD);
		emulatorProcess = AndroidEmulatorStart(ANDROID_AVD, emuSettings);
	}
});

Teardown(context =>
{
	// no virtual device was used
	if (emulatorProcess == null || !DEVICE_BOOT || TARGET.ToLower() == "boot")
		return;

	// stop and cleanup the emulator
	AdbEmuKill(adbSettings);

	System.Threading.Thread.Sleep(5000);

	// kill the process if it has not already exited
	try { emulatorProcess.Kill(); }
	catch { }

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
	var binlog = $"{BINLOG_DIR}/{name}-{CONFIGURATION}-android.binlog";

	if (USE_DOTNET)
	{
		SetDotNetEnvironmentVariables(DOTNET_PATH);

		DotNetCoreBuild(PROJECT.FullPath, new DotNetCoreBuildSettings {
			Configuration = CONFIGURATION,
			Framework = TARGET_FRAMEWORK,
			MSBuildSettings = new DotNetCoreMSBuildSettings {
				MaxCpuCount = 0
			},
			ArgumentCustomization = args => args
				.Append("/p:EmbedAssembliesIntoApk=true")
				.Append("/bl:" + binlog),
			ToolPath = DOTNET_PATH,
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

	CleanDirectories(TEST_RESULTS);

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

	var settings = new DotNetCoreToolSettings {
		DiagnosticOutput = true,
		ArgumentCustomization = args=>args.Append("run xharness android test " +
			$"--app=\"{TEST_APP}\" " +
			$"--package-name=\"{TEST_APP_PACKAGE_NAME}\" " +
			$"--instrumentation=\"{TEST_APP_INSTRUMENTATION}\" " +
			$"--device-arch=\"{DEVICE_ARCH}\" " +
			$"--output-directory=\"{TEST_RESULTS}\" " +
			$"--verbosity=\"Debug\" ")
	};

	DotNetCoreTool("tool", settings);

	var failed = XmlPeek($"{TEST_RESULTS}/TestResults.xml", "/assemblies/assembly[@failed > 0 or @errors > 0]/@failed");
	if (!string.IsNullOrEmpty(failed)) {
		throw new Exception($"At least {failed} test(s) failed.");
	}
});

RunTarget(TARGET);


void WriteConfigFile (string configPath)
{
	if (!ulong.TryParse (DataPartitionSizeMB, out var diskSize))
		Information ($"Invalid data partition size '{DataPartitionSizeMB}' - must be a positive integer value expressing size in megabytes");

	if (!ulong.TryParse (RamSizeMB, out var ramSize))
		Information ($"Invalid RAM size '{RamSizeMB}' - must be a positive integer value expressing size in megabytes");


	ConfigValue[] values;

	if (IsRunningOnWindows())
	{
		values = new [] {
			new ConfigValue {
				Key = "disk.dataPartition.size=",
				Value = diskSize,
				Suffix = "M",
			},
			new ConfigValue {
				Key = "hw.ramSize=",
				Value = ramSize,
			},
			new ConfigValue {
				Key = "vm.heapSize=",
				Value = 228,
			},
			new ConfigValue {
				Key = "disk.cachePartition.size=",
				Value = 66,
			},
			new ConfigValue {
				Key = "hw.cpu.ncore=",
				Value = 3,
			}
		};
	}
	else
	{
		values = new [] {
			new ConfigValue {
				Key = "hw.cpu.ncore=",
				Value = 3,
			}
		};
	}
	

	var lines = new List<string> ();
	using (var reader = System.IO.File.OpenText (configPath)) {
		while (!reader.EndOfStream) {
			var line = reader.ReadLine ();
			foreach (var value in values) {
				if (line == value.ToString ()) {
					value.Set = true;
					Information ($"Value already present: {line}");
				} else if (line.StartsWith (value.Key, StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
			}
			lines.Add (line);
		}
	}

	var append = values.Where (v => !v.Set);
	if (!append.Any ()) {
		Information($"Skip writing file: {configPath}");
		return;
	}

	lines.AddRange (append.Select (v => v.ToString ()));
	System.IO.File.WriteAllLines (configPath, lines);
}

class ConfigValue
{
	/// <summary>
	/// Key including the `=` symbol
	/// </summary>
	public string Key { get; set; }

	public ulong Value { get; set; }

	/// <summary>
	/// Set to true if the file contains this value
	/// </summary>
	public bool Set { get; set; }

	/// <summary>
	/// Optional suffix such as `M`
	/// </summary>
	public string Suffix { get; set; } = "";

	public override string ToString ()
	{
		return $"{Key}{Value}{Suffix}";
	}
}