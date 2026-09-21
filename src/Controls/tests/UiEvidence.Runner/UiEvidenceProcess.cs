using System.Diagnostics;

namespace Microsoft.Maui.UiEvidence;

static class UiEvidenceProcess
{
	public static async Task<(int ExitCode, string Output, string Error)> RunAsync(
		string fileName,
		IEnumerable<string> arguments,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var duration = timeout ?? TimeSpan.FromSeconds(30);
		using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		deadline.CancelAfter(duration);
		using var process = new Process
		{
			StartInfo = new ProcessStartInfo(fileName)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};
		foreach (var argument in arguments)
			process.StartInfo.ArgumentList.Add(argument);

		cancellationToken.ThrowIfCancellationRequested();
		process.Start();
		var output = process.StandardOutput.ReadToEndAsync(deadline.Token);
		var error = process.StandardError.ReadToEndAsync(deadline.Token);
		try
		{
			await Task.WhenAll(output, error, process.WaitForExitAsync(deadline.Token))
				.WaitAsync(deadline.Token).ConfigureAwait(false);
			return (process.ExitCode, await output.ConfigureAwait(false), await error.ConfigureAwait(false));
		}
		catch (OperationCanceledException ex) when (deadline.IsCancellationRequested)
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
			cancellationToken.ThrowIfCancellationRequested();
			throw new TimeoutException($"{fileName} did not complete within {duration.TotalSeconds} seconds.", ex);
		}
	}
}
