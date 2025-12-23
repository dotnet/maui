using System.Runtime.InteropServices;

namespace Microsoft.Maui.IntegrationTests
{
	public static class TestEnvironment
	{
		public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

		public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

		public static bool IsRunningOnCI => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_NAME"));

		public static bool IsArm64 => RuntimeInformation.OSArchitecture == Architecture.Arm64;

		#region Local Development Overrides
		// ┌──────────────────────────────────────────────────────────────────────────────┐
		// │ Toggle these values for local development. Environment variables take        │
		// │ precedence over these values when set.                                       │
		// └──────────────────────────────────────────────────────────────────────────────┘

		/// <summary>
		/// When true, skips Xcode version validation during iOS/macOS builds.
		/// Useful when your Xcode version doesn't exactly match what the SDK expects.
		/// Can also be set via SKIP_XCODE_VERSION_CHECK environment variable.
		/// </summary>
		public static bool SkipXcodeVersionCheck =>
			Environment.GetEnvironmentVariable("SKIP_XCODE_VERSION_CHECK")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true
			|| true; // ← Toggle to true if needed locally

		/// <summary>
		/// Specifies the iOS test device target for XHarness (e.g., "ios-simulator-64_18.5").
		/// Can also be set via IOS_TEST_DEVICE environment variable.
		/// </summary>
		public static string? IosTestDevice =>
			Environment.GetEnvironmentVariable("IOS_TEST_DEVICE")
			?? null; // ← Set to a specific device string if needed locally (e.g., "ios-simulator-64_18.5")

		#endregion

		/// <summary>
		/// Gets the appropriate iOS simulator runtime identifier based on the host architecture.
		/// Returns "iossimulator-arm64" on ARM64 Macs and "iossimulator-x64" on Intel Macs.
		/// </summary>
		public static string IOSSimulatorRuntimeIdentifier =>
			IsArm64 ? "iossimulator-arm64" : "iossimulator-x64";


		static string _mauiDir = "";
		public static string GetMauiDirectory()
		{
			if (Directory.Exists(_mauiDir))
				return _mauiDir;

			return _mauiDir = GetTopDirRecursive(Path.GetFullPath(Path.GetDirectoryName(typeof(TestEnvironment).Assembly.Location) ?? ""));

			static string GetTopDirRecursive(string searchDirectory, int maxSearchDepth = 7)
			{
				if (File.Exists(Path.Combine(searchDirectory, "Microsoft.Maui.sln")))
					return searchDirectory;

				if (maxSearchDepth <= 0)
					throw new DirectoryNotFoundException("Unable to locate root maui directory!");

				return GetTopDirRecursive(Directory.GetParent(searchDirectory)?.FullName ?? "", --maxSearchDepth);
			}
		}

		static string? _logDirectory = null;
		public static string GetLogDirectory()
		{
			if (_logDirectory == null)
			{
				var envLogDirectory = Environment.GetEnvironmentVariable("LogDirectory");
				if (!string.IsNullOrEmpty(envLogDirectory))
				{
					// LogDirectory env var is already a complete log directory path, use it directly
					_logDirectory = envLogDirectory;
				}
				else
				{
					var artifactsStaging = Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY");
					if (artifactsStaging != null)
					{
						_logDirectory = $"{artifactsStaging}/logs";
					}
					else
					{
						_logDirectory = $"{GetTestDirectoryRoot()}/logs";
					}
				}
			}
			return _logDirectory;
		}

		static string _testOutputDirectory = "";
		public static string GetTestDirectoryRoot()
		{
			if (Directory.Exists(_testOutputDirectory))
				return _testOutputDirectory;

			// Set when running on Azure Pipelines https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables
			var rootDir = Environment.GetEnvironmentVariable("AGENT_TEMPDIRECTORY");
			if (Directory.Exists(rootDir))
			{
				_testOutputDirectory = Path.Combine(rootDir, $"test-dir");
			}
			else
			{
				_testOutputDirectory = Path.Combine(GetMauiDirectory(), "bin", "test-dir");
			}

			Directory.CreateDirectory(_testOutputDirectory);
			return _testOutputDirectory;
		}

		public static string GetAndroidSdkPath()
		{
			var sdkPath = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
			if (string.IsNullOrEmpty(sdkPath))
				sdkPath = Environment.GetEnvironmentVariable("ANDROID_HOME");
			if (string.IsNullOrEmpty(sdkPath))
				sdkPath = IsWindows
					? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Android", "android-sdk")
					: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Android", "sdk");

			return sdkPath;
		}

		public static string GetAndroidCommandLineToolsPath()
		{
			var sdkPath = GetAndroidSdkPath();
			var toolsPath = Path.Combine(sdkPath, "cmdline-tools", "latest", "bin");
			if (!Directory.Exists(toolsPath))
			{
				var toolsTopDir = Path.Combine(sdkPath, "cmdline-tools");
				if (Directory.Exists(toolsTopDir))
				{
					var versionedDirectories = Directory.EnumerateDirectories(toolsTopDir).Where(d => Version.TryParse(Path.GetFileName(d), out Version? v));
					var latestVersionDir = versionedDirectories.OrderByDescending(d => Version.Parse(Path.GetFileName(d))).FirstOrDefault();
					if (Directory.Exists(latestVersionDir))
						toolsPath = Path.Combine(latestVersionDir, "bin");
				}
			}

			return toolsPath;
		}
	}
}
