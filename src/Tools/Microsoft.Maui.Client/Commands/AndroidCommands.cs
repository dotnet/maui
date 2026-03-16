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

/// <summary>
/// Implementation of 'maui android' command group.
/// </summary>
public static class AndroidCommands
{
	public static Command Create()
	{
		var command = new Command("android", "Android SDK and device management");

		command.AddCommand(CreateInstallCommand());
		command.AddCommand(CreateJdkCommand());
		command.AddCommand(CreateSdkCommand());
		command.AddCommand(CreateEmulatorCommand());

		return command;
	}

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
			var sdkPath = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "sdk-path"));
			var jdkPath = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "jdk-path"));
			var jdkVersion = context.ParseResult.GetValueForOption(
				(Option<int>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "jdk-version"));
			var rawPackages = context.ParseResult.GetValueForOption(
				(Option<string[]>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "packages"));
			var acceptLicenses = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "accept-licenses"));
			
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
					if (androidProvider.IsSdkInstalled && androidProvider.SdkPathRequiresElevation && !ProcessRunner.IsRunningElevated())
					{
						spectre.WriteWarning($"Android SDK is in a protected location ({androidProvider.SdkPath}). Administrator access is required to install packages.");
						if (ProcessRunner.RelaunchElevated())
						{
							formatter.WriteSuccess("Android environment installed successfully (elevated)");
							return;
						}
						else
						{
							formatter.WriteError(new Exception("Elevation was cancelled or failed. Run this command from an administrator terminal."));
							context.ExitCode = 1;
							return;
						}
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
			catch (UnauthorizedAccessException) when (PlatformDetector.IsWindows)
			{
				if (!useJson)
					formatter.WriteWarning("Administrator access required. Requesting elevation...");

				if (ProcessRunner.RelaunchElevated())
				{
					formatter.WriteSuccess("Android environment installed successfully (elevated)");
					return; // Exit after elevation — the elevated process handled the work
				}
				else
				{
					var error = new Exception("Elevation was cancelled or failed. Run this command from an administrator terminal.");
					formatter.WriteError(error);
					context.ExitCode = 1;
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		return command;
	}

	private static Command CreateJdkCommand()
	{
		var command = new Command("jdk", "Manage JDK installation");

		// jdk check
		var checkCommand = new Command("check", "Check JDK installation status");
		checkCommand.SetHandler(async (InvocationContext context) =>
		{
			var jdkManager = Program.JdkManager;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
				var formatter = Program.GetFormatter(context);

			var healthCheck = await jdkManager.CheckHealthAsync(context.GetCancellationToken());

			if (useJson)
			{
				formatter.Write(healthCheck);
			}
			else
			{
				var statusIcon = healthCheck.Status == Models.CheckStatus.Ok ? "✓" : 
								 healthCheck.Status == Models.CheckStatus.Warning ? "⚠" : "✗";
				formatter.WriteInfo($"{statusIcon} {healthCheck.Message}");
				
				if (healthCheck.Details?.TryGetValue("path", out var path) == true)
					formatter.WriteProgress($"Path: {path}");
			}
		});

		// jdk install
		var installCommand = new Command("install", "Install OpenJDK")
		{
			new Option<int>("--version", () => 17, "JDK version (17 or 21)"),
			new Option<string>("--path", "Installation path")
		};
		installCommand.SetHandler(async (InvocationContext context) =>
		{
			var jdkManager = Program.JdkManager;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var version = context.ParseResult.GetValueForOption(
				(Option<int>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "version"));
			var path = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "path"));				var formatter = Program.GetFormatter(context);

			try
			{
				if (dryRun)
				{
					formatter.WriteInfo($"[dry-run] Would install OpenJDK {version}");
					return;
				}

				formatter.WriteProgress($"Installing OpenJDK {version}...");
				await jdkManager.InstallAsync(version, path, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, version = version, path = jdkManager.DetectedJdkPath });
				}
				else
				{
					formatter.WriteSuccess($"OpenJDK {version} installed to {jdkManager.DetectedJdkPath}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		// jdk list
		var listCommand = new Command("list", "List available JDK versions");
		listCommand.SetHandler((InvocationContext context) =>
		{
			var jdkManager = Program.JdkManager;
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);

			var versions = jdkManager.GetAvailableVersions().ToList();

			if (useJson)
			{
				var formatter = Program.GetFormatter(context);
				formatter.Write(new { versions = versions });
			}
			else
			{
				var formatter = Program.GetFormatter(context);
				formatter.WriteTable(
					versions,
					("Version", v => v.ToString()),
					("Status", v => jdkManager.DetectedJdkVersion == v ? "✓ current" : ""));
			}
		});

		command.AddCommand(checkCommand);
		command.AddCommand(installCommand);
		command.AddCommand(listCommand);

		return command;
	}

	private static Command CreateSdkCommand()
	{
		var command = new Command("sdk", "Manage Android SDK");

		// sdk check
		var checkCommand = new Command("check", "Check Android SDK installation status");
		checkCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
				var formatter = Program.GetFormatter(context);

			var checks = await androidProvider.CheckHealthAsync(context.GetCancellationToken());
			var sdkCheck = checks.FirstOrDefault(c => c.Name == "Android SDK");

			if (sdkCheck != null)
			{
				if (useJson)
				{
					formatter.Write(sdkCheck);
				}
				else
				{
					var statusIcon = sdkCheck.Status == Models.CheckStatus.Ok ? "✓" : 
									 sdkCheck.Status == Models.CheckStatus.Warning ? "⚠" : "✗";
					formatter.WriteInfo($"{statusIcon} {sdkCheck.Message ?? "Android SDK"}");
					
					if (sdkCheck.Details?.TryGetValue("path", out var path) == true)
						formatter.WriteProgress($"Path: {path}");
				}
			}
		});

		// sdk install
		var sdkInstallPkgsArg = new Argument<string[]>("packages", "SDK packages to install (prompted interactively if omitted)");
		sdkInstallPkgsArg.Arity = ArgumentArity.ZeroOrMore;
		sdkInstallPkgsArg.SetDefaultValue(Array.Empty<string>());
		var installCommand = new Command("install", "Install SDK packages")
		{
			sdkInstallPkgsArg
		};
		installCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var packages = context.ParseResult.GetValueForArgument(sdkInstallPkgsArg);				var formatter = Program.GetFormatter(context);

			try
			{
				// Interactive selection if no packages specified
				if ((packages == null || packages.Length == 0) && !useJson && formatter is SpectreOutputFormatter spectre)
				{
					var (installed, available) = await spectre.StatusAsync("Fetching available packages...", async () =>
					{
						var inst = await androidProvider.GetInstalledPackagesAsync(context.GetCancellationToken());
						var avail = await androidProvider.GetAvailablePackagesAsync(context.GetCancellationToken());
						return (inst, avail);
					});

					var installedPaths = new HashSet<string>(installed.Select(p => p.Path), StringComparer.OrdinalIgnoreCase);

					// Merge and dedup
					var allPackages = installed.Concat(available)
						.GroupBy(p => p.Path)
						.Select(g => g.FirstOrDefault(p => p.IsInstalled) ?? g.First())
						.ToList();

					// Step 1: Pick API level
					var platforms = allPackages
						.Where(p => p.Path.StartsWith("platforms;android-", StringComparison.Ordinal))
						.OrderByDescending(p =>
						{
							var apiStr = p.Path.Substring("platforms;android-".Length);
							return int.TryParse(apiStr, out var n) ? n : 0;
						})
						.ToList();

					if (platforms.Count == 0)
					{
						throw new InvalidOperationException("No Android platforms found. Run 'maui android install' for full setup.");
					}

					var selectedPlatform = spectre.Prompt(
						new SelectionPrompt<SdkPackage>()
							.Title("[bold]Select an Android API level[/]")
							.PageSize(15)
							.HighlightStyle(new Style(Color.DodgerBlue1))
							.UseConverter(p =>
							{
								var apiStr = p.Path.Substring("platforms;android-".Length);
								var status = installedPaths.Contains(p.Path) ? "[green]✓ Installed[/]" : "[dim]Not installed[/]";
								return $"[bold]Android {Markup.Escape(apiStr)}[/]  {Markup.Escape(p.Description ?? "")}  {status}";
							})
							.AddChoices(platforms));

					var apiLevel = selectedPlatform.Path.Substring("platforms;android-".Length);

					// Step 2: Pick components for that API level
					var componentChoices = new List<(string Label, string Path, bool Installed)>
					{
						($"Platform (platforms;android-{apiLevel})", $"platforms;android-{apiLevel}", installedPaths.Contains($"platforms;android-{apiLevel}")),
						($"Platform Tools", "platform-tools", installedPaths.Contains("platform-tools")),
					};

					// Find matching build-tools
					var matchingBuildTools = allPackages
						.Where(p => p.Path.StartsWith($"build-tools;{apiLevel}.", StringComparison.Ordinal)
							|| p.Path == $"build-tools;{apiLevel}.0.0")
						.OrderByDescending(p => p.Version)
						.FirstOrDefault();
					var buildToolsPath = matchingBuildTools?.Path ?? $"build-tools;{apiLevel}.0.0";
					componentChoices.Add(($"Build Tools ({buildToolsPath})", buildToolsPath, installedPaths.Contains(buildToolsPath)));

					componentChoices.Add(("Emulator", "emulator", installedPaths.Contains("emulator")));

					// Find system images for this API level
					var sysImages = allPackages
						.Where(p => p.Path.StartsWith($"system-images;android-{apiLevel};", StringComparison.Ordinal))
						.ToList();
					foreach (var img in sysImages)
					{
						var parts = img.Path.Split(';');
						var variant = parts.Length > 2 ? parts[2] : "";
						var arch = parts.Length > 3 ? parts[3] : "";
						componentChoices.Add(($"System Image ({variant}/{arch})", img.Path, installedPaths.Contains(img.Path)));
					}

					// If no system images found in available list, add a default
					if (!sysImages.Any())
					{
						var defaultArch = PlatformDetector.IsArm64 ? "arm64-v8a" : "x86_64";
						var defaultImg = $"system-images;android-{apiLevel};google_apis;{defaultArch}";
						componentChoices.Add(($"System Image (google_apis/{defaultArch})", defaultImg, false));
					}

					var selectedComponents = spectre.Prompt(
						new MultiSelectionPrompt<(string Label, string Path, bool Installed)>()
							.Title("[bold]Select components to install[/]")
							.PageSize(15)
							.HighlightStyle(new Style(Color.DodgerBlue1))
							.InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
							.UseConverter(c =>
							{
								var status = c.Installed ? " [green]✓[/]" : "";
								return $"{Markup.Escape(c.Label)}{status}";
							})
							.AddChoices(componentChoices));

					packages = selectedComponents.Select(c => c.Path).ToArray();

					if (packages.Length == 0)
					{
						spectre.WriteWarning("No components selected. Nothing to install.");
						return;
					}

					spectre.WriteInfo($"Will install: {string.Join(", ", packages)}");
				}

				if (packages == null || packages.Length == 0)
				{
					throw new InvalidOperationException(
						"No packages specified. Provide package names or run interactively.");
				}

				// Proactively detect if SDK is in a protected location and request elevation
				if (PlatformDetector.IsWindows && androidProvider.SdkPathRequiresElevation && !ProcessRunner.IsRunningElevated())
				{
					if (!useJson)
						formatter.WriteWarning($"Android SDK is in a protected location ({androidProvider.SdkPath}). Administrator access required.");

					if (ProcessRunner.RelaunchElevated())
					{
						if (useJson)
							formatter.Write(new { success = true, installed = packages, elevated = true });
						else
							formatter.WriteSuccess("Packages installed successfully (elevated)");
						return;
					}
					else
					{
						throw new UnauthorizedAccessException(
							$"Administrator access is required to install packages to {androidProvider.SdkPath}. Run this command from an administrator terminal, or set ANDROID_HOME to a user-writable location.");
					}
				}

				if (dryRun)
				{
					formatter.WriteInfo($"[dry-run] Would install: {string.Join(", ", packages)}");
					return;
				}

				if (!useJson && formatter is SpectreOutputFormatter spectreInstall)
				{
					var total = packages.Length;
					for (var i = 0; i < total; i++)
					{
						var pkg = packages[i];
						var counter = $"[dim]({i + 1}/{total})[/]";
						await spectreInstall.Console.Status()
							.AutoRefresh(true)
							.Spinner(Spinner.Known.Dots)
							.SpinnerStyle(new Style(Color.Blue))
							.StartAsync($"[blue]Installing:[/] {Markup.Escape(pkg)}  {counter}", async _ =>
							{
								await androidProvider.InstallPackagesAsync(new[] { pkg }, true, context.GetCancellationToken());
							});
						spectreInstall.Console.MarkupLine($"  [green]✓[/] {Markup.Escape(pkg)}");
					}
				}
				else
				{
					await androidProvider.InstallPackagesAsync(packages, true, context.GetCancellationToken());
				}

				if (useJson)
				{
					formatter.Write(new { success = true, installed = packages });
				}
				else
				{
					formatter.WriteSuccess($"Installed: {string.Join(", ", packages)}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		// sdk list
		var listCommand = new Command("list", "List SDK packages")
		{
			new Option<bool>("--available", "List packages available for installation"),
			new Option<bool>("--all", "List both installed and available packages")
		};
		listCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var showAvailable = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "available"));
			var showAll = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "all"));				var formatter = Program.GetFormatter(context);

			try
			{
				var packages = new List<SdkPackage>();
				
				if (showAll)
				{
					// Get both installed and available
					var installed = await androidProvider.GetInstalledPackagesAsync(context.GetCancellationToken());
					var available = await androidProvider.GetAvailablePackagesAsync(context.GetCancellationToken());
					packages.AddRange(installed);
					packages.AddRange(available);
				}
				else if (showAvailable)
				{
					var available = await androidProvider.GetAvailablePackagesAsync(context.GetCancellationToken());
					var installed = await androidProvider.GetInstalledPackagesAsync(context.GetCancellationToken());
					var installedPaths = new HashSet<string>(installed.Select(p => p.Path), StringComparer.OrdinalIgnoreCase);

					// Mark available packages that are already installed
					packages = available.Select(p => installedPaths.Contains(p.Path) ? p with { IsInstalled = true } : p).ToList();
				}
				else
				{
					packages = await androidProvider.GetInstalledPackagesAsync(context.GetCancellationToken());
				}

				if (useJson)
				{
					formatter.Write(packages);
				}
				else
				{
					if (!packages.Any())
					{
						formatter.WriteWarning(showAvailable ? "No packages available." : "No packages installed.");
						return;
					}

					var spectre = formatter as SpectreOutputFormatter;

					void WriteGroupedPackages(IEnumerable<SdkPackage> pkgs, string title)
					{
						if (!pkgs.Any())
						{
							formatter.WriteWarning($"No {title.ToLowerInvariant()}.");
							return;
						}

						formatter.WriteInfo($"{title}:");

						// Group by category (part before first ';')
						var groups = pkgs
							.GroupBy(p => p.Path.Contains(';', StringComparison.Ordinal) ? p.Path.Substring(0, p.Path.IndexOf(';', StringComparison.Ordinal)) : p.Path)
							.OrderBy(g => g.Key);

						foreach (var group in groups)
						{
							var items = group.OrderByDescending(p => p.Version).ToList();
							if (items.Count == 1)
							{
								var p = items[0];
								spectre?.WriteMarkupLine($"  {Markup.Escape(p.Path)}  [dim]{Markup.Escape(p.Version ?? "")}[/]  [dim italic]{Markup.Escape(p.Description ?? "")}[/]");
							}
							else
							{
								// Sub-group by major version and show only latest per major
								var byMajor = items
									.GroupBy(p =>
									{
										var ver = p.Path.Contains(';', StringComparison.Ordinal) ? p.Path.Substring(p.Path.LastIndexOf(';') + 1) : p.Version ?? "";
										var dot = ver.IndexOf('.', StringComparison.Ordinal);
										return dot > 0 ? ver.Substring(0, dot) : ver;
									})
									.OrderByDescending(g => int.TryParse(g.Key, out var n) ? n : 0)
									.ToList();

								spectre?.WriteMarkupLine($"  [bold]{Markup.Escape(group.Key)}[/]  [dim]({byMajor.Count} major, {items.Count} total)[/]");
								foreach (var major in byMajor)
								{
									var latest = major.First();
									var latestVer = latest.Path.Contains(';', StringComparison.Ordinal) ? latest.Path.Substring(latest.Path.LastIndexOf(';') + 1) : latest.Version ?? "";
									var others = major.Count() - 1;
									var suffix = others > 0 ? $" [dim](+{others} older)[/]" : "";
									spectre?.WriteMarkupLine($"    {Markup.Escape(latestVer),-20} [dim]{Markup.Escape(latest.Description ?? "")}[/]{suffix}");
								}
							}
						}
					}

					if (showAll)
					{
						WriteGroupedPackages(packages.Where(p => p.IsInstalled), "Installed packages");
						WriteGroupedPackages(packages.Where(p => !p.IsInstalled), "Available packages");
					}
					else
					{
						WriteGroupedPackages(packages, showAvailable ? "Available packages" : "Installed packages");
					}
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		// sdk accept-licenses
		var acceptLicensesCommand = new Command("accept-licenses", "Accept Android SDK licenses interactively");
		acceptLicensesCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
				var formatter = Program.GetFormatter(context);

			try
			{
				// Check if licenses already accepted
				var accepted = await androidProvider.AreLicensesAcceptedAsync(context.GetCancellationToken());
				if (accepted)
				{
					if (useJson)
					{
						formatter.Write(new { 
							success = true, 
							status = "already_accepted",
							message = "SDK licenses are already accepted" 
						});
					}
					else
					{
						formatter.WriteSuccess("SDK licenses are already accepted");
					}
					return;
				}

				// Get the command for interactive license acceptance
				var licenseCommand = androidProvider.GetLicenseAcceptanceCommand();
				if (licenseCommand == null)
				{
					if (useJson)
					{
						formatter.Write(new { 
							success = false, 
							status = "sdk_not_found",
							message = "Android SDK not found. Run 'maui android install' first." 
						});
					}
					else
					{
						formatter.WriteError(new Exception("Android SDK not found. Run 'maui android install' first."));
					}
					context.ExitCode = 1;
					return;
				}

				if (useJson)
				{
					// For IDE integration: return the command to run in a terminal
					formatter.Write(new { 
						success = true, 
						status = "requires_interaction",
						message = "Run the following command in a terminal to accept licenses interactively",
						command = licenseCommand.Value.Command,
						arguments = licenseCommand.Value.Arguments,
						full_command = $"{licenseCommand.Value.Command} {licenseCommand.Value.Arguments}"
					});
				}
				else
				{
					// For CLI: run interactively
					formatter.WriteInfo("Starting interactive license acceptance...");
					formatter.WriteInfo("Review each license and type 'y' to accept.\n");
					
					// Run sdkmanager --licenses interactively (inherits stdin/stdout)
					var processInfo = new System.Diagnostics.ProcessStartInfo
					{
						FileName = licenseCommand.Value.Command,
						Arguments = licenseCommand.Value.Arguments,
						UseShellExecute = false,
						RedirectStandardInput = false,
						RedirectStandardOutput = false,
						RedirectStandardError = false
					};
					
					// Set environment variables for JDK
					var jdkPath = androidProvider.JdkPath;
					if (!string.IsNullOrEmpty(jdkPath))
					{
						processInfo.Environment["JAVA_HOME"] = jdkPath;
						var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
						processInfo.Environment["PATH"] = $"{Path.Combine(jdkPath, "bin")}{Path.PathSeparator}{currentPath}";
					}
					
					using var process = System.Diagnostics.Process.Start(processInfo);
					if (process != null)
					{
						await process.WaitForExitAsync(context.GetCancellationToken());
						
						if (process.ExitCode == 0)
						{
							formatter.WriteSuccess("License acceptance completed");
						}
						else
						{
							formatter.WriteWarning($"License acceptance exited with code {process.ExitCode}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		command.AddCommand(checkCommand);
		command.AddCommand(installCommand);
		command.AddCommand(listCommand);
		command.AddCommand(acceptLicensesCommand);
		command.AddCommand(CreateSdkUninstallCommand());

		return command;
	}

	private static Command CreateSdkUninstallCommand()
	{
		var command = new Command("uninstall", "Uninstall SDK packages")
		{
			new Argument<string[]>("packages", "Package names to uninstall") { Arity = ArgumentArity.OneOrMore }
		};

		command.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;

			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var packages = context.ParseResult.GetValueForArgument(
				(Argument<string[]>)context.ParseResult.CommandResult.Command.Arguments.First());				var formatter = Program.GetFormatter(context);

			try
			{
				if (dryRun)
				{
					formatter.WriteInfo($"[dry-run] Would uninstall: {string.Join(", ", packages)}");
					return;
				}

				await androidProvider.UninstallPackagesAsync(packages, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, uninstalled = packages });
				}
				else
				{
					formatter.WriteSuccess($"Uninstalled: {string.Join(", ", packages)}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		return command;
	}

	private static Command CreateEmulatorCommand()
	{
		var command = new Command("emulator", "Manage Android emulators");

		// emulator list
		var listCommand = new Command("list", "List available emulators");
		listCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
				var formatter = Program.GetFormatter(context);

			try
			{
				var avds = await androidProvider.GetAvdsAsync(context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(avds);
				}
				else
				{
					if (!avds.Any())
					{
						formatter.WriteWarning("No emulators found.");
						return;
					}

					formatter.WriteTable(
						avds,
						("Name", a => a.Name),
						("Target", a => a.Target ?? "—"),
						("Device", a => a.DeviceProfile ?? "—"));
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		// emulator create
		var nameArg = new Argument<string>("name", "AVD name (prompted interactively if omitted)");
		nameArg.Arity = ArgumentArity.ZeroOrOne;
		nameArg.SetDefaultValue(string.Empty);
		var createCommand = new Command("create", "Create a new emulator")
		{
			nameArg,
			new Option<string>("--package", "System image package (auto-detects most recent if not specified)"),
			new Option<string>("--device", "Device definition"),
			new Option<bool>("--force", "Overwrite existing emulator with the same name")
		};
		createCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var name = context.ParseResult.GetValueForArgument(nameArg);
			var package = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "package"));
			var device = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "device"));
			var force = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "force"));				var formatter = Program.GetFormatter(context);

			try
			{
				// --- Step 1: System image selection ---
				if (string.IsNullOrEmpty(package) && !useJson && formatter is SpectreOutputFormatter spectre)
				{
					var sysImages = await spectre.StatusAsync("Finding installed system images...", async () =>
					{
						var installed = await androidProvider.GetInstalledPackagesAsync(context.GetCancellationToken());
						return installed.Where(p => p.Path.StartsWith("system-images;", StringComparison.Ordinal)).ToList();
					});

					if (sysImages.Count == 0)
					{
						throw new InvalidOperationException(
							"No system images installed. Install one first with: maui android install");
					}

					var selected = spectre.Prompt(
						new SelectionPrompt<SdkPackage>()
							.Title("[bold]Select a system image[/]")
							.PageSize(12)
							.HighlightStyle(new Style(Color.DodgerBlue1))
							.UseConverter(p =>
							{
								var parts = p.Path.Split(';');
								var api = parts.Length > 1 ? parts[1] : "";
								var variant = parts.Length > 2 ? parts[2] : "";
								var arch = parts.Length > 3 ? parts[3] : "";
								return $"[bold]{Markup.Escape(api)}[/]  {Markup.Escape(variant)}  [dim]{Markup.Escape(arch)}[/]";
							})
							.AddChoices(sysImages.OrderByDescending(p => p.Path)));
					package = selected.Path;
				}
				else if (string.IsNullOrEmpty(package))
				{
					package = await androidProvider.GetMostRecentSystemImageAsync(context.GetCancellationToken());
					if (string.IsNullOrEmpty(package))
					{
						throw new InvalidOperationException(
							"No system images installed. Install one first with: maui android install");
					}
					if (!useJson)
						formatter.WriteInfo($"Auto-detected system image: {package}");
				}

				// --- Step 2: Device profile selection ---
				if (string.IsNullOrEmpty(device) && !useJson && formatter is SpectreOutputFormatter spectre2)
				{
					var deviceProfiles = new List<(string Id, string Name)>
					{
						("pixel_6", "Pixel 6"),
						("pixel_8", "Pixel 8"),
						("pixel_9", "Pixel 9"),
						("pixel_fold", "Pixel Fold"),
						("pixel_tablet", "Pixel Tablet"),
						("medium_phone", "Medium Phone"),
						("small_phone", "Small Phone"),
					};

					var selectedDevice = spectre2.Prompt(
						new SelectionPrompt<(string Id, string Name)>()
							.Title("[bold]Select a device profile[/]")
							.HighlightStyle(new Style(Color.DodgerBlue1))
							.UseConverter(d => $"[bold]{Markup.Escape(d.Name)}[/]  [dim]{Markup.Escape(d.Id)}[/]")
							.AddChoices(deviceProfiles));
					device = selectedDevice.Id;
				}

				device ??= "pixel_6";

				// --- Step 3: Name prompt (after selections so user sees what they picked) ---
				if (string.IsNullOrEmpty(name) && !useJson && formatter is SpectreOutputFormatter spectre3)
				{
					// Build a sensible default name from the selected image
					var apiPart = package?.Split(';').ElementAtOrDefault(1) ?? "android";
					var defaultName = $"MAUI_{apiPart.Replace("-", "_", StringComparison.Ordinal)}";

					name = spectre3.Prompt(
						new TextPrompt<string>("[bold]Enter a name for the emulator[/]")
							.DefaultValue(defaultName)
							.Validate(n =>
							{
								if (string.IsNullOrWhiteSpace(n))
									return ValidationResult.Error("[red]Name cannot be empty[/]");
								if (n.Contains(' ', StringComparison.Ordinal))
									return ValidationResult.Error("[red]Name cannot contain spaces[/]");
								return ValidationResult.Success();
							}));
				}

				if (string.IsNullOrEmpty(name))
				{
					throw new InvalidOperationException(
						"AVD name is required. Provide it as an argument or run interactively.");
				}

				if (dryRun)
				{
					formatter.WriteInfo($"[dry-run] Would create AVD: {name}");
					formatter.WriteProgress($"Package: {package}");
					formatter.WriteProgress($"Device: {device}");
					return;
				}

				await androidProvider.CreateAvdAsync(name, device, package!, force, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, package = package, device = device });
				}
				else
				{
					formatter.WriteSuccess($"Created AVD: {name} (device: {device})");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		// emulator start
		var startNameArg = new Argument<string>("name", "AVD name to start (prompted interactively if omitted)");
		startNameArg.Arity = ArgumentArity.ZeroOrOne;
		startNameArg.SetDefaultValue(string.Empty);
		var startCommand = new Command("start", "Start an emulator")
		{
			startNameArg,
			new Option<bool>("--cold-boot", "Perform a cold boot"),
			new Option<bool>("--wait", "Wait for boot completion")
		};
		startCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var name = context.ParseResult.GetValueForArgument(startNameArg);
			var coldBoot = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "cold-boot"));
			var wait = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "wait"));				var formatter = Program.GetFormatter(context);

			try
			{
				// Interactive AVD selection if name not provided
				if (string.IsNullOrEmpty(name) && !useJson && formatter is SpectreOutputFormatter spectre0)
				{
					var avds = await spectre0.StatusAsync("Finding emulators...", async () =>
						await androidProvider.GetAvdsAsync(context.GetCancellationToken()));

					if (!avds.Any())
					{
						throw new InvalidOperationException(
							"No emulators found. Create one first with: maui android emulator create");
					}

					var selectedAvd = spectre0.Prompt(
						new SelectionPrompt<AvdInfo>()
							.Title("[bold]Select an emulator to start[/]")
							.PageSize(10)
							.HighlightStyle(new Style(Color.DodgerBlue1))
							.UseConverter(a =>
							{
								var target = a.Target ?? a.SystemImage ?? "unknown";
								return $"[bold]{Markup.Escape(a.Name)}[/]  [dim]{Markup.Escape(target)}[/]";
							})
							.AddChoices(avds));
					name = selectedAvd.Name;
				}

				if (string.IsNullOrEmpty(name))
				{
					throw new InvalidOperationException(
						"AVD name is required. Provide it as an argument or run interactively.");
				}

				if (dryRun)
				{
					formatter.WriteInfo($"[dry-run] Would start AVD: {name}");
					return;
				}

				if (!useJson && formatter is SpectreOutputFormatter spectre)
				{
					await spectre.StatusAsync($"Starting {name}...", async () =>
					{
						await androidProvider.StartAvdAsync(name, coldBoot, wait, context.GetCancellationToken());
					});
				}
				else
				{
					formatter.WriteProgress($"Starting {name}...");
					await androidProvider.StartAvdAsync(name, coldBoot, wait, context.GetCancellationToken());
				}

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, status = "started" });
				}
				else
				{
					formatter.WriteSuccess($"Started AVD: {name}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		command.AddCommand(listCommand);
		command.AddCommand(createCommand);
		command.AddCommand(startCommand);

		// emulator stop
		var stopNameArg = new Argument<string>("name", "Emulator name (prompted interactively if omitted)");
		stopNameArg.Arity = ArgumentArity.ZeroOrOne;
		stopNameArg.SetDefaultValue(string.Empty);
		var stopCommand = new Command("stop", "Stop a running emulator")
		{
			stopNameArg
		};
		stopCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var name = context.ParseResult.GetValueForArgument(stopNameArg);				var formatter = Program.GetFormatter(context);

			try
			{
				// Find running emulators
				var devices = await androidProvider.GetDevicesAsync(context.GetCancellationToken());
				var runningEmulators = devices.Where(d => d.IsEmulator).ToList();

				// Interactive selection if name not provided
				if (string.IsNullOrEmpty(name) && !useJson && formatter is SpectreOutputFormatter spectreStop)
				{
					if (!runningEmulators.Any())
					{
						throw new Errors.MauiToolException(
							Errors.ErrorCodes.AndroidEmulatorNotFound,
							"No running emulators found.");
					}

					var selectedDevice = spectreStop.Prompt(
						new SelectionPrompt<Device>()
							.Title("[bold]Select a running emulator to stop[/]")
							.PageSize(10)
							.HighlightStyle(new Style(Color.DodgerBlue1))
							.UseConverter(d =>
							{
								var avdName = d.Details?.TryGetValue("avd", out var avd) == true ? avd?.ToString() : null;
								var label = avdName ?? d.Name ?? d.Id;
								return $"[bold]{Markup.Escape(label)}[/]  [dim]{Markup.Escape(d.Id)}[/]";
							})
							.AddChoices(runningEmulators));
					name = selectedDevice.Details?.TryGetValue("avd", out var avdVal) == true
						? avdVal?.ToString() ?? selectedDevice.Id
						: selectedDevice.Id;
				}

				if (string.IsNullOrEmpty(name))
				{
					throw new InvalidOperationException(
						"Emulator name is required. Provide it as an argument or run interactively.");
				}

				var emulator = runningEmulators.FirstOrDefault(d =>
					d.Details != null &&
					d.Details.TryGetValue("avd", out var avd) &&
					string.Equals(avd?.ToString(), name, StringComparison.OrdinalIgnoreCase));

				if (emulator == null)
				{
					// Fall back to matching by serial/id directly
					emulator = runningEmulators.FirstOrDefault(d =>
						string.Equals(d.Id, name, StringComparison.OrdinalIgnoreCase));
				}

				if (emulator == null)
				{
					throw new Errors.MauiToolException(
						Errors.ErrorCodes.AndroidEmulatorNotFound,
						$"No running emulator found with name '{name}'");
				}

				var serial = emulator.Id;

				if (dryRun)
				{
					formatter.WriteInfo($"[dry-run] Would stop emulator: {name} ({serial})");
					return;
				}

				if (!useJson && formatter is SpectreOutputFormatter spectreStop2)
				{
					await spectreStop2.StatusAsync($"Stopping {name}...", async () =>
					{
						await androidProvider.StopEmulatorAsync(serial, context.GetCancellationToken());
					});
				}
				else
				{
					await androidProvider.StopEmulatorAsync(serial, context.GetCancellationToken());
				}

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, serial = serial, status = "stopped" });
				}
				else
				{
					formatter.WriteSuccess($"Stopped emulator: {name} ({serial})");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		command.AddCommand(stopCommand);

		// emulator delete
		var deleteNameArg = new Argument<string>("name", "AVD name to delete (prompted interactively if omitted)");
		deleteNameArg.Arity = ArgumentArity.ZeroOrOne;
		deleteNameArg.SetDefaultValue(string.Empty);
		var deleteCommand = new Command("delete", "Delete an emulator")
		{
			deleteNameArg
		};
		deleteCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var name = context.ParseResult.GetValueForArgument(deleteNameArg);				var formatter = Program.GetFormatter(context);

			try
			{
				// Interactive selection if name not provided
				if (string.IsNullOrEmpty(name) && !useJson && formatter is SpectreOutputFormatter spectreDel)
				{
					var avds = await spectreDel.StatusAsync("Finding emulators...", async () =>
						await androidProvider.GetAvdsAsync(context.GetCancellationToken()));

					if (!avds.Any())
					{
						throw new InvalidOperationException(
							"No emulators found. Nothing to delete.");
					}

					var selectedAvd = spectreDel.Prompt(
						new SelectionPrompt<AvdInfo>()
							.Title("[bold red]Select an emulator to delete[/]")
							.PageSize(10)
							.HighlightStyle(new Style(Color.Red1))
							.UseConverter(a =>
							{
								var target = a.Target ?? a.SystemImage ?? "unknown";
								return $"[bold]{Markup.Escape(a.Name)}[/]  [dim]{Markup.Escape(target)}[/]";
							})
							.AddChoices(avds));
					name = selectedAvd.Name;
				}

				if (string.IsNullOrEmpty(name))
				{
					throw new InvalidOperationException(
						"AVD name is required. Provide it as an argument or run interactively.");
				}

				if (dryRun)
				{
					formatter.WriteInfo($"[dry-run] Would delete AVD: {name}");
					return;
				}

				await androidProvider.DeleteAvdAsync(name, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, status = "deleted" });
				}
				else
				{
					formatter.WriteSuccess($"Deleted AVD: {name}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		command.AddCommand(deleteCommand);

		return command;
	}

	/// <summary>
	/// Resolves packages to install: uses explicit --packages if provided, otherwise shows interactive prompts.
	/// </summary>
	private static async Task<List<string>> ResolveInstallPackagesAsync(
		string[]? explicitPackages,
		SpectreOutputFormatter spectre,
		IAndroidProvider androidProvider,
		CancellationToken cancellationToken)
	{
		// If user specified packages explicitly, use those
		if (explicitPackages is { Length: > 0 })
			return explicitPackages.ToList();

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
		catch
		{
			// Can't list packages (SDK not installed yet) — use sensible defaults
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
				return int.TryParse(apiStr, out var n) ? n : 0;
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

	private static List<string> GetDefaultPackages() => new()
	{
		"platform-tools",
		"emulator",
		"platforms;android-35",
		"build-tools;35.0.0",
		$"system-images;android-35;google_apis;{(PlatformDetector.IsArm64 ? "arm64-v8a" : "x86_64")}"
	};

	private record PlatformChoice(string PackagePath, string DisplayName, string Description, bool IsInstalled);
	private record InstallScope(string Name, string Description, List<string> Packages);
}
