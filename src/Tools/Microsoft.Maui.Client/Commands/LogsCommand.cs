// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.Client.Errors;
using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Output;
using Microsoft.Maui.Client.Utils;

namespace Microsoft.Maui.Client.Commands;

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

	public static Command Create()
	{
		var command = new Command("logs", "View device logs");
		command.AddOption(DeviceOption);
		command.AddOption(FilterOption);
		command.AddOption(FollowOption);

		command.SetHandler(async (InvocationContext context) =>
		{
			var formatter = Program.GetFormatter(context);
			var deviceId = context.ParseResult.GetValueForOption(DeviceOption);
			var filter = context.ParseResult.GetValueForOption(FilterOption);
			var follow = context.ParseResult.GetValueForOption(FollowOption);

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

				await StreamDeviceLogsAsync(device, filter, follow, context.GetCancellationToken());
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

	internal static async Task StreamDeviceLogsAsync(Device device, string? filter, bool follow, CancellationToken cancellationToken)
	{
		string command;
		string args;

		var platform = device.Platform?.ToLowerInvariant()
			?? throw new MauiToolException(ErrorCodes.PlatformNotSupported, "Device platform is not set.");

		switch (platform)
		{
			case Platforms.Android:
				var sdkPath = PlatformDetector.Paths.GetAndroidSdkPath();
				var adbPath = sdkPath != null
					? Path.Combine(sdkPath, "platform-tools", PlatformDetector.IsWindows ? "adb.exe" : "adb")
					: "adb";

				command = adbPath;
				args = $"-s {ProcessRunner.SanitizeArg(device.Id)} logcat";

				if (!string.IsNullOrEmpty(filter))
					args += $" -s {ProcessRunner.SanitizeArg(filter)}";
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
				args = $"simctl spawn {ProcessRunner.SanitizeArg(device.Id)} log stream";

				if (!string.IsNullOrEmpty(filter))
					args += $" --predicate 'subsystem CONTAINS \"{ProcessRunner.SanitizeArg(filter)}\"'";
				break;

			default:
				throw new MauiToolException(
					ErrorCodes.PlatformNotSupported,
					$"Log streaming not supported for platform: {device.Platform}");
		}

		// Stream output directly to console
		using var process = new System.Diagnostics.Process
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
				WriteColorizedLogLine(e.Data);
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

	/// <summary>
	/// Writes a log line with color based on log level.
	/// Detects Android logcat format (E/, W/, I/, D/, V/) and iOS os_log levels.
	/// </summary>
	private static void WriteColorizedLogLine(string line)
	{
		// Android logcat: "06-01 12:00:00.000  1234  5678 E Tag: message"
		// or brief format: "E/Tag(1234): message"
		var color = DetectLogLevel(line);
		if (color != null)
		{
			Console.ForegroundColor = color.Value;
			Console.WriteLine(line);
			Console.ResetColor();
		}
		else
		{
			Console.WriteLine(line);
		}
	}

	private static ConsoleColor? DetectLogLevel(string line)
	{
		if (line.Length < 2)
			return null;

		// Android logcat long format: level letter at column ~31, or after PID/TID
		// "06-01 12:00:00.000  1234  5678 E Tag: ..."
		// Android brief format: "E/Tag(1234): ..."
		// Also handle: " E " anywhere early in the line

		// Brief format: starts with "X/" where X is level
		if (line.Length > 1 && line[1] == '/')
		{
			return line[0] switch
			{
				'E' => ConsoleColor.Red,
				'W' => ConsoleColor.Yellow,
				'I' => ConsoleColor.Cyan,
				'D' => ConsoleColor.Gray,
				'V' => ConsoleColor.DarkGray,
				_ => null
			};
		}

		// Long format: find single letter level after timestamp+pids (roughly chars 30-35)
		// Pattern: "MM-DD HH:MM:SS.mmm  PID  TID L Tag: msg"
		var searchEnd = Math.Min(line.Length, 40);
		for (int i = 20; i < searchEnd; i++)
		{
			if (line[i] == ' ' && i + 2 < line.Length && line[i + 2] == ' ')
			{
				return line[i + 1] switch
				{
					'E' => ConsoleColor.Red,
					'W' => ConsoleColor.Yellow,
					'I' => ConsoleColor.Cyan,
					'D' => ConsoleColor.Gray,
					'V' => ConsoleColor.DarkGray,
					_ => null
				};
			}
		}

		// iOS: look for common level indicators
		if (line.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
			line.Contains("<Error>", StringComparison.Ordinal))
			return ConsoleColor.Red;
		if (line.Contains("Warning", StringComparison.OrdinalIgnoreCase) ||
			line.Contains("<Warning>", StringComparison.Ordinal))
			return ConsoleColor.Yellow;
		if (line.Contains("<Debug>", StringComparison.Ordinal))
			return ConsoleColor.Gray;

		return null;
	}
}
