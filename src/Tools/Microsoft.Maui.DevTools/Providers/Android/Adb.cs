// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

		return ParseDeviceList(result.StandardOutput);
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

			var device = new Device
			{
				Id = serial,
				Name = ExtractDeviceName(line) ?? serial,
				Platform = "android",
				Type = serial.StartsWith("emulator") ? DeviceType.Emulator : DeviceType.Physical,
				State = MapAdbState(status),
				Details = ParseDeviceDetails(line)
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

	private static string? ExtractDeviceName(string line)
	{
		// Try to extract model from device info
		var modelMatch = System.Text.RegularExpressions.Regex.Match(line, @"model:(\S+)");
		if (modelMatch.Success)
			return modelMatch.Groups[1].Value.Replace('_', ' ');

		var deviceMatch = System.Text.RegularExpressions.Regex.Match(line, @"device:(\S+)");
		if (deviceMatch.Success)
			return deviceMatch.Groups[1].Value;

		return null;
	}

	private static Dictionary<string, object> ParseDeviceDetails(string line)
	{
		var details = new Dictionary<string, object>();

		var matches = System.Text.RegularExpressions.Regex.Matches(line, @"(\w+):(\S+)");
		foreach (System.Text.RegularExpressions.Match match in matches)
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
