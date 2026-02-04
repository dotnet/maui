// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Maui.DevTools.Utils;

/// <summary>
/// Detects the current platform and provides platform-specific information.
/// </summary>
public static class PlatformDetector
{
	public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
	public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
	public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	public static bool IsArm64 => RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64;
	public static bool IsX64 => RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.X64;

	public static string OsName => IsMacOS ? "macOS" : IsWindows ? "Windows" : IsLinux ? "Linux" : "Unknown";

	public static string GetCurrentPlatformName()
	{
		var os = OsName;
		var arch = IsArm64 ? "arm64" : "x64";
		return $"{os} ({arch})";
	}

	public static string OsVersion
	{
		get
		{
			if (IsMacOS)
			{
				// Try to get macOS version from sw_vers
				try
				{
					var result = ProcessRunner.RunSync("sw_vers", "-productVersion");
					if (result.ExitCode == 0)
						return result.StandardOutput.Trim();
				}
				catch { }
			}
			return Environment.OSVersion.Version.ToString();
		}
	}

	public static string Architecture => RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();

	/// <summary>
	/// Gets platform-specific paths.
	/// </summary>
	public static class Paths
	{
		public static string HomeDirectory => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

		public static string DefaultAndroidSdkPath => IsMacOS
			? Path.Combine(HomeDirectory, "Library", "Developer", "Android", "sdk")
			: IsWindows
				? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "sdk")
				: Path.Combine(HomeDirectory, "Android", "Sdk");

		public static string DefaultJdkPath => IsMacOS
			? Path.Combine(HomeDirectory, "Library", "Developer", "Android", "jdk")
			: IsWindows
				? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "jdk")
				: Path.Combine(HomeDirectory, ".jdk");

		public static string MauiCacheDirectory => Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			".maui", "cache");

		/// <summary>
		/// Gets the Android SDK path from environment or default location.
		/// </summary>
		public static string? GetAndroidSdkPath()
		{
			// Check environment variables first
			var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME");
			if (!string.IsNullOrEmpty(androidHome) && Directory.Exists(androidHome))
				return androidHome;

			var androidSdkRoot = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
			if (!string.IsNullOrEmpty(androidSdkRoot) && Directory.Exists(androidSdkRoot))
				return androidSdkRoot;

			// Check default location
			if (Directory.Exists(DefaultAndroidSdkPath))
				return DefaultAndroidSdkPath;

			// Check Android Studio default locations
			if (IsMacOS)
			{
				var androidStudioPath = Path.Combine(HomeDirectory, "Library", "Android", "sdk");
				if (Directory.Exists(androidStudioPath))
					return androidStudioPath;
			}
			else if (IsWindows)
			{
				var androidStudioPath = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"Android", "Sdk");
				if (Directory.Exists(androidStudioPath))
					return androidStudioPath;
			}

			return null;
		}

		/// <summary>
		/// Gets the JDK path from environment or default location.
		/// </summary>
		public static string? GetJdkPath()
		{
			// Check JAVA_HOME first
			var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
			if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
			{
				var javaBin = Path.Combine(javaHome, "bin", "java");
				if (File.Exists(javaBin) || (IsWindows && File.Exists(javaBin + ".exe")))
					return javaHome;
			}

			// Check default location (may have nested structure)
			var jdkHome = FindJdkHome(DefaultJdkPath);
			if (jdkHome != null)
				return jdkHome;

			// Check common JDK locations on macOS
			if (IsMacOS)
			{
				var javaVirtualMachines = "/Library/Java/JavaVirtualMachines";
				if (Directory.Exists(javaVirtualMachines))
				{
					var jdks = Directory.GetDirectories(javaVirtualMachines)
						.Where(d => d.Contains("jdk", StringComparison.OrdinalIgnoreCase) || 
						            d.Contains("temurin", StringComparison.OrdinalIgnoreCase) || 
						            d.Contains("zulu", StringComparison.OrdinalIgnoreCase))
						.OrderByDescending(d => d)
						.FirstOrDefault();
					
					if (jdks != null)
					{
						var contentsHome = Path.Combine(jdks, "Contents", "Home");
						if (Directory.Exists(contentsHome))
							return contentsHome;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Finds the JDK home directory by looking for bin/java executable.
		/// Handles nested directory structures like jdk/jdk-17/jdk-17.0.18+8/Contents/Home
		/// </summary>
		private static string? FindJdkHome(string basePath, int maxDepth = 4)
		{
			if (!Directory.Exists(basePath))
				return null;

			// Check if this is the JDK home
			var javaBin = Path.Combine(basePath, "bin", "java");
			if (File.Exists(javaBin) || (IsWindows && File.Exists(javaBin + ".exe")))
				return basePath;

			// On macOS, check Contents/Home
			if (IsMacOS)
			{
				var contentsHome = Path.Combine(basePath, "Contents", "Home");
				if (Directory.Exists(contentsHome))
				{
					javaBin = Path.Combine(contentsHome, "bin", "java");
					if (File.Exists(javaBin))
						return contentsHome;
				}
			}

			// Recurse into subdirectories
			if (maxDepth > 0)
			{
				try
				{
					foreach (var subdir in Directory.GetDirectories(basePath))
					{
						var found = FindJdkHome(subdir, maxDepth - 1);
						if (found != null)
							return found;
					}
				}
				catch
				{
					// Ignore permission errors
				}
			}

			return null;
		}
	}
}
