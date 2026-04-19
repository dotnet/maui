using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Maui.Windows.Run.Interop;

namespace Microsoft.Maui.Windows.Run;

/// <summary>
/// Activates an MSIX app by its Application User Model ID (AUMID) using COM,
/// then waits for the process to exit. Handles Ctrl+C gracefully.
/// </summary>
internal static class AppActivator
{
	/// <summary>
	/// Activates the app and waits for it to exit.
	/// </summary>
	/// <param name="aumid">Application User Model ID (PackageFamilyName!AppId)</param>
	/// <param name="packageFullName">Optional package full name for TerminateAllProcesses on Ctrl+C</param>
	/// <param name="cancellationToken">Cancellation token (triggered by Ctrl+C)</param>
	/// <returns>The app's exit code, or 1 on failure.</returns>
	public static async Task<int> ActivateAndWaitAsync(string aumid, string? packageFullName, CancellationToken cancellationToken)
	{
		Console.WriteLine($"Launching: {aumid}");

		var activationManager = (IApplicationActivationManager)new ApplicationActivationManager();

		var hr = activationManager.ActivateApplication(
			aumid,
			null,
			(uint)(ActivateOptions.NoErrorUI | ActivateOptions.NoSplashScreen),
			out var processId);

		if (hr < 0)
		{
			Marshal.ThrowExceptionForHR(hr);
		}

		Console.WriteLine($"App launched with PID: {processId}");

		// Register Ctrl+C handler BEFORE acquiring the process to avoid a race
		// where cancellation fires before the handler is set up.
		Process? process = null;
		cancellationToken.Register(() =>
		{
			Console.WriteLine("Terminating app...");
			TryTerminatePackage(packageFullName, process);
		});

		try
		{
			process = Process.GetProcessById((int)processId);
		}
		catch (ArgumentException)
		{
			Console.WriteLine("App process exited immediately.");
			return 0;
		}

		try
		{
			await process.WaitForExitAsync(cancellationToken);
			var exitCode = process.ExitCode;
			Console.WriteLine($"App exited with code: {exitCode}");
			return exitCode;
		}
		catch (OperationCanceledException)
		{
			// Ctrl+C was pressed — ensure termination in case handler ran before process was set
			TryTerminatePackage(packageFullName, process);
			return 0;
		}
		finally
		{
			process.Dispose();
		}
	}

	static void TryTerminatePackage(string? packageFullName, Process? fallbackProcess)
	{
		if (packageFullName is not null)
		{
			try
			{
				var debugSettings = (IPackageDebugSettings)new PackageDebugSettings();
				debugSettings.TerminateAllProcesses(packageFullName);
				return;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"Warning: Failed to terminate package processes: {ex.Message}");
			}
		}

		// Fallback: kill the specific process
		try
		{
			if (fallbackProcess is not null && !fallbackProcess.HasExited)
			{
				fallbackProcess.Kill(entireProcessTree: true);
			}
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"Warning: Failed to kill process: {ex.Message}");
		}
	}
}
