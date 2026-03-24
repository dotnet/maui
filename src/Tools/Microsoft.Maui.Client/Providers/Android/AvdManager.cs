// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.Maui.Client.Errors;
using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Utils;
using Xamarin.Android.Tools;

namespace Microsoft.Maui.Client.Providers.Android;

/// <summary>
/// Wrapper for Android Virtual Device (AVD) Manager operations.
/// Delegates to Xamarin.Android.Tools.AvdManagerRunner for core functionality.
/// </summary>
public class AvdManager
{
	const int EmulatorStartupCheckDelayMs = 3000;

	readonly AvdManagerRunner? _runner;
	readonly EmulatorRunner? _emulatorRunner;
	readonly Adb? _adb;

	public AvdManager(Func<string?> getSdkPath, Func<string?> getJdkPath, Adb? adb = null)
	{
		_adb = adb;
		var sdkPath = getSdkPath();
		var env = AndroidEnvironment.BuildEnvironmentVariables(sdkPath, getJdkPath());

		var avdManagerPath = ResolveAvdManagerPath(sdkPath);
		if (avdManagerPath != null)
			_runner = new AvdManagerRunner(avdManagerPath, env);

		var emulatorPath = ResolveEmulatorPath(sdkPath);
		if (emulatorPath != null)
			_emulatorRunner = new EmulatorRunner(emulatorPath, env);
	}

	public bool IsAvailable => _runner != null;

	public string? EmulatorPath => _emulatorRunner != null ? "emulator" : null;

	static string? ResolveAvdManagerPath(string? sdkPath)
	{
		if (string.IsNullOrEmpty(sdkPath))
			return null;

		var ext = OperatingSystem.IsWindows() ? ".bat" : "";

		// Search cmdline-tools/{version}/bin/ (highest version first), then latest, then legacy tools/bin/
		var cmdlineToolsDir = Path.Combine(sdkPath, "cmdline-tools");
		if (Directory.Exists(cmdlineToolsDir))
		{
			var subdirs = new List<(string name, Version? version)>();
			foreach (var dir in Directory.GetDirectories(cmdlineToolsDir))
			{
				var name = Path.GetFileName(dir);
				if (string.IsNullOrEmpty(name) || name == "latest")
					continue;
				Version.TryParse(name, out var v);
				subdirs.Add((name, v ?? new Version(0, 0)));
			}
			subdirs.Sort((a, b) => b.version!.CompareTo(a.version));

			foreach (var (name, _) in subdirs)
			{
				var toolPath = Path.Combine(cmdlineToolsDir, name, "bin", "avdmanager" + ext);
				if (File.Exists(toolPath))
					return toolPath;
			}
			var latestPath = Path.Combine(cmdlineToolsDir, "latest", "bin", "avdmanager" + ext);
			if (File.Exists(latestPath))
				return latestPath;
		}

		var legacyPath = Path.Combine(sdkPath, "tools", "bin", "avdmanager" + ext);
		return File.Exists(legacyPath) ? legacyPath : null;
	}

	static string? ResolveEmulatorPath(string? sdkPath)
	{
		if (string.IsNullOrEmpty(sdkPath))
			return null;

		var ext = OperatingSystem.IsWindows() ? ".exe" : "";
		var emulatorPath = Path.Combine(sdkPath, "emulator", "emulator" + ext);
		return File.Exists(emulatorPath) ? emulatorPath : null;
	}

	public async Task<List<AvdInfo>> GetAvdsAsync(CancellationToken cancellationToken = default)
	{
		if (_runner == null)
			return new List<AvdInfo>();

		try
		{
			var avds = await _runner.ListAvdsAsync(cancellationToken);
			return avds.Select(MapToMauiAvd).ToList();
		}
		catch (InvalidOperationException ex)
		{
			System.Diagnostics.Trace.WriteLine($"AVD list failed: {ex.Message}");
			return new List<AvdInfo>();
		}
	}

	static AvdInfo MapToMauiAvd(Xamarin.Android.Tools.AvdInfo avd)
	{
		string? systemImage = null;
		string? target = null;

		// Try to read system image from AVD config.ini
		if (!string.IsNullOrEmpty(avd.Path))
		{
			var configPath = System.IO.Path.Combine(avd.Path, "config.ini");
			if (File.Exists(configPath))
			{
				try
				{
					foreach (var line in File.ReadLines(configPath))
					{
						if (line.StartsWith("image.sysdir.1=", StringComparison.Ordinal))
						{
							// e.g. "image.sysdir.1=system-images/android-36/google_apis/arm64-v8a/"
							var val = line.Substring("image.sysdir.1=".Length).Trim().TrimEnd('/');
							systemImage = val.Replace("/", ";", StringComparison.Ordinal);
							// Extract target like "Android 36 (google_apis/arm64-v8a)"
							var parts = val.Split('/');
							if (parts.Length >= 2)
							{
								var api = parts[1]; // android-36
								var variant = parts.Length > 2 ? string.Join("/", parts.Skip(2)) : "";
								target = string.IsNullOrEmpty(variant)
									? api.Replace("android-", "Android ", StringComparison.Ordinal)
									: $"{api.Replace("android-", "Android ", StringComparison.Ordinal)} ({variant})";
							}
						}
						else if (line.StartsWith("tag.display=", StringComparison.Ordinal) && target == null)
						{
							target = line.Substring("tag.display=".Length).Trim();
						}
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Trace.WriteLine($"AVD config read error for '{avd.Name}': {ex.Message}");
				}
			}
		}

		return new AvdInfo
		{
			Name = avd.Name,
			DeviceProfile = avd.DeviceProfile,
			SystemImage = systemImage,
			Target = target,
			Path = avd.Path,
		};
	}

	public async Task<AvdInfo> CreateAvdAsync(string name, string deviceProfile, string systemImage,
		bool force = false, CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"AVD Manager not found",
				"maui android install");

		try
		{
			await _runner!.GetOrCreateAvdAsync(name, systemImage, deviceProfile, force, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidAvdCreateFailed,
				$"Failed to create AVD '{name}': {ex.Message}");
		}

		return new AvdInfo
		{
			Name = name,
			DeviceProfile = deviceProfile,
		};
	}

	public async Task StartAvdAsync(string name, bool coldBoot = false, bool wait = false,
		CancellationToken cancellationToken = default)
	{
		if (_emulatorRunner == null)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidEmulatorNotFound,
				"Android emulator not installed",
				"maui android sdk install emulator");

		try
		{
			// Check if this emulator is already running before attempting to launch
			if (_adb != null)
			{
				var devices = await _adb.GetDevicesAsync(cancellationToken);
				var alreadyRunning = devices.FirstOrDefault(d =>
					d.IsEmulator && d.IsRunning &&
					(string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(d.EmulatorId, name, StringComparison.OrdinalIgnoreCase)));
				if (alreadyRunning != null)
					return; // Already running, nothing to do
			}

			if (wait && _adb?.Runner != null)
			{
				var result = await _emulatorRunner.BootEmulatorAsync(name, _adb.Runner,
					new EmulatorBootOptions { ColdBoot = coldBoot }, cancellationToken);
				if (!result.Success)
					throw new MauiToolException(
						ErrorCodes.AndroidEmulatorNotFound,
						result.ErrorMessage ?? $"Failed to boot AVD '{name}'");
			}
			else
			{
				var process = _emulatorRunner.LaunchEmulator(name, coldBoot);

				// Wait briefly to detect immediate crashes (e.g. stale lock files,
				// missing system image, HAXM issues). A healthy emulator stays alive.
				try
				{
					await Task.Delay(EmulatorStartupCheckDelayMs, cancellationToken);
				}
				catch (OperationCanceledException)
				{
					try
					{ process.Kill(); }
					catch { }
					process.Dispose();
					throw;
				}

				if (process.HasExited && process.ExitCode != 0)
				{
					process.Dispose();
					throw new MauiToolException(
						ErrorCodes.AndroidEmulatorNotFound,
						$"Emulator exited immediately with code {process.ExitCode}. " +
						$"This often means stale lock files exist. " +
						$"Try stopping all emulators first with: maui android emulator stop");
				}

				// Release the process handle; the emulator continues running independently.
				process.Dispose();
			}
		}
		catch (Exception ex) when (ex is not OperationCanceledException and not MauiToolException)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidEmulatorNotFound,
				$"Failed to start AVD '{name}': {ex.Message}");
		}
	}

	public async Task DeleteAvdAsync(string name, CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"AVD Manager not found",
				"maui android install");

		try
		{
			await _runner!.DeleteAvdAsync(name, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidAvdDeleteFailed,
				$"Failed to delete AVD '{name}': {ex.Message}");
		}
	}
}
