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
	private readonly Func<string?> _getSdkPath;
	private readonly Func<string?> _getJdkPath;
	private readonly AvdManagerRunner _runner;
	private readonly EmulatorRunner _emulatorRunner;

	public AvdManager(Func<string?> getSdkPath, Func<string?> getJdkPath)
	{
		_getSdkPath = getSdkPath;
		_getJdkPath = getJdkPath;
		_runner = new AvdManagerRunner(getSdkPath, getJdkPath);
		_emulatorRunner = new EmulatorRunner(getSdkPath, getJdkPath);
	}

	private string? SdkPath => _getSdkPath();
	private string? JdkPath => _getJdkPath();

	public string? AvdManagerPath => _runner.AvdManagerPath;

	public string? EmulatorPath => _runner.EmulatorPath;

	public bool IsAvailable => _runner.IsAvailable;

	public async Task<List<AvdInfo>> GetAvdsAsync(CancellationToken cancellationToken = default)
	{
		var result = await _runner.ListAvdsAsync(cancellationToken);
		if (!result.Success || result.Data == null)
			return new List<AvdInfo>();

		return result.Data.Select(MapToMauiAvd).ToList();
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
			Status = avd.Status,
			ApiLevel = avd.ApiLevel,
			Abi = avd.Abi,
			TagId = avd.TagId,
			PlayStoreEnabled = avd.PlayStoreEnabled
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

		var result = await _runner.CreateAvdAsync(name, deviceProfile, systemImage, force, cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidAvdCreateFailed,
				$"Failed to create AVD '{name}': {result.ErrorMessage ?? result.StandardError}",
				nativeError: result.StandardError);
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

		var result = await _emulatorRunner.StartAvdAsync(name, coldBoot, wait, cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidEmulatorNotFound,
				$"Failed to start AVD '{name}': {result.ErrorMessage}");
		}
	}

	public async Task DeleteAvdAsync(string name, CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"AVD Manager not found",
				"maui android install");

		var result = await _runner.DeleteAvdAsync(name, cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidAvdDeleteFailed,
				$"Failed to delete AVD '{name}': {result.ErrorMessage ?? result.StandardError}",
				nativeError: result.StandardError);
		}
	}
}
