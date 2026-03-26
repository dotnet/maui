// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Errors;
using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Providers.Android;
using Microsoft.Maui.Client.Utils;

namespace Microsoft.Maui.Client.Services;

/// <summary>
/// Doctor service that performs system health checks.
/// </summary>
public class DoctorService : IDoctorService
{
	readonly IAndroidProvider? _androidProvider;

	public DoctorService(IAndroidProvider? androidProvider = null)
	{
		_androidProvider = androidProvider;
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
					$"Unknown category: {category}. Valid categories: dotnet, android, windows");
		}

		return CreateReport(checks);
	}

	static DoctorReport CreateReport(List<HealthCheck> checks)
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

		if (fix.Command == null)
			return false;

		try
		{
			var (fileName, args) = ParseCommand(fix.Command);
			var result = await ProcessRunner.RunAsync(fileName, args, cancellationToken: cancellationToken);
			return result.ExitCode == 0;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Trace.WriteLine($"Auto-fix '{fix.Command}' failed: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	/// Splits a command string into executable and arguments array.
	/// Handles quoted executables (e.g., "C:\Program Files\tool.exe" --flag).
	/// </summary>
	internal static (string fileName, string[] args) ParseCommand(string command)
	{
		var trimmed = command.Trim();

		if (trimmed.StartsWith('"'))
		{
			var endQuote = trimmed.IndexOf('"', 1);
			if (endQuote > 0)
			{
				var fileName = trimmed[1..endQuote];
				var rest = trimmed[(endQuote + 1)..].Trim();
				var args = string.IsNullOrEmpty(rest) ? [] : rest.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				return (fileName, args);
			}
		}

		var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 0)
			return (trimmed, []);

		return (parts[0], parts[1..]);
	}

	async Task<HealthCheck> CheckDotNetSdkAsync(CancellationToken cancellationToken)
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

		var result = await ProcessRunner.RunAsync("dotnet", ["--version"],
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

	async Task<HealthCheck> CheckMauiWorkloadAsync(CancellationToken cancellationToken)
	{
		var result = await ProcessRunner.RunAsync("dotnet", ["workload", "list"],
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

	async Task<HealthCheck> CheckWindowsSdkAsync(CancellationToken cancellationToken)
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
