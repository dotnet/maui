using System.Diagnostics;

namespace Microsoft.Maui.UiEvidence;

static class UiEvidenceProcess
{
	public static Task<(int ExitCode, string Output, string Error)> RunAsync(
		string fileName,
		IEnumerable<string> arguments,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var startInfo = new ProcessStartInfo(fileName);
		foreach (var argument in arguments)
			startInfo.ArgumentList.Add(argument);
		return RunAsync(startInfo, timeout, cancellationToken);
	}

	internal static async Task<(int ExitCode, string Output, string Error)> RunAsync(
		ProcessStartInfo startInfo,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		startInfo.RedirectStandardOutput = true;
		startInfo.RedirectStandardError = true;
		startInfo.UseShellExecute = false;
		startInfo.CreateNoWindow = true;
		var duration = timeout ?? TimeSpan.FromSeconds(30);
		using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		deadline.CancelAfter(duration);
		using var process = new Process { StartInfo = startInfo };
		var started = false;
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			process.Start();
			started = true;
			var output = process.StandardOutput.ReadToEndAsync(deadline.Token);
			var error = process.StandardError.ReadToEndAsync(deadline.Token);
			var pending = new List<Task> { output, error, process.WaitForExitAsync(deadline.Token) };
			while (pending.Count != 0)
			{
				var completed = await Task.WhenAny(pending).WaitAsync(deadline.Token).ConfigureAwait(false);
				await completed.ConfigureAwait(false);
				pending.Remove(completed);
			}
			return (process.ExitCode, await output.ConfigureAwait(false), await error.ConfigureAwait(false));
		}
		catch (Exception ex)
		{
			var cancelled = ex is OperationCanceledException && deadline.IsCancellationRequested;
			try
			{
				await deadline.CancelAsync().ConfigureAwait(false);
				if (started)
				{
					try
					{
						if (!process.HasExited)
							process.Kill(entireProcessTree: true);
					}
					catch (InvalidOperationException) when (process.HasExited)
					{
						// The owned process can exit between the check and termination.
					}
					await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
				}
			}
			catch (Exception cleanupError)
			{
				ex.Data["UiEvidenceProcessCleanupError"] = cleanupError;
				Console.Error.WriteLine($"UI evidence process cleanup failed: {cleanupError.Message}");
			}
			if (cancelled)
			{
				cancellationToken.ThrowIfCancellationRequested();
				throw new TimeoutException($"{startInfo.FileName} did not complete within {duration.TotalSeconds} seconds.", ex);
			}
			throw;
		}
	}
}
