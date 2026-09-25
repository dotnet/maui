using System.Diagnostics;
using System.Text;
using Xunit;

namespace Microsoft.Maui.UiEvidence;

public sealed class UiEvidenceProcessTests : IDisposable
{
	readonly string _root = Path.Combine(Path.GetTempPath(), "ui-evidence-process-" + Guid.NewGuid().ToString("N"));

	public UiEvidenceProcessTests() => Directory.CreateDirectory(_root);

	[Fact]
	public async Task RunAsync_DrainsBothFullPipesAndPreservesExitCode()
	{
		var result = await Run(
			"for ($i=0; $i -lt 4096; $i++) { [Console]::Error.WriteLine(('e' * 128)) }; " +
			"for ($i=0; $i -lt 4096; $i++) { [Console]::Out.WriteLine(('o' * 128)) }; exit 7");

		Assert.Equal(7, result.ExitCode);
		Assert.True(result.Output.Length > 524288);
		Assert.True(result.Error.Length > 524288);
	}

	[Fact]
	public async Task RunAsync_OutputReadFaultTerminatesOwnedProcessAndPreservesError()
	{
		var pidPath = Path.Combine(_root, "fault.pid");
		var command = $"[IO.File]::WriteAllText('{Escape(pidPath)}', [string]$PID); " +
			"$output = [Console]::OpenStandardOutput(); $output.WriteByte(255); $output.Flush(); Start-Sleep -Seconds 60";
		var startInfo = new ProcessStartInfo("pwsh")
		{
			StandardOutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
		};
		foreach (var argument in new[] { "-NoProfile", "-NonInteractive", "-EncodedCommand", Convert.ToBase64String(Encoding.Unicode.GetBytes(command)) })
			startInfo.ArgumentList.Add(argument);
		var watch = Stopwatch.StartNew();
		try
		{
			await Assert.ThrowsAsync<DecoderFallbackException>(() =>
				UiEvidenceProcess.RunAsync(startInfo, TimeSpan.FromSeconds(10)));
			Assert.True(watch.Elapsed < TimeSpan.FromSeconds(9), "A read fault must not wait for the process deadline.");
			Assert.True(HasExited(int.Parse(File.ReadAllText(pidPath))));
		}
		finally
		{
			StopTestProcess(pidPath);
		}
	}

	[Fact]
	public async Task RunAsync_QuietProcessTimeoutTerminatesOwnedProcess()
	{
		var pidPath = Path.Combine(_root, "timeout.pid");
		var watch = Stopwatch.StartNew();
		try
		{
			await Assert.ThrowsAsync<TimeoutException>(() => Run(
				$"[IO.File]::WriteAllText('{Escape(pidPath)}', [string]$PID); Start-Sleep -Seconds 60",
				TimeSpan.FromSeconds(3)));
			Assert.InRange(watch.Elapsed, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(15));
			Assert.True(HasExited(int.Parse(File.ReadAllText(pidPath))));
		}
		finally
		{
			StopTestProcess(pidPath);
		}
	}

	[Fact]
	public async Task RunAsync_CancellationTerminatesOwnedProcess()
	{
		var pidPath = Path.Combine(_root, "cancel.pid");
		using var cancellation = new CancellationTokenSource();
		var task = Run($"[IO.File]::WriteAllText('{Escape(pidPath)}', [string]$PID); Start-Sleep -Seconds 60",
			cancellationToken: cancellation.Token);
		try
		{
			Assert.True(SpinWait.SpinUntil(() => File.Exists(pidPath), TimeSpan.FromSeconds(10)));
			cancellation.Cancel();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
			Assert.True(HasExited(int.Parse(File.ReadAllText(pidPath))));
		}
		finally
		{
			cancellation.Cancel();
			StopTestProcess(pidPath);
		}
	}

	[Fact]
	public async Task RunAsync_ChildRetainingPipesCannotBypassDeadline()
	{
		var pidPath = Path.Combine(_root, "child.pid");
		var watch = Stopwatch.StartNew();
		try
		{
			await Assert.ThrowsAsync<TimeoutException>(() => Run(
				"$child = Start-Process pwsh -ArgumentList '-NoProfile -NonInteractive -Command Start-Sleep -Seconds 60' -NoNewWindow -PassThru; " +
				$"[IO.File]::WriteAllText('{Escape(pidPath)}', [string]$child.Id)",
				TimeSpan.FromSeconds(5)));
			Assert.InRange(watch.Elapsed, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(18));
		}
		finally
		{
			StopTestProcess(pidPath);
		}
	}

	static Task<(int ExitCode, string Output, string Error)> Run(
		string command, TimeSpan? timeout = null, CancellationToken cancellationToken = default) =>
		UiEvidenceProcess.RunAsync("pwsh",
			["-NoProfile", "-NonInteractive", "-EncodedCommand", Convert.ToBase64String(Encoding.Unicode.GetBytes(command))],
			timeout, cancellationToken);

	static string Escape(string path) => path.Replace("'", "''", StringComparison.Ordinal);

	static bool HasExited(int id)
	{
		try
		{
			using var process = Process.GetProcessById(id);
			return process.HasExited;
		}
		catch (ArgumentException)
		{
			return true;
		}
	}

	static void StopTestProcess(string pidPath)
	{
		if (!File.Exists(pidPath) || !int.TryParse(File.ReadAllText(pidPath), out var id) || HasExited(id))
			return;
		using var process = Process.GetProcessById(id);
		process.Kill(entireProcessTree: true);
		Assert.True(process.WaitForExit(10000));
	}

	public void Dispose() => Directory.Delete(_root, recursive: true);
}
