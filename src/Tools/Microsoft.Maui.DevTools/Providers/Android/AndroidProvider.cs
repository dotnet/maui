// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Provider for Android SDK and device operations.
/// </summary>
public class AndroidProvider : IAndroidProvider
{
	private readonly SdkManager _sdkManager;
	private readonly AvdManager _avdManager;
	private readonly Adb _adb;
	private readonly JdkManager _jdkManager;

	private string? _sdkPath;
	private string? _jdkPath;

	public string? SdkPath => _sdkPath ??= PlatformDetector.Paths.GetAndroidSdkPath();
	public string? JdkPath => _jdkPath ??= PlatformDetector.Paths.GetJdkPath();

	public bool IsSdkInstalled => !string.IsNullOrEmpty(SdkPath) && Directory.Exists(SdkPath);
	public bool IsJdkInstalled => !string.IsNullOrEmpty(JdkPath) && Directory.Exists(JdkPath);

	public AndroidProvider()
	{
		_jdkManager = new JdkManager();
		_sdkManager = new SdkManager(() => SdkPath, () => JdkPath);
		_avdManager = new AvdManager(() => SdkPath, () => JdkPath);
		_adb = new Adb(() => SdkPath);
	}

	public async Task<List<HealthCheck>> CheckHealthAsync(CancellationToken cancellationToken = default)
	{
		var checks = new List<HealthCheck>();

		// Check JDK
		var jdkCheck = await _jdkManager.CheckHealthAsync(cancellationToken);
		checks.Add(jdkCheck);

		// Check Android SDK
		if (!IsSdkInstalled)
		{
			checks.Add(new HealthCheck
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
					Command = "dotnet maui android bootstrap --accept-licenses"
				}
			});
			return checks;
		}

		checks.Add(new HealthCheck
		{
			Category = "android",
			Name = "Android SDK",
			Status = CheckStatus.Ok,
			Details = new Dictionary<string, object> { ["path"] = SdkPath! }
		});

		// Check SDK Manager
		if (!_sdkManager.IsAvailable)
		{
			checks.Add(new HealthCheck
			{
				Category = "android",
				Name = "SDK Manager",
				Status = CheckStatus.Error,
				Message = "sdkmanager not found",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.AndroidSdkManagerNotFound,
					Description = "Install command-line tools",
					AutoFixable = true,
					Command = "dotnet maui android bootstrap"
				}
			});
		}
		else
		{
			checks.Add(new HealthCheck
			{
				Category = "android",
				Name = "SDK Manager",
				Status = CheckStatus.Ok
			});

			// Check licenses
			var licensesAccepted = await _sdkManager.AreLicensesAcceptedAsync(cancellationToken);
			if (!licensesAccepted)
			{
				checks.Add(new HealthCheck
				{
					Category = "android",
					Name = "SDK Licenses",
					Status = CheckStatus.Warning,
					Message = "SDK licenses not accepted",
					Fix = new FixInfo
					{
						IssueId = ErrorCodes.AndroidLicensesNotAccepted,
						Description = "Accept Android SDK licenses",
						AutoFixable = true,
						Command = "dotnet maui android sdk accept-licenses"
					}
				});
			}
			else
			{
				checks.Add(new HealthCheck
				{
					Category = "android",
					Name = "SDK Licenses",
					Status = CheckStatus.Ok
				});
			}
		}

		// Check ADB
		if (!_adb.IsAvailable)
		{
			checks.Add(new HealthCheck
			{
				Category = "android",
				Name = "ADB",
				Status = CheckStatus.Warning,
				Message = "adb not found (install platform-tools)",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.AndroidAdbNotFound,
					Description = "Install platform-tools",
					AutoFixable = true,
					Command = "dotnet maui android sdk install platform-tools"
				}
			});
		}
		else
		{
			checks.Add(new HealthCheck
			{
				Category = "android",
				Name = "ADB",
				Status = CheckStatus.Ok
			});
		}

		// Check Emulator
		var emulatorPath = Path.Combine(SdkPath!, "emulator", "emulator");
		if (PlatformDetector.IsWindows)
			emulatorPath += ".exe";

		if (!File.Exists(emulatorPath))
		{
			checks.Add(new HealthCheck
			{
				Category = "android",
				Name = "Emulator",
				Status = CheckStatus.Warning,
				Message = "Emulator not installed",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.AndroidEmulatorNotFound,
					Description = "Install emulator",
					AutoFixable = true,
					Command = "dotnet maui android sdk install emulator"
				}
			});
		}
		else
		{
			checks.Add(new HealthCheck
			{
				Category = "android",
				Name = "Emulator",
				Status = CheckStatus.Ok
			});
		}

		return checks;
	}

	public async Task<List<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
	{
		if (!_adb.IsAvailable)
			return new List<Device>();

		return await _adb.GetDevicesAsync(cancellationToken);
	}

	public async Task<List<AvdInfo>> GetAvdsAsync(CancellationToken cancellationToken = default)
	{
		return await _avdManager.GetAvdsAsync(cancellationToken);
	}

	public async Task<AvdInfo> CreateAvdAsync(string name, string deviceProfile, string systemImage, 
		bool force = false, CancellationToken cancellationToken = default)
	{
		return await _avdManager.CreateAvdAsync(name, deviceProfile, systemImage, force, cancellationToken);
	}

	public async Task StartAvdAsync(string name, bool coldBoot = false, bool wait = false, 
		CancellationToken cancellationToken = default)
	{
		await _avdManager.StartAvdAsync(name, coldBoot, wait, cancellationToken);
	}

	public async Task StopEmulatorAsync(string deviceSerial, CancellationToken cancellationToken = default)
	{
		await _adb.StopEmulatorAsync(deviceSerial, cancellationToken);
	}

	public async Task<List<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
	{
		return await _sdkManager.GetInstalledPackagesAsync(cancellationToken);
	}

	public async Task<string?> GetMostRecentSystemImageAsync(CancellationToken cancellationToken = default)
	{
		var packages = await GetInstalledPackagesAsync(cancellationToken);
		
		// Filter to system images and sort by Android API level (descending)
		// System image format: system-images;android-XX;google_apis;arm64-v8a
		var systemImages = packages
			.Where(p => p.Path.StartsWith("system-images;android-", StringComparison.OrdinalIgnoreCase))
			.Select(p => new { Package = p, ApiLevel = ExtractApiLevel(p.Path) })
			.Where(x => x.ApiLevel > 0)
			.OrderByDescending(x => x.ApiLevel)
			.ToList();

		return systemImages.FirstOrDefault()?.Package.Path;
	}

	private static int ExtractApiLevel(string systemImagePath)
	{
		// Parse "system-images;android-35;google_apis;arm64-v8a" -> 35
		var parts = systemImagePath.Split(';');
		if (parts.Length >= 2)
		{
			var androidPart = parts[1]; // "android-35"
			if (androidPart.StartsWith("android-", StringComparison.OrdinalIgnoreCase))
			{
				var levelStr = androidPart.Substring(8); // Remove "android-"
				if (int.TryParse(levelStr, out var level))
					return level;
			}
		}
		return 0;
	}

	public async Task InstallPackagesAsync(IEnumerable<string> packages, bool acceptLicenses = false, 
		CancellationToken cancellationToken = default)
	{
		await _sdkManager.InstallPackagesAsync(packages, acceptLicenses, cancellationToken);
	}

	public async Task AcceptLicensesAsync(CancellationToken cancellationToken = default)
	{
		await _sdkManager.AcceptLicensesAsync(cancellationToken);
	}

	public async Task InstallJdkAsync(int version = 17, string? installPath = null, 
		CancellationToken cancellationToken = default)
	{
		await _jdkManager.InstallAsync(version, installPath, cancellationToken);
		_jdkPath = installPath ?? PlatformDetector.Paths.DefaultJdkPath;
	}

	public async Task BootstrapAsync(string? sdkPath = null, string? jdkPath = null, int jdkVersion = 17, 
		IEnumerable<string>? additionalPackages = null, CancellationToken cancellationToken = default)
	{
		// Step 1: Install JDK if not present
		if (!IsJdkInstalled)
		{
			await InstallJdkAsync(jdkVersion, jdkPath, cancellationToken);
		}

		// Step 2: Install Android SDK if not present
		if (!IsSdkInstalled)
		{
			var targetSdkPath = sdkPath ?? PlatformDetector.Paths.DefaultAndroidSdkPath;
			await _sdkManager.BootstrapSdkAsync(targetSdkPath, cancellationToken);
			_sdkPath = targetSdkPath;
		}

		// Step 3: Accept licenses
		await AcceptLicensesAsync(cancellationToken);

		// Step 4: Install recommended packages
		var packages = new List<string>
		{
			"platform-tools",
			"emulator",
			"platforms;android-34",
			"build-tools;34.0.0",
			$"system-images;android-34;google_apis;{(PlatformDetector.IsArm64 ? "arm64-v8a" : "x86_64")}"
		};

		if (additionalPackages != null)
			packages.AddRange(additionalPackages);

		await InstallPackagesAsync(packages, true, cancellationToken);
	}

	public async Task<string> TakeScreenshotAsync(string deviceSerial, string outputPath, 
		CancellationToken cancellationToken = default)
	{
		return await _adb.TakeScreenshotAsync(deviceSerial, outputPath, cancellationToken);
	}
}
