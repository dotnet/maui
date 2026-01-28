using System.Text.Json;
using Xunit.Abstractions;

namespace Microsoft.Maui.IntegrationTests.Apple
{
	public class Simulator
	{
		readonly string XCRunTool = "xcrun";
		readonly ITestOutputHelper? _output;

		public Simulator(ITestOutputHelper? output = null)
		{
			_output = output;
		}

		string? _xharnessID;
		
		/// <summary>
		/// Gets the XHarness target ID (e.g., "ios-simulator-64_18.5").
		/// If IOS_TEST_DEVICE is not set or doesn't include a version, auto-detects
		/// the latest available iOS runtime on this machine.
		/// </summary>
		public string XHarnessID
		{
			get
			{
				if (_xharnessID == null)
				{
					_xharnessID = ResolveXHarnessID();
				}
				return _xharnessID;
			}
			set => _xharnessID = value;
		}

		string ResolveXHarnessID()
		{
			var envValue = TestEnvironment.IosTestDevice;
			
			// If env var is set and includes a version (has underscore), use it directly
			if (!string.IsNullOrEmpty(envValue) && envValue.Contains('_', StringComparison.Ordinal))
			{
				return envValue;
			}

			// Try to get the base target (e.g., "ios-simulator-64")
			var baseTarget = envValue ?? "ios-simulator-64";

			// Auto-detect the latest iOS version and append it
			var latestVersion = TryGetLatestIOSVersion();
			if (latestVersion != null)
			{
				var versionSuffix = $"{latestVersion.Major}.{latestVersion.Minor}";
				var resolvedTarget = $"{baseTarget}_{versionSuffix}";
				_output?.WriteLine($"Auto-detected iOS target: {resolvedTarget}");
				return resolvedTarget;
			}

			// Fallback to base target (may fail if XHarness can't find a match)
			_output?.WriteLine($"Warning: Could not auto-detect iOS version, using '{baseTarget}' which may fail.");
			return baseTarget;
		}

		/// <summary>
		/// Gets the latest available iOS runtime version from simctl.
		/// Prefers iOS 18.x over newer versions for XHarness compatibility (iPhone XS device type).
		/// </summary>
		Version? TryGetLatestIOSVersion()
		{
			try
			{
				var output = ToolRunner.Run(XCRunTool, "simctl list runtimes --json", out int exitCode, timeoutInSeconds: 30, output: _output);
				if (exitCode != 0 || string.IsNullOrEmpty(output))
					return null;

				using var doc = JsonDocument.Parse(output);
				var runtimes = doc.RootElement.GetProperty("runtimes");

				Version? preferredVersion = null; // iOS 18.x
				Version? latestVersion = null;    // Any iOS version

				foreach (var runtime in runtimes.EnumerateArray())
				{
					if (!runtime.TryGetProperty("platform", out var platform) || platform.GetString() != "iOS")
						continue;
					if (!runtime.TryGetProperty("isAvailable", out var available) || !available.GetBoolean())
						continue;
					if (!runtime.TryGetProperty("version", out var versionProp))
						continue;

					var versionStr = versionProp.GetString();
					if (Version.TryParse(versionStr, out var version))
					{
						// Track latest overall
						if (latestVersion == null || version > latestVersion)
							latestVersion = version;

						// Prefer iOS 18.x for XHarness compatibility (has iPhone XS device type)
						if (version.Major == 18 && (preferredVersion == null || version > preferredVersion))
							preferredVersion = version;
					}
				}

				// Use 18.x if available, otherwise fall back to latest
				return preferredVersion ?? latestVersion;
			}
			catch (Exception ex)
			{
				_output?.WriteLine($"Failed to detect iOS version: {ex.Message}");
				return null;
			}
		}

		string _udid = "";
		public string GetUDID()
		{
			if (!string.IsNullOrEmpty(_udid))
				return _udid;

			var xharnessOutput = XHarness.GetSimulatorUDID(XHarnessID, output: _output).Trim();
			
			// XHarness returns a UDID on success, or an error message on failure.
			// A valid UDID is a UUID format (e.g., "DE87D078-70D4-47F6-9F21-82612D9D4F7E")
			if (IsValidUDID(xharnessOutput))
			{
				_udid = xharnessOutput;
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

		public bool Launch()
		{
			var output = ToolRunner.Run(XCRunTool, $"simctl boot {GetUDID()}", out int exitCode, timeoutInSeconds: 30, output: _output);
			// Exit code 0 = successfully booted
			// Exit code 149 with "current state: Booted" = already running (also success)
			return exitCode == 0 || (exitCode == 149 && output.Contains("current state: Booted", StringComparison.Ordinal));
		}

		public bool Shutdown()
		{
			ToolRunner.Run(XCRunTool, $"simctl shutdown {GetUDID()}", out int exitCode, timeoutInSeconds: 60, output: _output);
			return exitCode == 0;
		}

		public bool ShowWindow()
		{
			ToolRunner.Run("open", $"-a Simulator", out int exitCode, timeoutInSeconds: 30, output: _output);
			return exitCode == 0;
		}
	}
}

