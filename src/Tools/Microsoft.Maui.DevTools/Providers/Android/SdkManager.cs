// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;
using Xamarin.Android.Tools;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Wrapper for Android SDK Manager operations.
/// Delegates to Xamarin.Android.Tools.SdkManagerRunner for core functionality.
/// </summary>
public class SdkManager
{
	private readonly Func<string?> _getSdkPath;
	private readonly Func<string?> _getJdkPath;
	private readonly SdkManagerRunner _runner;

	public SdkManager(Func<string?> getSdkPath, Func<string?> getJdkPath)
	{
		_getSdkPath = getSdkPath;
		_getJdkPath = getJdkPath;
		_runner = new SdkManagerRunner(getSdkPath, getJdkPath);
	}

	private string? SdkPath => _getSdkPath();
	private string? JdkPath => _getJdkPath();

	public string? SdkManagerPath => _runner.SdkManagerPath;

	public bool IsAvailable => _runner.IsAvailable;

	public async Task<HealthCheck> CheckHealthAsync(CancellationToken cancellationToken = default)
	{
		if (!IsAvailable || string.IsNullOrEmpty(SdkPath))
		{
			return new HealthCheck
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
			};
		}

		return new HealthCheck
		{
			Category = "android",
			Name = "Android SDK",
			Status = CheckStatus.Ok,
			Message = "Android SDK installed",
			Details = new Dictionary<string, object>
			{
				["path"] = SdkPath
			}
		};
	}

	public async Task<List<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
	{
		var result = await _runner.ListPackagesAsync(cancellationToken);
		if (!result.Success || result.Data == default)
			return new List<SdkPackage>();

		return result.Data.Installed.Select(MapToMauiPackage).ToList();
	}

	public async Task<List<SdkPackage>> GetAvailablePackagesAsync(CancellationToken cancellationToken = default)
	{
		var result = await _runner.ListPackagesAsync(cancellationToken);
		if (!result.Success || result.Data == default)
			return new List<SdkPackage>();

		return result.Data.Available.Select(MapToMauiPackage).ToList();
	}

	private static SdkPackage MapToMauiPackage(SdkPackageInfo pkg)
	{
		return new SdkPackage
		{
			Path = pkg.Path,
			Version = pkg.Version,
			Description = pkg.Description,
			Location = pkg.Location,
			IsInstalled = pkg.IsInstalled
		};
	}

	public async Task InstallPackagesAsync(IEnumerable<string> packages, bool acceptLicenses = false,
		CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"SDK Manager not found. Run 'maui android install' first.",
				"maui android install");

		var result = await _runner.InstallPackagesAsync(packages, acceptLicenses, cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidPackageInstallFailed,
				$"Failed to install packages: {result.ErrorMessage ?? result.StandardError}",
				nativeError: result.StandardError);
		}
	}

	public async Task AcceptLicensesAsync(CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"SDK Manager not found",
				"maui android install");

		// License acceptance may return non-zero even when successful
		await _runner.AcceptLicensesAsync(cancellationToken);
	}

	public async Task UninstallPackagesAsync(IEnumerable<string> packages, CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"SDK Manager not found. Run 'maui android install' first.",
				"maui android install");

		var result = await _runner.UninstallPackagesAsync(packages, cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidPackageInstallFailed,
				$"Failed to uninstall packages: {result.ErrorMessage ?? result.StandardError}",
				nativeError: result.StandardError);
		}
	}

	public Task<bool> AreLicensesAcceptedAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_runner.AreLicensesAccepted());
	}

	public async Task InstallSdkAsync(string targetPath, CancellationToken cancellationToken = default)
	{
		// Create target directory
		Directory.CreateDirectory(targetPath);

		// Download command-line tools
		var downloadUrl = GetCommandLineToolsUrl();
		var tempZipPath = Path.Combine(Path.GetTempPath(), "commandlinetools.zip");

		try
		{
			using var httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromMinutes(10);
			
			var response = await httpClient.GetAsync(downloadUrl, cancellationToken);
			response.EnsureSuccessStatusCode();

			await using var fs = File.Create(tempZipPath);
			await response.Content.CopyToAsync(fs, cancellationToken);
		}
		catch (Exception ex)
		{
			throw new MauiToolException(
				ErrorCodes.DownloadFailed,
				$"Failed to download Android command-line tools: {ex.Message}",
				nativeError: ex.Message);
		}

		// Extract to SDK path
		var cmdlineToolsPath = Path.Combine(targetPath, "cmdline-tools");
		Directory.CreateDirectory(cmdlineToolsPath);

		System.IO.Compression.ZipFile.ExtractToDirectory(tempZipPath, cmdlineToolsPath, overwriteFiles: true);

		// Rename to 'latest'
		var extractedPath = Path.Combine(cmdlineToolsPath, "cmdline-tools");
		var latestPath = Path.Combine(cmdlineToolsPath, "latest");
		
		if (Directory.Exists(extractedPath))
		{
			if (Directory.Exists(latestPath))
				Directory.Delete(latestPath, recursive: true);
			Directory.Move(extractedPath, latestPath);
		}

		// Clean up
		File.Delete(tempZipPath);
	}

	private static string GetCommandLineToolsUrl()
	{
		// Latest command-line tools URLs (universal binaries on macOS)
		if (PlatformDetector.IsMacOS)
			return "https://dl.google.com/android/repository/commandlinetools-mac-11076708_latest.zip";
		else if (PlatformDetector.IsWindows)
			return "https://dl.google.com/android/repository/commandlinetools-win-11076708_latest.zip";
		else
			return "https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip";
	}
}
