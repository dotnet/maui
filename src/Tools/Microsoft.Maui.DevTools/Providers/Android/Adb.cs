// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;
using Xamarin.Android.Tools;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Wrapper for Android Debug Bridge (adb) operations.
/// Delegates to Xamarin.Android.Tools.AdbRunner for core functionality.
/// </summary>
public class Adb
{
	private readonly Func<string?> _getSdkPath;
	private readonly AdbRunner _runner;

	public Adb(Func<string?> getSdkPath)
	{
		_getSdkPath = getSdkPath;
		_runner = new AdbRunner(getSdkPath);
	}

	private string? SdkPath => _getSdkPath();

	public string? AdbPath => _runner.AdbPath;

	public bool IsAvailable => _runner.IsAvailable;

	public async Task<List<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
	{
		var result = await _runner.ListDevicesAsync(enrichProperties: true, cancellationToken);
		if (!result.Success || result.Data == null)
			return new List<Device>();

		return result.Data.Select(MapToMauiDevice).ToList();
	}

	private static Device MapToMauiDevice(AndroidDeviceInfo info)
	{
		var isEmulator = info.Type == AndroidDeviceType.Emulator;
		var state = MapDeviceState(info.State);
		var isRunning = state == DeviceState.Connected || state == DeviceState.Booted;

		return new Device
		{
			Id = info.Serial,
			Name = info.Name ?? info.Serial,
			Platforms = new[] { "android" },
			Type = isEmulator ? DeviceType.Emulator : DeviceType.Physical,
			State = state,
			IsEmulator = isEmulator,
			IsRunning = isRunning,
			ConnectionType = isEmulator ? ConnectionType.Local : ConnectionType.Usb,
			EmulatorId = info.AvdName,
			Model = info.Model,
			Manufacturer = info.Manufacturer,
			Version = info.SdkVersion,
			VersionName = info.ReleaseVersion ?? AndroidEnvironment.MapApiLevelToVersion(info.SdkVersion),
			Architecture = AndroidEnvironment.MapAbiToArchitecture(info.Abi),
			PlatformArchitecture = info.Abi,
			RuntimeIdentifiers = AndroidEnvironment.GetRuntimeIdentifiers(AndroidEnvironment.MapAbiToArchitecture(info.Abi)),
			Idiom = DeviceIdiom.Phone,
			Details = info.Properties.Count > 0 
				? info.Properties.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) 
				: null
		};
	}

	private static DeviceState MapDeviceState(AndroidDeviceState state)
	{
		return state switch
		{
			AndroidDeviceState.Device => DeviceState.Connected,
			AndroidDeviceState.Offline => DeviceState.Offline,
			AndroidDeviceState.Unauthorized => DeviceState.Disconnected,
			AndroidDeviceState.Bootloader => DeviceState.Booting,
			_ => DeviceState.Unknown
		};
	}

	public async Task StopEmulatorAsync(string deviceSerial, CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw new MauiToolException(ErrorCodes.AndroidAdbNotFound, "ADB not found");

		var result = await _runner.StopEmulatorAsync(deviceSerial, cancellationToken);

		// emu kill doesn't always return success, so don't throw on failure
	}

	public async Task<string> TakeScreenshotAsync(string deviceSerial, string outputPath,
		CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw new MauiToolException(ErrorCodes.AndroidAdbNotFound, "ADB not found");

		var result = await _runner.TakeScreenshotAsync(deviceSerial, outputPath, cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidDeviceNotFound,
				$"Failed to capture screenshot: {result.ErrorMessage ?? result.StandardError}",
				nativeError: result.StandardError);
		}

		return outputPath;
	}

	public async Task WaitForDeviceAsync(string? deviceSerial = null, TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw new MauiToolException(ErrorCodes.AndroidAdbNotFound, "ADB not found");

		var result = await _runner.WaitForDeviceAsync(deviceSerial, timeout, cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidDeviceNotFound,
				result.ErrorMessage ?? "Timeout waiting for device",
				nativeError: result.StandardError);
		}
	}
}
