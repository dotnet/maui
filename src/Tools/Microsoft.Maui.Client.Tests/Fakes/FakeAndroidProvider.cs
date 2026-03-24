// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Providers.Android;

namespace Microsoft.Maui.Client.Tests.Fakes;

/// <summary>
/// Hand-written fake for <see cref="IAndroidProvider"/> used in unit tests.
/// Set the public properties to control return values; inspect the tracking
/// lists to verify which methods were called and with what arguments.
/// </summary>
public class FakeAndroidProvider : IAndroidProvider
{
	// --- Configurable return values ---

	public string? SdkPath { get; set; }
	public string? JdkPath { get; set; }
	public bool IsSdkInstalled { get; set; }
	public bool IsJdkInstalled { get; set; }
	public bool SdkPathRequiresElevation { get; set; }

	public List<HealthCheck> HealthChecks { get; set; } = new();
	public List<Device> Devices { get; set; } = new();
	public List<AvdInfo> Avds { get; set; } = new();
	public List<SdkPackage> InstalledPackages { get; set; } = new();
	public List<SdkPackage> AvailablePackages { get; set; } = new();
	public string? MostRecentSystemImage { get; set; }
	public bool LicensesAccepted { get; set; }
	public (string Command, string Arguments)? LicenseAcceptanceCommand { get; set; }

	/// <summary>
	/// Optional delegate for <see cref="CreateAvdAsync"/>. When set, the delegate
	/// produces the return value; otherwise a default <see cref="AvdInfo"/> is built
	/// from the supplied arguments.
	/// </summary>
	public Func<string, string, string, bool, AvdInfo>? CreateAvdFunc { get; set; }

	/// <summary>
	/// Optional delegate for <see cref="GetMostRecentSystemImageAsync"/>. When set
	/// it is called instead of returning <see cref="MostRecentSystemImage"/>.
	/// </summary>
	public Func<CancellationToken, Task<string?>>? GetMostRecentSystemImageFunc { get; set; }

	/// <summary>
	/// Optional delegate invoked by <see cref="InstallAsync"/> so tests can simulate
	/// progress reporting.
	/// </summary>
	public Action<string?, string?, int, IEnumerable<string>?, IProgress<string>?, CancellationToken>? InstallCallback { get; set; }

	// --- Call tracking ---

	public List<string> DeletedAvds { get; } = new();
	public List<(string Name, bool ColdBoot, bool Wait)> StartedAvds { get; } = new();
	public List<(string Name, string DeviceProfile, string SystemImage, bool Force)> CreatedAvds { get; } = new();
	public List<List<string>> InstalledPackageSets { get; } = new();
	public List<string> StoppedEmulators { get; } = new();
	public List<List<string>> UninstalledPackageSets { get; } = new();
	public int AcceptLicensesCalled { get; private set; }
	public List<(string? SdkPath, string? JdkPath, int JdkVersion, List<string>? AdditionalPackages)> InstallCalls { get; } = new();
	public List<string> InstallSdkToolsCalls { get; } = new();
	public List<int> InstallJdkCalls { get; } = new();
	public bool Disposed { get; private set; }

	// --- IAndroidProvider implementation ---

	public Task<List<HealthCheck>> CheckHealthAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(HealthChecks);

	public Task<List<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(Devices);

	public Task<List<AvdInfo>> GetAvdsAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(Avds);

	public Task<AvdInfo> CreateAvdAsync(string name, string deviceProfile, string systemImage, bool force = false, CancellationToken cancellationToken = default)
	{
		CreatedAvds.Add((name, deviceProfile, systemImage, force));

		var result = CreateAvdFunc != null
			? CreateAvdFunc(name, deviceProfile, systemImage, force)
			: new AvdInfo { Name = name, DeviceProfile = deviceProfile, SystemImage = systemImage };

		return Task.FromResult(result);
	}

	public Task DeleteAvdAsync(string name, CancellationToken cancellationToken = default)
	{
		DeletedAvds.Add(name);
		return Task.CompletedTask;
	}

	public Task StartAvdAsync(string name, bool coldBoot = false, bool wait = false, CancellationToken cancellationToken = default)
	{
		StartedAvds.Add((name, coldBoot, wait));
		return Task.CompletedTask;
	}

	public Task StopEmulatorAsync(string deviceSerial, CancellationToken cancellationToken = default)
	{
		StoppedEmulators.Add(deviceSerial);
		return Task.CompletedTask;
	}

	public Task<List<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(InstalledPackages);

	public Task<List<SdkPackage>> GetAvailablePackagesAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(AvailablePackages);

	public Task<string?> GetMostRecentSystemImageAsync(CancellationToken cancellationToken = default)
	{
		if (GetMostRecentSystemImageFunc != null)
			return GetMostRecentSystemImageFunc(cancellationToken);

		return Task.FromResult(MostRecentSystemImage);
	}

	public Task InstallPackagesAsync(IEnumerable<string> packages, bool acceptLicenses = false, CancellationToken cancellationToken = default)
	{
		InstalledPackageSets.Add(packages.ToList());
		return Task.CompletedTask;
	}

	public Task InstallPackagesAsync(IEnumerable<string> packages, bool acceptLicenses, Action<string, int, int>? onProgress, CancellationToken cancellationToken = default)
	{
		InstalledPackageSets.Add(packages.ToList());
		return Task.CompletedTask;
	}

	public Task UninstallPackagesAsync(IEnumerable<string> packages, CancellationToken cancellationToken = default)
	{
		UninstalledPackageSets.Add(packages.ToList());
		return Task.CompletedTask;
	}

	public Task AcceptLicensesAsync(CancellationToken cancellationToken = default)
	{
		AcceptLicensesCalled++;
		return Task.CompletedTask;
	}

	public Task AcceptLicensesAsync(Action<string>? onProgress, CancellationToken cancellationToken = default)
	{
		AcceptLicensesCalled++;
		return Task.CompletedTask;
	}

	public Task<bool> AreLicensesAcceptedAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(LicensesAccepted);

	public (string Command, string Arguments)? GetLicenseAcceptanceCommand()
		=> LicenseAcceptanceCommand;

	public Task InstallJdkAsync(int version = 17, string? installPath = null, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
	{
		InstallJdkCalls.Add(version);
		return Task.CompletedTask;
	}

	public Task InstallAsync(string? sdkPath = null, string? jdkPath = null, int jdkVersion = 17, IEnumerable<string>? additionalPackages = null, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
	{
		InstallCalls.Add((sdkPath, jdkPath, jdkVersion, additionalPackages?.ToList()));
		InstallCallback?.Invoke(sdkPath, jdkPath, jdkVersion, additionalPackages, progress, cancellationToken);
		return Task.CompletedTask;
	}

	public Task InstallSdkToolsAsync(string targetPath, Action<string, int, string>? onProgress = null, CancellationToken cancellationToken = default)
	{
		InstallSdkToolsCalls.Add(targetPath);
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		Disposed = true;
	}
}
