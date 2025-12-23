using System.Text.Json;

namespace Microsoft.Maui.IntegrationTests.Apple
{
	public class Simulator
	{
		readonly string XCRunTool = "xcrun";

		public string XHarnessID { get; set; } = TestEnvironment.IosTestDevice ?? "ios-simulator-64";

		string _udid = "";
		public string GetUDID()
		{
			if (!string.IsNullOrEmpty(_udid))
				return _udid;

			var xharnessOutput = XHarness.GetSimulatorUDID(XHarnessID).Trim();
			
			// XHarness returns a UDID on success, or an error message on failure.
			// A valid UDID is a UUID format (e.g., "DE87D078-70D4-47F6-9F21-82612D9D4F7E")
			if (IsValidUDID(xharnessOutput))
			{
				_udid = xharnessOutput;
			}
			// If XHarness couldn't find a device and we're not on CI, try to auto-detect
			else if (!TestEnvironment.IsRunningOnCI)
			{
				TestContext.WriteLine($"XHarness failed to find device for '{XHarnessID}', attempting auto-detection...");
				_udid = TryGetLatestAvailableSimulatorUDID();
			}

			return _udid;
		}

		/// <summary>
		/// Checks if the string is a valid UDID (UUID format).
		/// </summary>
		static bool IsValidUDID(string value)
		{
			return !string.IsNullOrEmpty(value) && Guid.TryParse(value, out _);
		}

		/// <summary>
		/// Attempts to find the latest available iOS simulator by querying simctl directly.
		/// This is a fallback for local development when XHarness target doesn't match installed runtimes.
		/// </summary>
		string TryGetLatestAvailableSimulatorUDID()
		{
			try
			{
				var output = ToolRunner.Run(XCRunTool, "simctl list runtimes --json", out int exitCode, timeoutInSeconds: 30);
				if (exitCode != 0 || string.IsNullOrEmpty(output))
					return "";

				// Parse runtimes to find the latest iOS runtime
				using var doc = JsonDocument.Parse(output);
				var runtimes = doc.RootElement.GetProperty("runtimes");
				
				string? latestRuntimeId = null;
				Version latestVersion = new Version(0, 0);
				
				foreach (var runtime in runtimes.EnumerateArray())
				{
					if (!runtime.TryGetProperty("platform", out var platform) || platform.GetString() != "iOS")
						continue;
					if (!runtime.TryGetProperty("isAvailable", out var available) || !available.GetBoolean())
						continue;
					if (!runtime.TryGetProperty("version", out var versionProp))
						continue;
					
					var versionStr = versionProp.GetString();
					if (Version.TryParse(versionStr, out var version) && version > latestVersion)
					{
						latestVersion = version;
						latestRuntimeId = runtime.GetProperty("identifier").GetString();
					}
				}

				if (string.IsNullOrEmpty(latestRuntimeId))
					return "";

				// Now find a device for this runtime
				output = ToolRunner.Run(XCRunTool, "simctl list devices available --json", out exitCode, timeoutInSeconds: 30);
				if (exitCode != 0 || string.IsNullOrEmpty(output))
					return "";

				using var devicesDoc = JsonDocument.Parse(output);
				var devices = devicesDoc.RootElement.GetProperty("devices");

				if (!devices.TryGetProperty(latestRuntimeId, out var runtimeDevices))
					return "";

				// Prefer iPhone devices, pick the first available one
				foreach (var device in runtimeDevices.EnumerateArray())
				{
					var name = device.GetProperty("name").GetString() ?? "";
					if (name.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
					{
						var udid = device.GetProperty("udid").GetString() ?? "";
						if (!string.IsNullOrEmpty(udid))
						{
							TestContext.WriteLine($"Auto-detected simulator: {name} ({latestVersion}) - {udid}");
							return udid;
						}
					}
				}

				// Fallback to any device in this runtime
				foreach (var device in runtimeDevices.EnumerateArray())
				{
					var udid = device.GetProperty("udid").GetString() ?? "";
					if (!string.IsNullOrEmpty(udid))
					{
						var name = device.GetProperty("name").GetString() ?? "";
						TestContext.WriteLine($"Auto-detected simulator: {name} ({latestVersion}) - {udid}");
						return udid;
					}
				}
			}
			catch (Exception ex)
			{
				TestContext.WriteLine($"Failed to auto-detect simulator: {ex.Message}");
			}

			return "";
		}

		public bool Launch()
		{
			var output = ToolRunner.Run(XCRunTool, $"simctl boot {GetUDID()}", out int exitCode, timeoutInSeconds: 30);
			// Exit code 0 = successfully booted
			// Exit code 149 with "current state: Booted" = already running (also success)
			return exitCode == 0 || (exitCode == 149 && output.Contains("current state: Booted", StringComparison.Ordinal));
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

