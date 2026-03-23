// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Sockets;
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

	public Adb(Func<string?> getSdkPath)
	{
		_adbPath = ResolveAdbPath(getSdkPath());
		if (_adbPath != null)
			_runner = new AdbRunner(_adbPath);
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
	/// Queries a running emulator for its AVD name via 'adb -s &lt;serial&gt; emu avd name',
	/// falling back to a direct emulator console query if that fails.
	/// </summary>
	private async Task<string?> QueryAvdNameAsync(string serial, CancellationToken cancellationToken)
	{
		if (AdbPath == null)
			return null;

		// Try adb emu avd name first
		try
		{
			var result = await ProcessRunner.RunAsync(AdbPath, $"-s {ProcessRunner.SanitizeArg(serial)} emu avd name",
				timeout: TimeSpan.FromSeconds(5), cancellationToken: cancellationToken);
			if (result.Success && !string.IsNullOrWhiteSpace(result.StandardOutput))
			{
				var name = result.StandardOutput
					.Split('\n', StringSplitOptions.RemoveEmptyEntries)
					.FirstOrDefault(l => !l.Equals("OK", StringComparison.OrdinalIgnoreCase))
					?.Trim();
				if (!string.IsNullOrEmpty(name))
					return name;
			}
		}
		catch (Exception) { /* fall through to console fallback */ }

		// Fallback: query the emulator console directly via TCP
		// Serial format is "emulator-XXXX" where XXXX is the console port
		return await QueryAvdNameViaConsoleAsync(serial, cancellationToken);
	}

	/// <summary>
	/// Queries AVD name by connecting to the emulator console port directly.
	/// Emulator serials follow the format "emulator-{port}" where port is the console port.
	/// </summary>
	private static async Task<string?> QueryAvdNameViaConsoleAsync(string serial, CancellationToken cancellationToken)
	{
		if (!serial.StartsWith("emulator-", StringComparison.OrdinalIgnoreCase))
			return null;

		if (!int.TryParse(serial.AsSpan("emulator-".Length), out var port))
			return null;

		try
		{
			using var client = new TcpClient();
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			cts.CancelAfter(TimeSpan.FromSeconds(3));

			await client.ConnectAsync("127.0.0.1", port, cts.Token);
			using var stream = client.GetStream();
			using var reader = new StreamReader(stream);
			using var writer = new StreamWriter(stream) { AutoFlush = true };

			// Read the welcome banner ("Android Console: ...")
			await reader.ReadLineAsync(cts.Token);
			// Read "OK"
			await reader.ReadLineAsync(cts.Token);

			// Send "avd name" command
			await writer.WriteLineAsync("avd name".AsMemory(), cts.Token);

			// Read AVD name
			var name = await reader.ReadLineAsync(cts.Token);
			return string.IsNullOrWhiteSpace(name) || name.Equals("OK", StringComparison.OrdinalIgnoreCase)
				? null
				: name.Trim();
		}
		catch
		{
			return null;
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

	public async Task<string> TakeScreenshotAsync(string deviceSerial, string outputPath,
		CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw new MauiToolException(ErrorCodes.AndroidAdbNotFound, "ADB not found");

		var remotePath = "/sdcard/screenshot.png";

		// Capture screenshot on device
		var captureResult = await ProcessRunner.RunAsync(AdbPath!, $"-s {ProcessRunner.SanitizeArg(deviceSerial)} shell screencap -p {remotePath}", cancellationToken: cancellationToken);
		if (!captureResult.Success)
			throw new MauiToolException(ErrorCodes.AndroidDeviceNotFound,
				$"Failed to capture screenshot: {captureResult.StandardError}",
				nativeError: captureResult.StandardError);

		// Pull screenshot to local path
		var pullResult = await ProcessRunner.RunAsync(AdbPath!, $"-s {ProcessRunner.SanitizeArg(deviceSerial)} pull {remotePath} {ProcessRunner.SanitizeArg(outputPath)}", cancellationToken: cancellationToken);
		if (!pullResult.Success)
			throw new MauiToolException(ErrorCodes.AndroidDeviceNotFound,
				$"Failed to pull screenshot: {pullResult.StandardError}",
				nativeError: pullResult.StandardError);

		return outputPath;
	}
}
