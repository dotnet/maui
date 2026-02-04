// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Wrapper for Android Virtual Device (AVD) Manager operations.
/// </summary>
public class AvdManager
{
	private readonly Func<string?> _getSdkPath;
	private readonly Func<string?> _getJdkPath;

	public AvdManager(Func<string?> getSdkPath, Func<string?> getJdkPath)
	{
		_getSdkPath = getSdkPath;
		_getJdkPath = getJdkPath;
	}

	private string? SdkPath => _getSdkPath();
	private string? JdkPath => _getJdkPath();

	public string? AvdManagerPath
	{
		get
		{
			if (string.IsNullOrEmpty(SdkPath))
				return null;

			var cmdlineToolsPath = Path.Combine(SdkPath, "cmdline-tools", "latest", "bin", "avdmanager");
			if (PlatformDetector.IsWindows)
				cmdlineToolsPath += ".bat";

			if (File.Exists(cmdlineToolsPath))
				return cmdlineToolsPath;

			// Try older location
			var toolsPath = Path.Combine(SdkPath, "tools", "bin", "avdmanager");
			if (PlatformDetector.IsWindows)
				toolsPath += ".bat";

			if (File.Exists(toolsPath))
				return toolsPath;

			return null;
		}
	}

	public string? EmulatorPath
	{
		get
		{
			if (string.IsNullOrEmpty(SdkPath))
				return null;

			var path = Path.Combine(SdkPath, "emulator", "emulator");
			if (PlatformDetector.IsWindows)
				path += ".exe";

			return File.Exists(path) ? path : null;
		}
	}

	public bool IsAvailable => AvdManagerPath != null;

	private Dictionary<string, string>? GetEnvironment()
	{
		var env = new Dictionary<string, string>();
		
		if (!string.IsNullOrEmpty(JdkPath))
		{
			env["JAVA_HOME"] = JdkPath;
			env["PATH"] = $"{Path.Combine(JdkPath, "bin")}{Path.PathSeparator}{Environment.GetEnvironmentVariable("PATH")}";
		}

		if (!string.IsNullOrEmpty(SdkPath))
		{
			env["ANDROID_HOME"] = SdkPath;
			env["ANDROID_SDK_ROOT"] = SdkPath;
		}

		return env.Count > 0 ? env : null;
	}

	public async Task<List<AvdInfo>> GetAvdsAsync(CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			return new List<AvdInfo>();

		var result = await ProcessRunner.RunAsync(
			AvdManagerPath!,
			"list avd",
			environmentVariables: GetEnvironment(),
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
			return new List<AvdInfo>();

		return ParseAvdList(result.StandardOutput);
	}

	private static List<AvdInfo> ParseAvdList(string output)
	{
		var avds = new List<AvdInfo>();
		var lines = output.Split('\n');
		
		string? currentName = null;
		string? currentDevice = null;
		string? currentPath = null;
		string? currentTarget = null;

		foreach (var line in lines)
		{
			var trimmed = line.Trim();
			
			if (trimmed.StartsWith("Name:"))
			{
				// Save previous AVD if exists
				if (currentName != null)
				{
					avds.Add(new AvdInfo
					{
						Name = currentName,
						DeviceProfile = currentDevice,
						Path = currentPath,
						SystemImage = currentTarget,
						Target = currentTarget
					});
				}
				currentName = trimmed.Substring(5).Trim();
				currentDevice = null;
				currentPath = null;
				currentTarget = null;
			}
			else if (trimmed.StartsWith("Device:"))
			{
				currentDevice = trimmed.Substring(7).Trim();
			}
			else if (trimmed.StartsWith("Path:"))
			{
				currentPath = trimmed.Substring(5).Trim();
			}
			else if (trimmed.StartsWith("Target:"))
			{
				currentTarget = trimmed.Substring(7).Trim();
			}
			else if (trimmed.StartsWith("Based on:"))
			{
				// Use "Based on:" if Target: is not present
				if (currentTarget == null)
					currentTarget = trimmed.Substring(9).Trim();
			}
		}

		// Don't forget the last one
		if (currentName != null)
		{
			avds.Add(new AvdInfo
			{
				Name = currentName,
				DeviceProfile = currentDevice,
				Path = currentPath,
				SystemImage = currentTarget,
				Target = currentTarget
			});
		}

		return avds;
	}

	public async Task<AvdInfo> CreateAvdAsync(string name, string deviceProfile, string systemImage,
		bool force = false, CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"AVD Manager not found",
				"dotnet maui android bootstrap");

		var forceFlag = force ? " --force" : "";
		var args = $"create avd --name \"{name}\" --device \"{deviceProfile}\" --package \"{systemImage}\"{forceFlag}";

		// avdmanager requires 'no' for hardware profile question
		var result = await ProcessRunner.RunAsync(
			"/bin/bash",
			$"-c \"echo no | '{AvdManagerPath}' {args}\"",
			environmentVariables: GetEnvironment(),
			timeout: TimeSpan.FromMinutes(2),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidAvdCreateFailed,
				$"Failed to create AVD '{name}': {result.StandardError}",
				nativeError: result.StandardError);
		}

		return new AvdInfo
		{
			Name = name,
			DeviceProfile = deviceProfile,
			SystemImage = systemImage
		};
	}

	public async Task StartAvdAsync(string name, bool coldBoot = false, bool wait = false,
		CancellationToken cancellationToken = default)
	{
		if (EmulatorPath == null)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidEmulatorNotFound,
				"Android emulator not installed",
				"dotnet maui android sdk install emulator");

		var args = $"-avd \"{name}\"";
		if (coldBoot)
			args += " -no-snapshot-load";

		// Start emulator in background
		var process = new System.Diagnostics.Process
		{
			StartInfo = new System.Diagnostics.ProcessStartInfo
			{
				FileName = EmulatorPath,
				Arguments = args,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			}
		};

		var env = GetEnvironment();
		if (env != null)
		{
			foreach (var kvp in env)
				process.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
		}

		process.Start();

		if (wait)
		{
			// Wait for emulator to boot using adb
			var adb = new Adb(() => SdkPath);
			await adb.WaitForDeviceAsync(cancellationToken: cancellationToken);
		}
	}

	public async Task DeleteAvdAsync(string name, CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"AVD Manager not found",
				"dotnet maui android bootstrap");

		var result = await ProcessRunner.RunAsync(
			AvdManagerPath!,
			$"delete avd --name \"{name}\"",
			environmentVariables: GetEnvironment(),
			timeout: TimeSpan.FromSeconds(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidAvdDeleteFailed,
				$"Failed to delete AVD '{name}': {result.StandardError}",
				nativeError: result.StandardError);
		}
	}
}
