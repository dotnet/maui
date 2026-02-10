// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Models;

namespace Microsoft.Maui.DevTools.Providers.Apple;

/// <summary>
/// Interface for Apple platform operations.
/// </summary>
public interface IAppleProvider
{
	/// <summary>
	/// Gets whether Xcode is installed.
	/// </summary>
	bool IsXcodeInstalled { get; }

	/// <summary>
	/// Gets the Xcode developer path.
	/// </summary>
	string? XcodePath { get; }

	/// <summary>
	/// Gets health check results for Apple tools.
	/// </summary>
	Task<List<HealthCheck>> CheckHealthAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists available iOS simulators.
	/// </summary>
	Task<List<Device>> ListSimulatorsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists available iOS runtimes.
	/// </summary>
	Task<List<Runtime>> ListRuntimesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a simulator by UDID.
	/// </summary>
	Task<Device?> GetSimulatorAsync(string udid, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new simulator.
	/// </summary>
	Task<Device> CreateSimulatorAsync(string name, string deviceType, string runtime, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a simulator.
	/// </summary>
	Task DeleteSimulatorAsync(string udid, CancellationToken cancellationToken = default);

	/// <summary>
	/// Boots a simulator.
	/// </summary>
	Task BootSimulatorAsync(string udid, CancellationToken cancellationToken = default);

	/// <summary>
	/// Shuts down a simulator.
	/// </summary>
	Task ShutdownSimulatorAsync(string udid, CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists runtimes available for download.
	/// </summary>
	Task<List<Runtime>> ListAvailableRuntimesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Installs an iOS runtime by version or identifier.
	/// </summary>
	Task InstallRuntimeAsync(string version, IProgress<string>? progress = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Accepts the Xcode license agreement.
	/// </summary>
	Task AcceptXcodeLicenseAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Checks if the Xcode license has been accepted.
	/// </summary>
	Task<bool> IsXcodeLicenseAcceptedAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Lists all installed Xcode versions.
	/// </summary>
	Task<List<XcodeInstallation>> ListXcodeInstallationsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Switches the active Xcode to the specified path.
	/// </summary>
	Task SelectXcodeAsync(string xcodePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an iOS runtime.
/// </summary>
public class Runtime
{
	public required string Identifier { get; init; }
	public required string Name { get; init; }
	public required string Version { get; init; }
	public bool IsAvailable { get; init; }
	public bool IsInstalled { get; init; }
	public string? Source { get; init; }
}
