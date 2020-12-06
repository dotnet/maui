#addin nuget:?package=Cake.AppleSimulator&version=0.2.0
#addin nuget:?package=Cake.Android.Adb&version=3.2.0
#addin nuget:?package=Cake.Android.AvdManager&version=2.2.0
#addin nuget:?package=Cake.FileHelpers&version=3.3.0

var TARGET = Argument("target", "Default");

var IOS_SIM_NAME = Argument("ios-device", EnvironmentVariable("IOS_SIM_NAME") ?? "iPhone 11");
var IOS_SIM_RUNTIME = Argument("ios-runtime", EnvironmentVariable("IOS_SIM_RUNTIME") ?? "com.apple.CoreSimulator.SimRuntime.iOS-14-2");
var IOS_PROJ = "./DeviceTests.iOS/DeviceTests.iOS.csproj";
var IOS_BUNDLE_ID = "com.xamarin.essentials.devicetests";
var IOS_IPA_PATH = "./DeviceTests.iOS/bin/iPhoneSimulator/Release/XamarinEssentialsDeviceTestsiOS.app";
var IOS_TEST_RESULTS_PATH = MakeAbsolute((FilePath)"../output/test-results/ios");

var ANDROID_PROJ = "./DeviceTests.Android/DeviceTests.Android.csproj";
var ANDROID_APK_PATH = MakeAbsolute((FilePath)"./DeviceTests.Android/bin/Debug/com.xamarin.essentials.devicetests-Signed.apk");
var ANDROID_INSTRUMENTATION_NAME = "com.xamarin.essentials.devicetests.TestInstrumentation";
var ANDROID_TEST_RESULTS_PATH = MakeAbsolute((FilePath)"../output/test-results/android");
var ANDROID_AVD = EnvironmentVariable("ANDROID_AVD") ?? "CABOODLE";
var ANDROID_PKG_NAME = "com.xamarin.essentials.devicetests";
var ANDROID_EMU_TARGET = Argument("avd-target", EnvironmentVariable("ANDROID_EMU_TARGET") ?? "system-images;android-30;google_apis_playstore;x86");
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


// iOS tasks

Task("build-ios")
    .Does(() =>
{
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
    // Clean up after previous runs
    CleanDirectories(IOS_TEST_RESULTS_PATH.FullPath);

    // Run the tests
    var resultCode = StartProcess("xharness", "ios test " +
        $"--app=\"{IOS_IPA_PATH}\" " +
        $"--targets=\"ios-simulator-64\" " +
        $"--output-directory=\"{IOS_TEST_RESULTS_PATH}\" " +
        $"--verbosity=\"Debug\" ");

    // Rename test result files
    var resultFiles = GetFiles($"{IOS_TEST_RESULTS_PATH}/*.xml");
    foreach (var resultFile in resultFiles) {
        MoveFile(resultFile, resultFile.ChangeExtension("TestResults.xml"));
    }

    if (resultCode != 0)
        throw new Exception("xharness had an error.");
});


// Android tasks

Task("build-android")
    .Does(() =>
{
    MSBuild(ANDROID_PROJ, c => {
        c.Configuration = "Debug"; // needs to be debug so unit tests get discovered
        c.Restore = true;
        c.Properties["ContinuousIntegrationBuild"]  = new List<string> { "false" };
        c.Targets.Clear();
        c.Targets.Add("Rebuild");
        c.Targets.Add("SignAndroidPackage");
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
    // Clean up after previous runs
    CleanDirectories(ANDROID_TEST_RESULTS_PATH.FullPath);

    var avdSettings = new AndroidAvdManagerToolSettings { SdkRoot = ANDROID_HOME };
    var adbSettings = new AdbToolSettings { SdkRoot = ANDROID_HOME };
    var emuSettings = new AndroidEmulatorToolSettings { SdkRoot = ANDROID_HOME, ArgumentCustomization = args => args.Append("-no-window") };

    // Delete AVD first, if it exists
    Information("Deleting AVD if exists: {0}...", ANDROID_AVD);
    try { AndroidAvdDelete(ANDROID_AVD, avdSettings); }
    catch { }

    // Create the AVD
    Information("Creating AVD: {0}...", ANDROID_AVD);
    AndroidAvdCreate(ANDROID_AVD, ANDROID_EMU_TARGET, ANDROID_EMU_DEVICE, force: true, settings: avdSettings);

    // Start the emulator
    Information("Starting Emulator: {0}...", ANDROID_AVD);
    var emulatorProcess = AndroidEmulatorStart(ANDROID_AVD, emuSettings);

    var waited = 0;
    while (AdbShell("getprop sys.boot_completed", adbSettings).FirstOrDefault() != "1") {
        System.Threading.Thread.Sleep(1000);
        if (waited++ > 60 * 10)
            break;
    }
    Information("Waited {0} seconds for the emulator to boot up.", waited);

    // Run the tests
    var resultCode = StartProcess("xharness", "android test " +
        $"--app=\"{ANDROID_APK_PATH}\" " +
        $"--package-name=\"{ANDROID_PKG_NAME}\" " +
        $"--instrumentation=\"{ANDROID_INSTRUMENTATION_NAME}\" " +
        $"--device-arch=\"x86\" " +
        $"--output-directory=\"{ANDROID_TEST_RESULTS_PATH}\" " +
        $"--verbosity=\"Debug\" ");

    // Stop / cleanup the emulator
    AdbEmuKill(adbSettings);

    System.Threading.Thread.Sleep(5000);

    // Kill the process if it has not already exited
    try { emulatorProcess.Kill(); }
    catch { }

    if (resultCode != 0)
        throw new Exception("xharness had an error.");

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
        c.Properties["AppxPackageSigningEnabled"] = new List<string> { "true" };
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

    var certPath = GetFiles("./**/DeviceTests.UWP_TemporaryKey.pfx").First();
    Information("Installing certificate: {0}", certPath);

    StartProcess("certutil", "-p Microsoft -importpfx \"" + MakeAbsolute(certPath).FullPath + "\"");

    // Install the appx
    var dependencies = GetFiles("./**/AppPackages/**/Dependencies/x86/*.appx");
    foreach (var dep in dependencies) {
        Information("Installing Dependency appx: {0}", dep);
        StartProcess("powershell", "Add-AppxPackage -Path \"" + MakeAbsolute(dep).FullPath + "\"");
    }
    var appxBundlePath = GetFiles("./**/AppPackages/**/*.msixbundle").First();
    Information("Installing appx: {0}", appxBundlePath);
    StartProcess("powershell", "Add-AppxPackage -Path \"" + MakeAbsolute(appxBundlePath).FullPath + "\"");

    // Start the TCP Test results listener
    Information("Started TCP Test Results Listener on port: {0}:{1}", TCP_LISTEN_HOST, TCP_LISTEN_PORT);
    var tcpListenerTask = DownloadTcpTextAsync(TCP_LISTEN_PORT, UWP_TEST_RESULTS_PATH);

    // Launch the app
    Information("Running appx: {0}", appxBundlePath);
    var ip = TCP_LISTEN_HOST.Replace(".", "-");
    var executeCommand = $"xamarin-essentials-device-tests://{ip}_{TCP_LISTEN_PORT}";
    Information("Running appx: {0}", executeCommand);
    StartProcess("explorer", executeCommand);

    // Wait for the test results to come back
    Information("Waiting for tests...");
    tcpListenerTask.Wait();

    // Uninstall the app(this will terminate it too)
    uninstallPS();
});


RunTarget(TARGET);
