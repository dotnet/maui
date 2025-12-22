using System.Text.RegularExpressions;

namespace Microsoft.Maui.IntegrationTests
{
	public static class XHarness
	{
		static readonly string XHarnessTool = "xharness";
		const int DEFAULT_TIMEOUT = 300;

		// XHarness exit codes that indicate failures (from Microsoft.DotNet.XHarness.Common.CLI.ExitCode)
		// These should NEVER be treated as success even if timeout occurred
		const int EXIT_CODE_TESTS_FAILED = 1;
		const int EXIT_CODE_PACKAGE_NOT_FOUND = 4;
		const int EXIT_CODE_TIMED_OUT = 70;
		const int EXIT_CODE_GENERAL_FAILURE = 71;
		const int EXIT_CODE_PACKAGE_INSTALLATION_FAILURE = 78;
		const int EXIT_CODE_FAILED_TO_GET_BUNDLE_INFO = 79;
		const int EXIT_CODE_APP_CRASH = 80;
		const int EXIT_CODE_DEVICE_NOT_FOUND = 81;
		const int EXIT_CODE_RETURN_CODE_NOT_SET = 82;
		const int EXIT_CODE_APP_LAUNCH_FAILURE = 83;
		const int EXIT_CODE_DEVICE_FILE_COPY_FAILURE = 84;
		const int EXIT_CODE_PACKAGE_INSTALLATION_TIMEOUT = 86;
		const int EXIT_CODE_APP_NOT_SIGNED = 87;
		const int EXIT_CODE_SIMULATOR_FAILURE = 88;
		const int EXIT_CODE_DEVICE_FAILURE = 89;
		const int EXIT_CODE_APP_LAUNCH_TIMEOUT = 90;
		const int EXIT_CODE_ADB_FAILURE = 91;
		const int EXIT_CODE_TCP_CONNECTION_FAILED = 92;

		public static bool RunAndroid(string packageName, string resultDir, int expectedExitCode, int launchTimeoutSeconds = 120)
		{
			var timeoutString = TimeSpan.FromSeconds(launchTimeoutSeconds).ToString();
			var args = $"android run --package-name={packageName} --output-directory=\"{resultDir}\" --expected-exit-code={expectedExitCode} --timeout=\"{timeoutString}\" --verbosity=Debug";
			return Run(args, launchTimeoutSeconds + 30);
		}

		/// <summary>
		/// Attempts to launch an iOS app on the target device. Assumes success if the app run doesn't end within the specified timeout.
		/// Exit code detection is not working on iOS/tvOS 15+, however a crash will return early.
		/// </summary>
		/// <param name="appPath"></param>
		/// <param name="resultDir"></param>
		/// <param name="targetDevice">XHarness target device string (e.g., "ios-simulator-64_18.5")</param>
		/// <param name="deviceUdid">Optional specific device UDID to use. When provided, uses --device instead of --target to use an already-booted simulator.</param>
		/// <param name="launchTimeoutSeconds"></param>
		/// <returns>True if the app launch command timed out, false if it exits early.</returns>
		public static bool RunAppleForTimeout(string appPath, string resultDir, string targetDevice, string? deviceUdid = null, int launchTimeoutSeconds = 75)
		{
			var timeoutString = TimeSpan.FromSeconds(launchTimeoutSeconds).ToString();
			
			// When a device UDID is provided, use --device to target the already-booted simulator directly.
			// This avoids XHarness from managing (and potentially shutting down) the simulator.
			// When no UDID is provided, fall back to --target which lets XHarness find/create a simulator.
			string deviceArg = !string.IsNullOrEmpty(deviceUdid) 
				? $"--device=\"{deviceUdid}\"" 
				: $"--target={targetDevice}";
			
			var args = $"apple run --app=\"{appPath}\" --output-directory=\"{resultDir}\" {deviceArg} --timeout=\"{timeoutString}\" --verbosity=Debug";
			var xhOutput = RunForOutput(args, out int exitCode, launchTimeoutSeconds + 30);

			// Check for known failure exit codes - these should NEVER be treated as success
			// even if the timeout occurred, as they indicate the app failed to launch or crashed
			if (IsKnownFailureExitCode(exitCode))
			{
				TestContext.WriteLine($"XHarness returned failure exit code: {exitCode} ({GetExitCodeDescription(exitCode)})");
				TestContext.WriteLine(xhOutput);
				LogXHarnessOutputDirectory(resultDir);
				return false;
			}

			var launchLogMatch = false;
			var launchLog = Path.Combine(resultDir, $"{Path.GetFileNameWithoutExtension(appPath)}.log");
			if (File.Exists(launchLog))
			{
				var launchLogContent = File.ReadAllText(launchLog);
				launchLogMatch = Regex.IsMatch(launchLogContent, @"Killing process \d* as it was cancelled");
			}

			bool didTimeoutAsExpected = xhOutput.Contains($"Run timed out after {launchTimeoutSeconds} seconds", StringComparison.OrdinalIgnoreCase)
				&& launchLogMatch
				&& exitCode != 0;

			if (!didTimeoutAsExpected)
			{
				TestContext.WriteLine($"XHarness did not timeout as expected. Exit code: {exitCode}, Timeout match: {xhOutput.Contains($"Run timed out after {launchTimeoutSeconds} seconds", StringComparison.OrdinalIgnoreCase)}, Launch log match: {launchLogMatch}");
				TestContext.WriteLine(xhOutput);
				LogXHarnessOutputDirectory(resultDir);
			}

			return didTimeoutAsExpected;
		}

		private static bool IsKnownFailureExitCode(int exitCode)
		{
			return exitCode == EXIT_CODE_TESTS_FAILED ||
				   exitCode == EXIT_CODE_PACKAGE_NOT_FOUND ||
				   exitCode == EXIT_CODE_GENERAL_FAILURE ||
				   exitCode == EXIT_CODE_PACKAGE_INSTALLATION_FAILURE ||
				   exitCode == EXIT_CODE_FAILED_TO_GET_BUNDLE_INFO ||
				   exitCode == EXIT_CODE_APP_CRASH ||
				   exitCode == EXIT_CODE_DEVICE_NOT_FOUND ||
				   exitCode == EXIT_CODE_RETURN_CODE_NOT_SET ||
				   exitCode == EXIT_CODE_APP_LAUNCH_FAILURE ||
				   exitCode == EXIT_CODE_DEVICE_FILE_COPY_FAILURE ||
				   exitCode == EXIT_CODE_PACKAGE_INSTALLATION_TIMEOUT ||
				   exitCode == EXIT_CODE_APP_NOT_SIGNED ||
				   exitCode == EXIT_CODE_SIMULATOR_FAILURE ||
				   exitCode == EXIT_CODE_DEVICE_FAILURE ||
				   exitCode == EXIT_CODE_APP_LAUNCH_TIMEOUT ||
				   exitCode == EXIT_CODE_ADB_FAILURE ||
				   exitCode == EXIT_CODE_TCP_CONNECTION_FAILED;
		}

		public static bool InstallSimulator(string targetDevice)
		{
			return Run($"apple simulators install \"{targetDevice}\" ");
		}

		public static string GetSimulatorUDID(string targetDevice)
		{
			return RunForOutput($"apple device \"{targetDevice}\" ", out _, timeoutInSeconds: 30);
		}

		public static bool Run(string args, int timeoutInSeconds = DEFAULT_TIMEOUT)
		{
			var xhOutput = RunForOutput(args, out int exitCode, timeoutInSeconds);
			if (exitCode != 0)
				TestContext.WriteLine(xhOutput);

			return exitCode == 0;
		}

		public static string RunForOutput(string args, out int exitCode, int timeoutInSeconds = DEFAULT_TIMEOUT)
		{
			return DotnetInternal.RunForOutput(XHarnessTool, args, out exitCode, timeoutInSeconds);
		}

		/// <summary>
		/// Gets a human-readable description for an XHarness exit code.
		/// </summary>
		private static string GetExitCodeDescription(int exitCode)
		{
			return exitCode switch
			{
				EXIT_CODE_TESTS_FAILED => "Tests Failed",
				EXIT_CODE_PACKAGE_NOT_FOUND => "Package Not Found",
				EXIT_CODE_TIMED_OUT => "Timed Out",
				EXIT_CODE_GENERAL_FAILURE => "General Failure",
				EXIT_CODE_PACKAGE_INSTALLATION_FAILURE => "Package Installation Failure",
				EXIT_CODE_FAILED_TO_GET_BUNDLE_INFO => "Failed to Get Bundle Info",
				EXIT_CODE_APP_CRASH => "App Crash",
				EXIT_CODE_DEVICE_NOT_FOUND => "Device Not Found",
				EXIT_CODE_RETURN_CODE_NOT_SET => "Return Code Not Set",
				EXIT_CODE_APP_LAUNCH_FAILURE => "App Launch Failure",
				EXIT_CODE_DEVICE_FILE_COPY_FAILURE => "Device File Copy Failure",
				EXIT_CODE_PACKAGE_INSTALLATION_TIMEOUT => "Package Installation Timeout",
				EXIT_CODE_APP_NOT_SIGNED => "App Not Signed",
				EXIT_CODE_SIMULATOR_FAILURE => "Simulator Failure",
				EXIT_CODE_DEVICE_FAILURE => "Device Failure",
				EXIT_CODE_APP_LAUNCH_TIMEOUT => "App Launch Timeout",
				EXIT_CODE_ADB_FAILURE => "ADB Failure",
				EXIT_CODE_TCP_CONNECTION_FAILED => "TCP Connection Failed",
				_ => $"Unknown ({exitCode})"
			};
		}

		/// <summary>
		/// Logs all files in the XHarness output directory and their contents for debugging.
		/// </summary>
		private static void LogXHarnessOutputDirectory(string resultDir)
		{
			try
			{
				if (!Directory.Exists(resultDir))
				{
					TestContext.WriteLine($"XHarness output directory does not exist: {resultDir}");
					return;
				}

				TestContext.WriteLine($"=== XHarness Output Directory: {resultDir} ===");
				var files = Directory.GetFiles(resultDir, "*", SearchOption.AllDirectories);
				
				if (files.Length == 0)
				{
					TestContext.WriteLine("No files found in output directory.");
					return;
				}

				foreach (var file in files)
				{
					var relativePath = Path.GetRelativePath(resultDir, file);
					var fileInfo = new FileInfo(file);
					TestContext.WriteLine($"--- File: {relativePath} ({fileInfo.Length} bytes) ---");
					
					// Log content of log and txt files
					if (file.EndsWith(".log", StringComparison.OrdinalIgnoreCase) || 
					    file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
					{
						try
						{
							var content = File.ReadAllText(file);
							// Limit output to prevent extremely long logs
							if (content.Length > 10000)
							{
								TestContext.WriteLine($"[Truncated - showing last 10000 chars of {content.Length}]");
								content = content.Substring(content.Length - 10000);
							}
							TestContext.WriteLine(content);
						}
						catch (Exception ex)
						{
							TestContext.WriteLine($"Failed to read file: {ex.Message}");
						}
					}
				}
				TestContext.WriteLine("=== End of XHarness Output Directory ===");
			}
			catch (Exception ex)
			{
				TestContext.WriteLine($"Error logging XHarness output directory: {ex.Message}");
			}
		}

	}
}
