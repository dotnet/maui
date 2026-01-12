namespace Microsoft.Maui.IntegrationTests
{
	public static class XHarness
	{
		static readonly string XHarnessTool = "xharness";
		const int DEFAULT_TIMEOUT = 300;

		public static bool RunAndroid(string packageName, string resultDir, int expectedExitCode, int launchTimeoutSeconds = 120)
		{
			var timeoutString = TimeSpan.FromSeconds(launchTimeoutSeconds).ToString();
			var args = $"android run --package-name={packageName} --output-directory=\"{resultDir}\" --expected-exit-code={expectedExitCode} --timeout=\"{timeoutString}\" --verbosity=Debug";
			return Run(args, launchTimeoutSeconds + 30);
		}

		/// <summary>
		/// Attempts to launch an iOS app on the target device and verifies it runs without crashing.
		/// Since exit code detection doesn't work reliably on iOS 15+, we consider success as:
		/// - The app ran until timeout (exit code 90 = APP_LAUNCH_TIMEOUT)
		/// - The app was killed after running (exit code 83 with mlaunch 137 = SIGKILL from timeout)
		/// - The app exited cleanly (exit code 0)
		/// Failure is indicated by crash/error exit codes (80/APP_CRASH, 81/DEVICE_NOT_FOUND, 78/INSTALL_FAILURE).
		/// </summary>
		/// <param name="appPath">Path to the .app bundle</param>
		/// <param name="resultDir">Directory for XHarness output logs</param>
		/// <param name="targetDevice">XHarness target device string (e.g., "ios-simulator-64_18.5")</param>
		/// <param name="deviceUdid">Optional specific device UDID to use</param>
		/// <param name="launchTimeoutSeconds">How long to let the app run before killing it (default: 15s)</param>
		/// <returns>True if the app ran successfully (didn't crash), false otherwise</returns>
		public static bool RunAppleForTimeout(string appPath, string resultDir, string targetDevice, string? deviceUdid = null, int launchTimeoutSeconds = 15)
		{
			var timeoutString = TimeSpan.FromSeconds(launchTimeoutSeconds).ToString();
			
			string deviceArg = $"--target={targetDevice}";
			if (!string.IsNullOrEmpty(deviceUdid))
			{
				deviceArg += $" --device=\"{deviceUdid}\"";
			}
			
			var args = $"apple run --app=\"{appPath}\" --output-directory=\"{resultDir}\" {deviceArg} --timeout=\"{timeoutString}\" --verbosity=Debug";
			var xhOutput = RunForOutput(args, out int exitCode, launchTimeoutSeconds + 30);

			// XHarness exit codes - see https://github.com/dotnet/xharness/blob/main/src/Microsoft.DotNet.XHarness.Common/CLI/ExitCode.cs
			// Success cases:
			//   0  = SUCCESS (app exited cleanly)
			//   90 = APP_LAUNCH_TIMEOUT (app ran until we killed it - expected for UI apps)
			//   83 = APP_LAUNCH_FAILURE - but if caused by mlaunch 137 (SIGKILL), it's actually the timeout kill
			// Failure cases:
			//   78 = PACKAGE_INSTALLATION_FAILURE
			//   80 = APP_CRASH
			//   81 = DEVICE_NOT_FOUND
			
			// Check if exit code 83 is actually a timeout kill (mlaunch exited with 137 = SIGKILL)
			bool isTimeoutKill = exitCode == 83 && xhOutput.Contains("mlaunch exited with 137", StringComparison.Ordinal);
			bool isSuccess = exitCode == 0 || exitCode == 90 || isTimeoutKill;

			if (!isSuccess)
			{
				TestContext.WriteLine($"XHarness failed with exit code {exitCode}");
				TestContext.WriteLine(xhOutput);
			}

			return isSuccess;
		}

		public static bool InstallSimulator(string targetDevice)
		{
			return Run($"apple simulators install \"{targetDevice}\" ");
		}

		public static string GetSimulatorUDID(string targetDevice)
		{
			var logDir = TestEnvironment.GetLogDirectory();
			Directory.CreateDirectory(logDir);
			var diagnosticsPath = Path.Combine(logDir, $"xharness-device-{targetDevice.Replace("/", "-", StringComparison.Ordinal)}.log");
			return RunForOutput($"apple device \"{targetDevice}\" --diagnostics=\"{diagnosticsPath}\"", out _, timeoutInSeconds: 30);
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
	}
}