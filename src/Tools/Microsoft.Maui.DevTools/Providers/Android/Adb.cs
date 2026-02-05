// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Wrapper for Android Debug Bridge (adb) operations.
/// </summary>
public class Adb
{
	private readonly Func<string?> _getSdkPath;

	// Compiled regex patterns for parsing
	private static readonly Regex s_getPropRegex = new(@"\[([^\]]+)\]:\s*\[([^\]]*)\]", RegexOptions.Compiled);
	private static readonly Regex s_modelRegex = new(@"model:(\S+)", RegexOptions.Compiled);
	private static readonly Regex s_deviceRegex = new(@"device:(\S+)", RegexOptions.Compiled);
	private static readonly Regex s_detailsRegex = new(@"(\w+):(\S+)", RegexOptions.Compiled);

	public Adb(Func<string?> getSdkPath)
	{
		_getSdkPath = getSdkPath;
	}

	private string? SdkPath => _getSdkPath();

	public string? AdbPath
	{
		get
		{
			// Check SDK path first
			if (!string.IsNullOrEmpty(SdkPath))
			{
				var sdkAdb = Path.Combine(SdkPath, "platform-tools", "adb");
				if (PlatformDetector.IsWindows)
					sdkAdb += ".exe";

				if (File.Exists(sdkAdb))
					return sdkAdb;
			}

			// Check PATH
			return ProcessRunner.GetCommandPath("adb");
		}
	}

	public bool IsAvailable => AdbPath != null;

	public async Task<List<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			return new List<Device>();

		var result = await ProcessRunner.RunAsync(
			AdbPath!,
			"devices -l",
			timeout: TimeSpan.FromSeconds(10),
			cancellationToken: cancellationToken);

		if (!result.Success)
			return new List<Device>();

		var devices = ParseDeviceList(result.StandardOutput);

		// Enrich connected physical devices with detailed properties
		var enrichedDevices = new List<Device>();
		foreach (var device in devices)
		{
			if (device.IsRunning && !device.IsEmulator)
			{
				var enriched = await EnrichDevicePropertiesAsync(device, cancellationToken);
				enrichedDevices.Add(enriched);
			}
			else
			{
				enrichedDevices.Add(device);
			}
		}

		return enrichedDevices;
	}

	private async Task<Device> EnrichDevicePropertiesAsync(Device device, CancellationToken cancellationToken)
	{
		try
		{
			// Get multiple properties in one call for efficiency
			var propsResult = await ProcessRunner.RunAsync(
				AdbPath!,
				$"-s {device.Id} shell getprop",
				timeout: TimeSpan.FromSeconds(5),
				cancellationToken: cancellationToken);

			if (!propsResult.Success)
				return device;

			var props = ParseGetProp(propsResult.StandardOutput);

			// Extract properties
			props.TryGetValue("ro.build.version.sdk", out var sdkVersion);
			props.TryGetValue("ro.build.version.release", out var versionRelease);
			props.TryGetValue("ro.product.manufacturer", out var manufacturer);
			props.TryGetValue("ro.product.cpu.abi", out var abi);
			props.TryGetValue("ro.product.model", out var model);
			props.TryGetValue("ro.product.brand", out var brand);

			// Build details dictionary
			var details = device.Details ?? new Dictionary<string, object>();
			if (!string.IsNullOrEmpty(sdkVersion))
				details["sdk_version"] = sdkVersion;
			if (!string.IsNullOrEmpty(versionRelease))
				details["version_release"] = versionRelease;
			if (!string.IsNullOrEmpty(manufacturer))
				details["manufacturer"] = manufacturer;
			if (!string.IsNullOrEmpty(abi))
				details["abi"] = abi;
			if (!string.IsNullOrEmpty(brand))
				details["brand"] = brand;

			// Determine architecture from ABI
			var architecture = AndroidEnvironment.MapAbiToArchitecture(abi) ?? device.Architecture;

			// Create enriched device name
			var displayName = !string.IsNullOrEmpty(brand) && !string.IsNullOrEmpty(model)
				? $"{CapitalizeFirst(brand)} {model}"
				: device.Name;

			return device with
			{
				Name = displayName,
				Version = sdkVersion,
				VersionName = !string.IsNullOrEmpty(versionRelease) ? $"Android {versionRelease}" : null,
				Manufacturer = CapitalizeFirst(manufacturer),
				Model = model ?? device.Model,
				Architecture = architecture,
				PlatformArchitecture = abi ?? device.PlatformArchitecture,
				RuntimeIdentifiers = AndroidEnvironment.GetRuntimeIdentifiers(architecture),
				Details = details
			};
		}
		catch
		{
			// Return original device if enrichment fails
			return device;
		}
	}

	private static string? CapitalizeFirst(string? value)
	{
		if (string.IsNullOrEmpty(value))
			return value;
		return char.ToUpper(value[0], System.Globalization.CultureInfo.InvariantCulture) + value.Substring(1);
	}

	private static Dictionary<string, string> ParseGetProp(string output)
	{
		var props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var lines = output.Split('\n');

		foreach (var line in lines)
		{
			var match = s_getPropRegex.Match(line);
			if (match.Success)
			{
				props[match.Groups[1].Value] = match.Groups[2].Value;
			}
		}

		return props;
	}

	private static List<Device> ParseDeviceList(string output)
	{
		var devices = new List<Device>();
		var lines = output.Split('\n');

		foreach (var line in lines)
		{
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith("List of devices"))
				continue;

			var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2)
				continue;

			var serial = parts[0];
			var status = parts[1];

			// Skip header or invalid lines
			if (serial == "attached" || !IsValidSerial(serial))
				continue;

			var isEmulator = serial.StartsWith("emulator");
			var state = MapAdbState(status);
			var deviceDetails = ParseDeviceDetails(line);
			var model = ExtractDeviceName(line);
			var architecture = ExtractArchitecture(deviceDetails);

			var device = new Device
			{
				Id = serial,
				Name = model ?? serial,
				Platforms = new[] { "android" },
				Type = isEmulator ? DeviceType.Emulator : DeviceType.Physical,
				State = state,
				IsEmulator = isEmulator,
				IsRunning = state == DeviceState.Booted || state == DeviceState.Connected,
				ConnectionType = isEmulator ? Models.ConnectionType.Local : Models.ConnectionType.Usb,
				EmulatorId = isEmulator ? serial : null,
				Model = model,
				Manufacturer = ExtractManufacturer(deviceDetails),
				Architecture = architecture,
				PlatformArchitecture = architecture,
				RuntimeIdentifiers = AndroidEnvironment.GetRuntimeIdentifiers(architecture),
				Idiom = DeviceIdiom.Phone,
				Details = deviceDetails
			};

			devices.Add(device);
		}

		return devices;
	}

	private static bool IsValidSerial(string serial)
	{
		return !string.IsNullOrEmpty(serial) && 
			   serial != "attached" && 
			   !serial.StartsWith("*");
	}

	private static string? ExtractManufacturer(Dictionary<string, object>? details)
	{
		if (details?.TryGetValue("manufacturer", out var mfr) == true)
			return mfr?.ToString();
		return null;
	}

	private static string? ExtractArchitecture(Dictionary<string, object>? details)
	{
		if (details?.TryGetValue("abi", out var abi) == true)
			return AndroidEnvironment.MapAbiToArchitecture(abi?.ToString());
		return null;
	}

	private static string? ExtractDeviceName(string line)
	{
		var modelMatch = s_modelRegex.Match(line);
		if (modelMatch.Success)
			return modelMatch.Groups[1].Value.Replace('_', ' ');

		var deviceMatch = s_deviceRegex.Match(line);
		if (deviceMatch.Success)
			return deviceMatch.Groups[1].Value;

		return null;
	}

	private static Dictionary<string, object> ParseDeviceDetails(string line)
	{
		var details = new Dictionary<string, object>();

		var matches = s_detailsRegex.Matches(line);
		foreach (Match match in matches)
		{
			details[match.Groups[1].Value] = match.Groups[2].Value;
		}

		return details;
	}

	private static DeviceState MapAdbState(string state)
	{
		return state.ToLowerInvariant() switch
		{
			"device" => DeviceState.Connected,
			"offline" => DeviceState.Offline,
			"unauthorized" => DeviceState.Disconnected,
			"bootloader" => DeviceState.Booting,
			_ => DeviceState.Unknown
		};
	}

	public async Task StopEmulatorAsync(string deviceSerial, CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw new MauiToolException(ErrorCodes.AndroidAdbNotFound, "ADB not found");

		var result = await ProcessRunner.RunAsync(
			AdbPath!,
			$"-s {deviceSerial} emu kill",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		// emu kill doesn't always return success, so just check the device list
		await Task.Delay(1000, cancellationToken);
	}

	public async Task<string> TakeScreenshotAsync(string deviceSerial, string outputPath,
		CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw new MauiToolException(ErrorCodes.AndroidAdbNotFound, "ADB not found");

		var tempPath = "/sdcard/screenshot.png";

		// Capture screenshot on device
		var captureResult = await ProcessRunner.RunAsync(
			AdbPath!,
			$"-s {deviceSerial} shell screencap -p {tempPath}",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!captureResult.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidDeviceNotFound,
				$"Failed to capture screenshot: {captureResult.StandardError}",
				nativeError: captureResult.StandardError);
		}

		// Pull screenshot to local machine
		var pullResult = await ProcessRunner.RunAsync(
			AdbPath!,
			$"-s {deviceSerial} pull {tempPath} \"{outputPath}\"",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!pullResult.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidDeviceNotFound,
				$"Failed to pull screenshot: {pullResult.StandardError}",
				nativeError: pullResult.StandardError);
		}

		// Clean up on device
		await ProcessRunner.RunAsync(
			AdbPath!,
			$"-s {deviceSerial} shell rm {tempPath}",
			timeout: TimeSpan.FromSeconds(10),
			cancellationToken: cancellationToken);

		return outputPath;
	}

	public async Task WaitForDeviceAsync(string? deviceSerial = null, TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw new MauiToolException(ErrorCodes.AndroidAdbNotFound, "ADB not found");

		var args = deviceSerial != null ? $"-s {deviceSerial} wait-for-device" : "wait-for-device";
		var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(2);

		var result = await ProcessRunner.RunAsync(
			AdbPath!,
			args,
			timeout: effectiveTimeout,
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidDeviceNotFound,
				"Timeout waiting for device",
				nativeError: result.StandardError);
		}

		// Wait for boot completion
		for (int i = 0; i < 60; i++)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var bootResult = await ProcessRunner.RunAsync(
				AdbPath!,
				$"{(deviceSerial != null ? $"-s {deviceSerial} " : "")}shell getprop sys.boot_completed",
				timeout: TimeSpan.FromSeconds(5),
				cancellationToken: cancellationToken);

			if (bootResult.Success && bootResult.StandardOutput.Trim() == "1")
				return;

			await Task.Delay(1000, cancellationToken);
		}

		throw new MauiToolException(
			ErrorCodes.AndroidDeviceNotFound,
			"Timeout waiting for device to complete boot");
	}
}
