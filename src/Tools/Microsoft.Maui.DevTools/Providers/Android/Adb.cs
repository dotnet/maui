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
	private readonly AdbRunner _runner;

	public Adb(Func<string?> getSdkPath)
	{
		_runner = new AdbRunner(getSdkPath);
	}

	public string? AdbPath => _runner.AdbPath;

	public bool IsAvailable => _runner.IsAvailable;

	public async Task<List<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var devices = await _runner.ListDevicesAsync(cancellationToken);
			return devices.Select(MapToMauiDevice).ToList();
		}
		catch (InvalidOperationException)
		{
			return new List<Device>();
		}
	}

	private static Device MapToMauiDevice(AndroidDeviceInfo info)
	{
		var isEmulator = info.IsEmulator;
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
			Model = info.Model,
			Idiom = DeviceIdiom.Phone,
		};
	}

	private static DeviceState MapDeviceState(AndroidDeviceState state)
	{
		return state switch
		{
			AndroidDeviceState.Online => DeviceState.Connected,
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

		await _runner.StopEmulatorAsync(deviceSerial, cancellationToken);
	}

	public async Task WaitForDeviceAsync(string? deviceSerial = null, TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw new MauiToolException(ErrorCodes.AndroidAdbNotFound, "ADB not found");

		try
		{
			await _runner.WaitForDeviceAsync(deviceSerial, timeout, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidDeviceNotFound,
				ex.Message);
		}
	}

	public async Task<string> TakeScreenshotAsync(string deviceSerial, string outputPath,
		CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw new MauiToolException(ErrorCodes.AndroidAdbNotFound, "ADB not found");

		var remotePath = "/sdcard/screenshot.png";

		// Capture screenshot on device
		var captureResult = await ProcessRunner.RunAsync(AdbPath!, $"-s {deviceSerial} shell screencap -p {remotePath}", cancellationToken: cancellationToken);
		if (!captureResult.Success)
			throw new MauiToolException(ErrorCodes.AndroidDeviceNotFound,
				$"Failed to capture screenshot: {captureResult.StandardError}",
				nativeError: captureResult.StandardError);

		// Pull screenshot to local path
		var pullResult = await ProcessRunner.RunAsync(AdbPath!, $"-s {deviceSerial} pull {remotePath} {outputPath}", cancellationToken: cancellationToken);
		if (!pullResult.Success)
			throw new MauiToolException(ErrorCodes.AndroidDeviceNotFound,
				$"Failed to pull screenshot: {pullResult.StandardError}",
				nativeError: pullResult.StandardError);

		return outputPath;
	}
}
