// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Output;
using Microsoft.Maui.DevTools.Services;

namespace Microsoft.Maui.DevTools.Commands;

/// <summary>
/// Top-level screenshot command per spec.
/// </summary>
public static class ScreenshotCommand
{
	private static readonly Option<string?> OutputOption = new(
		aliases: new[] { "-o", "--output" },
		description: "Output file path (default: screenshot_{timestamp}.png)");

	private static readonly Option<int> WaitOption = new(
		aliases: new[] { "-w", "--wait" },
		description: "Wait seconds before capturing",
		getDefaultValue: () => 0);

	private static readonly Option<string?> DeviceOption = new(
		aliases: new[] { "-d", "--device" },
		description: "Target device ID (auto-detects if not specified)");

	public static Command Create()
	{
		var command = new Command("screenshot", "Capture a screenshot of the running app");
		command.AddOption(OutputOption);
		command.AddOption(WaitOption);
		command.AddOption(DeviceOption);

		command.SetHandler(async (InvocationContext context) =>
		{
			var formatter = Program.GetFormatter(context);
			var output = context.ParseResult.GetValueForOption(OutputOption);
			var wait = context.ParseResult.GetValueForOption(WaitOption);
			var deviceId = context.ParseResult.GetValueForOption(DeviceOption);

			try
			{
				// Wait if requested
				if (wait > 0)
				{
					formatter.WriteInfo($"Waiting {wait} seconds...");
					await Task.Delay(TimeSpan.FromSeconds(wait), context.GetCancellationToken());
				}

				// Auto-detect device if not specified
				if (string.IsNullOrWhiteSpace(deviceId))
				{
					var devices = await Program.DeviceManager.GetAllDevicesAsync(context.GetCancellationToken());
					var runningDevice = devices.FirstOrDefault(d => d.State == DeviceState.Booted);

					if (runningDevice == null)
					{
						formatter.WriteError(new ErrorResult
						{
							Code = ErrorCodes.DeviceNotFound,
							Category = "tool",
							Message = "No running device found. Start a device or specify one with --device"
						});
						context.ExitCode = 1;
						return;
					}

					deviceId = runningDevice.Id;
					formatter.WriteInfo($"Using device: {runningDevice.Name} ({deviceId})");
				}

				// Generate output path if not specified
				output ??= $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";

				var path = await Program.DeviceManager.TakeScreenshotAsync(deviceId, output, context.GetCancellationToken());

				if (formatter is JsonOutputFormatter)
				{
					formatter.Write(new
					{
						success = true,
						path,
						device = deviceId
					});
				}
				else
				{
					formatter.WriteSuccess($"Screenshot saved to {path}");
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
