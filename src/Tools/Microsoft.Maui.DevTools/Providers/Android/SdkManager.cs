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

			var ext = PlatformDetector.IsWindows ? ".bat" : string.Empty;
			var cmdlineToolsDir = Path.Combine(SdkPath, "cmdline-tools");

			if (Directory.Exists(cmdlineToolsDir))
			{
				// Search through version directories, sorted by version (like AndroidSdk.Tools)
				// Check common directory names: latest, then versioned folders
				var searchDirs = new List<string> { "latest" };
				
				try
				{
					// Add versioned directories sorted descending (newest first)
					var versionedDirs = Directory.GetDirectories(cmdlineToolsDir)
						.Select(d => Path.GetFileName(d))
						.Where(n => n != "latest" && !string.IsNullOrEmpty(n))
						.OrderByDescending(n => {
							// Try to parse as version, otherwise sort alphabetically
							if (Version.TryParse(n, out var v))
								return v;
							return new Version(0, 0);
						})
						.ToList();
					searchDirs.AddRange(versionedDirs);
				}
				catch { }

				foreach (var dir in searchDirs)
				{
					var toolPath = Path.Combine(cmdlineToolsDir, dir, "bin", "sdkmanager" + ext);
					if (File.Exists(toolPath))
						return toolPath;
				}
			}

			// Try older location (tools/bin)
			var toolsPath = Path.Combine(SdkPath, "tools", "bin", "sdkmanager" + ext);
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
					Command = "dotnet maui android install"
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
		=> AndroidEnvironment.GetEnvironment(SdkPath, JdkPath);

	/// <summary>
	/// Runs sdkmanager --list once and returns both installed and available packages.
	/// </summary>
	private async Task<(List<SdkPackage> Installed, List<SdkPackage> Available)> ListAllPackagesAsync(CancellationToken cancellationToken)
	{
		if (!IsAvailable)
			return ([], []);

		var result = await ProcessRunner.RunAsync(
			SdkManagerPath!,
			"--list",
			environmentVariables: GetEnvironment(),
			timeout: TimeSpan.FromMinutes(2),
			cancellationToken: cancellationToken);

		if (!result.Success)
			return ([], []);

		return ParseAllPackages(result.StandardOutput);
	}

	public async Task<List<SdkPackage>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
	{
		var (installed, _) = await ListAllPackagesAsync(cancellationToken);
		return installed;
	}

	public async Task<List<SdkPackage>> GetAvailablePackagesAsync(CancellationToken cancellationToken = default)
	{
		var (_, available) = await ListAllPackagesAsync(cancellationToken);
		return available;
	}

	private static (List<SdkPackage> Installed, List<SdkPackage> Available) ParseAllPackages(string output)
	{
		var installed = new List<SdkPackage>();
		var available = new List<SdkPackage>();

		var currentSection = (string?)null;
		var lines = output.Split('\n');

		foreach (var line in lines)
		{
			if (line.Contains("Installed packages:", StringComparison.Ordinal))
			{
				currentSection = "installed";
				continue;
			}
			if (line.Contains("Available Packages:", StringComparison.Ordinal))
			{
				currentSection = "available";
				continue;
			}
			if (line.Contains("Available Updates:", StringComparison.Ordinal))
			{
				currentSection = null;
				continue;
			}

			if (currentSection != null && !string.IsNullOrWhiteSpace(line))
			{
				var parts = line.Split('|').Select(p => p.Trim()).ToArray();
				if (parts.Length >= 2 && !parts[0].StartsWith("Path", StringComparison.Ordinal) && !parts[0].StartsWith("---", StringComparison.Ordinal))
				{
					var pkg = new SdkPackage
					{
						Path = parts[0],
						Version = parts.Length > 1 ? parts[1] : null,
						Description = parts.Length > 2 ? parts[2] : null,
						IsInstalled = currentSection == "installed"
					};

					if (currentSection == "installed")
						installed.Add(pkg);
					else
						available.Add(pkg);
				}
			}
		}

		return (installed, available);
	}

	public async Task InstallPackagesAsync(IEnumerable<string> packages, bool acceptLicenses = false,
		CancellationToken cancellationToken = default)
	{
		if (!IsAvailable)
			throw MauiToolException.AutoFixable(
				ErrorCodes.AndroidSdkManagerNotFound,
				"SDK Manager not found. Run 'dotnet maui android install' first.",
				"dotnet maui android install");

		var packageList = string.Join(" ", packages.Select(p => $"\"{p}\""));
		var args = packageList;

		ProcessResult result;
		if (acceptLicenses)
		{
			result = await ProcessRunner.RunAsync(
				SdkManagerPath!,
				args,
				environmentVariables: GetEnvironment(),
				timeout: TimeSpan.FromMinutes(30),
				continuousInput: "y",
				cancellationToken: cancellationToken);
		}
		else
		{
			result = await ProcessRunner.RunAsync(
				SdkManagerPath!,
				args,
				environmentVariables: GetEnvironment(),
				timeout: TimeSpan.FromMinutes(30),
				cancellationToken: cancellationToken);
		}

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
				"dotnet maui android install");

		var result = await ProcessRunner.RunAsync(
			SdkManagerPath!,
			"--licenses",
			environmentVariables: GetEnvironment(),
			timeout: TimeSpan.FromMinutes(5),
			continuousInput: "y",
			cancellationToken: cancellationToken);

		// License acceptance may return non-zero even when successful
		// if licenses were already accepted, so we don't throw on failure
	}

	public Task<bool> AreLicensesAcceptedAsync(CancellationToken cancellationToken = default)
	{
		if (!IsAvailable || string.IsNullOrEmpty(SdkPath))
			return Task.FromResult(false);

		var licensesPath = Path.Combine(SdkPath, "licenses");
		if (!Directory.Exists(licensesPath))
			return Task.FromResult(false);

		var licenseFiles = Directory.GetFiles(licensesPath);
		return Task.FromResult(licenseFiles.Length > 0);
	}

	public async Task InstallSdkAsync(string targetPath, CancellationToken cancellationToken = default)
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
		// Latest command-line tools URLs (universal binaries on macOS)
		if (PlatformDetector.IsMacOS)
			return "https://dl.google.com/android/repository/commandlinetools-mac-11076708_latest.zip";
		else if (PlatformDetector.IsWindows)
			return "https://dl.google.com/android/repository/commandlinetools-win-11076708_latest.zip";
		else
			return "https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip";
	}
}
