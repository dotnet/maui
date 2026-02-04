// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Providers.Apple;

/// <summary>
/// Wrapper for xcode-select commands.
/// </summary>
public class XcodeSelect
{
	/// <summary>
	/// Gets the current Xcode developer directory.
	/// </summary>
	public string? GetDeveloperDirectory()
	{
		if (!PlatformDetector.IsMacOS)
			return null;

		var result = ProcessRunner.RunSync(
			"xcode-select", "-p",
			timeout: TimeSpan.FromSeconds(10));

		if (result.Success && !string.IsNullOrWhiteSpace(result.StandardOutput))
		{
			return result.StandardOutput.Trim();
		}

		return null;
	}

	/// <summary>
	/// Gets the Xcode version.
	/// </summary>
	public async Task<string?> GetVersionAsync(CancellationToken cancellationToken = default)
	{
		if (!PlatformDetector.IsMacOS)
			return null;

		var result = await ProcessRunner.RunAsync(
			"xcodebuild", "-version",
			timeout: TimeSpan.FromSeconds(10),
			cancellationToken: cancellationToken);

		if (result.Success)
		{
			var lines = result.StandardOutput.Split('\n');
			if (lines.Length > 0)
			{
				// Format: "Xcode 15.0\nBuild version 15A240d"
				return lines[0].Replace("Xcode ", string.Empty, StringComparison.Ordinal).Trim();
			}
		}

		return null;
	}

	/// <summary>
	/// Switches to the specified Xcode installation.
	/// </summary>
	public async Task SwitchAsync(string xcodePath, CancellationToken cancellationToken = default)
	{
		var result = await ProcessRunner.RunAsync(
			"sudo", $"xcode-select --switch \"{xcodePath}\"",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new Errors.MauiToolException(
				Errors.ErrorCodes.XcodeSelectFailed,
				$"Failed to switch Xcode: {result.StandardError}",
				nativeError: result.StandardError);
		}
	}
}
