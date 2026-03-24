// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.Client.Errors;
using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Output;
using Microsoft.Maui.Client.Services;

namespace Microsoft.Maui.Client.Commands;

/// <summary>
/// Implementation of 'maui device' commands.
/// </summary>
public static class DeviceCommand
{
	public static Command Create()
	{
		var command = new Command("device", "Manage devices and emulators");

		command.AddCommand(CreateListCommand());
		command.AddCommand(CreateDeviceLogsCommand());

		return command;
	}

	private static Command CreateListCommand()
	{
		var platformOption = new Option<string>("--platform", () => "all", "Filter by platform (android, apple, windows, all)");
		var command = new Command("list", "List available devices")
		{
			platformOption
		};

		command.SetHandler(async (InvocationContext context) =>
		{
			var deviceManager = Program.DeviceManager;
			var formatter = Program.GetFormatter(context);
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var platform = context.ParseResult.GetValueForOption(platformOption);

			try
			{
				var devices = platform == "all"
					? await deviceManager.GetAllDevicesAsync(context.GetCancellationToken())
					: await deviceManager.GetDevicesByPlatformAsync(platform!, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(devices);
				}
				else
				{
					if (!devices.Any())
					{
						formatter.WriteWarning("No devices found.");
						return;
					}

					formatter.WriteResult(new DeviceListResult { Devices = devices.ToList() });
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

	/// <summary>
	/// Creates the 'device logs' subcommand that shares the same handler logic as the root 'logs' command.
	/// </summary>
	private static Command CreateDeviceLogsCommand()
	{
		var deviceOption = new Option<string?>(new[] { "-d", "--device" }, "Target device ID (auto-detects if not specified)");
		var filterOption = new Option<string?>(new[] { "-f", "--filter" }, "Filter logs by tag or pattern");
		var followOption = new Option<bool>("--follow", () => true, "Follow log output (like tail -f)");

		var command = new Command("logs", "View device logs")
		{
			deviceOption, filterOption, followOption
		};

		command.SetHandler(async (InvocationContext context) =>
		{
			var formatter = Program.GetFormatter(context);
			var deviceId = context.ParseResult.GetValueForOption(deviceOption);
			var filter = context.ParseResult.GetValueForOption(filterOption);
			var follow = context.ParseResult.GetValueForOption(followOption);

			try
			{
				Device device;
				if (string.IsNullOrWhiteSpace(deviceId))
				{
					device = await Program.DeviceManager.GetRunningDeviceOrThrowAsync(context.GetCancellationToken());
					formatter.WriteInfo($"Streaming logs from: {device.Name}");
				}
				else
				{
					device = await Program.DeviceManager.GetDeviceByIdAsync(deviceId, context.GetCancellationToken())
						?? throw new MauiToolException(ErrorCodes.DeviceNotFound, $"Device not found: {deviceId}");
				}

				// Delegate to the shared log streaming implementation
				await LogsCommand.StreamDeviceLogsAsync(device, filter, follow, context.GetCancellationToken());
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine();
				formatter.WriteInfo("Log streaming stopped.");
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
