// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Errors;
using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Providers.Android;
using Microsoft.Maui.DevTools.Providers.Apple;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Services;

/// <summary>
/// Doctor service that performs system health checks.
/// </summary>
public class DoctorService : IDoctorService
{
	private readonly IAndroidProvider? _androidProvider;
	private readonly IAppleProvider? _appleProvider;

	public DoctorService(IAndroidProvider? androidProvider = null, IAppleProvider? appleProvider = null)
	{
		_androidProvider = androidProvider;
		_appleProvider = appleProvider;
	}

	public async Task<DoctorReport> RunAllChecksAsync(CancellationToken cancellationToken = default)
	{
		var checks = new List<HealthCheck>();

		// .NET SDK check
		checks.Add(await CheckDotNetSdkAsync(cancellationToken));

		// MAUI workload check
		checks.Add(await CheckMauiWorkloadAsync(cancellationToken));

		// Android checks (if provider available)
		if (_androidProvider != null)
		{
			var androidChecks = await _androidProvider.CheckHealthAsync(cancellationToken);
			checks.AddRange(androidChecks);
		}

		// Apple checks (macOS only) - use provider if available
		if (PlatformDetector.IsMacOS)
		{
			if (_appleProvider != null)
			{
				var appleChecks = await _appleProvider.CheckHealthAsync(cancellationToken);
				checks.AddRange(appleChecks);
			}
			else
			{
				checks.Add(await CheckXcodeAsync(cancellationToken));
			}
		}

		// Windows checks (Windows only)
		if (PlatformDetector.IsWindows)
		{
			checks.Add(await CheckWindowsSdkAsync(cancellationToken));
		}

		return CreateReport(checks);
	}

	public async Task<DoctorReport> RunCategoryChecksAsync(string category, CancellationToken cancellationToken = default)
	{
		var checks = new List<HealthCheck>();

		switch (category.ToLowerInvariant())
		{
			case "dotnet":
				checks.Add(await CheckDotNetSdkAsync(cancellationToken));
				checks.Add(await CheckMauiWorkloadAsync(cancellationToken));
				break;

			case "android":
				if (_androidProvider != null)
				{
					var androidChecks = await _androidProvider.CheckHealthAsync(cancellationToken);
					checks.AddRange(androidChecks);
				}
				break;

			case "apple":
			case "ios":
				if (PlatformDetector.IsMacOS)
				{
					checks.Add(await CheckXcodeAsync(cancellationToken));
				}
				else
				{
					checks.Add(new HealthCheck
					{
						Category = "apple",
						Name = "Platform",
						Status = CheckStatus.Skipped,
						Message = "Apple checks only available on macOS"
					});
				}
				break;

			case "windows":
				if (PlatformDetector.IsWindows)
				{
					checks.Add(await CheckWindowsSdkAsync(cancellationToken));
				}
				else
				{
					checks.Add(new HealthCheck
					{
						Category = "windows",
						Name = "Platform",
						Status = CheckStatus.Skipped,
						Message = "Windows checks only available on Windows"
					});
				}
				break;

			default:
				throw new MauiToolException(
					ErrorCodes.InvalidArgument,
					$"Unknown category: {category}. Valid categories: dotnet, android, apple, windows");
		}

		return CreateReport(checks);
	}

	public Task<DoctorReport> RunChecksByCategoryAsync(string category, CancellationToken cancellationToken = default)
	{
		// Alias for RunCategoryChecksAsync for compatibility
		return RunCategoryChecksAsync(category, cancellationToken);
	}

	private static DoctorReport CreateReport(List<HealthCheck> checks)
	{
		var errorCount = checks.Count(c => c.Status == CheckStatus.Error);
		var warningCount = checks.Count(c => c.Status == CheckStatus.Warning);
		var okCount = checks.Count(c => c.Status == CheckStatus.Ok);

		var status = errorCount > 0 ? HealthStatus.Unhealthy :
					 warningCount > 0 ? HealthStatus.Degraded : HealthStatus.Healthy;

		return new DoctorReport
		{
			CorrelationId = Guid.NewGuid().ToString("N")[..8],
			Timestamp = DateTime.UtcNow,
			Status = status,
			Checks = checks,
			Summary = new DoctorSummary
			{
				Total = checks.Count,
				Ok = okCount,
				Warning = warningCount,
				Error = errorCount
			}
		};
	}

	public async Task<bool> TryFixAsync(FixInfo fix, CancellationToken cancellationToken = default)
	{
		if (!fix.AutoFixable)
			return false;

		// Parse the command and execute it
		if (fix.Command != null)
		{
			// For now, we just report what would need to be run
			// In a full implementation, this would execute the fix
			Console.WriteLine($"Would run: {fix.Command}");
			return false;
		}

		return false;
	}

	private async Task<HealthCheck> CheckDotNetSdkAsync(CancellationToken cancellationToken)
	{
		var dotnetPath = ProcessRunner.GetCommandPath("dotnet");
		if (dotnetPath == null)
		{
			return new HealthCheck
			{
				Category = "dotnet",
				Name = ".NET SDK",
				Status = CheckStatus.Error,
				Message = ".NET SDK not found in PATH",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.DotNetNotFound,
					Description = "Install .NET SDK from https://dot.net",
					AutoFixable = false,
					ManualSteps = new[] { "Download and install .NET SDK from https://dot.net/download" }
				}
			};
		}

		var result = await ProcessRunner.RunAsync("dotnet", "--version", 
			timeout: TimeSpan.FromSeconds(10), cancellationToken: cancellationToken);

		if (!result.Success)
		{
			return new HealthCheck
			{
				Category = "dotnet",
				Name = ".NET SDK",
				Status = CheckStatus.Error,
				Message = "Failed to get .NET SDK version"
			};
		}

		var version = result.StandardOutput.Trim();

		return new HealthCheck
		{
			Category = "dotnet",
			Name = ".NET SDK",
			Status = CheckStatus.Ok,
			Message = $".NET {version}",
			Details = new Dictionary<string, object>
			{
				["version"] = version,
				["path"] = dotnetPath
			}
		};
	}

	private async Task<HealthCheck> CheckMauiWorkloadAsync(CancellationToken cancellationToken)
	{
		var result = await ProcessRunner.RunAsync("dotnet", "workload list",
			timeout: TimeSpan.FromSeconds(30), cancellationToken: cancellationToken);

		if (!result.Success)
		{
			return new HealthCheck
			{
				Category = "dotnet",
				Name = "MAUI Workload",
				Status = CheckStatus.Error,
				Message = "Failed to list workloads"
			};
		}

		// Check if maui workload is installed
		var hasMaui = result.StandardOutput.Contains("maui", StringComparison.OrdinalIgnoreCase);

		if (!hasMaui)
		{
			return new HealthCheck
			{
				Category = "dotnet",
				Name = "MAUI Workload",
				Status = CheckStatus.Error,
				Message = "MAUI workload not installed",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.MauiWorkloadMissing,
					Description = "Install MAUI workload",
					AutoFixable = true,
					Command = "dotnet workload install maui"
				}
			};
		}

		return new HealthCheck
		{
			Category = "dotnet",
			Name = "MAUI Workload",
			Status = CheckStatus.Ok,
			Message = "MAUI workload installed"
		};
	}

	private async Task<HealthCheck> CheckXcodeAsync(CancellationToken cancellationToken)
	{
		var result = await ProcessRunner.RunAsync("xcode-select", "-p",
			timeout: TimeSpan.FromSeconds(10), cancellationToken: cancellationToken);

		if (!result.Success)
		{
			return new HealthCheck
			{
				Category = "apple",
				Name = "Xcode",
				Status = CheckStatus.Error,
				Message = "Xcode command line tools not installed",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.XcodeNotFound,
					Description = "Install Xcode from the App Store",
					AutoFixable = false,
					ManualSteps = new[]
					{
						"Install Xcode from the Mac App Store",
						"Run: xcode-select --install"
					}
				}
			};
		}

		var xcodePath = result.StandardOutput.Trim();

		// Get Xcode version
		var versionResult = await ProcessRunner.RunAsync("xcodebuild", "-version",
			timeout: TimeSpan.FromSeconds(10), cancellationToken: cancellationToken);

		var version = "unknown";
		if (versionResult.Success)
		{
			var lines = versionResult.StandardOutput.Split('\n');
			if (lines.Length > 0)
				version = lines[0].Replace("Xcode ", string.Empty, StringComparison.Ordinal).Trim();
		}

		return new HealthCheck
		{
			Category = "apple",
			Name = "Xcode",
			Status = CheckStatus.Ok,
			Message = $"Xcode {version}",
			Details = new Dictionary<string, object>
			{
				["version"] = version,
				["path"] = xcodePath
			}
		};
	}

	private async Task<HealthCheck> CheckWindowsSdkAsync(CancellationToken cancellationToken)
	{
		// Check for Windows SDK in common locations
		var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
		var sdkPath = Path.Combine(programFiles, "Windows Kits", "10");

		if (!Directory.Exists(sdkPath))
		{
			return new HealthCheck
			{
				Category = "windows",
				Name = "Windows SDK",
				Status = CheckStatus.Error,
				Message = "Windows SDK not found",
				Fix = new FixInfo
				{
					IssueId = ErrorCodes.WindowsSdkNotFound,
					Description = "Install Windows SDK",
					AutoFixable = false,
					ManualSteps = new[]
					{
						"Install Windows SDK from Visual Studio Installer or",
						"Download from https://developer.microsoft.com/windows/downloads/windows-sdk/"
					}
				}
			};
		}

		// Try to find installed versions
		var includePath = Path.Combine(sdkPath, "Include");
		string? installedVersion = null;
		if (Directory.Exists(includePath))
		{
			var versions = Directory.GetDirectories(includePath)
				.Select(Path.GetFileName)
				.Where(v => v?.StartsWith("10.") ?? false)
				.OrderByDescending(v => v)
				.FirstOrDefault();
			installedVersion = versions;
		}

		return new HealthCheck
		{
			Category = "windows",
			Name = "Windows SDK",
			Status = CheckStatus.Ok,
			Message = installedVersion != null ? $"Windows SDK {installedVersion}" : "Windows SDK found",
			Details = new Dictionary<string, object>
			{
				["path"] = sdkPath,
				["version"] = installedVersion ?? "unknown"
			}
		};
	}
}
