// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xamarin.Android.Tools;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Delegates to <see cref="AndroidEnvironmentHelper"/> from Xamarin.Android.Tools.
/// Provides short aliases used throughout the MAUI DevTools.
/// </summary>
internal static class AndroidEnvironment
{
	public static Dictionary<string, string>? GetEnvironment(string? sdkPath, string? jdkPath)
		=> AndroidEnvironmentHelper.GetEnvironment(sdkPath, jdkPath);

	public static string? MapAbiToArchitecture(string? abi)
		=> AndroidEnvironmentHelper.MapAbiToArchitecture(abi);

	public static string[]? GetRuntimeIdentifiers(string? architecture)
		=> AndroidEnvironmentHelper.GetRuntimeIdentifiers(architecture);

	public static string? MapApiLevelToVersion(string? apiLevel)
		=> AndroidEnvironmentHelper.MapApiLevelToVersion(apiLevel);

	public static string? MapTagIdToSubModel(string? tagId, bool playStoreEnabled = false)
		=> AndroidEnvironmentHelper.MapTagIdToDisplayName(tagId, playStoreEnabled);
}
