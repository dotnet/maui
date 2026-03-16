// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Errors;
using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Utils;
using System.Diagnostics;
using XatSdkManager = Xamarin.Android.Tools.SdkManager;
using XatSdkPackage = Xamarin.Android.Tools.SdkPackage;

namespace Microsoft.Maui.Client.Providers.Android;

/// <summary>
/// Wrapper for Android SDK Manager operations.
/// Delegates to Xamarin.Android.Tools.SdkManager for core functionality.
/// </summary>
public class SdkManager : IDisposable
{
	private readonly Func<string?> _getSdkPath;
	private readonly Func<string?> _getJdkPath;
	private readonly XatSdkManager _sdkManager;

	/// <summary>
	/// Creates a logger that forwards android-tools diagnostics when verbose mode is active.
	/// When verbose is false, only Error/Warning levels are forwarded; others are suppressed
	/// to avoid polluting CLI output with expected warnings about missing JDK paths, etc.
	/// </summary>
	static Action<TraceLevel, string> CreateLogger(bool verbose = false)
	{
		if (verbose)
			return (level, msg) => Console.Error.WriteLine($"[android-tools:{level}] {msg}");

		return (level, msg) =>
		{
			if (level == TraceLevel.Error)
				Console.Error.WriteLine($"[android-tools:error] {msg}");
		};
	}

	public SdkManager(Func<string?> getSdkPath, Func<string?> getJdkPath, bool verbose = false)
	{
		_getSdkPath = getSdkPath;
		_getJdkPath = getJdkPath;
		_sdkManager = new XatSdkManager(logger: CreateLogger(verbose));
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
			if (IsPermissionError(ex))
				throw new UnauthorizedAccessException($"Failed to install packages (permission denied): {ex.Message}", ex);

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
			if (IsPermissionError(ex))
				throw new UnauthorizedAccessException($"Failed to uninstall packages (permission denied): {ex.Message}", ex);

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

	/// <summary>
	/// Checks if an exception from sdkmanager indicates a file/directory permission problem.
	/// The Android sdkmanager process reports permission errors as text in stderr/stdout
	/// rather than throwing UnauthorizedAccessException, so we pattern-match the message.
	/// </summary>
	private static bool IsPermissionError(Exception ex)
	{
		if (ex is UnauthorizedAccessException)
			return true;

		var message = ex.Message;
		if (string.IsNullOrEmpty(message))
			return false;

		return message.Contains("Failed to read or create install properties file", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("access is denied", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("Permission denied", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("Access to the path", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Checks whether the current SDK path is in a location that typically requires
	/// administrator privileges to write to (e.g., Program Files).
	/// </summary>
	public bool SdkPathRequiresElevation()
	{
		if (!PlatformDetector.IsWindows)
			return false;

		var sdkPath = _getSdkPath();
		if (string.IsNullOrEmpty(sdkPath))
			return false;

		var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
		var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

		return sdkPath.StartsWith(programFiles, StringComparison.OrdinalIgnoreCase)
			|| sdkPath.StartsWith(programFilesX86, StringComparison.OrdinalIgnoreCase);
	}
}
