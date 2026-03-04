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
			var result = new List<Device>();

			foreach (var info in devices)
			{
				// If this is a running emulator without an AVD name, query it via adb
				if (info.IsEmulator && string.IsNullOrEmpty(info.AvdName) && info.Status == AdbDeviceStatus.Online)
				{
					info.AvdName = await QueryAvdNameAsync(info.Serial, cancellationToken);
				}
				result.Add(MapToMauiDevice(info));
			}

			return result;
		}
		catch (InvalidOperationException)
		{
			return new List<Device>();
		}
	}

	/// <summary>
	/// Queries a running emulator for its AVD name via 'adb -s &lt;serial&gt; emu avd name'.
	/// </summary>
	private async Task<string?> QueryAvdNameAsync(string serial, CancellationToken cancellationToken)
	{
		if (AdbPath == null) return null;

		try
		{
			var result = await ProcessRunner.RunAsync(AdbPath, $"-s {serial} emu avd name",
				timeout: TimeSpan.FromSeconds(5), cancellationToken: cancellationToken);
			if (result.Success && !string.IsNullOrWhiteSpace(result.StandardOutput))
			{
				// Output is typically "AVD_NAME\nOK\n" — take the first non-empty line
				var name = result.StandardOutput
					.Split('\n', StringSplitOptions.RemoveEmptyEntries)
					.FirstOrDefault(l => !l.Equals("OK", StringComparison.OrdinalIgnoreCase))
					?.Trim();
				return string.IsNullOrEmpty(name) ? null : name;
			}
		}
		catch { /* non-critical — fall back to no AVD name */ }

		return null;
	}

	private static Device MapToMauiDevice(AdbDeviceInfo info)
	{
		var isEmulator = info.IsEmulator;
		var state = MapDeviceState(info.Status);
		var isRunning = state == DeviceState.Connected || state == DeviceState.Booted;

		var details = new Dictionary<string, object>();
		if (!string.IsNullOrEmpty(info.AvdName))
			details["avd"] = info.AvdName;

		return new Device
		{
			Id = info.Serial,
			Name = !string.IsNullOrEmpty(info.AvdName) ? info.AvdName : (info.Model ?? info.Serial),
			Platforms = new[] { "android" },
			Type = isEmulator ? DeviceType.Emulator : DeviceType.Physical,
			State = state,
			IsEmulator = isEmulator,
			IsRunning = isRunning,
			ConnectionType = isEmulator ? ConnectionType.Local : ConnectionType.Usb,
			EmulatorId = info.AvdName,
			Model = info.Model,
			Idiom = DeviceIdiom.Phone,
			Details = details.Count > 0 ? details : null,
		};
	}

	private static DeviceState MapDeviceState(AdbDeviceStatus status)
	{
		return status switch
		{
			AdbDeviceStatus.Online => DeviceState.Connected,
			AdbDeviceStatus.Offline => DeviceState.Offline,
			AdbDeviceStatus.Unauthorized => DeviceState.Disconnected,
			AdbDeviceStatus.NotRunning => DeviceState.Shutdown,
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
