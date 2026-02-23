// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Output;
using Microsoft.Maui.DevTools.Providers.Apple;

namespace Microsoft.Maui.DevTools.Commands;

/// <summary>
/// Apple platform commands.
/// </summary>
public static class AppleCommands
{
	public static Command Create(IAppleProvider appleProvider, Func<InvocationContext, IOutputFormatter> getFormatter)
	{
		var appleCommand = new Command("apple", "Apple platform management (macOS only)");

		appleCommand.AddCommand(CreateInstallCommand(appleProvider, getFormatter));
		appleCommand.AddCommand(CreateSimulatorCommand(appleProvider, getFormatter));
		appleCommand.AddCommand(CreateRuntimeCommand(appleProvider, getFormatter));
		appleCommand.AddCommand(CreateXcodeCommand(appleProvider, getFormatter));

		return appleCommand;
	}

	private static Command CreateInstallCommand(IAppleProvider provider, Func<InvocationContext, IOutputFormatter> getFormatter)
	{
		var command = new Command("install", "Set up Apple development environment (Xcode, runtimes)")
		{
			new Option<string>("--runtime", "iOS runtime version to install (e.g., 18.5)"),
			new Option<bool>("--accept-licenses", "Automatically accept Xcode license")
		};

		command.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var runtimeVersion = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "runtime"));
			var acceptLicenses = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "accept-licenses"));

			var progress = new Progress<string>(message =>
			{
				if (formatter is JsonOutputFormatter)
					formatter.WriteProgress(message);
				else
					Console.WriteLine(message);
			});

			try
			{
				if (dryRun)
				{
					Console.WriteLine("[dry-run] Would set up Apple development environment:");
					Console.WriteLine("  • Check Xcode installation");
					Console.WriteLine("  • Check Xcode license");
					if (acceptLicenses)
						Console.WriteLine("  • Accept Xcode license if needed");
					Console.WriteLine("  • Check installed runtimes");
					if (!string.IsNullOrEmpty(runtimeVersion))
						Console.WriteLine($"  • Install iOS {runtimeVersion} runtime if needed");
					return;
				}

				var results = new List<object>();

				// Step 1: Check Xcode
				((IProgress<string>)progress).Report("Checking Xcode installation...");
				var checks = await provider.CheckHealthAsync(context.GetCancellationToken());
				var xcodeCheck = checks.FirstOrDefault(c => c.Name == "Xcode");

				if (xcodeCheck == null || xcodeCheck.Status == CheckStatus.Error)
				{
					throw new Errors.MauiToolException(
						Errors.ErrorCodes.XcodeNotFound,
						"Xcode is not installed. Please install Xcode from the App Store or https://developer.apple.com/xcode/");
				}

				if (formatter is not JsonOutputFormatter)
				{
					Console.WriteLine($"✓ Xcode {(xcodeCheck.Details?.TryGetValue("version", out var ver) == true ? ver : "installed")}");
				}
				results.Add(new { step = "xcode", status = "ok", message = xcodeCheck.Message });

				// Step 2: Check and accept license
				((IProgress<string>)progress).Report("Checking Xcode license...");
				var licenseAccepted = await provider.IsXcodeLicenseAcceptedAsync(context.GetCancellationToken());

				if (!licenseAccepted)
				{
					if (acceptLicenses)
					{
						((IProgress<string>)progress).Report("Accepting Xcode license...");
						await provider.AcceptXcodeLicenseAsync(context.GetCancellationToken());
						if (formatter is not JsonOutputFormatter)
							Console.WriteLine("✓ Xcode license accepted");
						results.Add(new { step = "license", status = "ok", message = "Xcode license accepted" });
					}
					else
					{
						if (formatter is not JsonOutputFormatter)
							Console.WriteLine("⚠ Xcode license not accepted (use --accept-licenses or run: maui apple xcode accept-licenses)");
						results.Add(new { step = "license", status = "warning", message = "Xcode license not accepted" });
					}
				}
				else
				{
					if (formatter is not JsonOutputFormatter)
						Console.WriteLine("✓ Xcode license accepted");
					results.Add(new { step = "license", status = "ok", message = "Xcode license already accepted" });
				}

				// Step 3: Check runtimes
				((IProgress<string>)progress).Report("Checking installed runtimes...");
				var runtimes = await provider.ListRuntimesAsync(context.GetCancellationToken());

				if (formatter is not JsonOutputFormatter)
				{
					if (runtimes.Count > 0)
						Console.WriteLine($"✓ {runtimes.Count} runtime(s) installed: {string.Join(", ", runtimes.Select(r => r.Name))}");
					else
						Console.WriteLine("⚠ No iOS runtimes installed");
				}
				results.Add(new { step = "runtimes", status = runtimes.Count > 0 ? "ok" : "warning", installed = runtimes.Select(r => r.Name) });

				// Step 4: Install runtime if requested
				if (!string.IsNullOrEmpty(runtimeVersion))
				{
					var alreadyInstalled = runtimes.Any(r =>
						r.Version.StartsWith(runtimeVersion, StringComparison.OrdinalIgnoreCase) ||
						r.Name.Contains(runtimeVersion, StringComparison.OrdinalIgnoreCase));

					if (alreadyInstalled)
					{
						if (formatter is not JsonOutputFormatter)
							Console.WriteLine($"✓ iOS {runtimeVersion} runtime already installed");
						results.Add(new { step = "runtime_install", status = "ok", message = $"iOS {runtimeVersion} already installed" });
					}
					else
					{
						((IProgress<string>)progress).Report($"Installing iOS {runtimeVersion} runtime...");
						await provider.InstallRuntimeAsync(runtimeVersion, progress, context.GetCancellationToken());
						if (formatter is not JsonOutputFormatter)
							Console.WriteLine($"✓ iOS {runtimeVersion} runtime installed");
						results.Add(new { step = "runtime_install", status = "ok", message = $"iOS {runtimeVersion} installed" });
					}
				}

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new { success = true, message = "Apple development environment ready", steps = results });
				}
				else
				{
					Console.WriteLine();
					Console.WriteLine("✓ Apple development environment is ready");
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

	private static Command CreateSimulatorCommand(IAppleProvider provider, Func<InvocationContext, IOutputFormatter> getFormatter)
	{
		var simulatorCommand = new Command("simulator", "Manage iOS simulators");

		// simulator list
		var listCommand = new Command("list", "List available simulators")
		{
			new Option<string?>("--runtime", "Filter by runtime (e.g., 'iOS-18-5', 'iOS')"),
			new Option<string?>("--device-type", "Filter by device type (e.g., 'iPhone', 'iPad')"),
			new Option<string?>("--state", "Filter by state (booted, shutdown)")
		};
		listCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var runtimeFilter = context.ParseResult.GetValueForOption(
				(Option<string?>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "runtime"));
			var deviceTypeFilter = context.ParseResult.GetValueForOption(
				(Option<string?>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "device-type"));
			var stateFilter = context.ParseResult.GetValueForOption(
				(Option<string?>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "state"));
			try
			{
				var devices = await provider.ListSimulatorsAsync(context.GetCancellationToken());

				// Apply filters
				if (!string.IsNullOrEmpty(runtimeFilter))
					devices = devices.Where(d => 
						(d.VersionName?.Contains(runtimeFilter, StringComparison.OrdinalIgnoreCase) ?? false) ||
						(d.Version?.Contains(runtimeFilter, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

				if (!string.IsNullOrEmpty(deviceTypeFilter))
					devices = devices.Where(d => 
						d.Name.Contains(deviceTypeFilter, StringComparison.OrdinalIgnoreCase) ||
						(d.Model?.Contains(deviceTypeFilter, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

				if (!string.IsNullOrEmpty(stateFilter))
				{
					if (Enum.TryParse<Models.DeviceState>(stateFilter, ignoreCase: true, out var parsedState))
						devices = devices.Where(d => d.State == parsedState).ToList();
				}

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new
					{
						devices = devices.Select(d => new
						{
							udid = d.Id,
							name = d.Name,
							platform = d.Platform,
							state = d.State.ToString().ToLowerInvariant()
						})
					});
				}
				else
				{
					if (devices.Count == 0)
					{
						Console.WriteLine("No simulators found.");
						return;
					}

					Console.WriteLine();
					Console.WriteLine("  STATE     | UDID                                 | NAME");
					Console.WriteLine("  ----------|--------------------------------------|---------------------------");
					foreach (var device in devices.OrderBy(d => d.Name))
					{
						var state = device.State.ToString().ToLowerInvariant().PadRight(8);
						var udid = device.Id.Length > 36 ? device.Id[..36] : device.Id.PadRight(36);
						Console.WriteLine($"  {state} | {udid} | {device.Name}");
					}
					Console.WriteLine();
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		simulatorCommand.AddCommand(listCommand);

		// simulator start <udid>
		var startCommand = new Command("start", "Start a simulator");
		var udidArgument = new Argument<string>("udid", "Simulator UDID");
		startCommand.AddArgument(udidArgument);
		startCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var udid = context.ParseResult.GetValueForArgument(udidArgument);

			try
			{
				await provider.BootSimulatorAsync(udid, context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new { success = true, udid });
				}
				else
				{
					formatter.WriteSuccess($"Simulator {udid} started");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		simulatorCommand.AddCommand(startCommand);

		// simulator stop <udid>
		var stopCommand = new Command("stop", "Stop a simulator");
		var stopUdidArg = new Argument<string>("udid", "Simulator UDID");
		stopCommand.AddArgument(stopUdidArg);
		stopCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var udid = context.ParseResult.GetValueForArgument(stopUdidArg);

			try
			{
				await provider.ShutdownSimulatorAsync(udid, context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new { success = true, udid });
				}
				else
				{
					formatter.WriteSuccess($"Simulator {udid} stopped");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		simulatorCommand.AddCommand(stopCommand);

		// simulator create <name> <device-type> <runtime>
		var createCommand = new Command("create", "Create a new simulator");
		var nameArg = new Argument<string>("name", "Simulator name");
		var deviceTypeArg = new Argument<string>("device-type", "Device type (e.g., com.apple.CoreSimulator.SimDeviceType.iPhone-15)");
		var runtimeArg = new Argument<string>("runtime", "Runtime identifier (e.g., com.apple.CoreSimulator.SimRuntime.iOS-17-0)");
		createCommand.AddArgument(nameArg);
		createCommand.AddArgument(deviceTypeArg);
		createCommand.AddArgument(runtimeArg);
		createCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var name = context.ParseResult.GetValueForArgument(nameArg);
			var deviceType = context.ParseResult.GetValueForArgument(deviceTypeArg);
			var runtime = context.ParseResult.GetValueForArgument(runtimeArg);

			try
			{
				var device = await provider.CreateSimulatorAsync(name, deviceType, runtime, context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new
					{
						success = true,
						udid = device.Id,
						name = device.Name
					});
				}
				else
				{
					formatter.WriteSuccess($"Created simulator '{name}' with UDID: {device.Id}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		simulatorCommand.AddCommand(createCommand);

		// simulator delete <udid>
		var deleteCommand = new Command("delete", "Delete a simulator");
		var deleteUdidArg = new Argument<string>("udid", "Simulator UDID");
		deleteCommand.AddArgument(deleteUdidArg);
		deleteCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var udid = context.ParseResult.GetValueForArgument(deleteUdidArg);

			try
			{
				await provider.DeleteSimulatorAsync(udid, context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new { success = true, udid });
				}
				else
				{
					formatter.WriteSuccess($"Deleted simulator {udid}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		simulatorCommand.AddCommand(deleteCommand);

		return simulatorCommand;
	}

	private static Command CreateRuntimeCommand(IAppleProvider provider, Func<InvocationContext, IOutputFormatter> getFormatter)
	{
		var runtimeCommand = new Command("runtime", "Manage iOS runtimes");

		// runtime check
		var checkCommand = new Command("check", "Check runtime installation status");
		checkCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);

			try
			{
				var checks = await provider.CheckHealthAsync(context.GetCancellationToken());
				var runtimeCheck = checks.FirstOrDefault(c => c.Name == "iOS Simulators");

				if (runtimeCheck != null)
				{
					if (formatter is JsonOutputFormatter)
					{
						formatter.Write(runtimeCheck);
					}
					else
					{
						var statusIcon = runtimeCheck.Status == Models.CheckStatus.Ok ? "✓" :
										 runtimeCheck.Status == Models.CheckStatus.Warning ? "⚠" : "✗";
						Console.WriteLine($"{statusIcon} {runtimeCheck.Message}");
					}
				}
				else
				{
					var runtimes = await provider.ListRuntimesAsync(context.GetCancellationToken());
					if (formatter is JsonOutputFormatter)
					{
						formatter.Write(new { status = runtimes.Count > 0 ? "ok" : "warning", count = runtimes.Count });
					}
					else
					{
						Console.WriteLine(runtimes.Count > 0
							? $"✓ {runtimes.Count} runtime(s) installed"
							: "⚠ No runtimes found");
					}
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		runtimeCommand.AddCommand(checkCommand);

		// runtime list
		var availableOption = new Option<bool>("--available", "List runtimes available for download");
		var allOption = new Option<bool>("--all", "List both installed and available runtimes");
		var listCommand = new Command("list", "List runtimes") { availableOption, allOption };
		listCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var showAvailable = context.ParseResult.GetValueForOption(availableOption);
			var showAll = context.ParseResult.GetValueForOption(allOption);

			try
			{
				var runtimes = new List<Runtime>();

				if (showAll)
				{
					var installed = await provider.ListRuntimesAsync(context.GetCancellationToken());
					var available = await provider.ListAvailableRuntimesAsync(context.GetCancellationToken());
					// Merge: keep installed, add available that aren't already installed
					runtimes.AddRange(installed);
					var installedIds = new HashSet<string>(installed.Select(r => r.Identifier), StringComparer.OrdinalIgnoreCase);
					runtimes.AddRange(available.Where(r => !installedIds.Contains(r.Identifier)));
				}
				else if (showAvailable)
				{
					runtimes = await provider.ListAvailableRuntimesAsync(context.GetCancellationToken());
				}
				else
				{
					runtimes = await provider.ListRuntimesAsync(context.GetCancellationToken());
				}

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new
					{
						runtimes = runtimes.Select(r => new
						{
							identifier = r.Identifier,
							name = r.Name,
							version = r.Version,
							available = r.IsAvailable,
							installed = r.IsInstalled,
							source = r.Source
						})
					});
				}
				else
				{
					if (runtimes.Count == 0)
					{
						Console.WriteLine(showAvailable ? "No runtimes available for download." : "No runtimes found.");
						return;
					}

					if (showAll)
					{
						Console.WriteLine();
						Console.WriteLine("  INSTALLED | VERSION | NAME");
						Console.WriteLine("  ----------|---------|--------------------------------");
						foreach (var runtime in runtimes.OrderByDescending(r => r.Version))
						{
							var installed = runtime.IsInstalled ? "✓" : " ";
							Console.WriteLine($"  {installed,-9} | {runtime.Version,-7} | {runtime.Name}");
						}
					}
					else
					{
						Console.WriteLine();
						Console.WriteLine("  AVAILABLE | VERSION | NAME");
						Console.WriteLine("  ----------|---------|--------------------------------");
						foreach (var runtime in runtimes.OrderByDescending(r => r.Version))
						{
							var available = runtime.IsAvailable ? "✓" : " ";
							Console.WriteLine($"  {available,-9} | {runtime.Version,-7} | {runtime.Name}");
						}
					}
					Console.WriteLine();
					Console.WriteLine($"  Identifier format: com.apple.CoreSimulator.SimRuntime.iOS-17-0");
					Console.WriteLine();
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		runtimeCommand.AddCommand(listCommand);

		// runtime install
		var installCommand = new Command("install", "Install an iOS runtime");
		var versionArg = new Argument<string>("version", "Runtime version to install (e.g., 18.0)");
		installCommand.AddArgument(versionArg);
		installCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var version = context.ParseResult.GetValueForArgument(versionArg);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);

			try
			{
				if (dryRun)
				{
					Console.WriteLine($"[dry-run] Would install iOS {version} runtime");
					return;
				}

				var progress = new Progress<string>(message =>
				{
					if (formatter is JsonOutputFormatter)
						formatter.WriteProgress(message);
					else
						Console.WriteLine(message);
				});

				await provider.InstallRuntimeAsync(version, progress, context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new { success = true, version, message = $"iOS {version} runtime installed" });
				}
				else
				{
					formatter.WriteSuccess($"iOS {version} runtime installed");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		runtimeCommand.AddCommand(installCommand);

		return runtimeCommand;
	}

	private static Command CreateXcodeCommand(IAppleProvider provider, Func<InvocationContext, IOutputFormatter> getFormatter)
	{
		var xcodeCommand = new Command("xcode", "Manage Xcode installations");

		// xcode check
		var checkCommand = new Command("check", "Check Xcode installation status");
		checkCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);

			try
			{
				var checks = await provider.CheckHealthAsync(context.GetCancellationToken());
				var xcodeCheck = checks.FirstOrDefault(c => c.Name == "Xcode");

				if (xcodeCheck != null)
				{
					if (formatter is JsonOutputFormatter)
					{
						formatter.Write(xcodeCheck);
					}
					else
					{
						var statusIcon = xcodeCheck.Status == Models.CheckStatus.Ok ? "✓" :
										 xcodeCheck.Status == Models.CheckStatus.Warning ? "⚠" : "✗";
						Console.WriteLine($"{statusIcon} {xcodeCheck.Message}");

						if (xcodeCheck.Details?.TryGetValue("path", out var path) == true)
							Console.WriteLine($"  Path: {path}");

						// Also check license
						var licenseAccepted = await provider.IsXcodeLicenseAcceptedAsync(context.GetCancellationToken());
						Console.WriteLine(licenseAccepted
							? "✓ Xcode license accepted"
							: "⚠ Xcode license not accepted (run: maui apple xcode accept-licenses)");
					}
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		xcodeCommand.AddCommand(checkCommand);

		// xcode list
		var listCommand = new Command("list", "List installed Xcode versions");
		listCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);

			try
			{
				var installations = await provider.ListXcodeInstallationsAsync(context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new
					{
						installations = installations.Select(x => new
						{
							path = x.Path,
							developer_dir = x.DeveloperDir,
							version = x.Version,
							build = x.Build,
							is_selected = x.IsSelected
						})
					});
				}
				else
				{
					if (installations.Count == 0)
					{
						Console.WriteLine("No Xcode installations found.");
						return;
					}

					Console.WriteLine();
					Console.WriteLine("  SELECTED | VERSION    | BUILD     | PATH");
					Console.WriteLine("  ---------|------------|-----------|------------------------------------");
					foreach (var xcode in installations)
					{
						var selected = xcode.IsSelected ? "  ►" : "   ";
						var version = (xcode.Version ?? "unknown").PadRight(10);
						var build = (xcode.Build ?? "unknown").PadRight(9);
						Console.WriteLine($"{selected}      | {version} | {build} | {xcode.Path}");
					}
					Console.WriteLine();
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		xcodeCommand.AddCommand(listCommand);

		// xcode select <path>
		var selectCommand = new Command("select", "Switch active Xcode installation");
		var pathArg = new Argument<string>("path", "Path to Xcode.app (e.g., /Applications/Xcode.app)");
		selectCommand.AddArgument(pathArg);
		selectCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var xcodePath = context.ParseResult.GetValueForArgument(pathArg);

			try
			{
				// Resolve to Contents/Developer if user passed .app path
				var devDir = xcodePath;
				if (xcodePath.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
				{
					devDir = Path.Combine(xcodePath, "Contents", "Developer");
				}

				if (!Directory.Exists(devDir))
				{
					throw new Errors.MauiToolException(
						Errors.ErrorCodes.XcodeNotFound,
						$"Xcode developer directory not found: {devDir}");
				}

				await provider.SelectXcodeAsync(devDir, context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new { success = true, path = xcodePath, developer_dir = devDir });
				}
				else
				{
					formatter.WriteSuccess($"Switched to Xcode at {xcodePath}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		xcodeCommand.AddCommand(selectCommand);

		// xcode accept-licenses
		var acceptLicensesCommand = new Command("accept-licenses", "Accept the Xcode license agreement");
		acceptLicensesCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);

			try
			{
				var accepted = await provider.IsXcodeLicenseAcceptedAsync(context.GetCancellationToken());
				if (accepted)
				{
					if (formatter is JsonOutputFormatter)
					{
						formatter.Write(new { success = true, status = "already_accepted", message = "Xcode license is already accepted" });
					}
					else
					{
						Console.WriteLine("✓ Xcode license is already accepted");
					}
					return;
				}

				if (dryRun)
				{
					Console.WriteLine("[dry-run] Would accept Xcode license agreement");
					return;
				}

				await provider.AcceptXcodeLicenseAsync(context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new { success = true, message = "Xcode license accepted" });
				}
				else
				{
					formatter.WriteSuccess("Xcode license accepted");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		xcodeCommand.AddCommand(acceptLicensesCommand);

		return xcodeCommand;
	}
}
