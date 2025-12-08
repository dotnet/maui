
namespace Microsoft.Maui.IntegrationTests.Apple
{
	public class Simulator
	{
		readonly string XCRunTool = "xcrun";

		public string XHarnessID { get; set; } = System.Environment.GetEnvironmentVariable("IOS_TEST_DEVICE") ?? "ios-simulator-64";

		string _udid = "";
		public string GetUDID()
		{
			if (!string.IsNullOrEmpty(_udid))
				return _udid;

			return _udid = XHarness.GetSimulatorUDID(XHarnessID).Trim();
		}

		public bool Launch()
		{
			ToolRunner.Run(XCRunTool, $"simctl boot {GetUDID()}", out int exitCode, timeoutInSeconds: 30);
			if (exitCode != 0)
				return false;

			// Wait for the simulator to be fully booted before returning
			// The boot command can return before the simulator is ready to accept app launches
			return WaitForSimulatorReady(timeoutInSeconds: 60);
		}

		bool WaitForSimulatorReady(int timeoutInSeconds = 60)
		{
			var udid = GetUDID();
			var startTime = DateTime.UtcNow;
			var timeout = TimeSpan.FromSeconds(timeoutInSeconds);

			while (DateTime.UtcNow - startTime < timeout)
			{
				var output = ToolRunner.Run(XCRunTool, $"simctl bootstatus {udid}", out int exitCode, timeoutInSeconds: 10);
				if (exitCode == 0)
					return true;

				// Wait a bit before checking again
				System.Threading.Thread.Sleep(2000);
			}

			return false;
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

