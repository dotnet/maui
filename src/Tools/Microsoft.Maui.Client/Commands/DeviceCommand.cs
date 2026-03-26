// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Parsing;
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

		command.Add(CreateListCommand());

		return command;
	}

	static Command CreateListCommand()
	{
		var platformOption = new Option<string>("--platform") { Description = "Filter by platform (android, apple, windows, all)", DefaultValueFactory = _ => "all" };
		var command = new Command("list", "List available devices")
		{
			platformOption
		};

		command.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
		{
			var deviceManager = Program.DeviceManager;
			var formatter = Program.GetFormatter(parseResult);
			var useJson = parseResult.GetValue(GlobalOptions.JsonOption);
			var platform = parseResult.GetValue(platformOption);

			try
			{
				var devices = platform == "all"
					? await deviceManager.GetAllDevicesAsync(cancellationToken)
					: await deviceManager.GetDevicesByPlatformAsync(platform!, cancellationToken);

				if (useJson)
				{
					formatter.Write(devices);
				}
				else
				{
					if (!devices.Any())
					{
						formatter.WriteWarning("No devices found.");
						return 0;
					}

					formatter.WriteResult(new DeviceListResult { Devices = devices.ToList() });
				}
				return 0;
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				return 1;
			}
		});

		return command;
	}
}
