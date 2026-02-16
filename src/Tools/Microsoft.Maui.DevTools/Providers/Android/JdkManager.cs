// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// JDK detection and installation manager.
/// </summary>
public class JdkManager : IJdkManager
{
	private const int DefaultJdkVersion = 17;
	private const int MinJdkVersion = 11;
	private const int MaxJdkVersion = 21;

	public string? DetectedJdkPath { get; private set; }
	public int? DetectedJdkVersion { get; private set; }

	public bool IsInstalled => !string.IsNullOrEmpty(DetectedJdkPath);

	public JdkManager()
	{
		Detect();
	}

	private void Detect()
	{
		DetectedJdkPath = PlatformDetector.Paths.GetJdkPath();
		if (DetectedJdkPath != null)
		{
			DetectedJdkVersion = GetJdkVersion(DetectedJdkPath);
		}
	}

	private static int? GetJdkVersion(string jdkPath)
	{
		var javaBin = Path.Combine(jdkPath, "bin", "java");
		if (PlatformDetector.IsWindows)
			javaBin += ".exe";

		if (!File.Exists(javaBin))
			return null;

		try
		{
			var result = ProcessRunner.RunSync(javaBin, "-version", timeout: TimeSpan.FromSeconds(10));
			// Java version output is on stderr
			var output = result.StandardError + result.StandardOutput;
			
			// Parse version from output like: openjdk version "17.0.1" or java version "1.8.0_292"
			var match = System.Text.RegularExpressions.Regex.Match(output, @"version ""(\d+)");
			if (match.Success && int.TryParse(match.Groups[1].Value, out var version))
			{
				return version;
			}
		}
		catch { }

		return null;
	}

	public async Task<HealthCheck> CheckHealthAsync(CancellationToken cancellationToken = default)
	{
		Detect();

		if (!IsInstalled)
		{
			return new HealthCheck
			{
				Category = "android",
				Name = "JDK",
				Status = CheckStatus.Error,
				Message = "JDK not found",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.JdkNotFound,
					Description = "Install OpenJDK 17",
					AutoFixable = true,
					Command = "maui android jdk install"
				}
			};
		}

		if (DetectedJdkVersion.HasValue)
		{
			if (DetectedJdkVersion < MinJdkVersion)
			{
				return new HealthCheck
				{
					Category = "android",
					Name = "JDK",
					Status = CheckStatus.Error,
					Message = $"JDK {DetectedJdkVersion} is too old (minimum: {MinJdkVersion})",
					Details = new Dictionary<string, object>
					{
						["path"] = DetectedJdkPath!,
						["version"] = DetectedJdkVersion.Value
					},
					Fix = new FixInfo
					{
						IssueId = ErrorCodes.JdkVersionUnsupported,
						Description = $"Install OpenJDK {DefaultJdkVersion}",
						AutoFixable = true,
						Command = "maui android jdk install"
					}
				};
			}

			return new HealthCheck
			{
				Category = "android",
				Name = "JDK",
				Status = CheckStatus.Ok,
				Message = $"JDK {DetectedJdkVersion}",
				Details = new Dictionary<string, object>
				{
					["path"] = DetectedJdkPath!,
					["version"] = DetectedJdkVersion.Value
				}
			};
		}

		return new HealthCheck
		{
			Category = "android",
			Name = "JDK",
			Status = CheckStatus.Warning,
			Message = "JDK found but version unknown",
			Details = new Dictionary<string, object>
			{
				["path"] = DetectedJdkPath!
			}
		};
	}

	public async Task InstallAsync(int version = DefaultJdkVersion, string? installPath = null,
		CancellationToken cancellationToken = default)
	{
		if (version < MinJdkVersion || version > MaxJdkVersion)
		{
			throw new MauiToolException(
				ErrorCodes.JdkVersionUnsupported,
				$"JDK version {version} is not supported. Supported versions: {MinJdkVersion}-{MaxJdkVersion}");
		}

		var targetPath = installPath ?? PlatformDetector.Paths.DefaultJdkPath;
		Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

		var downloadUrl = GetDownloadUrl(version);
		var tempArchivePath = Path.Combine(Path.GetTempPath(), $"openjdk-{version}.tar.gz");

		try
		{
			// Download
			using var httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromMinutes(10);

			var response = await httpClient.GetAsync(downloadUrl, cancellationToken);
			response.EnsureSuccessStatusCode();

			await using (var fs = File.Create(tempArchivePath))
			{
				await response.Content.CopyToAsync(fs, cancellationToken);
			}

			// Extract
			await ExtractArchiveAsync(tempArchivePath, targetPath, cancellationToken);

			// Update detected path
			DetectedJdkPath = targetPath;
			DetectedJdkVersion = version;
		}
		catch (Exception ex) when (ex is not MauiToolException)
		{
			throw new MauiToolException(
				ErrorCodes.JdkInstallFailed,
				$"Failed to install JDK: {ex.Message}",
				nativeError: ex.Message);
		}
		finally
		{
			if (File.Exists(tempArchivePath))
				File.Delete(tempArchivePath);
		}
	}

	private static string GetDownloadUrl(int version)
	{
		// Use Eclipse Temurin (Adoptium) builds
		var os = PlatformDetector.IsMacOS ? "mac" : PlatformDetector.IsWindows ? "windows" : "linux";
		var arch = PlatformDetector.IsArm64 ? "aarch64" : "x64";
		var ext = PlatformDetector.IsWindows ? "zip" : "tar.gz";

		// Latest LTS versions from Adoptium
		return version switch
		{
			17 => $"https://api.adoptium.net/v3/binary/latest/17/ga/{os}/{arch}/jdk/hotspot/normal/eclipse?project=jdk",
			21 => $"https://api.adoptium.net/v3/binary/latest/21/ga/{os}/{arch}/jdk/hotspot/normal/eclipse?project=jdk",
			_ => throw new MauiToolException(ErrorCodes.JdkVersionUnsupported, $"JDK version {version} is not available for automatic download")
		};
	}

	private async Task ExtractArchiveAsync(string archivePath, string targetPath, CancellationToken cancellationToken)
	{
		// Ensure target directory exists
		if (Directory.Exists(targetPath))
			Directory.Delete(targetPath, recursive: true);
		Directory.CreateDirectory(targetPath);

		if (PlatformDetector.IsWindows)
		{
			// Use PowerShell for zip extraction
			System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, targetPath);
		}
		else
		{
			// Use tar for tar.gz extraction
			var result = await ProcessRunner.RunAsync(
				"tar",
				$"-xzf \"{archivePath}\" -C \"{targetPath}\" --strip-components=1",
				timeout: TimeSpan.FromMinutes(5),
				cancellationToken: cancellationToken);

			if (!result.Success)
			{
				throw new MauiToolException(
					ErrorCodes.JdkInstallFailed,
					$"Failed to extract JDK: {result.StandardError}",
					nativeError: result.StandardError);
			}
		}

		// On macOS, the JDK is inside Contents/Home
		if (PlatformDetector.IsMacOS)
		{
			var contentsHome = Path.Combine(targetPath, "Contents", "Home");
			if (Directory.Exists(contentsHome))
			{
				// Move contents up
				var tempDir = Path.Combine(Path.GetTempPath(), $"jdk-temp-{Guid.NewGuid()}");
				Directory.Move(contentsHome, tempDir);
				Directory.Delete(targetPath, recursive: true);
				Directory.Move(tempDir, targetPath);
			}
		}
	}

	public IEnumerable<int> GetAvailableVersions()
	{
		yield return 17;
		yield return 21;
	}
}
