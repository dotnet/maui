// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
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

		return command;
	}

	static Command CreateListCommand()
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
}
