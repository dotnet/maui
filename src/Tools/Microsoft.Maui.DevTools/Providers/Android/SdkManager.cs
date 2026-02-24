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

	public SdkManager(Func<string?> getSdkPath, Func<string?> getJdkPath)
	{
		_getSdkPath = getSdkPath;
		_getJdkPath = getJdkPath;
		_sdkManager = new XatSdkManager(logger: (level, msg) => 
		{
			if (level >= TraceLevel.Info)
				Console.WriteLine($"[SdkManager] {msg}");
		});
		UpdatePaths();
	}

	private void UpdatePaths()
	{
		_sdkManager.AndroidSdkPath = _getSdkPath();
		_sdkManager.JavaSdkPath = _getJdkPath();
	}

	private string? SdkPath => _getSdkPath();
	private string? JdkPath => _getJdkPath();

	public string? SdkManagerPath => _sdkManager.FindSdkManagerPath();

	public bool IsAvailable => !string.IsNullOrEmpty(SdkManagerPath);

	public void Dispose()
	{
		_sdkManager.Dispose();
	}

	public Task<HealthCheck> CheckHealthAsync(CancellationToken cancellationToken = default)
	{
		UpdatePaths();
		
		if (!IsAvailable || string.IsNullOrEmpty(SdkPath))
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
			Details = new Dictionary<string, object>
			{
				["path"] = SdkPath
			}
		});
	}

	public async Task<List<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
	{
		UpdatePaths();
		
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
		UpdatePaths();
		
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

	private static SdkPackage MapToMauiPackage(XatSdkPackage pkg)
	{
		return new SdkPackage
		{
			Path = pkg.Path,
			Version = pkg.Version,
			Description = pkg.Description,
			IsInstalled = pkg.IsInstalled
		};
	}

	public async Task InstallPackagesAsync(IEnumerable<string> packages, bool acceptLicenses = false,
		CancellationToken cancellationToken = default)
	{
		UpdatePaths();
		
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"SDK Manager not found. Run 'maui android install' first.",
				"maui android install");

		try
		{
			await _sdkManager.InstallAsync(packages, acceptLicenses, cancellationToken);
		}
		catch (Exception ex)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidPackageInstallFailed,
				$"Failed to install packages: {ex.Message}",
				nativeError: ex.Message);
		}
	}

	public async Task AcceptLicensesAsync(CancellationToken cancellationToken = default)
	{
		UpdatePaths();
		
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"SDK Manager not found",
				"maui android install");

		await _sdkManager.AcceptLicensesAsync(cancellationToken);
	}

	public async Task UninstallPackagesAsync(IEnumerable<string> packages, CancellationToken cancellationToken = default)
	{
		UpdatePaths();
		
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"SDK Manager not found. Run 'maui android install' first.",
				"maui android install");

		try
		{
			await _sdkManager.UninstallAsync(packages, cancellationToken);
		}
		catch (Exception ex)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidPackageInstallFailed,
				$"Failed to uninstall packages: {ex.Message}",
				nativeError: ex.Message);
		}
	}

	public Task<bool> AreLicensesAcceptedAsync(CancellationToken cancellationToken = default)
	{
		UpdatePaths();
		return Task.FromResult(_sdkManager.AreLicensesAccepted());
	}

	public async Task InstallSdkAsync(string targetPath, IProgress<string>? progress = null, 
		CancellationToken cancellationToken = default)
	{
		// Use the new BootstrapAsync from android-tools
		_sdkManager.AndroidSdkPath = targetPath;
		
		var bootstrapProgress = new Progress<Xamarin.Android.Tools.SdkBootstrapProgress>(p =>
		{
			progress?.Report($"{p.Phase}: {p.Message}");
		});

		await _sdkManager.BootstrapAsync(targetPath, bootstrapProgress, cancellationToken);
	}
}
