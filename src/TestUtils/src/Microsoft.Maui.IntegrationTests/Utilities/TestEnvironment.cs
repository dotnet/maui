using System.Runtime.InteropServices;

namespace Microsoft.Maui.IntegrationTests
{
	public static class TestEnvironment
	{
		public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

		public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

		public static bool IsRunningOnCI => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_NAME"));


		static string _mauiDir = "";
		public static string GetMauiDirectory()
		{
			if (Directory.Exists(_mauiDir))
				return _mauiDir;

			return _mauiDir = GetTopDirRecursive(Path.GetFullPath(Path.GetDirectoryName(typeof(TestEnvironment).Assembly.Location) ?? ""));

			static string GetTopDirRecursive(string searchDirectory, int maxSearchDepth = 7)
			{
				if (File.Exists(Path.Combine(searchDirectory, "Microsoft.Maui.slnx")))
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
				var artifactsStaging = Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY");
				var envLogDirectory = Environment.GetEnvironmentVariable("LogDirectory");
				if (envLogDirectory != null)
				{
					_logDirectory = envLogDirectory;
				}
				else
				{
					if (artifactsStaging != null)
					{
						_logDirectory = artifactsStaging;
					}
					else
					{
						_logDirectory = GetTestDirectoryRoot();
					}
				}
			}
			return $"{_logDirectory}/logs";
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
