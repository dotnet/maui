// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Providers.Apple;

namespace Microsoft.Maui.Client.Tests.Fakes;

/// <summary>
/// Hand-written fake for <see cref="IAppleProvider"/> used in unit tests.
/// Set the public properties to control return values; inspect the tracking
/// lists to verify which methods were called and with what arguments.
/// </summary>
public class FakeAppleProvider : IAppleProvider
{
	// --- Configurable return values ---

	public bool IsXcodeInstalled { get; set; }
	public string? XcodePath { get; set; }

	public List<HealthCheck> HealthChecks { get; set; } = new();
	public List<Device> Simulators { get; set; } = new();
	public List<Runtime> Runtimes { get; set; } = new();
	public List<Runtime> AvailableRuntimes { get; set; } = new();
	public List<XcodeInstallation> XcodeInstallations { get; set; } = new();
	public bool XcodeLicenseAccepted { get; set; }

	/// <summary>
	/// Map of UDID → Device for <see cref="GetSimulatorAsync"/>.
	/// </summary>
	public Dictionary<string, Device> SimulatorsByUdid { get; set; } = new();

	/// <summary>
	/// Optional delegate for <see cref="CreateSimulatorAsync"/>.
	/// </summary>
	public Func<string, string, string, Device>? CreateSimulatorFunc { get; set; }

	// --- Call tracking ---

	public List<string> DeletedSimulators { get; } = new();
	public List<string> BootedSimulators { get; } = new();
	public List<string> ShutdownSimulators { get; } = new();
	public List<string> InstalledRuntimes { get; } = new();
	public int AcceptXcodeLicenseCalled { get; private set; }
	public List<string> SelectedXcodePaths { get; } = new();
	public List<(string Name, string DeviceType, string Runtime)> CreatedSimulators { get; } = new();

	// --- IAppleProvider implementation ---

	public Task<List<HealthCheck>> CheckHealthAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(HealthChecks);

	public Task<List<Device>> ListSimulatorsAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(Simulators);

	public Task<List<Runtime>> ListRuntimesAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(Runtimes);

	public Task<Device?> GetSimulatorAsync(string udid, CancellationToken cancellationToken = default)
	{
		SimulatorsByUdid.TryGetValue(udid, out var device);
		return Task.FromResult<Device?>(device);
	}

	public Task<Device> CreateSimulatorAsync(string name, string deviceType, string runtime, CancellationToken cancellationToken = default)
	{
		CreatedSimulators.Add((name, deviceType, runtime));

		var result = CreateSimulatorFunc != null
			? CreateSimulatorFunc(name, deviceType, runtime)
			: new Device { Id = Guid.NewGuid().ToString(), Name = name, Platforms = new[] { "ios" }, Type = DeviceType.Simulator, State = DeviceState.Shutdown, IsEmulator = true, IsRunning = false };

		return Task.FromResult(result);
	}

	public Task DeleteSimulatorAsync(string udid, CancellationToken cancellationToken = default)
	{
		DeletedSimulators.Add(udid);
		return Task.CompletedTask;
	}

	public Task BootSimulatorAsync(string udid, CancellationToken cancellationToken = default)
	{
		BootedSimulators.Add(udid);
		return Task.CompletedTask;
	}

	public Task ShutdownSimulatorAsync(string udid, CancellationToken cancellationToken = default)
	{
		ShutdownSimulators.Add(udid);
		return Task.CompletedTask;
	}

	public Task<List<Runtime>> ListAvailableRuntimesAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(AvailableRuntimes);

	public Task InstallRuntimeAsync(string version, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
	{
		InstalledRuntimes.Add(version);
		return Task.CompletedTask;
	}

	public Task AcceptXcodeLicenseAsync(CancellationToken cancellationToken = default)
	{
		AcceptXcodeLicenseCalled++;
		return Task.CompletedTask;
	}

	public Task<bool> IsXcodeLicenseAcceptedAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(XcodeLicenseAccepted);

	public Task<List<XcodeInstallation>> ListXcodeInstallationsAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(XcodeInstallations);

	public Task SelectXcodeAsync(string xcodePath, CancellationToken cancellationToken = default)
	{
		SelectedXcodePaths.Add(xcodePath);
		return Task.CompletedTask;
	}
}
