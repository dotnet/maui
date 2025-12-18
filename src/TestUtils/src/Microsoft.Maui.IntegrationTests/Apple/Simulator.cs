using System.Text.RegularExpressions;

namespace Microsoft.Maui.IntegrationTests.Apple
{
	public class Simulator
	{
		readonly string XCRunTool = "xcrun";

		// Regex pattern for valid simulator UDID (UUID format: 8-4-4-4-12 hex characters)
		static readonly Regex UdidPattern = new Regex(@"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$", RegexOptions.Compiled);

		public string XHarnessID { get; set; } = System.Environment.GetEnvironmentVariable("IOS_TEST_DEVICE") ?? "ios-simulator-64";

		string _udid = "";
		public string GetUDID()
		{
			if (!string.IsNullOrEmpty(_udid))
				return _udid;

			var output = XHarness.GetSimulatorUDID(XHarnessID);

			// The output may contain error messages before the actual UDID.
			// Search for a valid UDID pattern in the output.
			foreach (var line in output.Split('\n', '\r'))
			{
				var trimmedLine = line.Trim();
				if (UdidPattern.IsMatch(trimmedLine))
				{
					_udid = trimmedLine;
					return _udid;
				}
			}

			// If no valid UDID found, return the trimmed output (will likely cause a meaningful error downstream)
			TestContext.WriteLine($"Warning: Could not find valid UDID in xharness output. Full output:\n{output}");
			return _udid = output.Trim();
		}

		public bool Launch()
		{
			ToolRunner.Run(XCRunTool, $"simctl boot {GetUDID()}", out int exitCode, timeoutInSeconds: 30);
			return exitCode == 0;
		}

		public bool Shutdown()
		{
			ToolRunner.Run(XCRunTool, $"simctl shutdown {GetUDID()}", out int exitCode, timeoutInSeconds: 60);
			return exitCode == 0;
		}

		public bool ShowWindow()
		{
			ToolRunner.Run("open", $"-a Simulator", out int exitCode, timeoutInSeconds: 30);
			return exitCode == 0;
		}
	}
}

