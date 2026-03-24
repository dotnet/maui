// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Errors;
using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Utils;
using Xamarin.Android.Tools;

namespace Microsoft.Maui.Client.Providers.Android;

/// <summary>
/// Wrapper for Android Debug Bridge (adb) operations.
/// Delegates to Xamarin.Android.Tools.AdbRunner for core functionality.
/// </summary>
public class Adb
{
	private readonly AdbRunner? _runner;
	private readonly string? _adbPath;

	public Adb(Func<string?> getSdkPath, IDictionary<string, string>? environmentVariables = null)
	{
		_adbPath = ResolveAdbPath(getSdkPath());
		if (_adbPath != null)
			_runner = new AdbRunner(_adbPath, environmentVariables);
	}

	public string? AdbPath => _adbPath;

	public bool IsAvailable => _runner != null;

	internal AdbRunner? Runner => _runner;

	private static string? ResolveAdbPath(string? sdkPath)
	{
		if (string.IsNullOrEmpty(sdkPath))
			return null;

		var ext = OperatingSystem.IsWindows() ? ".exe" : "";
		var path = Path.Combine(sdkPath, "platform-tools", "adb" + ext);
		return File.Exists(path) ? path : null;
	}

	public async Task<List<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
	{
		if (_runner == null)
			return new List<Device>();

		try
		{
			// AdbRunner.ListDevicesAsync already queries AVD names for online emulators
			// via getprop ro.boot.qemu.avd_name + emu avd name fallback
			var devices = await _runner.ListDevicesAsync(cancellationToken);
			return devices.Select(MapToMauiDevice).ToList();
		}
		catch (InvalidOperationException ex)
		{
			System.Diagnostics.Trace.WriteLine($"ADB GetDevicesAsync failed: {ex.Message}");
			return new List<Device>();
		}
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

		await _runner!.StopEmulatorAsync(deviceSerial, cancellationToken);
	}

}
