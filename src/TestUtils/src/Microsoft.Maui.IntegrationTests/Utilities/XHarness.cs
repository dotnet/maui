using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace Microsoft.Maui.IntegrationTests
{
	public static class XHarness
	{
		static readonly string XHarnessTool = "xharness";
		const int DEFAULT_TIMEOUT = 300;

		public static bool RunAndroid(string packageName, string resultDir, int expectedExitCode, int launchTimeoutSeconds = 120, ITestOutputHelper? output = null)
		{
			var timeoutString = TimeSpan.FromSeconds(launchTimeoutSeconds).ToString();
			var args = $"android run --package-name={packageName} --output-directory=\"{resultDir}\" --expected-exit-code={expectedExitCode} --timeout=\"{timeoutString}\" --verbosity=Debug";
			return Run(args, launchTimeoutSeconds + 30, output: output);
		}

		/// <summary>
		/// Launches an instrumented iOS template and requires both its completion marker
		/// and a successful XHarness exit. A launch timeout alone does not prove the app ran.
		/// </summary>
		/// <param name="appPath">Path to the .app bundle</param>
		/// <param name="resultDir">Directory for XHarness output logs</param>
		/// <param name="targetDevice">XHarness target device string (e.g., "ios-simulator-64_18.5")</param>
		/// <param name="completionMarker">Unique marker emitted by this app after its smoke-test interval</param>
		/// <param name="deviceUdid">Optional specific device UDID to use</param>
		/// <param name="output">Optional test output helper for logging</param>
		/// <returns>True if the app completed its smoke test and exited successfully</returns>
		public static bool RunApple(string appPath, string resultDir, string targetDevice, string completionMarker, string? deviceUdid = null, ITestOutputHelper? output = null)
		{
			// XHarness applies --timeout before discovery and installation, not after app startup.
			var timeoutString = TimeSpan.FromSeconds(DEFAULT_TIMEOUT).ToString();

			string deviceArg = $"--target={targetDevice}";
			if (!string.IsNullOrEmpty(deviceUdid))
			{
				deviceArg += $" --device=\"{deviceUdid}\"";
			}

			var args = $"apple run --app=\"{appPath}\" --output-directory=\"{resultDir}\" {deviceArg} --timeout=\"{timeoutString}\" --verbosity=Debug";
			var xhOutput = RunForOutput(args, out int exitCode, DEFAULT_TIMEOUT + 30, output: output);
			bool isSuccess = AppleRunCompleted(exitCode, resultDir, completionMarker);

			if (!isSuccess)
			{
				output?.WriteLine($"iOS smoke test did not complete: XHarness exit code {exitCode}, expected marker '{completionMarker}' in '{resultDir}'.");
				output?.WriteLine(xhOutput);
			}

			return isSuccess;
		}

		internal static bool AppleRunCompleted(int exitCode, string resultDir, string completionMarker)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(completionMarker);
			// XHarness timestamps each line; iOS may also prefix Console output with an NSLog header.
			var markerLine = new Regex(
				@"^(?:\[\d{2}:\d{2}:\d{2}\.\d+\] )?(?:\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d+[+-]\d{4} [^\[\r\n]+\[\d+:\d+\] )?" +
				Regex.Escape(completionMarker) + @"\s*$", RegexOptions.CultureInvariant);
			return exitCode == 0 && Directory.Exists(resultDir) &&
				Directory.EnumerateFiles(resultDir, "*.log").Any(path =>
					File.ReadLines(path).Any(markerLine.IsMatch));
		}

		public static bool InstallSimulator(string targetDevice, ITestOutputHelper? output = null)
		{
			return Run($"apple simulators install \"{targetDevice}\" ", output: output);
		}

		public static string GetSimulatorUDID(string targetDevice, ITestOutputHelper? output = null)
		{
			var logDir = TestEnvironment.GetLogDirectory();
			Directory.CreateDirectory(logDir);
			var diagnosticsPath = Path.Combine(logDir, $"xharness-device-{targetDevice.Replace("/", "-", StringComparison.Ordinal)}.log");
			return RunForOutput($"apple device \"{targetDevice}\" --diagnostics=\"{diagnosticsPath}\"", out _, timeoutInSeconds: 30, output: output);
		}

		public static bool Run(string args, int timeoutInSeconds = DEFAULT_TIMEOUT, ITestOutputHelper? output = null)
		{
			var xhOutput = RunForOutput(args, out int exitCode, timeoutInSeconds, output: output);
			if (exitCode != 0)
				output?.WriteLine(xhOutput);

			return exitCode == 0;
		}

		public static string RunForOutput(string args, out int exitCode, int timeoutInSeconds = DEFAULT_TIMEOUT, ITestOutputHelper? output = null)
		{
			return DotnetInternal.RunForOutput(XHarnessTool, args, out exitCode, timeoutInSeconds, output: output);
		}
	}
}