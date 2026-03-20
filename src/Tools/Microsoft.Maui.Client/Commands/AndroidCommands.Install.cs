// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Output;
using Microsoft.Maui.Client.Providers.Android;
using Microsoft.Maui.Client.Utils;
using Spectre.Console;

namespace Microsoft.Maui.Client.Commands;

public static partial class AndroidCommands
{
	private static Command CreateInstallCommand()
	{
		var packagesOption = new Option<string[]>("--packages", "SDK packages to install (replaces defaults; comma-separated or multiple --packages flags)")
		{
			AllowMultipleArgumentsPerToken = true
		};

		var command = new Command("install", "Set up Android development environment")
		{
			new Option<string>("--sdk-path", "Custom SDK installation path"),
			new Option<string>("--jdk-path", "Custom JDK installation path"),
			new Option<int>("--jdk-version", () => 17, "JDK version to install (17 or 21)"),
			new Option<bool>("--accept-licenses", "Non-interactively accept all SDK licenses"),
			packagesOption
		};

		command.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var sdkPath = context.GetOption<string>("sdk-path");
			var jdkPath = context.GetOption<string>("jdk-path");
			var jdkVersion = context.GetOption<int>("jdk-version");
			var rawPackages = context.GetOption<string[]>("packages");
			var acceptLicenses = context.GetOption<bool>("accept-licenses");
			
			// Support comma-separated packages: "pkg1,pkg2,pkg3" becomes ["pkg1", "pkg2", "pkg3"]
			var packages = rawPackages?
				.SelectMany(p => p.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				.ToArray();				var formatter = Program.GetFormatter(context);

			try
			{
				if (dryRun)
				{
					formatter.WriteInfo("[dry-run] Would install Android environment:");
					formatter.WriteProgress($"JDK version: {jdkVersion}");
					formatter.WriteProgress($"JDK path: {jdkPath ?? "(default)"}");
					formatter.WriteProgress($"SDK path: {sdkPath ?? "(default)"}");
					formatter.WriteProgress($"Accept licenses: {acceptLicenses}");
					if (packages?.Any() == true)
						formatter.WriteProgress($"Extra packages: {string.Join(", ", packages)}");
					return;
				}

				if (!useJson && formatter is SpectreOutputFormatter spectre)
				{
					// Proactively detect if SDK is in a protected location and request elevation
					if (TryRequestElevation(androidProvider, formatter, useJson))
					{
						formatter.WriteSuccess("Android environment installed successfully (elevated)");
						return;
					}

					// Resolve package list: explicit --packages or interactive selection
					var pkgList = await ResolveInstallPackagesAsync(packages, spectre, androidProvider, context.GetCancellationToken());

					await spectre.LiveProgressAsync(async (ctx) =>
					{
						// Step 1: JDK
						var jdkTask = ctx.AddTask("Installing JDK");
						if (!androidProvider.IsJdkInstalled)
						{
							var jdkManager = Program.JdkManager;
							await jdkManager.InstallAsync(jdkVersion, jdkPath,
								onProgress: (pct, msg) => jdkTask.Update(pct, $"JDK: {msg}"),
								context.GetCancellationToken());
							jdkTask.Complete($"OpenJDK {jdkVersion} installed");
						}
						else
						{
							jdkTask.Complete("JDK already installed");
						}

						// Step 2: SDK command-line tools
						var sdkTask = ctx.AddTask("Installing SDK Tools");
						if (!androidProvider.IsSdkInstalled)
						{
							var targetSdkPath = sdkPath ?? PlatformDetector.Paths.DefaultAndroidSdkPath;
							await androidProvider.InstallSdkToolsAsync(targetSdkPath,
								onProgress: (phase, pct, msg) =>
								{
									var label = phase switch
									{
										"ReadingManifest" => "Reading manifest...",
										"Downloading" => $"Downloading: {msg}",
										"Verifying" => "Verifying checksum...",
										"Extracting" => "Extracting...",
										"Complete" => "SDK Tools installed",
										_ => msg
									};
									sdkTask.Update(Math.Max(0, pct), label);
								},
								context.GetCancellationToken());
							sdkTask.Complete("SDK Tools installed");
						}
						else
						{
							sdkTask.Complete("SDK Tools already installed");
						}

						// Step 3: Accept licenses
						var licenseTask = ctx.AddTask("Accepting licenses");
						licenseTask.SetIndeterminate("Checking licenses...");
						await androidProvider.AcceptLicensesAsync(
							onProgress: msg => licenseTask.SetIndeterminate(msg),
							context.GetCancellationToken());
						licenseTask.Complete("Licenses accepted");

						// Step 4: Install packages
						var pkgTask = ctx.AddTask($"Installing packages (0/{pkgList.Count})");
						pkgTask.Update(0, $"Installing packages (0/{pkgList.Count})...");
						await androidProvider.InstallPackagesAsync(pkgList, true,
							onProgress: (pkg, idx, total) =>
							{
								var pct = (double)idx / total * 100;
								pkgTask.Update(pct, $"Installing {pkg} ({idx + 1}/{total})");
							},
							context.GetCancellationToken());
						pkgTask.Complete($"{pkgList.Count} packages installed");
					});
				}
				else
				{
					var progress = new Progress<string>(message =>
					{
						formatter.WriteProgress(message);
					});

					await androidProvider.InstallAsync(
						sdkPath: sdkPath,
						jdkPath: jdkPath,
						jdkVersion: jdkVersion,
						additionalPackages: packages is { Length: > 0 } ? packages : null,
						progress: progress,
						cancellationToken: context.GetCancellationToken());
				}

				formatter.WriteSuccess("Android environment installed successfully");
			}
			catch (UnauthorizedAccessException uaEx) when (PlatformDetector.IsWindows)
			{
				if (!useJson)
					formatter.WriteWarning("Administrator access required. Requesting elevation...");

				if (ProcessRunner.RelaunchElevated())
				{
					formatter.WriteSuccess("Android environment installed successfully (elevated)");
					return;
				}

				formatter.WriteError(uaEx);
				context.ExitCode = 1;
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		return command;
	}
}
