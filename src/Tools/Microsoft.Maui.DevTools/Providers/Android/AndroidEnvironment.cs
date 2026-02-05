// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Shared environment and mapping utilities for Android SDK tools.
/// </summary>
internal static class AndroidEnvironment
{
	/// <summary>
	/// Builds the environment variables needed by Android SDK tools (sdkmanager, avdmanager, etc.).
	/// </summary>
	public static Dictionary<string, string>? GetEnvironment(string? sdkPath, string? jdkPath)
	{
		var env = new Dictionary<string, string>();

		if (!string.IsNullOrEmpty(jdkPath))
		{
			env["JAVA_HOME"] = jdkPath;
			env["PATH"] = $"{Path.Combine(jdkPath, "bin")}{Path.PathSeparator}{Environment.GetEnvironmentVariable("PATH")}";
		}

		if (!string.IsNullOrEmpty(sdkPath))
		{
			env["ANDROID_HOME"] = sdkPath;
			env["ANDROID_SDK_ROOT"] = sdkPath;
		}

		return env.Count > 0 ? env : null;
	}

	/// <summary>
	/// Maps an Android ABI string to a short CPU architecture name.
	/// </summary>
	public static string? MapAbiToArchitecture(string? abi)
	{
		return abi switch
		{
			"arm64-v8a" => "arm64",
			"armeabi-v7a" => "arm",
			"x86_64" => "x64",
			"x86" => "x86",
			_ => abi
		};
	}

	/// <summary>
	/// Gets .NET runtime identifiers for a given architecture.
	/// </summary>
	public static string[]? GetRuntimeIdentifiers(string? architecture)
	{
		return architecture switch
		{
			"arm64" => ["android-arm64"],
			"arm" => ["android-arm"],
			"x64" => ["android-x64"],
			"x86" => ["android-x86"],
			_ => null
		};
	}

	/// <summary>
	/// Maps an Android API level to an OS version string (e.g., "33" → "13.0", "35" → "15.0").
	/// Returns the release version without "Android" prefix, matching ServiceHub expectations.
	/// </summary>
	public static string? MapApiLevelToVersion(string? apiLevel)
	{
		if (string.IsNullOrEmpty(apiLevel))
			return null;

		return apiLevel switch
		{
			"36" => "16.0",
			"35" => "15.0",
			"34" => "14.0",
			"33" => "13.0",
			"32" => "12.1",
			"31" => "12.0",
			"30" => "11.0",
			"29" => "10.0",
			"28" => "9.0",
			"27" => "8.1",
			"26" => "8.0",
			"25" => "7.1",
			"24" => "7.0",
			"23" => "6.0",
			"22" => "5.1",
			"21" => "5.0",
			_ => null
		};
	}

	/// <summary>
	/// Maps a system image tag ID to a human-readable variant name (e.g., "google_apis" → "Google API's").
	/// </summary>
	public static string? MapTagIdToSubModel(string? tagId)
	{
		if (string.IsNullOrEmpty(tagId))
			return null;

		return tagId switch
		{
			"google_apis" => "Google API's",
			"google_apis_playstore" => "Google API's, Play Store",
			"default" => "AOSP",
			"android-wear" => "Wear OS",
			"android-tv" => "Android TV",
			"android-automotive" => "Android Automotive",
			"chromeos" => "Chrome OS",
			_ => tagId
		};
	}
}
