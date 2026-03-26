// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using Microsoft.Maui.Client.Output;
using Microsoft.Maui.Client.Providers.Android;
using Microsoft.Maui.Client.Utils;
using Spectre.Console;

namespace Microsoft.Maui.Client.Commands;

/// <summary>
/// Implementation of 'maui android' command group.
/// Sub-commands are in partial class files: Install, Jdk, Sdk, Emulator.
/// </summary>
public static partial class AndroidCommands
{
	public static Command Create()
	{
		var command = new Command("android", "Android SDK and device management");

		command.Add(CreateInstallCommand());
		command.Add(CreateJdkCommand());
		command.Add(CreateSdkCommand());
		command.Add(CreateEmulatorCommand());

		return command;
	}


	/// <summary>
	/// Resolves packages to install: uses explicit --packages if provided, otherwise shows interactive prompts.
	/// In CI mode, returns defaults without interactive prompts.
	/// </summary>
	static async Task<List<string>> ResolveInstallPackagesAsync(
		string[]? explicitPackages,
		SpectreOutputFormatter spectre,
		IAndroidProvider androidProvider,
		bool isCi,
		CancellationToken cancellationToken)
	{
		// If user specified packages explicitly, use those
		if (explicitPackages is { Length: > 0 })
			return explicitPackages.ToList();

		// In CI mode, use defaults without interactive prompts
		if (isCi)
			return GetDefaultPackages();

		// Try to fetch available packages for interactive selection
		List<SdkPackage> installed;
		List<SdkPackage> available;
		try
		{
			(installed, available) = await spectre.StatusAsync("Fetching available packages...", async () =>
			{
				var inst = await androidProvider.GetInstalledPackagesAsync(cancellationToken);
				var avail = await androidProvider.GetAvailablePackagesAsync(cancellationToken);
				return (inst, avail);
			});
		}
		catch (Exception ex)
		{
			System.Diagnostics.Trace.WriteLine($"Package list fetch failed, using defaults: {ex.Message}");
			return GetDefaultPackages();
		}

		// Merge: all known platforms (installed + available), dedup by path
		var allPackages = installed.Concat(available)
			.GroupBy(p => p.Path)
			.Select(g => g.FirstOrDefault(p => p.IsInstalled) ?? g.First()) // prefer installed entry
			.ToList();

		// Build platform choices: platforms;android-XX
		var platforms = allPackages
			.Where(p => p.Path.StartsWith("platforms;android-", StringComparison.Ordinal))
			.OrderByDescending(p =>
			{
				var apiStr = p.Path.Substring("platforms;android-".Length);
				// Handle both integer (36) and dotted (36.1) API levels
				if (Version.TryParse(apiStr, out var v))
					return v;
				if (int.TryParse(apiStr, out var n))
					return new Version(n, 0);
				return new Version(0, 0);
			})
			.ToList();

		if (platforms.Count == 0)
			return GetDefaultPackages();

		// Prompt 1: Select Android platform
		var platformChoices = platforms.Select(p =>
		{
			var apiStr = p.Path.Substring("platforms;android-".Length);
			var status = p.IsInstalled ? "[green]Installed[/]" : "[dim]Not installed[/]";
			// Display: "Android 35  Android SDK Platform 35  Installed"
			return new PlatformChoice(
				p.Path,
				$"Android {apiStr}",
				p.Description ?? $"Android SDK Platform {apiStr}",
				p.IsInstalled);
		}).ToList();

		var selectedPlatform = spectre.Prompt(
			new SelectionPrompt<PlatformChoice>()
				.Title("[bold]Select an Android platform to install[/]")
				.PageSize(15)
				.HighlightStyle(new Style(Color.DodgerBlue1))
				.UseConverter(c =>
				{
					var status = c.IsInstalled ? "[green]Installed[/]" : "[dim]Not installed[/]";
					return $"[bold]{Markup.Escape(c.DisplayName)}[/]  {Markup.Escape(c.Description)}  {status}";
				})
				.AddChoices(platformChoices));

		var apiLevel = selectedPlatform.PackagePath.Substring("platforms;android-".Length);

		// Find matching build-tools for this API level
		var matchingBuildTools = allPackages
			.Where(p => p.Path.StartsWith($"build-tools;{apiLevel}.", StringComparison.Ordinal)
				|| p.Path == $"build-tools;{apiLevel}.0.0")
			.OrderByDescending(p => p.Version)
			.FirstOrDefault();

		var buildToolsPath = matchingBuildTools?.Path ?? $"build-tools;{apiLevel}.0.0";
		var arch = PlatformDetector.IsArm64 ? "arm64-v8a" : "x86_64";
		var sysImagePath = $"system-images;android-{apiLevel};google_apis;{arch}";

		// Prompt 2: Select install scope
		var scopeChoices = new List<InstallScope>
		{
			new("Platform only",
				$"Install {selectedPlatform.PackagePath}",
				new List<string> { selectedPlatform.PackagePath }),
			new("Platform + Build Tools",
				$"Install platform and {buildToolsPath}",
				new List<string> { selectedPlatform.PackagePath, buildToolsPath, "platform-tools" }),
			new("Full Development Setup",
				"Platform, Build Tools, System Image, and Emulator",
				new List<string>
				{
					selectedPlatform.PackagePath,
					buildToolsPath,
					"platform-tools",
					"emulator",
					sysImagePath
				})
		};

		var selectedScope = spectre.Prompt(
			new SelectionPrompt<InstallScope>()
				.Title("[bold]Select what to install[/]")
				.HighlightStyle(new Style(Color.DodgerBlue1))
				.UseConverter(s => $"[bold]{Markup.Escape(s.Name)}[/]  [dim]{Markup.Escape(s.Description)}[/]")
				.AddChoices(scopeChoices));

		spectre.WriteInfo($"Will install: {string.Join(", ", selectedScope.Packages)}");
		return selectedScope.Packages;
	}

	static List<string> GetDefaultPackages() => new()
	{
		"platform-tools",
		"emulator",
		"platforms;android-35",
		"build-tools;35.0.0",
		$"system-images;android-35;google_apis;{(PlatformDetector.IsArm64 ? "arm64-v8a" : "x86_64")}"
	};

	record PlatformChoice(string PackagePath, string DisplayName, string Description, bool IsInstalled);
	record InstallScope(string Name, string Description, List<string> Packages);

	/// <summary>
	/// Checks if the SDK is in a protected location and attempts elevation if needed.
	/// Returns true if elevation was launched (caller should return), false if no elevation needed.
	/// Throws UnauthorizedAccessException if elevation was cancelled.
	/// </summary>
	static bool TryRequestElevation(IAndroidProvider androidProvider, IOutputFormatter formatter, bool useJson)
	{
		if (!PlatformDetector.IsWindows || !androidProvider.SdkPathRequiresElevation || ProcessRunner.IsRunningElevated())
			return false;

		if (!useJson)
			formatter.WriteWarning($"Android SDK is in a protected location ({androidProvider.SdkPath}). Administrator access required.");

		if (ProcessRunner.RelaunchElevated())
			return true;

		throw new UnauthorizedAccessException(
			$"Administrator access is required for {androidProvider.SdkPath}. Run this command from an administrator terminal, or set ANDROID_HOME to a user-writable location.");
	}
}
