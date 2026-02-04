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

		// simulator boot <udid>
		var bootCommand = new Command("boot", "Boot a simulator");
		var udidArgument = new Argument<string>("udid", "Simulator UDID");
		bootCommand.AddArgument(udidArgument);
		bootCommand.SetHandler(async (InvocationContext context) =>
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
					formatter.WriteSuccess($"Simulator {udid} booted");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		simulatorCommand.AddCommand(bootCommand);

		// simulator shutdown <udid>
		var shutdownCommand = new Command("shutdown", "Shutdown a simulator");
		var shutdownUdidArg = new Argument<string>("udid", "Simulator UDID");
		shutdownCommand.AddArgument(shutdownUdidArg);
		shutdownCommand.SetHandler(async (InvocationContext context) =>
		{
			var formatter = getFormatter(context);
			var udid = context.ParseResult.GetValueForArgument(shutdownUdidArg);

			try
			{
				await provider.ShutdownSimulatorAsync(udid, context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new { success = true, udid });
				}
				else
				{
					formatter.WriteSuccess($"Simulator {udid} shutdown");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		simulatorCommand.AddCommand(shutdownCommand);

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
}
