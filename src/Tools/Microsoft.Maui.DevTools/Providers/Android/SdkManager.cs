// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Wrapper for Android SDK Manager operations.
/// </summary>
public class SdkManager
{
	private readonly Func<string?> _getSdkPath;
	private readonly Func<string?> _getJdkPath;

	public SdkManager(Func<string?> getSdkPath, Func<string?> getJdkPath)
	{
		_getSdkPath = getSdkPath;
		_getJdkPath = getJdkPath;
	}

	private string? SdkPath => _getSdkPath();
	private string? JdkPath => _getJdkPath();

	public string? SdkManagerPath
	{
		get
		{
			if (string.IsNullOrEmpty(SdkPath))
				return null;

			var cmdlineToolsPath = Path.Combine(SdkPath, "cmdline-tools", "latest", "bin", "sdkmanager");
			if (PlatformDetector.IsWindows)
				cmdlineToolsPath += ".bat";

			if (File.Exists(cmdlineToolsPath))
				return cmdlineToolsPath;

			// Try older location
			var toolsPath = Path.Combine(SdkPath, "tools", "bin", "sdkmanager");
			if (PlatformDetector.IsWindows)
				toolsPath += ".bat";

			if (File.Exists(toolsPath))
				return toolsPath;

			return null;
		}
	}

	public bool IsAvailable => SdkManagerPath != null;

	public async Task<HealthCheck> CheckHealthAsync(CancellationToken cancellationToken = default)
	{
		if (!IsAvailable || string.IsNullOrEmpty(SdkPath))
		{
			return new HealthCheck
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
					Command = "dotnet maui android bootstrap"
				}
			};
		}

		return new HealthCheck
		{
			Category = "android",
			Name = "Android SDK",
			Status = CheckStatus.Ok,
			Message = "Android SDK installed",
			Details = new Dictionary<string, object>
			{
				["path"] = SdkPath
			}
		};
	}

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

	public async Task<List<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			return new List<SdkPackage>();

		var result = await ProcessRunner.RunAsync(
			SdkManagerPath!,
			"--list",
			environmentVariables: GetEnvironment(),
			timeout: TimeSpan.FromMinutes(2),
			cancellationToken: cancellationToken);

		if (!result.Success)
			return new List<SdkPackage>();

		return ParseInstalledPackages(result.StandardOutput);
	}

	private static List<SdkPackage> ParseInstalledPackages(string output)
	{
		var packages = new List<SdkPackage>();
		var inInstalledSection = false;
		var lines = output.Split('\n');

		foreach (var line in lines)
		{
			if (line.Contains("Installed packages:", StringComparison.Ordinal))
			{
				inInstalledSection = true;
				continue;
			}

			if (line.Contains("Available Packages:", StringComparison.Ordinal) || line.Contains("Available Updates:", StringComparison.Ordinal))
			{
				inInstalledSection = false;
				continue;
			}

			if (inInstalledSection && !string.IsNullOrWhiteSpace(line))
			{
				var parts = line.Split('|').Select(p => p.Trim()).ToArray();
				if (parts.Length >= 2 && !parts[0].StartsWith("Path") && !parts[0].StartsWith("---"))
				{
					packages.Add(new SdkPackage
					{
						Path = parts[0],
						Version = parts.Length > 1 ? parts[1] : null,
						Description = parts.Length > 2 ? parts[2] : null
					});
				}
			}
		}

		return packages;
	}

	public async Task InstallPackagesAsync(IEnumerable<string> packages, bool acceptLicenses = false,
		CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"SDK Manager not found. Run 'dotnet maui android bootstrap' first.",
				"dotnet maui android bootstrap");

		var packageList = string.Join(" ", packages.Select(p => $"\"{p}\""));
		var args = acceptLicenses ? $"--install {packageList}" : $"{packageList}";

		// For accepting licenses, we need to pipe 'y' to stdin
		var result = await ProcessRunner.RunAsync(
			SdkManagerPath!,
			args,
			environmentVariables: GetEnvironment(),
			timeout: TimeSpan.FromMinutes(30),
			cancellationToken: cancellationToken);

		if (!result.Success)
		{
			throw new MauiToolException(
				ErrorCodes.AndroidPackageInstallFailed,
				$"Failed to install packages: {result.StandardError}",
				nativeError: result.StandardError);
		}
	}

	public async Task AcceptLicensesAsync(CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"SDK Manager not found",
				"dotnet maui android bootstrap");

		// Use yes command to accept all licenses
		var yesCommand = PlatformDetector.IsWindows ? "cmd" : "yes";
		var yesArgs = PlatformDetector.IsWindows 
			? $"/c echo y | \"{SdkManagerPath}\" --licenses"
			: $"| \"{SdkManagerPath}\" --licenses";

		if (PlatformDetector.IsWindows)
		{
			var result = await ProcessRunner.RunAsync(
				"cmd",
				$"/c echo y | \"{SdkManagerPath}\" --licenses",
				environmentVariables: GetEnvironment(),
				timeout: TimeSpan.FromMinutes(5),
				cancellationToken: cancellationToken);
		}
		else
		{
			// On Unix, use a shell to pipe yes to sdkmanager
			var result = await ProcessRunner.RunAsync(
				"/bin/bash",
				$"-c \"yes | '{SdkManagerPath}' --licenses\"",
				environmentVariables: GetEnvironment(),
				timeout: TimeSpan.FromMinutes(5),
				cancellationToken: cancellationToken);
		}
	}

	public async Task<bool> AreLicensesAcceptedAsync(CancellationToken cancellationToken = default)
	{
		if (!IsAvailable || string.IsNullOrEmpty(SdkPath))
			return false;

		// Check if licenses directory exists and has accepted licenses
		var licensesPath = Path.Combine(SdkPath, "licenses");
		if (!Directory.Exists(licensesPath))
			return false;

		var licenseFiles = Directory.GetFiles(licensesPath);
		return licenseFiles.Length > 0;
	}

	public async Task BootstrapSdkAsync(string targetPath, CancellationToken cancellationToken = default)
	{
		// Create target directory
		Directory.CreateDirectory(targetPath);

		// Download command-line tools
		var downloadUrl = GetCommandLineToolsUrl();
		var tempZipPath = Path.Combine(Path.GetTempPath(), "commandlinetools.zip");

		try
		{
			using var httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromMinutes(10);
			
			var response = await httpClient.GetAsync(downloadUrl, cancellationToken);
			response.EnsureSuccessStatusCode();

			await using var fs = File.Create(tempZipPath);
			await response.Content.CopyToAsync(fs, cancellationToken);
		}
		catch (Exception ex)
		{
			throw new MauiToolException(
				ErrorCodes.DownloadFailed,
				$"Failed to download Android command-line tools: {ex.Message}",
				nativeError: ex.Message);
		}

		// Extract to SDK path
		var cmdlineToolsPath = Path.Combine(targetPath, "cmdline-tools");
		Directory.CreateDirectory(cmdlineToolsPath);

		System.IO.Compression.ZipFile.ExtractToDirectory(tempZipPath, cmdlineToolsPath, overwriteFiles: true);

		// Rename to 'latest'
		var extractedPath = Path.Combine(cmdlineToolsPath, "cmdline-tools");
		var latestPath = Path.Combine(cmdlineToolsPath, "latest");
		
		if (Directory.Exists(extractedPath))
		{
			if (Directory.Exists(latestPath))
				Directory.Delete(latestPath, recursive: true);
			Directory.Move(extractedPath, latestPath);
		}

		// Clean up
		File.Delete(tempZipPath);
	}

	private static string GetCommandLineToolsUrl()
	{
		// Latest command-line tools URLs
		if (PlatformDetector.IsMacOS)
		{
			return PlatformDetector.IsArm64
				? "https://dl.google.com/android/repository/commandlinetools-mac-11076708_latest.zip"
				: "https://dl.google.com/android/repository/commandlinetools-mac-11076708_latest.zip";
		}
		else if (PlatformDetector.IsWindows)
		{
			return "https://dl.google.com/android/repository/commandlinetools-win-11076708_latest.zip";
		}
		else
		{
			return "https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip";
		}
	}
}
