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
				// Match by AVD name in details dict or by EmulatorId
				var runningIndex = devices.FindIndex(d => 
					d.Platforms.Contains("android") && 
					d.IsEmulator &&
					(
						(d.Details != null &&
						 d.Details.TryGetValue("avd", out var avdName) && 
						 string.Equals(avdName?.ToString(), avd.Name, StringComparison.OrdinalIgnoreCase))
						||
						string.Equals(d.EmulatorId, avd.Name, StringComparison.OrdinalIgnoreCase)
					));

				// Extract metadata from system image path (e.g., "system-images;android-35;google_apis_playstore;arm64-v8a")
				var (apiLevel, tagId, abi) = ParseSystemImage(avd.SystemImage);
				var playStoreEnabled = tagId?.Contains("playstore", StringComparison.OrdinalIgnoreCase) ?? false;

				if (runningIndex >= 0)
				{
					// Merge AVD metadata into the running emulator device
					var running = devices[runningIndex];
					var subModel = AndroidEnvironment.MapTagIdToSubModel(tagId, playStoreEnabled);
					var details = running.Details != null
						? new Dictionary<string, object>(running.Details)
						: new Dictionary<string, object>();
					details["tag_id"] = tagId ?? "default";
					details["target"] = avd.Target ?? "unknown";

					devices[runningIndex] = running with
					{
						EmulatorId = avd.Name,
						SubModel = subModel,
						Details = details
					};
				}
				else
				{
					var architecture = AndroidEnvironment.MapAbiToArchitecture(abi) ?? (PlatformDetector.IsArm64 ? "arm64" : "x64");
					var resolvedAbi = abi ?? (PlatformDetector.IsArm64 ? "arm64-v8a" : "x86_64");
					var versionName = AndroidEnvironment.MapApiLevelToVersion(apiLevel);
					var subModel = AndroidEnvironment.MapTagIdToSubModel(tagId, playStoreEnabled);
					devices.Add(new Device
					{
						Id = avd.Name,
						Name = avd.Name,
						Platforms = new[] { "android" },
						Type = DeviceType.Emulator,
						State = DeviceState.Shutdown,
						IsEmulator = true,
						IsRunning = false,
						ConnectionType = Models.ConnectionType.Local,
						EmulatorId = avd.Name,
						Model = avd.DeviceProfile,
						SubModel = subModel,
						Manufacturer = "Google",
						Version = apiLevel,
						VersionName = versionName,
						Architecture = architecture,
						PlatformArchitecture = resolvedAbi,
						RuntimeIdentifiers = AndroidEnvironment.GetRuntimeIdentifiers(architecture),
						Idiom = DeviceIdiom.Phone,
						Details = new Dictionary<string, object>
						{
							["avd"] = avd.Name,
							["target"] = avd.Target ?? "unknown",
							["api_level"] = apiLevel ?? "unknown",
							["abi"] = resolvedAbi,
							["tag_id"] = tagId ?? "default"
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
		return allDevices.Where(d => d.Platforms.Any(p => p.Equals(platform, StringComparison.OrdinalIgnoreCase))).ToList();
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

	/// <summary>
	/// Parses a system image path like "system-images;android-35;google_apis_playstore;arm64-v8a"
	/// to extract API level, tag ID, and ABI.
	/// </summary>
	private static (string? ApiLevel, string? TagId, string? Abi) ParseSystemImage(string? systemImage)
	{
		if (string.IsNullOrEmpty(systemImage))
			return (null, null, null);

		var parts = systemImage.Split(';', '/');
		string? apiLevel = null;
		string? tagId = null;
		string? abi = null;

		foreach (var part in parts)
		{
			if (part.StartsWith("android-", StringComparison.OrdinalIgnoreCase))
				apiLevel = part.Substring("android-".Length);
			else if (part.Contains("google_apis", StringComparison.OrdinalIgnoreCase) || part == "default")
				tagId = part;
			else if (part is "arm64-v8a" or "x86_64" or "x86" or "armeabi-v7a")
				abi = part;
		}

		return (apiLevel, tagId, abi);
	}
}
