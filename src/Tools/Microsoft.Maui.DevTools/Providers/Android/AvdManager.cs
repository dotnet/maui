// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;
using Xamarin.Android.Tools;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Wrapper for Android Virtual Device (AVD) Manager operations.
/// Delegates to Xamarin.Android.Tools.AvdManagerRunner for core functionality.
/// </summary>
public class AvdManager
{
	private readonly AvdManagerRunner _runner;
	private readonly EmulatorRunner _emulatorRunner;

	public AvdManager(Func<string?> getSdkPath, Func<string?> getJdkPath)
	{
		_runner = new AvdManagerRunner(getSdkPath, getJdkPath);
		_emulatorRunner = new EmulatorRunner(getSdkPath, getJdkPath);
	}

	public string? AvdManagerPath => _runner.AvdManagerPath;

	public string? EmulatorPath => _emulatorRunner.EmulatorPath;

	public bool IsAvailable => _runner.IsAvailable;

	public async Task<List<AvdInfo>> GetAvdsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var avds = await _runner.ListAvdsAsync(cancellationToken);
			return avds.Select(MapToMauiAvd).ToList();
		}
		catch (InvalidOperationException)
		{
			return new List<AvdInfo>();
		}
	}

	private static AvdInfo MapToMauiAvd(Xamarin.Android.Tools.AvdInfo avd)
	{
		string? systemImage = null;
		string? target = null;

		// Try to read system image from AVD config.ini
		if (!string.IsNullOrEmpty(avd.Path))
		{
			var configPath = System.IO.Path.Combine(avd.Path, "config.ini");
			if (File.Exists(configPath))
			{
				try
				{
					foreach (var line in File.ReadLines(configPath))
					{
						if (line.StartsWith("image.sysdir.1=", StringComparison.Ordinal))
						{
							// e.g. "image.sysdir.1=system-images/android-36/google_apis/arm64-v8a/"
							var val = line.Substring("image.sysdir.1=".Length).Trim().TrimEnd('/');
							systemImage = val.Replace("/", ";", StringComparison.Ordinal);
							// Extract target like "Android 36 (google_apis/arm64-v8a)"
							var parts = val.Split('/');
							if (parts.Length >= 2)
							{
								var api = parts[1]; // android-36
								var variant = parts.Length > 2 ? string.Join("/", parts.Skip(2)) : "";
								target = string.IsNullOrEmpty(variant)
									? api.Replace("android-", "Android ", StringComparison.Ordinal)
									: $"{api.Replace("android-", "Android ", StringComparison.Ordinal)} ({variant})";
							}
						}
						else if (line.StartsWith("tag.display=", StringComparison.Ordinal) && target == null)
						{
							target = line.Substring("tag.display=".Length).Trim();
						}
					}
				}
				catch
				{
					// Ignore config read errors
				}
			}
		}

		return new AvdInfo
		{
			Name = avd.Name,
			DeviceProfile = avd.DeviceProfile,
			SystemImage = systemImage,
			Target = target,
			Path = avd.Path,
		};
	}

	public async Task<AvdInfo> CreateAvdAsync(string name, string deviceProfile, string systemImage,
		bool force = false, CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"AVD Manager not found",
				"maui android install");

		try
		{
			await _runner.CreateAvdAsync(name, systemImage, deviceProfile, force, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidAvdCreateFailed,
				$"Failed to create AVD '{name}': {ex.Message}");
		}

		return new AvdInfo
		{
			Name = name,
			DeviceProfile = deviceProfile,
		};
	}

	public async Task StartAvdAsync(string name, bool coldBoot = false, bool wait = false,
		CancellationToken cancellationToken = default)
	{
		if (!_emulatorRunner.IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidEmulatorNotFound,
				"Android emulator not installed",
				"maui android sdk install emulator");

		try
		{
			_emulatorRunner.StartAvd(name, coldBoot);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidEmulatorNotFound,
				$"Failed to start AVD '{name}': {ex.Message}");
		}
	}

	public async Task DeleteAvdAsync(string name, CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"AVD Manager not found",
				"maui android install");

		try
		{
			await _runner.DeleteAvdAsync(name, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidAvdDeleteFailed,
				$"Failed to delete AVD '{name}': {ex.Message}");
		}
	}
}
