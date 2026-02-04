// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Providers.Android;
using Microsoft.Maui.DevTools.Providers.Apple;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Services;

/// <summary>
/// Manages devices across all platforms.
/// </summary>
public class DeviceManager : IDeviceManager
{
	private readonly IAndroidProvider? _androidProvider;
	private readonly IAppleProvider? _appleProvider;

	public DeviceManager(IAndroidProvider? androidProvider = null, IAppleProvider? appleProvider = null)
	{
		_androidProvider = androidProvider;
		_appleProvider = appleProvider;
	}

	public async Task<IReadOnlyList<Device>> GetAllDevicesAsync(CancellationToken cancellationToken = default)
	{
		var devices = new List<Device>();

		// Get Android devices
		if (_androidProvider != null)
		{
			var androidDevices = await _androidProvider.GetDevicesAsync(cancellationToken);
			devices.AddRange(androidDevices);

			// Also get AVDs (virtual devices that may not be running)
			var avds = await _androidProvider.GetAvdsAsync(cancellationToken);
			foreach (var avd in avds)
			{
				// Check if this AVD is already in the running devices list
				var isRunning = devices.Any(d => 
					d.Platform == "android" && 
					d.Details != null &&
					d.Details.TryGetValue("avd", out var avdName) && 
					avdName?.ToString() == avd.Name);

				if (!isRunning)
				{
					devices.Add(new Device
					{
						Id = $"avd:{avd.Name}",
						Name = avd.Name,
						Platform = "android",
						Type = DeviceType.Emulator,
						State = DeviceState.Shutdown,
						Details = new Dictionary<string, object>
						{
							["avd"] = avd.Name,
							["target"] = avd.Target ?? "unknown"
						}
					});
				}
			}
		}

		// Get Apple devices (simulators)
		if (_appleProvider != null && PlatformDetector.IsMacOS)
		{
			var simulators = await _appleProvider.ListSimulatorsAsync(cancellationToken);
			devices.AddRange(simulators);
		}

		// TODO: Get Windows devices when WindowsProvider is implemented

		return devices;
	}

	public async Task<IReadOnlyList<Device>> GetDevicesByPlatformAsync(string platform, CancellationToken cancellationToken = default)
	{
		var allDevices = await GetAllDevicesAsync(cancellationToken);
		return allDevices.Where(d => d.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase)).ToList();
	}

	public async Task<Device?> GetDeviceByIdAsync(string deviceId, CancellationToken cancellationToken = default)
	{
		var allDevices = await GetAllDevicesAsync(cancellationToken);
		return allDevices.FirstOrDefault(d => d.Id.Equals(deviceId, StringComparison.OrdinalIgnoreCase));
	}

	public async Task<string> TakeScreenshotAsync(string deviceId, string? outputPath = null, CancellationToken cancellationToken = default)
	{
		var device = await GetDeviceByIdAsync(deviceId, cancellationToken);
		if (device == null)
		{
			throw new MauiToolException(
				ErrorCodes.DeviceNotFound,
				$"Device not found: {deviceId}");
		}

		outputPath ??= Path.Combine(
			Environment.CurrentDirectory,
			$"screenshot_{device.Platform}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

		switch (device.Platform.ToLowerInvariant())
		{
			case "android":
				if (_androidProvider == null)
					throw new MauiToolException(ErrorCodes.AndroidSdkNotFound, "Android provider not available");
				return await _androidProvider.TakeScreenshotAsync(deviceId, outputPath, cancellationToken);

			case "ios":
			case "apple":
				return await TakeAppleScreenshotAsync(deviceId, outputPath, cancellationToken);

			default:
				throw new MauiToolException(
					ErrorCodes.PlatformNotSupported,
					$"Screenshot not supported for platform: {device.Platform}");
		}
	}

	private async Task<string> TakeAppleScreenshotAsync(string udid, string outputPath, CancellationToken cancellationToken)
	{
		if (!PlatformDetector.IsMacOS)
			throw new MauiToolException(ErrorCodes.PlatformNotSupported, "Apple screenshot is only available on macOS");

		var result = await ProcessRunner.RunAsync(
			"xcrun", $"simctl io \"{udid}\" screenshot \"{outputPath}\"",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AppleSimctlFailed,
				$"Failed to take screenshot: {result.StandardError}",
				nativeError: result.StandardError);
		}

		return outputPath;
	}
}
