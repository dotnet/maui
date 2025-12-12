using System.Text.RegularExpressions;

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
		/// Attempts to launch an iOS app on the target device. Assumes success if the app run doesn't end within the specified timeout.
		/// Exit code detection is not working on iOS/tvOS 15+, however a crash will return early.
		/// </summary>
		/// <param name="appPath"></param>
		/// <param name="resultDir"></param>
		/// <param name="targetDevice"></param>
		/// <param name="deviceUDID">Optional UDID of a specific already-booted simulator to use.</param>
		/// <param name="launchTimeoutSeconds"></param>
		/// <returns>True if the app launch command timed out, false if it exits early.</returns>
		public static bool RunAppleForTimeout(string appPath, string resultDir, string targetDevice, string? deviceUDID = null, int launchTimeoutSeconds = 75)
		{
			var timeoutString = TimeSpan.FromSeconds(launchTimeoutSeconds).ToString();
			var deviceArg = string.IsNullOrEmpty(deviceUDID) ? "" : $" --device=\"{deviceUDID}\"";
			var args = $"apple run --app=\"{appPath}\" --output-directory=\"{resultDir}\" --target={targetDevice}{deviceArg} --timeout=\"{timeoutString}\" --verbosity=Debug";
			var xhOutput = RunForOutput(args, out int exitCode, launchTimeoutSeconds + 30);

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
