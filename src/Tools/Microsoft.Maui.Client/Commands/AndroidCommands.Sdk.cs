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
							if (Version.TryParse(apiStr, out var v))
								return v;
							if (int.TryParse(apiStr, out var n))
								return new Version(n, 0);
							return new Version(0, 0);
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
				if (TryRequestElevation(androidProvider, formatter, useJson))
				{
					if (useJson)
						formatter.Write(new { success = true, installed = packages, elevated = true });
					else
						formatter.WriteSuccess("Packages installed successfully (elevated)");
					return;
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
			var showAvailable = context.GetOption<bool>("available");
			var showAll = context.GetOption<bool>("all");				var formatter = Program.GetFormatter(context);

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

}
