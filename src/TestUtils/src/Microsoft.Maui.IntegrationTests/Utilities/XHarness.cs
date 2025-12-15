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
		/// <param name="targetDevice"></param>
		/// <param name="launchTimeoutSeconds"></param>
		/// <returns>True if the app launch command timed out, false if it exits early.</returns>
		public static bool RunAppleForTimeout(string appPath, string resultDir, string targetDevice, int launchTimeoutSeconds = 75)
		{
			var timeoutString = TimeSpan.FromSeconds(launchTimeoutSeconds).ToString();
			var args = $"apple run --app=\"{appPath}\" --output-directory=\"{resultDir}\" --target={targetDevice} --timeout=\"{timeoutString}\" --verbosity=Debug";
			var xhOutput = RunForOutput(args, out int exitCode, launchTimeoutSeconds + 30);

			// Check for known failure exit codes - these should NEVER be treated as success
			// even if the timeout occurred, as they indicate the app failed to launch or crashed
			if (IsKnownFailureExitCode(exitCode))
			{
				TestContext.WriteLine($"XHarness returned failure exit code: {exitCode}");
				TestContext.WriteLine(xhOutput);
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
				TestContext.WriteLine(xhOutput);

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

	}
}
