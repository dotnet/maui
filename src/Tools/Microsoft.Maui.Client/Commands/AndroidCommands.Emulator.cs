// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Output;
using Microsoft.Maui.Client.Providers.Android;
using Microsoft.Maui.Client.Utils;
using Spectre.Console;

namespace Microsoft.Maui.Client.Commands;

public static partial class AndroidCommands
{
	static Command CreateEmulatorCommand()
	{
		var command = new Command("emulator", "Manage Android emulators");

		// emulator list
		var listCommand = new Command("list", "List available emulators");
		listCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
		{
			var androidProvider = Program.AndroidProvider;

			var useJson = parseResult.GetValue(GlobalOptions.JsonOption);
			var formatter = Program.GetFormatter(parseResult);

			try
			{
				var avds = await androidProvider.GetAvdsAsync(cancellationToken);

				if (useJson)
				{
					formatter.Write(avds);
				}
				else
				{
					if (!avds.Any())
					{
						formatter.WriteWarning("No emulators found.");
						return 0;
					}

					formatter.WriteTable(
						avds,
						("Name", a => a.Name),
						("Target", a => a.Target ?? "—"),
						("Device", a => a.DeviceProfile ?? "—"));
				}
				return 0;
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				return 1;
			}
		});

		// emulator create
		var nameArg = new Argument<string>("name") { Description = "AVD name (prompted interactively if omitted)", Arity = ArgumentArity.ZeroOrOne, DefaultValueFactory = _ => string.Empty };
		var createCommand = new Command("create", "Create a new emulator")
		{
			nameArg,
			new Option<string>("--package") { Description = "System image package (auto-detects most recent if not specified)" },
			new Option<string>("--device") { Description = "Device definition" },
			new Option<bool>("--force") { Description = "Overwrite existing emulator with the same name" }
		};
		createCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
		{
			var androidProvider = Program.AndroidProvider;

			var useJson = parseResult.GetValue(GlobalOptions.JsonOption);
			var dryRun = parseResult.GetValue(GlobalOptions.DryRunOption);
			var name = parseResult.GetValue(nameArg);
			var package = parseResult.GetOption<string>("package");
			var device = parseResult.GetOption<string>("device");
			var force = parseResult.GetOption<bool>("force");
			var formatter = Program.GetFormatter(parseResult);

			try
			{
				var isCi = Program.IsCiMode(parseResult);

				// --- Step 1: System image selection ---
				if (string.IsNullOrEmpty(package) && !useJson && !isCi && formatter is SpectreOutputFormatter spectre)
				{
					var sysImages = await spectre.StatusAsync("Finding installed system images...", async () =>
					{
						var installed = await androidProvider.GetInstalledPackagesAsync(cancellationToken);
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
					package = await androidProvider.GetMostRecentSystemImageAsync(cancellationToken);
					if (string.IsNullOrEmpty(package))
					{
						throw new InvalidOperationException(
							"No system images installed. Install one first with: maui android install");
					}
					if (!useJson)
						formatter.WriteInfo($"Auto-detected system image: {package}");
				}

				// --- Step 2: Device profile selection ---
				if (string.IsNullOrEmpty(device) && !useJson && !isCi && formatter is SpectreOutputFormatter spectre2)
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
				if (string.IsNullOrEmpty(name) && !useJson && !isCi && formatter is SpectreOutputFormatter spectre3)
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
					return 0;
				}

				if (string.IsNullOrEmpty(package))
					throw new InvalidOperationException("No system image package specified. Provide one via --package or run interactively.");

				await androidProvider.CreateAvdAsync(name, device, package, force, cancellationToken);

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, package = package, device = device });
				}
				else
				{
					formatter.WriteSuccess($"Created AVD: {name} (device: {device})");
				}
				return 0;
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				return 1;
			}
		});

		// emulator start
		var startNameArg = new Argument<string>("name") { Description = "AVD name to start (prompted interactively if omitted)", Arity = ArgumentArity.ZeroOrOne, DefaultValueFactory = _ => string.Empty };
		var startCommand = new Command("start", "Start an emulator")
		{
			startNameArg,
			new Option<bool>("--cold-boot") { Description = "Perform a cold boot" },
			new Option<bool>("--wait") { Description = "Wait for boot completion" }
		};
		startCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
		{
			var androidProvider = Program.AndroidProvider;

			var useJson = parseResult.GetValue(GlobalOptions.JsonOption);
			var dryRun = parseResult.GetValue(GlobalOptions.DryRunOption);
			var name = parseResult.GetValue(startNameArg);
			var coldBoot = parseResult.GetOption<bool>("cold-boot");
			var wait = parseResult.GetOption<bool>("wait");
			var formatter = Program.GetFormatter(parseResult);

			try
			{
				var isCi = Program.IsCiMode(parseResult);

				// Interactive AVD selection if name not provided
				if (string.IsNullOrEmpty(name) && !useJson && !isCi && formatter is SpectreOutputFormatter spectre0)
				{
					var avds = await spectre0.StatusAsync("Finding emulators...", async () =>
						await androidProvider.GetAvdsAsync(cancellationToken));

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
					return 0;
				}

				if (!useJson && formatter is SpectreOutputFormatter spectre)
				{
					await spectre.StatusAsync($"Starting {name}...", async () =>
					{
						await androidProvider.StartAvdAsync(name, coldBoot, wait, cancellationToken);
					});
				}
				else
				{
					formatter.WriteProgress($"Starting {name}...");
					await androidProvider.StartAvdAsync(name, coldBoot, wait, cancellationToken);
				}

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, status = "started" });
				}
				else
				{
					formatter.WriteSuccess($"Started AVD: {name}");
				}
				return 0;
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				return 1;
			}
		});

		command.Add(listCommand);
		command.Add(createCommand);
		command.Add(startCommand);

		// emulator stop
		var stopNameArg = new Argument<string>("name") { Description = "Emulator name (prompted interactively if omitted)", Arity = ArgumentArity.ZeroOrOne, DefaultValueFactory = _ => string.Empty };
		var stopCommand = new Command("stop", "Stop a running emulator")
		{
			stopNameArg
		};
		stopCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
		{
			var androidProvider = Program.AndroidProvider;

			var useJson = parseResult.GetValue(GlobalOptions.JsonOption);
			var dryRun = parseResult.GetValue(GlobalOptions.DryRunOption);
			var name = parseResult.GetValue(stopNameArg);
			var formatter = Program.GetFormatter(parseResult);

			try
			{
				var isCi = Program.IsCiMode(parseResult);

				// Find running emulators
				var devices = await androidProvider.GetDevicesAsync(cancellationToken);
				var runningEmulators = devices.Where(d => d.IsEmulator).ToList();

				// Interactive selection if name not provided
				if (string.IsNullOrEmpty(name) && !useJson && !isCi && formatter is SpectreOutputFormatter spectreStop)
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
					return 0;
				}

				if (!useJson && formatter is SpectreOutputFormatter spectreStop2)
				{
					await spectreStop2.StatusAsync($"Stopping {name}...", async () =>
					{
						await androidProvider.StopEmulatorAsync(serial, cancellationToken);
					});
				}
				else
				{
					await androidProvider.StopEmulatorAsync(serial, cancellationToken);
				}

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, serial = serial, status = "stopped" });
				}
				else
				{
					formatter.WriteSuccess($"Stopped emulator: {name} ({serial})");
				}
				return 0;
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				return 1;
			}
		});
		command.Add(stopCommand);

		// emulator delete
		var deleteNameArg = new Argument<string>("name") { Description = "AVD name to delete (prompted interactively if omitted)", Arity = ArgumentArity.ZeroOrOne, DefaultValueFactory = _ => string.Empty };
		var deleteCommand = new Command("delete", "Delete an emulator")
		{
			deleteNameArg
		};
		deleteCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
		{
			var androidProvider = Program.AndroidProvider;

			var useJson = parseResult.GetValue(GlobalOptions.JsonOption);
			var dryRun = parseResult.GetValue(GlobalOptions.DryRunOption);
			var name = parseResult.GetValue(deleteNameArg);
			var formatter = Program.GetFormatter(parseResult);

			try
			{
				var isCi = Program.IsCiMode(parseResult);

				// Interactive selection if name not provided
				if (string.IsNullOrEmpty(name) && !useJson && !isCi && formatter is SpectreOutputFormatter spectreDel)
				{
					var avds = await spectreDel.StatusAsync("Finding emulators...", async () =>
						await androidProvider.GetAvdsAsync(cancellationToken));

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
					return 0;
				}

				await androidProvider.DeleteAvdAsync(name, cancellationToken);

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, status = "deleted" });
				}
				else
				{
					formatter.WriteSuccess($"Deleted AVD: {name}");
				}
				return 0;
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				return 1;
			}
		});
		command.Add(deleteCommand);

		return command;
	}
}
