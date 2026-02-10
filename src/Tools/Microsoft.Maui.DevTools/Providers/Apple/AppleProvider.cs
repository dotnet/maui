// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Providers.Apple;

/// <summary>
/// Apple platform provider using xcrun and simctl.
/// </summary>
public class AppleProvider : IAppleProvider
{
	private readonly Simctl _simctl;
	private readonly XcodeSelect _xcodeSelect;

	private string? _xcodePath;

	public string? XcodePath => _xcodePath ??= _xcodeSelect.GetDeveloperDirectory();
	public bool IsXcodeInstalled => !string.IsNullOrEmpty(XcodePath) && Directory.Exists(XcodePath);

	public AppleProvider()
	{
		_xcodeSelect = new XcodeSelect();
		_simctl = new Simctl();
	}

	public async Task<List<HealthCheck>> CheckHealthAsync(CancellationToken cancellationToken = default)
	{
		var checks = new List<HealthCheck>();

		// Check Xcode
		var xcodeCheck = await CheckXcodeAsync(cancellationToken);
		checks.Add(xcodeCheck);

		// Check Command Line Tools
		var cltCheck = await CheckCommandLineToolsAsync(cancellationToken);
		if (cltCheck != null)
			checks.Add(cltCheck);

		// Check iOS Simulators (only if Xcode is available)
		if (IsXcodeInstalled)
		{
			var simCheck = await CheckSimulatorsAsync(cancellationToken);
			checks.Add(simCheck);
		}

		return checks;
	}

	private async Task<HealthCheck> CheckXcodeAsync(CancellationToken cancellationToken)
	{
		if (!PlatformDetector.IsMacOS)
		{
			return new HealthCheck
			{
				Category = "apple",
				Name = "Xcode",
				Status = CheckStatus.NotApplicable,
				Message = "Xcode is only available on macOS"
			};
		}

		if (!IsXcodeInstalled)
		{
			return new HealthCheck
			{
				Category = "apple",
				Name = "Xcode",
				Status = CheckStatus.Error,
				Message = "Xcode not found",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.XcodeNotFound,
					Description = "Install Xcode from the App Store",
					AutoFixable = false,
					ManualSteps = new[]
					{
						"Open the App Store",
						"Search for \"Xcode\"",
						"Install Xcode",
						"After installation, run: sudo xcode-select --switch /Applications/Xcode.app"
					}
				}
			};
		}

		var version = await _xcodeSelect.GetVersionAsync(cancellationToken);

		return new HealthCheck
		{
			Category = "apple",
			Name = "Xcode",
			Status = CheckStatus.Ok,
			Message = $"Xcode {version ?? "installed"}",
			Details = new Dictionary<string, object>
			{
				["version"] = version ?? "unknown",
				["path"] = XcodePath!
			}
		};
	}

	private async Task<HealthCheck?> CheckCommandLineToolsAsync(CancellationToken cancellationToken)
	{
		if (!PlatformDetector.IsMacOS)
			return null;

		var result = await ProcessRunner.RunAsync(
			"xcode-select", "-p",
			timeout: TimeSpan.FromSeconds(10),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			return new HealthCheck
			{
				Category = "apple",
				Name = "Command Line Tools",
				Status = CheckStatus.Error,
				Message = "Command Line Tools not installed",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.XcodeCommandLineToolsNotFound,
					Description = "Install Command Line Tools",
					AutoFixable = true,
					Command = "xcode-select --install"
				}
			};
		}

		return new HealthCheck
		{
			Category = "apple",
			Name = "Command Line Tools",
			Status = CheckStatus.Ok,
			Message = "Installed"
		};
	}

	private async Task<HealthCheck> CheckSimulatorsAsync(CancellationToken cancellationToken)
	{
		var devices = await ListSimulatorsAsync(cancellationToken);

		if (devices.Count == 0)
		{
			return new HealthCheck
			{
				Category = "apple",
				Name = "iOS Simulators",
				Status = CheckStatus.Warning,
				Message = "No iOS simulators found",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.AppleNoSimulators,
					Description = "Create an iOS simulator",
					AutoFixable = false,
					ManualSteps = new[]
					{
						"Open Xcode > Window > Devices and Simulators",
						"Click the + button to add a new simulator",
						"Or run: xcrun simctl create \"iPhone 15\" \"com.apple.CoreSimulator.SimDeviceType.iPhone-15\""
					}
				}
			};
		}

		var bootedCount = devices.Count(d => d.State == DeviceState.Booted);
		return new HealthCheck
		{
			Category = "apple",
			Name = "iOS Simulators",
			Status = CheckStatus.Ok,
			Message = $"{devices.Count} simulator(s) available, {bootedCount} booted",
			Details = new Dictionary<string, object>
			{
				["total"] = devices.Count,
				["booted"] = bootedCount
			}
		};
	}

	public async Task<List<Device>> ListSimulatorsAsync(CancellationToken cancellationToken = default)
	{
		return await _simctl.ListDevicesAsync(cancellationToken);
	}

	public async Task<List<Runtime>> ListRuntimesAsync(CancellationToken cancellationToken = default)
	{
		return await _simctl.ListRuntimesAsync(cancellationToken);
	}

	public async Task<Device?> GetSimulatorAsync(string udid, CancellationToken cancellationToken = default)
	{
		var devices = await ListSimulatorsAsync(cancellationToken);
		return devices.FirstOrDefault(d => d.Id.Equals(udid, StringComparison.OrdinalIgnoreCase));
	}

	public async Task<Device> CreateSimulatorAsync(string name, string deviceType, string runtime,
		CancellationToken cancellationToken = default)
	{
		return await _simctl.CreateDeviceAsync(name, deviceType, runtime, cancellationToken);
	}

	public async Task DeleteSimulatorAsync(string udid, CancellationToken cancellationToken = default)
	{
		await _simctl.DeleteDeviceAsync(udid, cancellationToken);
	}

	public async Task BootSimulatorAsync(string udid, CancellationToken cancellationToken = default)
	{
		await _simctl.BootDeviceAsync(udid, cancellationToken);
	}

	public async Task ShutdownSimulatorAsync(string udid, CancellationToken cancellationToken = default)
	{
		await _simctl.ShutdownDeviceAsync(udid, cancellationToken);
	}

	public async Task<List<Runtime>> ListAvailableRuntimesAsync(CancellationToken cancellationToken = default)
	{
		return await _simctl.ListAvailableRuntimesAsync(cancellationToken);
	}

	public async Task InstallRuntimeAsync(string version, IProgress<string>? progress = null,
		CancellationToken cancellationToken = default)
	{
		await _simctl.InstallRuntimeAsync(version, progress, cancellationToken);
	}

	public async Task AcceptXcodeLicenseAsync(CancellationToken cancellationToken = default)
	{
		await _xcodeSelect.AcceptLicenseAsync(cancellationToken);
	}

	public async Task<bool> IsXcodeLicenseAcceptedAsync(CancellationToken cancellationToken = default)
	{
		return await _xcodeSelect.IsLicenseAcceptedAsync(cancellationToken);
	}

	public async Task<List<XcodeInstallation>> ListXcodeInstallationsAsync(CancellationToken cancellationToken = default)
	{
		return await _xcodeSelect.ListInstallationsAsync(cancellationToken);
	}

	public async Task SelectXcodeAsync(string xcodePath, CancellationToken cancellationToken = default)
	{
		await _xcodeSelect.SwitchAsync(xcodePath, cancellationToken);
	}
}
