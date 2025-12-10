
namespace Microsoft.Maui.IntegrationTests.Apple
{
	public class Simulator
	{
		readonly string XCRunTool = "xcrun";
		readonly string PlistBuddyTool = "/usr/libexec/PlistBuddy";

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

		/// <summary>
		/// Disables simulator watchdogs by modifying the springboard plist.
		/// This is a workaround for an mlaunch bug where it shuts down the simulator to disable
		/// watchdogs but then incorrectly tries to launch on the shutdown simulator.
		/// By pre-disabling watchdogs, mlaunch will see they're already disabled and skip the
		/// problematic shutdown/reboot cycle.
		/// See: https://github.com/dotnet/macios mlaunch watchdog handling issue
		/// </summary>
		public bool DisableWatchdogs()
		{
			var udid = GetUDID();
			var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			var springboardPlist = Path.Combine(homeDir, "Library", "Developer", "CoreSimulator", "Devices", udid, "data", "Library", "Preferences", "com.apple.springboard.plist");

			TestContext.WriteLine($"Disabling watchdogs for simulator {udid}");
			TestContext.WriteLine($"Springboard plist path: {springboardPlist}");

			// First try to add the key, if it fails (key exists), try to set it
			var output = ToolRunner.Run(PlistBuddyTool, $"-c \"Add :FBLaunchWatchdogScale real 0\" \"{springboardPlist}\"", out int exitCode, timeoutInSeconds: 10);
			if (exitCode != 0)
			{
				// Key might already exist, try setting it instead
				output = ToolRunner.Run(PlistBuddyTool, $"-c \"Set :FBLaunchWatchdogScale 0\" \"{springboardPlist}\"", out exitCode, timeoutInSeconds: 10);
			}

			if (exitCode == 0)
			{
				TestContext.WriteLine("Successfully disabled FBLaunchWatchdogScale");
			}
			else
			{
				TestContext.WriteLine($"Warning: Failed to set FBLaunchWatchdogScale: {output}");
			}

			// Also try to set LaunchServicesApplicationWatchdogTimeout
			output = ToolRunner.Run(PlistBuddyTool, $"-c \"Add :LaunchServicesApplicationWatchdogTimeout integer 0\" \"{springboardPlist}\"", out exitCode, timeoutInSeconds: 10);
			if (exitCode != 0)
			{
				output = ToolRunner.Run(PlistBuddyTool, $"-c \"Set :LaunchServicesApplicationWatchdogTimeout 0\" \"{springboardPlist}\"", out exitCode, timeoutInSeconds: 10);
			}

			if (exitCode == 0)
			{
				TestContext.WriteLine("Successfully disabled LaunchServicesApplicationWatchdogTimeout");
			}
			else
			{
				TestContext.WriteLine($"Warning: Failed to set LaunchServicesApplicationWatchdogTimeout: {output}");
			}

			// Return true even if some settings failed - the test might still work
			return true;
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

				// Print diagnostic output to help troubleshoot boot issues
				TestContext.WriteLine($"Simulator not ready yet: {output}");

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

