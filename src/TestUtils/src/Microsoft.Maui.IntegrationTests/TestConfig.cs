namespace Microsoft.Maui.IntegrationTests
{
	/// <summary>
	/// Configuration settings for running integration tests locally.
	/// Toggle these values as needed for your local environment.
	/// Environment variables take precedence over these values.
	/// </summary>
	public static class TestConfig
	{
		/// <summary>
		/// When true, skips Xcode version validation during iOS/macOS builds.
		/// Useful when your Xcode version doesn't exactly match what the SDK expects.
		/// Can also be set via SKIP_XCODE_VERSION_CHECK environment variable.
		/// </summary>
		public static bool SkipXcodeVersionCheck => 
			Environment.GetEnvironmentVariable("SKIP_XCODE_VERSION_CHECK")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true
			|| false; // ← Toggle to true if needed locally

		/// <summary>
		/// Specifies the iOS test device target for XHarness (e.g., "ios-simulator-64_18.5").
		/// Can also be set via IOS_TEST_DEVICE environment variable.
		/// </summary>
		public static string? IosTestDevice => 
			Environment.GetEnvironmentVariable("IOS_TEST_DEVICE")
			?? null; // ← Set to a specific device string if needed locally (e.g., "ios-simulator-64_18.5")
	}
}
