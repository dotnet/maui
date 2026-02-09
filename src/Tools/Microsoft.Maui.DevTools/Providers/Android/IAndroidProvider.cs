// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Models;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Interface for Android SDK and device operations.
/// </summary>
public interface IAndroidProvider
{
	/// <summary>
	/// Gets the Android SDK path.
	/// </summary>
	string? SdkPath { get; }

	/// <summary>
	/// Gets the JDK path.
	/// </summary>
	string? JdkPath { get; }

	/// <summary>
	/// Checks if the Android SDK is installed.
	/// </summary>
	bool IsSdkInstalled { get; }

	/// <summary>
	/// Checks if a compatible JDK is installed.
	/// </summary>
	bool IsJdkInstalled { get; }

	/// <summary>
	/// Gets the health status of Android tooling.
	/// </summary>
	Task<List<HealthCheck>> CheckHealthAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists connected Android devices and running emulators.
	/// </summary>
	Task<List<Device>> GetDevicesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists available AVDs.
	/// </summary>
	Task<List<AvdInfo>> GetAvdsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new AVD.
	/// </summary>
	Task<AvdInfo> CreateAvdAsync(string name, string deviceProfile, string systemImage, bool force = false, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes an AVD.
	/// </summary>
	Task DeleteAvdAsync(string name, CancellationToken cancellationToken = default);

	/// <summary>
	/// Starts an AVD.
	/// </summary>
	Task StartAvdAsync(string name, bool coldBoot = false, bool wait = false, CancellationToken cancellationToken = default);

	/// <summary>
	/// Stops a running emulator.
	/// </summary>
	Task StopEmulatorAsync(string deviceSerial, CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists installed SDK packages.
	/// </summary>
	Task<List<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists SDK packages available for installation.
	/// </summary>
	Task<List<SdkPackage>> GetAvailablePackagesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the most recent installed system image for AVD creation.
	/// Returns the system image path (e.g., "system-images;android-35;google_apis;arm64-v8a") or null if none found.
	/// </summary>
	Task<string?> GetMostRecentSystemImageAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Installs SDK packages.
	/// </summary>
	Task InstallPackagesAsync(IEnumerable<string> packages, bool acceptLicenses = false, CancellationToken cancellationToken = default);

	/// <summary>
	/// Accepts all SDK licenses.
	/// </summary>
	Task AcceptLicensesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Checks if SDK licenses have been accepted.
	/// </summary>
	Task<bool> AreLicensesAcceptedAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the command and arguments to run for interactive license acceptance.
	/// Returns (command, arguments) tuple for IDE terminal integration.
	/// </summary>
	(string Command, string Arguments)? GetLicenseAcceptanceCommand();

	/// <summary>
	/// Installs JDK if not present.
	/// </summary>
	Task InstallJdkAsync(int version = 17, string? installPath = null, IProgress<string>? progress = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Installs the Android development environment.
	/// </summary>
	Task InstallAsync(string? sdkPath = null, string? jdkPath = null, int jdkVersion = 17, IEnumerable<string>? additionalPackages = null, IProgress<string>? progress = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Takes a screenshot from a device.
	/// </summary>
	Task<string> TakeScreenshotAsync(string deviceSerial, string outputPath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Information about an Android Virtual Device.
/// </summary>
public record AvdInfo
{
	public required string Name { get; init; }
	public string? DeviceProfile { get; init; }
	public string? SystemImage { get; init; }
	public string? Target { get; init; }
	public string? Path { get; init; }
	public string? Status { get; init; }
	public string? ApiLevel { get; init; }
	public string? Abi { get; init; }
	public string? TagId { get; init; }
	public bool PlayStoreEnabled { get; init; }
}

/// <summary>
/// Information about an SDK package (installed or available).
/// </summary>
public record SdkPackage
{
	public required string Path { get; init; }
	public string? Version { get; init; }
	public string? Description { get; init; }
	public string? Location { get; init; }
	public bool IsInstalled { get; init; }
}
