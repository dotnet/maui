// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Output;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Commands;

/// <summary>
/// Top-level logs command per spec.
/// </summary>
public static class LogsCommand
{
	private static readonly Option<string?> DeviceOption = new(
		aliases: new[] { "-d", "--device" },
		description: "Target device ID (auto-detects if not specified)");

	private static readonly Option<string?> FilterOption = new(
		aliases: new[] { "-f", "--filter" },
		description: "Filter logs by tag or pattern");

	private static readonly Option<bool> FollowOption = new(
		aliases: new[] { "--follow" },
		description: "Follow log output (like tail -f)",
		getDefaultValue: () => true);

	private static readonly Option<int> LinesOption = new(
		aliases: new[] { "-n", "--lines" },
		description: "Number of lines to show (0 = all)",
		getDefaultValue: () => 50);

	public static Command Create()
	{
		var command = new Command("logs", "View device logs");
		command.AddOption(DeviceOption);
		command.AddOption(FilterOption);
		command.AddOption(FollowOption);
		command.AddOption(LinesOption);

		command.SetHandler(async (InvocationContext context) =>
		{
			var formatter = Program.GetFormatter(context);
			var deviceId = context.ParseResult.GetValueForOption(DeviceOption);
			var filter = context.ParseResult.GetValueForOption(FilterOption);
			var follow = context.ParseResult.GetValueForOption(FollowOption);
			var lines = context.ParseResult.GetValueForOption(LinesOption);

			try
			{
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
					formatter.WriteInfo($"Streaming logs from: {runningDevice.Name}");
				}

				// Determine platform and run appropriate log command
				var device = await Program.DeviceManager.GetDeviceByIdAsync(deviceId, context.GetCancellationToken());
				if (device == null)
				{
					formatter.WriteError(new ErrorResult
					{
						Code = ErrorCodes.DeviceNotFound,
						Category = "tool",
						Message = $"Device not found: {deviceId}"
					});
					context.ExitCode = 1;
					return;
				}

				await StreamLogsAsync(device, filter, follow, lines, context.GetCancellationToken());
			}
			catch (OperationCanceledException)
			{
				// User cancelled, that's fine
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

	private static async Task StreamLogsAsync(Device device, string? filter, bool follow, int lines, CancellationToken cancellationToken)
	{
		string command;
		string args;

		switch (device.Platform.ToLowerInvariant())
		{
			case Platforms.Android:
				var sdkPath = PlatformDetector.Paths.GetAndroidSdkPath();
				var adbPath = sdkPath != null 
					? Path.Combine(sdkPath, "platform-tools", PlatformDetector.IsWindows ? "adb.exe" : "adb")
					: "adb";
				
				command = adbPath;
				args = $"-s {device.Id} logcat";
				
				if (!string.IsNullOrEmpty(filter))
					args += $" -s {filter}";
				if (!follow)
					args += " -d"; // Dump and exit
				break;

			case Platforms.iOS:
			case Platforms.MacCatalyst:
				if (!PlatformDetector.IsMacOS)
				{
					throw new MauiToolException(
						ErrorCodes.PlatformNotSupported,
						"iOS log streaming is only available on macOS");
				}

				command = "xcrun";
				args = $"simctl spawn {device.Id} log stream";
				
				if (!string.IsNullOrEmpty(filter))
					args += $" --predicate 'subsystem CONTAINS \"{filter}\"'";
				break;

			default:
				throw new MauiToolException(
					ErrorCodes.PlatformNotSupported,
					$"Log streaming not supported for platform: {device.Platform}");
		}

		// Stream output directly to console
		var process = new System.Diagnostics.Process
		{
			StartInfo = new System.Diagnostics.ProcessStartInfo
			{
				FileName = command,
				Arguments = args,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			}
		};

		process.OutputDataReceived += (s, e) =>
		{
			if (e.Data != null)
				Console.WriteLine(e.Data);
		};

		process.ErrorDataReceived += (s, e) =>
		{
			if (e.Data != null)
				Console.Error.WriteLine(e.Data);
		};

		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		// Wait for cancellation or process exit
		try
		{
			await process.WaitForExitAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			process.Kill(entireProcessTree: true);
			throw;
		}
	}
}
