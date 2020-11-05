#addin nuget:?package=Cake.AppleSimulator&version=0.2.0
#addin nuget:?package=Cake.Android.Adb&version=3.2.0
#addin nuget:?package=Cake.Android.AvdManager&version=2.2.0
#addin nuget:?package=Cake.FileHelpers&version=3.3.0

var TARGET = Argument("target", "Default");

var IOS_SIM_NAME = Argument("ios-device", EnvironmentVariable("IOS_SIM_NAME") ?? "iPhone 11");
var IOS_SIM_RUNTIME = Argument("ios-runtime", EnvironmentVariable("IOS_SIM_RUNTIME") ?? "com.apple.CoreSimulator.SimRuntime.iOS-14-0");
var IOS_PROJ = "./DeviceTests.iOS/DeviceTests.iOS.csproj";
var IOS_BUNDLE_ID = "com.xamarin.essentials.devicetests";
var IOS_IPA_PATH = "./DeviceTests.iOS/bin/iPhoneSimulator/Release/XamarinEssentialsDeviceTestsiOS.app";
var IOS_TEST_RESULTS_PATH = MakeAbsolute((FilePath)"../output/test-results/ios/TestResults.xml");

var ANDROID_PROJ = "./DeviceTests.Android/DeviceTests.Android.csproj";
var ANDROID_APK_PATH = "./DeviceTests.Android/bin/Release/com.xamarin.essentials.devicetests-Signed.apk";
var ANDROID_TEST_RESULTS_PATH = MakeAbsolute((FilePath)"../output/test-results/android/TestResults.xml");
var ANDROID_SCREENSHOT_PATH = MakeAbsolute((DirectoryPath)"../output/test-results/android");
var ANDROID_AVD = EnvironmentVariable("ANDROID_AVD") ?? "CABOODLE";
var ANDROID_PKG_NAME = "com.xamarin.essentials.devicetests";
var ANDROID_EMU_TARGET = Argument("avd-target", EnvironmentVariable("ANDROID_EMU_TARGET") ?? "system-images;android-29;google_apis_playstore;x86");
var ANDROID_EMU_DEVICE = Argument("avd-device", EnvironmentVariable("ANDROID_EMU_DEVICE") ?? "Nexus 5X");

var UWP_PROJ = "./DeviceTests.UWP/DeviceTests.UWP.csproj";
var UWP_TEST_RESULTS_PATH = MakeAbsolute((FilePath)"../output/test-results/uwp/TestResults.xml");
var UWP_PACKAGE_ID = "ec0cc741-fd3e-485c-81be-68815c480690";

var TCP_LISTEN_TIMEOUT = 240;
var TCP_LISTEN_PORT = 63559;
var TCP_LISTEN_HOST = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
    .AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    .ToString();

var OUTPUT_PATH = MakeAbsolute((DirectoryPath)"../output/");

var ANDROID_HOME = EnvironmentVariable("ANDROID_HOME");

System.Environment.SetEnvironmentVariable("PATH",
    $"{ANDROID_HOME}/tools/bin" + System.IO.Path.PathSeparator +
    $"{ANDROID_HOME}/platform-tools" + System.IO.Path.PathSeparator +
    $"{ANDROID_HOME}/emulator" + System.IO.Path.PathSeparator +
    EnvironmentVariable("PATH"));


// utils

Task DownloadTcpTextAsync(int port, FilePath filename, Action waitAction = null)
{
    filename = MakeAbsolute(filename);
    EnsureDirectoryExists(filename.GetDirectory());

    return System.Threading.Tasks.Task.Run(() => {
        var tcpListener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, port);
        tcpListener.Start();
        var listening = true;

        System.Threading.Tasks.Task.Run(() => {
            // Sleep until timeout elapses or tcp listener stopped after a successful connection
            var elapsed = 0;
            while(elapsed <= TCP_LISTEN_TIMEOUT && listening) {
                System.Threading.Thread.Sleep(1000);
                elapsed++;
                Information($"Still waiting for tests... {elapsed}/{TCP_LISTEN_TIMEOUT}");
                waitAction?.Invoke();
            }

            // If still listening, timeout elapsed, stop the listener
            if (listening) {
                tcpListener.Stop();
                listening = false;
            }
        });

        try {
            var tcpClient = tcpListener.AcceptTcpClient();

            using (var file = System.IO.File.Open(filename.FullPath, System.IO.FileMode.Create))
            using (var stream = tcpClient.GetStream())
                stream.CopyTo(file);

            tcpClient.Close();
            tcpListener.Stop();
            listening = false;
        } catch {
            throw new Exception("Test results listener failed or timed out.");
        }
    });
}

void AddPlatformToTestResults(FilePath testResultsFile, string platformName)
{
    if (FileExists(testResultsFile)) {
        var txt = FileReadText(testResultsFile);
        txt = txt.Replace("<test-case name=\"DeviceTests.", $"<test-case name=\"DeviceTests.{platformName}.");
        txt = txt.Replace("<test name=\"DeviceTests.", $"<test name=\"DeviceTests.{platformName}.");
        txt = txt.Replace("name=\"Test collection for DeviceTests.", $"name=\"Test collection for DeviceTests.{platformName}.");
        FileWriteText(testResultsFile, txt);
    }
}


// iOS tasks

Task("build-ios")
    .Does(() =>
{
    // Setup the test listener config to be built into the app
    FileWriteText((new FilePath(IOS_PROJ)).GetDirectory().CombineWithFilePath("tests.cfg"), $"{TCP_LISTEN_HOST}:{TCP_LISTEN_PORT}");

    MSBuild(IOS_PROJ, c => {
        c.Configuration = "Release";
        c.Restore = true;
        c.Properties["Platform"] = new List<string> { "iPhoneSimulator" };
        c.Properties["BuildIpa"] = new List<string> { "true" };
        c.Properties["ContinuousIntegrationBuild"] = new List<string> { "false" };
        c.Targets.Clear();
        c.Targets.Add("Rebuild");
        c.BinaryLogger = new MSBuildBinaryLogSettings {
            Enabled = true,
            FileName = OUTPUT_PATH.CombineWithFilePath("binlogs/device-tests-ios-build.binlog").FullPath,
        };
    });
});

Task("test-ios-emu")
    .IsDependentOn("build-ios")
    .Does(() =>
{
    var sims = ListAppleSimulators();
    foreach (var s in sims) {
        Information("Info: {0}({1} - {2} - {3})", s.Name, s.Runtime, s.UDID, s.Availability);
    }

    // Look for a matching simulator on the system
    var sim = sims.First(s => s.Name == IOS_SIM_NAME && s.Runtime == IOS_SIM_RUNTIME);

    // Boot the simulator
    Information("Booting: {0}({1} - {2})", sim.Name, sim.Runtime, sim.UDID);
    if (!sim.State.ToLower().Contains("booted"))
        BootAppleSimulator(sim.UDID);

    // Wait for it to be booted
    var booted = false;
    for (int i = 0; i < 100; i++) {
        if (ListAppleSimulators().Any(s => s.UDID == sim.UDID && s.State.ToLower().Contains("booted"))) {
            booted = true;
            break;
        }
        System.Threading.Thread.Sleep(1000);
    }

    // Install the IPA that was previously built
    var ipaPath = new FilePath(IOS_IPA_PATH);
    Information("Installing: {0}", ipaPath);
    InstalliOSApplication(sim.UDID, MakeAbsolute(ipaPath).FullPath);

    // Start our Test Results TCP listener
    Information("Started TCP Test Results Listener on port: {0}", TCP_LISTEN_PORT);
    var tcpListenerTask = DownloadTcpTextAsync(TCP_LISTEN_PORT, IOS_TEST_RESULTS_PATH);

    // Launch the IPA
    Information("Launching: {0}", IOS_BUNDLE_ID);
    LaunchiOSApplication(sim.UDID, IOS_BUNDLE_ID);

    // Wait for the TCP listener to get results
    Information("Waiting for tests...");
    tcpListenerTask.Wait();

    AddPlatformToTestResults(IOS_TEST_RESULTS_PATH, "iOS");

    // Close up simulators
    Information("Closing Simulator");
    ShutdownAllAppleSimulators();
});


// Android tasks

Task("build-android")
    .Does(() =>
{
    // Build the app in debug mode
    // needs to be debug so unit tests get discovered
    MSBuild(ANDROID_PROJ, c => {
        c.Configuration = "Debug";
        c.Restore = true;
        c.Properties["ContinuousIntegrationBuild"]  = new List<string> { "false" };
        c.Targets.Clear();
        c.Targets.Add("Rebuild");
        c.BinaryLogger = new MSBuildBinaryLogSettings {
            Enabled = true,
            FileName = OUTPUT_PATH.CombineWithFilePath("binlogs/device-tests-android-build.binlog").FullPath,
        };
    });
});

Task("test-android-emu")
    .IsDependentOn("build-android")
    .Does(() =>
{
    var avdSettings = new AndroidAvdManagerToolSettings { SdkRoot = ANDROID_HOME };

    // Delete AVD first, if it exists
    Information("Deleting AVD if exists: {0}...", ANDROID_AVD);
    try { AndroidAvdDelete(ANDROID_AVD, avdSettings); }
    catch { }

    // Create the AVD
    Information("Creating AVD: {0}...", ANDROID_AVD);
    AndroidAvdCreate(ANDROID_AVD, ANDROID_EMU_TARGET, ANDROID_EMU_DEVICE, force: true, settings: avdSettings);

    Information("Starting Emulator: {0}...", ANDROID_AVD);
    var emuSettings = new AndroidEmulatorToolSettings { SdkRoot = ANDROID_HOME, ArgumentCustomization = args => args.Append("-no-window") };
    var emulatorProcess = AndroidEmulatorStart(ANDROID_AVD, emuSettings);

    var adbSettings = new AdbToolSettings { SdkRoot = ANDROID_HOME };

    // Keep checking adb for an emulator with an AVD name matching the one we just started
    var emuSerial = string.Empty;
    for (int i = 0; i < 100; i++) {
        foreach (var device in AdbDevices(adbSettings).Where(d => d.Serial.StartsWith("emulator-"))) {
            if (AdbGetAvdName(device.Serial).Equals(ANDROID_AVD, StringComparison.OrdinalIgnoreCase)) {
                emuSerial = device.Serial;
                break;
            }
        }

        if (!string.IsNullOrEmpty(emuSerial))
            break;
        else
            System.Threading.Thread.Sleep(1000);
    }

    Information("Matched ADB Serial: {0}", emuSerial);
    adbSettings = new AdbToolSettings { SdkRoot = ANDROID_HOME, Serial = emuSerial };

    // Wait for the emulator to enter a 'booted' state
    AdbWaitForEmulatorToBoot(TimeSpan.FromSeconds(100), adbSettings);
    Information("Emulator finished booting.");

    // Read the logcat
    AdbLogcat(new AdbLogcatOptions { Clear = true }, settings: adbSettings);
    AdbLogcat(settings: adbSettings);

    // Try uninstalling the existing package(if installed)
    try {
        AdbUninstall(ANDROID_PKG_NAME, false, adbSettings);
        Information("Uninstalled old: {0}", ANDROID_PKG_NAME);
    } catch { }

    // Use the Install target to push the app onto emulator
    MSBuild(ANDROID_PROJ, c => {
        c.Configuration = "Debug";
        c.Properties["ContinuousIntegrationBuild"] = new List<string> { "false" };
        c.Properties["AdbTarget"] = new List<string> { "-s " + emuSerial };
        c.Targets.Clear();
        c.Targets.Add("Install");
        c.BinaryLogger = new MSBuildBinaryLogSettings {
            Enabled = true,
            FileName = OUTPUT_PATH.CombineWithFilePath("binlogs/device-tests-android-install.binlog").FullPath,
        };
    });

    // Start the TCP Test results listener
    Information("Started TCP Test Results Listener on port: {0}:{1}", TCP_LISTEN_HOST, TCP_LISTEN_PORT);
    // var printed = false;
    var tcpListenerTask = DownloadTcpTextAsync(TCP_LISTEN_PORT, ANDROID_TEST_RESULTS_PATH, () => {
        // if (!printed) {
        //     AdbScreenCapture(ANDROID_SCREENSHOT_PATH.CombineWithFilePath($"screenshot.png"), adbSettings);
        //     AdbLogcat(settings: adbSettings);
        //     printed = true;
        // }
    });

    // Launch the app on the emulator
    AdbShell($"am start -n {ANDROID_PKG_NAME}/{ANDROID_PKG_NAME}.MainActivity --es HOST_IP {TCP_LISTEN_HOST} --ei HOST_PORT {TCP_LISTEN_PORT}", adbSettings);
    AdbLogcat(settings: adbSettings);

    // Wait for the test results to come back
    Information("Waiting for tests...");
    tcpListenerTask.Wait();

    AddPlatformToTestResults(ANDROID_TEST_RESULTS_PATH, "Android");

    // Stop / cleanup the emulator
    AdbEmuKill(adbSettings);

    System.Threading.Thread.Sleep(5000);

    // Finally kill the process if it's not exited already
    try { emulatorProcess.Kill(); }
    catch { }

    Information("Done Tests");
});


// UWP tasks

Task("build-uwp")
    .Does(() =>
{
    MSBuild(UWP_PROJ, c => {
        c.Configuration = "Debug";
        c.Restore = true;
        c.Properties["ContinuousIntegrationBuild"] = new List<string> { "false" };
        c.Properties["AppxBundlePlatforms"] = new List<string> { "x86" };
        c.Properties["AppxBundle"] = new List<string> { "Always" };
        c.Targets.Clear();
        c.Targets.Add("Rebuild");
        c.BinaryLogger = new MSBuildBinaryLogSettings {
            Enabled = true,
            FileName = OUTPUT_PATH.CombineWithFilePath("binlogs/device-tests-uwp-build.binlog").FullPath,
        };
    });
});

Task("test-uwp-emu")
    .IsDependentOn("build-uwp")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    var uninstallPS = new Action(() => {
        try {
            StartProcess("powershell",
                "$app = Get-AppxPackage -Name " + UWP_PACKAGE_ID + "; if ($app) { Remove-AppxPackage -Package $app.PackageFullName }");
        } catch { }
    });

    // Try to uninstall the app if it exists from before
    uninstallPS();

    // Install the appx
    var dependencies = GetFiles("./**/AppPackages/**/Dependencies/x86/*.appx");
    foreach (var dep in dependencies) {
        Information("Installing Dependency appx: {0}", dep);
        StartProcess("powershell", "Add-AppxPackage -Path \"" + MakeAbsolute(dep).FullPath + "\"");
    }
    var appxBundlePath = GetFiles("./**/AppPackages/**/*.appxbundle").First();
    Information("Installing appx: {0}", appxBundlePath);
    StartProcess("powershell", "Add-AppxPackage -Path \"" + MakeAbsolute(appxBundlePath).FullPath + "\"");

    // Start the TCP Test results listener
    Information("Started TCP Test Results Listener on port: {0}:{1}", TCP_LISTEN_HOST, TCP_LISTEN_PORT);
    var tcpListenerTask = DownloadTcpTextAsync(TCP_LISTEN_PORT, UWP_TEST_RESULTS_PATH);

    // Launch the app
    Information("Running appx: {0}", appxBundlePath);
    var ip = TCP_LISTEN_HOST.Replace(".", "-");
    System.Diagnostics.Process.Start($"xamarin-essentials-device-tests://{ip}_{TCP_LISTEN_PORT}");

    // Wait for the test results to come back
    Information("Waiting for tests...");
    tcpListenerTask.Wait();

    AddPlatformToTestResults(UWP_TEST_RESULTS_PATH, "UWP");

    // Uninstall the app(this will terminate it too)
    uninstallPS();
});


RunTarget(TARGET);
