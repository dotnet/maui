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
}
