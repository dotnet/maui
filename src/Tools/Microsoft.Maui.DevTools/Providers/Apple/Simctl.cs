// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Providers.Apple;

/// <summary>
/// Wrapper for simctl commands.
/// </summary>
public class Simctl
{
	/// <summary>
	/// Lists all iOS simulators.
	/// </summary>
	public async Task<List<Device>> ListDevicesAsync(CancellationToken cancellationToken = default)
	{
		if (!PlatformDetector.IsMacOS)
			return new List<Device>();

		var result = await ProcessRunner.RunAsync(
			"xcrun", "simctl list devices --json",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
			return new List<Device>();

		try
		{
			var json = JsonDocument.Parse(result.StandardOutput);
			var devices = new List<Device>();

			if (json.RootElement.TryGetProperty("devices", out var devicesElement))
			{
				foreach (var runtime in devicesElement.EnumerateObject())
				{
					var runtimeName = ExtractRuntimeName(runtime.Name);

					foreach (var device in runtime.Value.EnumerateArray())
					{
						var udid = device.GetProperty("udid").GetString();
						var name = device.GetProperty("name").GetString();
						var state = device.GetProperty("state").GetString();
						var isAvailable = device.TryGetProperty("isAvailable", out var avail) && avail.GetBoolean();

						if (udid == null || name == null || !isAvailable)
							continue;

						devices.Add(new Device
						{
							Id = udid,
							Name = $"{name} ({runtimeName})",
							Platform = "ios",
							Type = DeviceType.Simulator,
							State = ParseState(state)
						});
					}
				}
			}

			return devices;
		}
		catch
		{
			return new List<Device>();
		}
	}

	/// <summary>
	/// Lists all available runtimes.
	/// </summary>
	public async Task<List<Runtime>> ListRuntimesAsync(CancellationToken cancellationToken = default)
	{
		if (!PlatformDetector.IsMacOS)
			return new List<Runtime>();

		var result = await ProcessRunner.RunAsync(
			"xcrun", "simctl list runtimes --json",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
			return new List<Runtime>();

		try
		{
			var json = JsonDocument.Parse(result.StandardOutput);
			var runtimes = new List<Runtime>();

			if (json.RootElement.TryGetProperty("runtimes", out var runtimesElement))
			{
				foreach (var runtime in runtimesElement.EnumerateArray())
				{
					var identifier = runtime.GetProperty("identifier").GetString();
					var name = runtime.GetProperty("name").GetString();
					var version = runtime.TryGetProperty("version", out var v) ? v.GetString() : null;
					var isAvailable = runtime.TryGetProperty("isAvailable", out var avail) && avail.GetBoolean();

					if (identifier == null || name == null)
						continue;

					runtimes.Add(new Runtime
					{
						Identifier = identifier,
						Name = name,
						Version = version ?? "unknown",
						IsAvailable = isAvailable
					});
				}
			}

			return runtimes;
		}
		catch
		{
			return new List<Runtime>();
		}
	}

	/// <summary>
	/// Creates a new simulator device.
	/// </summary>
	public async Task<Device> CreateDeviceAsync(string name, string deviceType, string runtime,
		CancellationToken cancellationToken = default)
	{
		var result = await ProcessRunner.RunAsync(
			"xcrun", $"simctl create \"{name}\" \"{deviceType}\" \"{runtime}\"",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AppleSimctlFailed,
				$"Failed to create simulator: {result.StandardError}",
				nativeError: result.StandardError);
		}

		var udid = result.StandardOutput.Trim();

		return new Device
		{
			Id = udid,
			Name = name,
			Platform = "ios",
			Type = DeviceType.Simulator,
			State = DeviceState.Shutdown
		};
	}

	/// <summary>
	/// Deletes a simulator device.
	/// </summary>
	public async Task DeleteDeviceAsync(string udid, CancellationToken cancellationToken = default)
	{
		var result = await ProcessRunner.RunAsync(
			"xcrun", $"simctl delete \"{udid}\"",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AppleSimctlFailed,
				$"Failed to delete simulator: {result.StandardError}",
				nativeError: result.StandardError);
		}
	}

	/// <summary>
	/// Boots a simulator device.
	/// </summary>
	public async Task BootDeviceAsync(string udid, CancellationToken cancellationToken = default)
	{
		var result = await ProcessRunner.RunAsync(
			"xcrun", $"simctl boot \"{udid}\"",
			timeout: TimeSpan.FromSeconds(60),
			cancellationToken: cancellationToken);

		// Boot can return error if already booted - that's ok
		if (!result.Success && !result.StandardError.Contains("Unable to boot device in current state", StringComparison.Ordinal))
		{
			throw new MauiToolException(
				ErrorCodes.AppleSimctlFailed,
				$"Failed to boot simulator: {result.StandardError}",
				nativeError: result.StandardError);
		}
	}

	/// <summary>
	/// Shuts down a simulator device.
	/// </summary>
	public async Task ShutdownDeviceAsync(string udid, CancellationToken cancellationToken = default)
	{
		var result = await ProcessRunner.RunAsync(
			"xcrun", $"simctl shutdown \"{udid}\"",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		// Shutdown can return error if already shutdown - that's ok
		if (!result.Success && !result.StandardError.Contains("Unable to shutdown device in current state", StringComparison.Ordinal))
		{
			throw new MauiToolException(
				ErrorCodes.AppleSimctlFailed,
				$"Failed to shutdown simulator: {result.StandardError}",
				nativeError: result.StandardError);
		}
	}

	private static string ExtractRuntimeName(string runtimeIdentifier)
	{
		// com.apple.CoreSimulator.SimRuntime.iOS-17-0 -> iOS 17.0
		var parts = runtimeIdentifier.Split('.');
		if (parts.Length > 0)
		{
			var last = parts[^1];
			return last.Replace('-', '.');
		}
		return runtimeIdentifier;
	}

	private static DeviceState ParseState(string? state)
	{
		return state?.ToLowerInvariant() switch
		{
			"booted" => DeviceState.Booted,
			"shutdown" => DeviceState.Shutdown,
			_ => DeviceState.Unknown
		};
	}
}
