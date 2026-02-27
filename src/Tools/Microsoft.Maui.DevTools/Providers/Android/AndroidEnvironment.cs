// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Provides Android environment helpers for ABI/API mapping and environment setup.
/// </summary>
internal static class AndroidEnvironment
{
	static readonly Dictionary<string, string> AbiToArchMap = new(StringComparer.OrdinalIgnoreCase) {
		["armeabi-v7a"] = "arm",
		["arm64-v8a"] = "aarch64",
		["x86"] = "x86",
		["x86_64"] = "x86_64",
	};

	public static Dictionary<string, string>? GetEnvironment(string? sdkPath, string? jdkPath)
	{
		var env = new Dictionary<string, string>();
		if (!string.IsNullOrEmpty(sdkPath))
			env["ANDROID_HOME"] = sdkPath!;
		if (!string.IsNullOrEmpty(jdkPath))
			env["JAVA_HOME"] = jdkPath!;
		return env.Count > 0 ? env : null;
	}

	public static string? MapAbiToArchitecture(string? abi)
	{
		if (abi != null && AbiToArchMap.TryGetValue(abi, out var arch))
			return arch;
		return null;
	}

	public static string[]? GetRuntimeIdentifiers(string? architecture)
	{
		return architecture switch {
			"aarch64" or "arm64" => ["android-arm64"],
			"x86_64" => ["android-x64"],
			"x86" => ["android-x86"],
			"arm" => ["android-arm"],
			_ => null,
		};
	}

	public static string? MapApiLevelToVersion(string? apiLevel)
	{
		if (string.IsNullOrEmpty(apiLevel))
			return null;
		return apiLevel switch {
			"35" => "15",
			"34" => "14",
			"33" => "13",
			"32" or "31" => "12",
			"30" => "11",
			"29" => "10",
			"28" => "9",
			_ => null,
		};
	}

	public static string? MapTagIdToSubModel(string? tagId, bool playStoreEnabled = false)
	{
		if (string.IsNullOrEmpty(tagId))
			return null;
		if (playStoreEnabled)
			return "Google Play";
		return tagId switch {
			"google_apis" => "Google APIs",
			"google_apis_playstore" => "Google Play",
			"android-wear" => "Wear OS",
			"android-tv" => "Android TV",
			"android-automotive" => "Android Automotive",
			"default" => null,
			_ => tagId,
		};
	}
}
