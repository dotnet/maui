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
		return new AvdInfo
		{
			Name = avd.Name,
			DeviceProfile = avd.DeviceProfile,
			SystemImage = avd.SystemImage,
			Target = avd.Target,
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
			await _runner.CreateAvdAsync(name, deviceProfile, systemImage, force, cancellationToken);
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
			SystemImage = systemImage
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
			if (wait)
			{
				using var process = await _emulatorRunner.StartAvdAndWaitForBootAsync(name, coldBoot, cancellationToken: cancellationToken);
			}
			else
			{
				_emulatorRunner.StartAvd(name, coldBoot);
			}
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
