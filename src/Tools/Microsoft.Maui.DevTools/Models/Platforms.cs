// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.DevTools.Models;

/// <summary>
/// Platform constants to avoid magic strings.
/// </summary>
public static class Platforms
{
	public const string Android = "android";
	public const string iOS = "ios";
	public const string MacCatalyst = "maccatalyst";
	public const string Windows = "windows";
	public const string All = "all";

	public static bool IsValid(string? platform) => platform?.ToLowerInvariant() switch
	{
		Android or iOS or MacCatalyst or Windows or All => true,
		_ => false
	};

	public static string Normalize(string? platform) => platform?.ToLowerInvariant() switch
	{
		"apple" or "iphone" or "ipad" => iOS,
		"mac" or "macos" or "catalyst" => MacCatalyst,
		"win" or "win32" or "win64" => Windows,
		_ => platform?.ToLowerInvariant() ?? All
	};
}
