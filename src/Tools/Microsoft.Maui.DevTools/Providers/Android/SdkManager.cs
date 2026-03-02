// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using System.Diagnostics;
using XatSdkManager = Xamarin.Android.Tools.SdkManager;
using XatSdkPackage = Xamarin.Android.Tools.SdkPackage;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Wrapper for Android SDK Manager operations.
/// Delegates to Xamarin.Android.Tools.SdkManager for core functionality.
/// </summary>
public class SdkManager : IDisposable
{
	private readonly Func<string?> _getSdkPath;
	private readonly Func<string?> _getJdkPath;
	private readonly XatSdkManager _sdkManager;

	// Suppress all console output from android-tools logger.
	// Warnings/verbose messages about invalid JDK paths, missing java_home, etc. are
	// expected on many dev machines and should not pollute CLI output.
	static readonly Action<TraceLevel, string> s_quietLogger = (level, msg) => { };

	public SdkManager(Func<string?> getSdkPath, Func<string?> getJdkPath)
	{
		_getSdkPath = getSdkPath;
		_getJdkPath = getJdkPath;
		_sdkManager = new XatSdkManager(logger: s_quietLogger);
	}

	private void SyncPaths()
	{
		_sdkManager.AndroidSdkPath = _getSdkPath();
		_sdkManager.JavaSdkPath = _getJdkPath();
	}

	public string? SdkManagerPath { get { SyncPaths(); return _sdkManager.FindSdkManagerPath(); } }

	public bool IsAvailable => !string.IsNullOrEmpty(SdkManagerPath);

	public void Dispose() => _sdkManager.Dispose();

	public Task<HealthCheck> CheckHealthAsync(CancellationToken cancellationToken = default)
	{
		SyncPaths();
		var sdkPath = _getSdkPath();

		if (!IsAvailable || string.IsNullOrEmpty(sdkPath))
		{
			return Task.FromResult(new HealthCheck
			{
				Category = "android",
				Name = "Android SDK",
				Status = CheckStatus.Error,
				Message = "Android SDK not found",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.AndroidSdkNotFound,
					Description = "Install Android SDK",
					AutoFixable = true,
					Command = "maui android install"
				}
			});
		}

		return Task.FromResult(new HealthCheck
		{
			Category = "android",
			Name = "Android SDK",
			Status = CheckStatus.Ok,
			Message = "Android SDK installed",
			Details = new Dictionary<string, object> { ["path"] = sdkPath }
		});
	}

	public async Task<List<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
	{
		SyncPaths();
		try
		{
			var (installed, _) = await _sdkManager.ListAsync(cancellationToken);
			return installed.Select(MapToMauiPackage).ToList();
		}
		catch
		{
			return new List<SdkPackage>();
		}
	}

	public async Task<List<SdkPackage>> GetAvailablePackagesAsync(CancellationToken cancellationToken = default)
	{
		SyncPaths();
		try
		{
			var (_, available) = await _sdkManager.ListAsync(cancellationToken);
			return available.Select(MapToMauiPackage).ToList();
		}
		catch
		{
			return new List<SdkPackage>();
		}
	}

	private static SdkPackage MapToMauiPackage(XatSdkPackage pkg) => new()
	{
		Path = pkg.Path,
		Version = pkg.Version,
		Description = pkg.Description,
		IsInstalled = pkg.IsInstalled
	};

	public async Task InstallPackagesAsync(IEnumerable<string> packages, bool acceptLicenses = false,
		CancellationToken cancellationToken = default)
	{
		await InstallPackagesAsync(packages, acceptLicenses, onProgress: null, cancellationToken);
	}

	public async Task InstallPackagesAsync(IEnumerable<string> packages, bool acceptLicenses,
		Action<string, int, int>? onProgress, CancellationToken cancellationToken = default)
	{
		SyncPaths();
		EnsureAvailable();

		try
		{
			await _sdkManager.InstallAsync(packages, acceptLicenses, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidPackageInstallFailed,
				$"Failed to install packages: {ex.Message}",
				nativeError: ex.Message);
		}
	}

	public async Task AcceptLicensesAsync(CancellationToken cancellationToken = default)
	{
		await AcceptLicensesAsync(onProgress: null, cancellationToken);
	}

	public async Task AcceptLicensesAsync(Action<string>? onProgress, CancellationToken cancellationToken = default)
	{
		SyncPaths();
		EnsureAvailable();
		await _sdkManager.AcceptLicensesAsync(cancellationToken);
	}

	public async Task UninstallPackagesAsync(IEnumerable<string> packages, CancellationToken cancellationToken = default)
	{
		SyncPaths();
		EnsureAvailable();

		try
		{
			await _sdkManager.UninstallAsync(packages, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidPackageInstallFailed,
				$"Failed to uninstall packages: {ex.Message}",
				nativeError: ex.Message);
		}
	}

	public Task<bool> AreLicensesAcceptedAsync(CancellationToken cancellationToken = default)
	{
		SyncPaths();
		return Task.FromResult(_sdkManager.AreLicensesAccepted());
	}

	public async Task InstallSdkAsync(string targetPath, IProgress<string>? progress = null, 
		CancellationToken cancellationToken = default)
	{
		_sdkManager.AndroidSdkPath = targetPath;
		var bootstrapProgress = new Progress<Xamarin.Android.Tools.SdkBootstrapProgress>(p =>
			progress?.Report($"{p.Phase}: {p.Message}"));
		await _sdkManager.BootstrapAsync(targetPath, bootstrapProgress, cancellationToken);
	}

	/// <summary>
	/// Installs SDK with structured progress reporting for rich UI rendering.
	/// </summary>
	public async Task InstallSdkAsync(string targetPath, 
		Action<Xamarin.Android.Tools.SdkBootstrapPhase, int, string>? onProgress = null, 
		CancellationToken cancellationToken = default)
	{
		_sdkManager.AndroidSdkPath = targetPath;
		var bootstrapProgress = new Progress<Xamarin.Android.Tools.SdkBootstrapProgress>(p =>
			onProgress?.Invoke(p.Phase, p.PercentComplete, p.Message));
		await _sdkManager.BootstrapAsync(targetPath, bootstrapProgress, cancellationToken);
	}

	private void EnsureAvailable()
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"SDK Manager not found. Run 'maui android install' first.",
				"maui android install");
	}
}
