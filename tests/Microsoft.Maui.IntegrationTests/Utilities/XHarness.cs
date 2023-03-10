
namespace Microsoft.Maui.IntegrationTests
{
	public static class XHarness
	{
		static readonly string XHarnessTool = "xharness";
		const int DEFAULT_TIMEOUT = 300;

		public static bool RunAndroid(string packageName, string resultDir, int expectedExitCode, int launchTimeoutSeconds = 90)
		{
			var timeoutString = TimeSpan.FromSeconds(launchTimeoutSeconds).ToString();
			var args = $"android run --package-name={packageName} --output-directory=\"{resultDir}\" --expected-exit-code={expectedExitCode} --timeout=\"{timeoutString}\" --verbosity=Debug";
			return Run(args, launchTimeoutSeconds + 30);
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
			return ToolRunner.Run(XHarnessTool, args, out exitCode, timeoutInSeconds: timeoutInSeconds);
		}

	}
}
