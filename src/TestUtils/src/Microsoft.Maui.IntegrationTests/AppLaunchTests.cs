using System.Diagnostics;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class AppLaunchTests : IDisposable
{
	readonly string _logDir = Path.Combine(TestEnvironment.GetLogDirectory(), $"launch-tool-{Guid.NewGuid():N}");

	public AppLaunchTests() => Directory.CreateDirectory(_logDir);

	[Fact]
	public void CapturesBothOutputStreamsAndExitCode()
	{
		var log = Path.Combine(_logDir, "tool.log");
		var args = OperatingSystem.IsWindows()
			? new[] { "/c", "echo stdout & echo stderr >&2 & exit /b 7" }
			: new[] { "-c", "echo stdout; echo stderr >&2; exit 7" };
		var output = AppLaunch.Run(OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
			args, log, TestContext.Current.CancellationToken, null, out var exitCode);

		Assert.Equal(7, exitCode);
		Assert.Contains("stdout", output, StringComparison.Ordinal);
		Assert.Contains("stderr", output, StringComparison.Ordinal);
		Assert.Equal(output, File.ReadAllText(log));
	}

	[Fact]
	public void CancellationKillsToolAndPreservesPartialLogs()
	{
		var log = Path.Combine(_logDir, "canceled.log");
		using var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(2));
		var args = OperatingSystem.IsWindows()
			? new[] { "-NoProfile", "-NonInteractive", "-Command", "[Console]::WriteLine($PID); [Console]::WriteLine('partial'); Start-Sleep 60" }
			: new[] { "-c", "echo $$; echo partial; sleep 60; echo unexpected" };
		Assert.ThrowsAny<OperationCanceledException>(() => AppLaunch.Run(
			OperatingSystem.IsWindows() ? "powershell.exe" : "/bin/sh", args, log, cancel.Token, null, out _));

		var lines = File.ReadAllLines(log);
		Assert.Contains("partial", lines);
		var pid = int.Parse(lines[0], System.Globalization.CultureInfo.InvariantCulture);
		try
		{
			using var process = Process.GetProcessById(pid);
			Assert.True(process.HasExited, $"Launch tool PID {pid} survived cancellation.");
		}
		catch (ArgumentException)
		{
			// A fully reaped process no longer has a PID entry.
		}
	}

	public void Dispose() => Directory.Delete(_logDir, recursive: true);
}
