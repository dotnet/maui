// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Providers.Apple;

/// <summary>
/// Wrapper for simctl commands.
/// </summary>
public class Simctl
{
	/// <summary>
	/// Lists all iOS simulators.
	/// </summary>
	public async Task<List<Device>> ListDevicesAsync(CancellationToken cancellationToken = default)
	{
		if (!PlatformDetector.IsMacOS)
			return new List<Device>();

		var result = await ProcessRunner.RunAsync(
			"xcrun", "simctl list devices --json",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
			return new List<Device>();

		try
		{
			var json = JsonDocument.Parse(result.StandardOutput);
			var devices = new List<Device>();

			if (json.RootElement.TryGetProperty("devices", out var devicesElement))
			{
				foreach (var runtime in devicesElement.EnumerateObject())
				{
					var runtimeName = ExtractRuntimeName(runtime.Name);
					var runtimeVersion = ExtractRuntimeVersion(runtime.Name);
					var platform = ExtractPlatformFromRuntime(runtime.Name);

					foreach (var device in runtime.Value.EnumerateArray())
					{
						var udid = device.GetProperty("udid").GetString();
						var name = device.GetProperty("name").GetString();
						var state = device.GetProperty("state").GetString();
						var isAvailable = device.TryGetProperty("isAvailable", out var avail) && avail.GetBoolean();

						if (udid == null || name == null || !isAvailable)
							continue;

						var deviceState = ParseState(state);
						var idiom = DetermineIdiomFromName(name);
						var architecture = PlatformDetector.IsArm64 ? "arm64" : "x64";

						devices.Add(new Device
						{
							Id = udid,
							Name = $"{name} ({runtimeName})",
							Platforms = new[] { platform },
							Type = DeviceType.Simulator,
							State = deviceState,
							IsEmulator = true,
							IsRunning = deviceState == DeviceState.Booted,
							ConnectionType = Models.ConnectionType.Local,
							EmulatorId = udid,
							Model = name,
							Manufacturer = "Apple",
							Version = runtimeVersion,
							VersionName = runtimeName,
							Architecture = architecture,
							PlatformArchitecture = architecture,
							RuntimeIdentifiers = GetAppleRuntimeIdentifiers(platform, architecture),
							Idiom = idiom
						});
					}
				}
			}

			return devices;
		}
		catch
		{
			return new List<Device>();
		}
	}

	private static string DetermineIdiomFromName(string name)
	{
		var lowerName = name.ToLowerInvariant();
		if (lowerName.Contains("ipad", StringComparison.Ordinal)) return DeviceIdiom.Tablet;
		if (lowerName.Contains("watch", StringComparison.Ordinal)) return DeviceIdiom.Watch;
		if (lowerName.Contains("tv", StringComparison.Ordinal)) return DeviceIdiom.TV;
		if (lowerName.Contains("vision", StringComparison.Ordinal)) return DeviceIdiom.Desktop; // visionOS
		return DeviceIdiom.Phone; // iPhone, default
	}

	private static string ExtractPlatformFromRuntime(string runtimeIdentifier)
	{
		if (runtimeIdentifier.Contains("tvOS", StringComparison.OrdinalIgnoreCase)) return "tvos";
		if (runtimeIdentifier.Contains("watchOS", StringComparison.OrdinalIgnoreCase)) return "watchos";
		if (runtimeIdentifier.Contains("xrOS", StringComparison.OrdinalIgnoreCase) || 
		    runtimeIdentifier.Contains("visionOS", StringComparison.OrdinalIgnoreCase)) return "visionos";
		return "ios"; // Default to iOS
	}

	private static string? ExtractRuntimeVersion(string runtimeIdentifier)
	{
		// Format: com.apple.CoreSimulator.SimRuntime.iOS-18-5 -> "18.5"
		var match = System.Text.RegularExpressions.Regex.Match(runtimeIdentifier, @"(\d+)-(\d+)(?:-(\d+))?$");
		if (match.Success)
		{
			var major = match.Groups[1].Value;
			var minor = match.Groups[2].Value;
			var patch = match.Groups[3].Success ? $".{match.Groups[3].Value}" : "";
			return $"{major}.{minor}{patch}";
		}
		return null;
	}

	private static string[]? GetAppleRuntimeIdentifiers(string platform, string architecture)
	{
		var rid = platform switch
		{
			"ios" => $"iossimulator-{architecture}",
			"tvos" => $"tvossimulator-{architecture}",
			"watchos" => $"watchossimulator-{architecture}",
			"maccatalyst" => $"maccatalyst-{architecture}",
			_ => $"iossimulator-{architecture}"
		};
		return new[] { rid };
	}

	/// <summary>
	/// Lists all available runtimes.
	/// </summary>
	public async Task<List<Runtime>> ListRuntimesAsync(CancellationToken cancellationToken = default)
	{
		if (!PlatformDetector.IsMacOS)
			return new List<Runtime>();

		var result = await ProcessRunner.RunAsync(
			"xcrun", "simctl list runtimes --json",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
			return new List<Runtime>();

		try
		{
			var json = JsonDocument.Parse(result.StandardOutput);
			var runtimes = new List<Runtime>();

			if (json.RootElement.TryGetProperty("runtimes", out var runtimesElement))
			{
				foreach (var runtime in runtimesElement.EnumerateArray())
				{
					var identifier = runtime.GetProperty("identifier").GetString();
					var name = runtime.GetProperty("name").GetString();
					var version = runtime.TryGetProperty("version", out var v) ? v.GetString() : null;
					var isAvailable = runtime.TryGetProperty("isAvailable", out var avail) && avail.GetBoolean();

					if (identifier == null || name == null)
						continue;

					runtimes.Add(new Runtime
					{
						Identifier = identifier,
						Name = name,
						Version = version ?? "unknown",
						IsAvailable = isAvailable,
						IsInstalled = true,
						Source = "disk"
					});
				}
			}

			return runtimes;
		}
		catch
		{
			return new List<Runtime>();
		}
	}

	/// <summary>
	/// Lists runtimes available for download (not yet installed).
	/// </summary>
	public async Task<List<Runtime>> ListAvailableRuntimesAsync(CancellationToken cancellationToken = default)
	{
		if (!PlatformDetector.IsMacOS)
			return new List<Runtime>();

		var result = await ProcessRunner.RunAsync(
			"xcrun", "simctl runtime list --json",
			timeout: TimeSpan.FromSeconds(60),
			cancellationToken: cancellationToken);

		if (!result.Success)
			return new List<Runtime>();

		try
		{
			var json = JsonDocument.Parse(result.StandardOutput);
			var runtimes = new List<Runtime>();

			// The output may vary by Xcode version; handle array at root or under a key
			JsonElement runtimesElement;
			if (json.RootElement.ValueKind == JsonValueKind.Array)
				runtimesElement = json.RootElement;
			else if (json.RootElement.TryGetProperty("runtimes", out var prop))
				runtimesElement = prop;
			else
				return runtimes;

			foreach (var runtime in runtimesElement.EnumerateArray())
			{
				var identifier = runtime.TryGetProperty("identifier", out var id) ? id.GetString() : null;
				var name = runtime.TryGetProperty("name", out var n) ? n.GetString() : null;
				var version = runtime.TryGetProperty("version", out var v) ? v.GetString() : null;
				var platform = runtime.TryGetProperty("platform", out var p) ? p.GetString() : null;
				var state = runtime.TryGetProperty("state", out var s) ? s.GetString() : null;
				var source = runtime.TryGetProperty("source", out var src) ? src.GetString() : null;

				if (identifier == null)
					continue;

				var displayName = name ?? $"{platform} {version}";
				var isInstalled = state?.Equals("Ready", StringComparison.OrdinalIgnoreCase) == true;

				runtimes.Add(new Runtime
				{
					Identifier = identifier,
					Name = displayName,
					Version = version ?? "unknown",
					IsAvailable = true,
					IsInstalled = isInstalled,
					Source = source
				});
			}

			return runtimes;
		}
		catch
		{
			return new List<Runtime>();
		}
	}

	/// <summary>
	/// Installs a runtime by platform and version.
	/// </summary>
	public async Task InstallRuntimeAsync(string version, IProgress<string>? progress = null,
		CancellationToken cancellationToken = default)
	{
		progress?.Report($"Installing iOS {version} runtime...");

		var result = await ProcessRunner.RunAsync(
			"xcodebuild", $"-downloadPlatform iOS -buildVersion {version}",
			timeout: TimeSpan.FromMinutes(60),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			// Fallback: try xcrun simctl runtime add
			var fallback = await ProcessRunner.RunAsync(
				"xcrun", $"simctl runtime add \"iOS {version}\"",
				timeout: TimeSpan.FromMinutes(60),
				cancellationToken: cancellationToken);

			if (!fallback.Success)
			{
				throw new Errors.MauiToolException(
					Errors.ErrorCodes.RuntimeInstallFailed,
					$"Failed to install iOS {version} runtime: {result.StandardError}",
					nativeError: result.StandardError);
			}
		}

		progress?.Report($"iOS {version} runtime installed successfully");
	}

	/// <summary>
	/// Creates a new simulator device.
	/// </summary>
	public async Task<Device> CreateDeviceAsync(string name, string deviceType, string runtime,
		CancellationToken cancellationToken = default)
	{
		var result = await ProcessRunner.RunAsync(
			"xcrun", $"simctl create \"{name}\" \"{deviceType}\" \"{runtime}\"",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AppleSimctlFailed,
				$"Failed to create simulator: {result.StandardError}",
				nativeError: result.StandardError);
		}

		var udid = result.StandardOutput.Trim();
		var platform = ExtractPlatformFromRuntime(runtime);
		var runtimeVersion = ExtractRuntimeVersion(runtime);
		var architecture = PlatformDetector.IsArm64 ? "arm64" : "x64";
		var idiom = DetermineIdiomFromName(deviceType);

		return new Device
		{
			Id = udid,
			Name = name,
			Platforms = new[] { platform },
			Type = DeviceType.Simulator,
			State = DeviceState.Shutdown,
			IsEmulator = true,
			IsRunning = false,
			ConnectionType = Models.ConnectionType.Local,
			EmulatorId = udid,
			Model = deviceType,
			Manufacturer = "Apple",
			Version = runtimeVersion,
			VersionName = ExtractRuntimeName(runtime),
			Architecture = architecture,
			PlatformArchitecture = architecture,
			RuntimeIdentifiers = GetAppleRuntimeIdentifiers(platform, architecture),
			Idiom = idiom
		};
	}

	/// <summary>
	/// Deletes a simulator device.
	/// </summary>
	public async Task DeleteDeviceAsync(string udid, CancellationToken cancellationToken = default)
	{
		var result = await ProcessRunner.RunAsync(
			"xcrun", $"simctl delete \"{udid}\"",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AppleSimctlFailed,
				$"Failed to delete simulator: {result.StandardError}",
				nativeError: result.StandardError);
		}
	}

	/// <summary>
	/// Boots a simulator device.
	/// </summary>
	public async Task BootDeviceAsync(string udid, CancellationToken cancellationToken = default)
	{
		var result = await ProcessRunner.RunAsync(
			"xcrun", $"simctl boot \"{udid}\"",
			timeout: TimeSpan.FromSeconds(60),
			cancellationToken: cancellationToken);

		// Boot can return error if already booted - that's ok
		if (!result.Success && !result.StandardError.Contains("Unable to boot device in current state", StringComparison.Ordinal))
		{
			throw new MauiToolException(
				ErrorCodes.AppleSimctlFailed,
				$"Failed to boot simulator: {result.StandardError}",
				nativeError: result.StandardError);
		}
	}

	/// <summary>
	/// Shuts down a simulator device.
	/// </summary>
	public async Task ShutdownDeviceAsync(string udid, CancellationToken cancellationToken = default)
	{
		var result = await ProcessRunner.RunAsync(
			"xcrun", $"simctl shutdown \"{udid}\"",
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		// Shutdown can return error if already shutdown - that's ok
		if (!result.Success && !result.StandardError.Contains("Unable to shutdown device in current state", StringComparison.Ordinal))
		{
			throw new MauiToolException(
				ErrorCodes.AppleSimctlFailed,
				$"Failed to shutdown simulator: {result.StandardError}",
				nativeError: result.StandardError);
		}
	}

	private static string ExtractRuntimeName(string runtimeIdentifier)
	{
		// com.apple.CoreSimulator.SimRuntime.iOS-17-0 -> iOS 17.0
		var parts = runtimeIdentifier.Split('.');
		if (parts.Length > 0)
		{
			var last = parts[^1];
			return last.Replace('-', '.');
		}
		return runtimeIdentifier;
	}

	private static DeviceState ParseState(string? state)
	{
		return state?.ToLowerInvariant() switch
		{
			"booted" => DeviceState.Booted,
			"shutdown" => DeviceState.Shutdown,
			_ => DeviceState.Unknown
		};
	}
}
