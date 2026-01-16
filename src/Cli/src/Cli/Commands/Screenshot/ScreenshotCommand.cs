using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Runtime.Versioning;
using NuGet.Frameworks;
using Xamarin.Android.Tools;

namespace Microsoft.Maui.Cli.Commands.Screenshot;

/// <summary>
/// Command implementation for capturing screenshots of running .NET MAUI applications.
/// </summary>
internal class ScreenshotCommand
{
    public const string CommandName = "screenshot";
    public const string CommandDescription = "Captures a screenshot of the currently running .NET MAUI application.";

    private readonly bool _verbose;

    public ScreenshotCommand(bool verbose = false)
    {
        _verbose = verbose;
    }

    /// <summary>
    /// Logger for AndroidSdkInfo. Errors, warnings, and info are always shown; debug only with --verbose.
    /// </summary>
    private void Log(TraceLevel level, string message)
    {
        if (level == TraceLevel.Error)
        {
            Console.Error.WriteLine(message);
        }
        else if (level == TraceLevel.Warning)
        {
            Console.WriteLine($"Warning: {message}");
        }
        else if (level == TraceLevel.Info)
        {
            Console.WriteLine(message);
        }
        else if (_verbose)
        {
            // Debug/Verbose only with --verbose
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Output file path option.
    /// </summary>
    public static readonly Option<string?> OutputOption = new("-o", "--output")
    {
        Description = "Output file path (default: screenshot_{timestamp}.png)"
    };

    /// <summary>
    /// Wait time before capturing option.
    /// </summary>
    public static readonly Option<int> WaitOption = new("-w", "--wait")
    {
        Description = "Wait time in seconds before capturing (default: 0)",
        DefaultValueFactory = _ => 0
    };

    /// <summary>
    /// Gets the screenshot command with all its options wired up.
    /// </summary>
    public static Command GetCommand()
    {
        var command = new Command(CommandName, CommandDescription);

        command.Options.Add(OutputOption);
        command.Options.Add(WaitOption);
        command.SetAction(Run);

        return command;
    }

    /// <summary>
    /// Entry point for the screenshot command.
    /// </summary>
    private static int Run(ParseResult result)
    {
        var outputPath = result.GetValue(OutputOption);
        var waitSeconds = result.GetValue(WaitOption);

        // Get global options
        var framework = result.GetValue(CommonOptions.FrameworkOption);
        var device = result.GetValue(CommonOptions.DeviceOption);
        var project = result.GetValue(CommonOptions.ProjectOption);
        var verbose = result.GetValue(CommonOptions.VerboseOption);

        // Generate default output path if not specified
        if (string.IsNullOrEmpty(outputPath))
        {
            outputPath = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        }

        var command = new ScreenshotCommand(verbose);
        return command.Execute(outputPath, waitSeconds, framework, device, project);
    }

    /// <summary>
    /// Executes the screenshot capture operation.
    /// </summary>
    /// <param name="outputPath">The path to save the screenshot.</param>
    /// <param name="waitSeconds">Number of seconds to wait before capturing.</param>
    /// <param name="framework">Target framework (optional).</param>
    /// <param name="device">Target device identifier (optional).</param>
    /// <param name="project">Path to the .NET MAUI project (optional).</param>
    /// <returns>Exit code (0 for success, non-zero for failure).</returns>
    public int Execute(string outputPath, int waitSeconds, string? framework, string? device, string? project)
    {
        // Wait if requested
        if (waitSeconds > 0)
        {
            Log(TraceLevel.Info, $"Waiting {waitSeconds} second(s) before capturing...");
            Thread.Sleep(waitSeconds * 1000);
        }

        // Determine platform from framework
        string? platform = GetTargetPlatformIdentifier(framework);

        if (string.IsNullOrEmpty(platform))
        {
            Log(TraceLevel.Error, "Error: Unable to determine target platform.");
            Log(TraceLevel.Error, "Please specify a framework using -f|--framework (e.g., net10.0-android, net10.0-ios)");
            return 1;
        }

        return platform.ToLowerInvariant() switch
        {
            "android" => ExecuteAndroid(outputPath, device),
            "ios" => ExecuteiOS(outputPath, device),
            "macos" or "maccatalyst" => ExecuteMacOS(outputPath, device),
            "windows" => ExecuteWindows(outputPath, device),
            _ => ExecuteUnsupportedPlatform(platform)
        };
    }

    /// <summary>
    /// Gets the target platform identifier from a target framework string.
    /// Uses NuGet.Frameworks to parse the TFM.
    /// If no framework is specified, returns the current OS platform.
    /// </summary>
    /// <param name="framework">The target framework (e.g., "net10.0-android").</param>
    /// <returns>The platform identifier (e.g., "android"), or null if not determinable.</returns>
    internal static string? GetTargetPlatformIdentifier(string? framework)
    {
        if (string.IsNullOrEmpty(framework))
        {
            // Default to current OS
            if (OperatingSystem.IsWindows())
                return "windows";
            if (OperatingSystem.IsMacOS())
                return "macos";
            if (OperatingSystem.IsMacCatalyst())
                return "maccatalyst";
            if (OperatingSystem.IsIOS())
                return "ios";
            if (OperatingSystem.IsAndroid())
                return "android";
            if (OperatingSystem.IsLinux())
                return "linux";
            return null;
        }

        // NuGetFramework.Parse() doesn't throw - it returns UnsupportedFramework for invalid inputs
        var nugetFramework = NuGetFramework.Parse(framework);
        return string.IsNullOrEmpty(nugetFramework.Platform) ? null : nugetFramework.Platform;
    }

    /// <summary>
    /// Creates a ProcessStartInfo configured for capturing output.
    /// Uses ArgumentList to avoid quoting issues.
    /// </summary>
    private static ProcessStartInfo CreateProcessStartInfo(string fileName, params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }
        return startInfo;
    }

    /// <summary>
    /// Captures a screenshot on Android using adb.
    /// </summary>
    private int ExecuteAndroid(string outputPath, string? device)
    {
        Log(TraceLevel.Info, $"Capturing Android screenshot to: {outputPath}");

        // Use AndroidSdkInfo to find the SDK path - checks $ANDROID_HOME, $ANDROID_SDK_ROOT, and default locations
        var sdkInfo = new AndroidSdkInfo(logger: Log);
        var sdkPath = sdkInfo.AndroidSdkPath;

        Log(TraceLevel.Verbose, $"Android SDK path: {sdkPath ?? "(not found)"}");

        if (string.IsNullOrEmpty(sdkPath))
        {
            Log(TraceLevel.Error, "Error: Could not find Android SDK. Make sure Android SDK is installed.");
            Log(TraceLevel.Error, "Set $ANDROID_HOME to your Android SDK path, or install via Android Studio.");
            return 1;
        }

        // Construct the adb path under platform-tools
        var adbName = OperatingSystem.IsWindows() ? "adb.exe" : "adb";
        var adbPath = Path.Combine(sdkPath, "platform-tools", adbName);

        Log(TraceLevel.Verbose, $"adb path: {adbPath}");

        if (!File.Exists(adbPath))
        {
            Log(TraceLevel.Error, $"Error: Could not find adb at: {adbPath}");
            Log(TraceLevel.Error, "Make sure Android SDK Platform Tools are installed.");
            return 1;
        }

        var startInfo = string.IsNullOrEmpty(device)
            ? CreateProcessStartInfo(adbPath, "exec-out", "screencap", "-p")
            : CreateProcessStartInfo(adbPath, "-s", device, "exec-out", "screencap", "-p");

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Log(TraceLevel.Error, "Error: Failed to start adb");
                return 1;
            }

            // Read binary screenshot data from stdout and write to file
            using (var fileStream = File.Create(outputPath))
            {
                process.StandardOutput.BaseStream.CopyTo(fileStream);
            }

            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Log(TraceLevel.Error, $"Error: adb exited with code {process.ExitCode}");
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    Log(TraceLevel.Error, stderr);
                }
                return process.ExitCode;
            }

            Log(TraceLevel.Info, "Screenshot captured successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Log(TraceLevel.Error, $"Error running adb: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Captures a screenshot on iOS Simulator using simctl.
    /// Note: Physical iOS device screenshots are not currently supported.
    /// </summary>
    private int ExecuteiOS(string outputPath, string? device)
    {
        // Check if this looks like a physical device (UDIDs are 40 hex characters or 25 characters with dashes for newer devices)
        // Simulator UDIDs are always formatted as standard GUIDs (8-4-4-4-12 format)
        // "booted" is a special keyword recognized by simctl to target the currently running simulator
        if (!string.IsNullOrEmpty(device) && !IsSimulatorUdid(device) && device != "booted")
        {
            Log(TraceLevel.Error, "Error: Screenshots from physical iOS devices are not currently supported.");
            Log(TraceLevel.Error, "Please use an iOS Simulator instead, or omit --device to use the booted simulator.");
            return 1;
        }

        Log(TraceLevel.Info, $"Capturing iOS Simulator screenshot to: {outputPath}");

        var deviceArg = string.IsNullOrEmpty(device) ? "booted" : device;
        var startInfo = CreateProcessStartInfo("xcrun", "simctl", "io", deviceArg, "screenshot", outputPath);
        return RunProcess(startInfo, "xcrun simctl");
    }

    /// <summary>
    /// Checks if the device identifier looks like a simulator UDID (standard GUID format).
    /// Simulators use GUIDs like "12345678-1234-1234-1234-123456789ABC".
    /// Physical devices use either 40-character hex strings or newer 25-character formats.
    /// </summary>
    private static bool IsSimulatorUdid(string device)
    {
        // Try to parse as a GUID - simulators use standard GUID format
        return Guid.TryParse(device, out _);
    }

    /// <summary>
    /// Captures a screenshot on Mac Catalyst using screencapture.
    /// </summary>
    private int ExecuteMacOS(string outputPath, string? device)
    {
        Log(TraceLevel.Info, $"Capturing Mac Catalyst screenshot to: {outputPath}");

        // Mac Catalyst apps run natively on macOS, use screencapture
        // -o flag prevents the screenshot sound
        var startInfo = CreateProcessStartInfo("screencapture", "-o", outputPath);
        return RunProcess(startInfo, "screencapture");
    }

    /// <summary>
    /// Captures a screenshot on Windows using Win32 APIs.
    /// </summary>
    [SupportedOSPlatform("windows")]
    private int ExecuteWindows(string outputPath, string? device)
    {
        Log(TraceLevel.Info, $"Capturing Windows screenshot to: {outputPath}");

        try
        {
            NativeMethods.CaptureScreen(outputPath);
            Log(TraceLevel.Info, "Screenshot captured successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Log(TraceLevel.Error, $"Error capturing Windows screenshot: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles unsupported platforms.
    /// </summary>
    private int ExecuteUnsupportedPlatform(string platform)
    {
        Log(TraceLevel.Error, $"Error: Platform '{platform}' is not supported for screenshot capture.");
        Log(TraceLevel.Error, "Supported platforms: android, ios, macos, maccatalyst, windows");
        return 1;
    }

    /// <summary>
    /// Runs an external process and handles output/errors.
    /// </summary>
    private int RunProcess(ProcessStartInfo startInfo, string toolName)
    {
        try
        {
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Log(TraceLevel.Error, $"Error: Failed to start {toolName}");
                return 1;
            }

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(stdout))
            {
                Log(TraceLevel.Info, stdout);
            }

            if (process.ExitCode != 0)
            {
                Log(TraceLevel.Error, $"Error: {toolName} exited with code {process.ExitCode}");
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    Log(TraceLevel.Error, stderr);
                }
                return process.ExitCode;
            }

            Log(TraceLevel.Info, "Screenshot captured successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Log(TraceLevel.Error, $"Error running {toolName}: {ex.Message}");
            return 1;
        }
    }
}
