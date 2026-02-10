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

		appleCommand.AddCommand(CreateSimulatorCommand(appleProvider, getFormatter));
		appleCommand.AddCommand(CreateRuntimeCommand(appleProvider, getFormatter));
		appleCommand.AddCommand(CreateXcodeCommand(appleProvider, getFormatter));

		return appleCommand;
	}

	private static Command CreateSimulatorCommand(IAppleProvider provider, Func<InvocationContext, IOutputFormatter> getFormatter)
	{
		var simulatorCommand = new Command("simulator", "Manage iOS simulators");

		// simulator list
		var listCommand = new Command("list", "List available simulators");
		listCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			try
			{
				var devices = await provider.ListSimulatorsAsync(context.GetCancellationToken());

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

		// runtime list
		var listCommand = new Command("list", "List available runtimes");
		listCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);

			try
			{
				var runtimes = await provider.ListRuntimesAsync(context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new
					{
						runtimes = runtimes.Select(r => new
						{
							identifier = r.Identifier,
							name = r.Name,
							version = r.Version,
							available = r.IsAvailable
						})
					});
				}
				else
				{
					if (runtimes.Count == 0)
					{
						Console.WriteLine("No runtimes found.");
						return;
					}

					Console.WriteLine();
					Console.WriteLine("  AVAILABLE | VERSION | NAME");
					Console.WriteLine("  ----------|---------|--------------------------------");
					foreach (var runtime in runtimes.OrderByDescending(r => r.Version))
					{
						var available = runtime.IsAvailable ? "✓" : " ";
						Console.WriteLine($"  {available,-9} | {runtime.Version,-7} | {runtime.Name}");
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

		return runtimeCommand;
	}

	private static Command CreateXcodeCommand(IAppleProvider provider, Func<InvocationContext, IOutputFormatter> getFormatter)
	{
		var xcodeCommand = new Command("xcode", "Manage Xcode installations");

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

		return xcodeCommand;
	}
}
