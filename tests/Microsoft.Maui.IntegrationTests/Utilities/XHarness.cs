
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.IntegrationTests
{
	public static class XHarness
	{
		static readonly string XHarnessTool = "xharness";

		public static bool RunAndroid(string packageName, string resultDir, int expectedExitCode, int launchTimeoutSeconds = 90)
		{
			var timeoutString = TimeSpan.FromSeconds(launchTimeoutSeconds).ToString();
			var args = $"android run --package-name={packageName} --output-directory=\"{resultDir}\" --expected-exit-code={expectedExitCode} --timeout=\"{timeoutString}\" --verbosity=Debug";
			var androidRunOutput = Run(args, out int exitCode, timeoutInSeconds: launchTimeoutSeconds + 30);
			//if (exitCode != 0)
			TestContext.WriteLine(androidRunOutput);

			return exitCode == 0;
		}

		public static string Run(string args, out int exitCode, int timeoutInSeconds = 600)
		{
			return ToolRunner.Run(XHarnessTool, args, out exitCode, timeoutInSeconds: timeoutInSeconds);
		}

	}
}
