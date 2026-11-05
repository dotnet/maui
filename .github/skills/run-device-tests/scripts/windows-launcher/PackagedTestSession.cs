using System.Diagnostics;

namespace Microsoft.Maui.TestUtils.WindowsTesting;

internal sealed class PackagedTestState
{
	public string OwnerId { get; set; } = "";
	public string IdentityName { get; set; } = "";
	public string Publisher { get; set; } = "";
	public string CertificateThumbprint { get; set; } = "";
	public string PackageFullName { get; set; } = "";
	public string PackageFamilyName { get; set; } = "";
	public string InstallLocation { get; set; } = "";
	public string ApplicationId { get; set; } = "App";
	public string AppUserModelId { get; set; } = "";
	public int TimeoutSeconds { get; set; } = 3600;
	public int ProcessId { get; set; }
	public long ProcessStartUtcTicks { get; set; }
	public bool ProcessIdentityVerified { get; set; }
	public bool InstallRequested { get; set; }
	public bool ProcessExited { get; set; }
	public int ExitCode { get; set; }
}

internal static class PackagedTestSession
{
	public static void Validate(PackagedTestState state)
	{
		if (!Guid.TryParse(state.OwnerId, out var owner) || string.IsNullOrEmpty(state.IdentityName) ||
			state.Publisher != "CN=MauiTests-" + owner.ToString("N") ||
			!state.PackageFullName.StartsWith(state.IdentityName + "_", StringComparison.Ordinal) ||
			!state.PackageFamilyName.StartsWith(state.IdentityName + "_", StringComparison.Ordinal) ||
			state.AppUserModelId != state.PackageFamilyName + "!" + state.ApplicationId ||
			!Path.IsPathFullyQualified(state.InstallLocation) || state.TimeoutSeconds is < 1 or > 86400)
			throw new InvalidOperationException("Invalid or unowned packaged test session.");
	}

	public static int Run(PackagedTestState state, string[] arguments,
		Func<string, string, Process> activate, Func<int, string> getPackageName,
		Action<PackagedTestState> save, CancellationToken cancellationToken = default)
	{
		Validate(state);
		using var process = activate(state.AppUserModelId, WindowsTestCommandLine.Format(arguments));
		state.ProcessId = process.Id;
		state.ProcessStartUtcTicks = process.StartTime.ToUniversalTime().Ticks;
		state.ProcessIdentityVerified = false;
		state.ProcessExited = false;
		save(state);
		if (getPackageName(process.Id) != state.PackageFullName)
			throw new InvalidOperationException("The activated PID does not have the owned MSIX package identity.");
		state.ProcessIdentityVerified = true;
		save(state);

		try
		{
			var deadline = Stopwatch.StartNew();
			while (!process.WaitForExit(100))
			{
				if (cancellationToken.IsCancellationRequested || deadline.Elapsed.TotalSeconds >= state.TimeoutSeconds)
				{
					process.Kill();
					if (!process.WaitForExit(30000))
						throw new TimeoutException("The recorded activated process could not be stopped.");
					state.ExitCode = cancellationToken.IsCancellationRequested ? 130 : 124;
					state.ProcessExited = true;
					save(state);
					return state.ExitCode;
				}
			}
			state.ExitCode = process.ExitCode;
			state.ProcessExited = true;
			save(state);
			return state.ExitCode;
		}
		finally
		{
			if (!process.HasExited)
			{
				process.Kill();
				if (!process.WaitForExit(30000))
					throw new TimeoutException("The recorded activated process did not stop during cleanup.");
				state.ProcessExited = true;
				save(state);
			}
		}
	}
}
