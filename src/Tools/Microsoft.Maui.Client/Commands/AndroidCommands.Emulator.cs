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
			var package = context.GetOption<string>("package");
			var device = context.GetOption<string>("device");
			var force = context.GetOption<bool>("force");
			var formatter = Program.GetFormatter(context);

			try
			{
				var isCi = Program.IsCiMode(context);

				// --- Step 1: System image selection ---
				if (string.IsNullOrEmpty(package) && !useJson && !isCi && formatter is SpectreOutputFormatter spectre)
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
					return;
				}

				if (string.IsNullOrEmpty(package))
					throw new InvalidOperationException("No system image package specified. Provide one via --package or run interactively.");

				await androidProvider.CreateAvdAsync(name, device, package, force, context.GetCancellationToken());

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
			var coldBoot = context.GetOption<bool>("cold-boot");
			var wait = context.GetOption<bool>("wait");
			var formatter = Program.GetFormatter(context);

			try
			{
				var isCi = Program.IsCiMode(context);

				// Interactive AVD selection if name not provided
				if (string.IsNullOrEmpty(name) && !useJson && !isCi && formatter is SpectreOutputFormatter spectre0)
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
			var name = context.ParseResult.GetValueForArgument(stopNameArg);
			var formatter = Program.GetFormatter(context);

			try
			{
				var isCi = Program.IsCiMode(context);

				// Find running emulators
				var devices = await androidProvider.GetDevicesAsync(context.GetCancellationToken());
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
			var name = context.ParseResult.GetValueForArgument(deleteNameArg);
			var formatter = Program.GetFormatter(context);

			try
			{
				var isCi = Program.IsCiMode(context);

				// Interactive selection if name not provided
				if (string.IsNullOrEmpty(name) && !useJson && !isCi && formatter is SpectreOutputFormatter spectreDel)
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
}
