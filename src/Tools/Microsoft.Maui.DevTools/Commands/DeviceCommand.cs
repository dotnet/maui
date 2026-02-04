// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.DevTools.Output;
using Microsoft.Maui.DevTools.Services;

namespace Microsoft.Maui.DevTools.Commands;

/// <summary>
/// Implementation of 'dotnet maui device' commands.
/// </summary>
public static class DeviceCommand
{
	public static Command Create()
	{
		var command = new Command("device", "Manage devices and emulators");

		command.AddCommand(CreateListCommand());
		command.AddCommand(CreateScreenshotCommand());

		return command;
	}

	private static Command CreateListCommand()
	{
		var command = new Command("list", "List available devices")
		{
			new Option<string>("--platform", () => "all", "Filter by platform (android, apple, windows, all)")
		};

		command.SetHandler(async (InvocationContext context) =>
		{
			var deviceManager = Program.DeviceManager;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var platform = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "platform"));

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

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
						Console.WriteLine("No devices found.");
						return;
					}

					Console.WriteLine();
					Console.WriteLine("  PLATFORM   | TYPE      | STATE       | ID                    | NAME");
					Console.WriteLine("  -----------|-----------|-------------|-----------------------|------------------");
					foreach (var device in devices)
					{
						var type = device.Type.ToString().ToLowerInvariant().PadRight(9);
						var state = device.State.ToString().ToLowerInvariant().PadRight(11);
						var id = device.Id.Length > 21 ? device.Id[..21] : device.Id.PadRight(21);
						Console.WriteLine($"  {device.Platform,-10} | {type} | {state} | {id} | {device.Name}");
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

		return command;
	}

	private static Command CreateScreenshotCommand()
	{
		var command = new Command("screenshot", "Capture device screenshot")
		{
			new Argument<string>("device-id", "Device ID to capture"),
			new Option<string>("--output", "Output file path")
		};

		command.SetHandler(async (InvocationContext context) =>
		{
			var deviceManager = Program.DeviceManager;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var deviceId = context.ParseResult.GetValueForArgument(
				(Argument<string>)context.ParseResult.CommandResult.Command.Arguments.First());
			var outputPath = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "output"));

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				var result = await deviceManager.TakeScreenshotAsync(deviceId, outputPath, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { path = result, success = true });
				}
				else
				{
					Console.WriteLine($"Screenshot saved to: {result}");
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
