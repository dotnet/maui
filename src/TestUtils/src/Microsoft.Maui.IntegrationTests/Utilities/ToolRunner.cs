using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace Microsoft.Maui.IntegrationTests
{
	public static class ToolRunner
	{
		public static string Run(string tool, string args, out int exitCode,
			string workingDirectory = "",
			int timeoutInSeconds = 600,
			ITestOutputHelper? output = null,
			bool killOnTimeout = true)
		{
			var info = new ProcessStartInfo(tool, args);

			if (Directory.Exists(workingDirectory))
				info.WorkingDirectory = workingDirectory;

			return Run(info, out exitCode, timeoutInSeconds, output: output, killOnTimeout: killOnTimeout);
		}

		public static string Run(ProcessStartInfo info, out int exitCode,
			int timeoutInSeconds = 600, Action<Process>? inputAction = null, ITestOutputHelper? output = null,
			bool killOnTimeout = true)
		{
			var procOutput = new StringBuilder();
			var stdoutClosed = new TaskCompletionSource();
			var stderrClosed = new TaskCompletionSource();
			using (var p = new Process())
			{
				p.StartInfo = info;
				output?.WriteLine($"[ToolRunner] Running: {p.StartInfo.FileName} {p.StartInfo.Arguments}");
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;
				if (inputAction != null)
				{
					p.StartInfo.RedirectStandardInput = true;
				}
				p.OutputDataReceived += (sender, o) =>
				{
					if (o.Data is null)
						stdoutClosed.TrySetResult();
					else if (o.Data.Length > 0)
					{
						lock (procOutput)
							procOutput.AppendLine(o.Data);
					}
				};
				p.ErrorDataReceived += (sender, e) =>
				{
					if (e.Data is null)
						stderrClosed.TrySetResult();
					else if (e.Data.Length > 0)
					{
						lock (procOutput)
							procOutput.AppendLine(e.Data);
					}
				};

				p.Start();
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();

				if (inputAction != null)
				{
					inputAction(p);
				}

				bool exited = p.WaitForExit(timeoutInSeconds * 1000);
				if (exited)
				{
					exitCode = p.ExitCode;
					output?.WriteLine($"[ToolRunner] Process '{Path.GetFileName(p.StartInfo.FileName)}' exited with code: {exitCode}");
				}
				else
				{
					exitCode = -1;
					WriteLine($"[ToolRunner] Process '{Path.GetFileName(p.StartInfo.FileName)}' (PID {p.Id}) timed out after {timeoutInSeconds} seconds.", output);
					if (killOnTimeout)
					{
						try
						{
							p.Kill(entireProcessTree: true);
						}
						catch (InvalidOperationException) when (p.HasExited)
						{
							// The process exited between the timeout and termination.
						}
						exited = p.WaitForExit(10000);
						if (!exited)
							throw new TimeoutException($"Process '{p.StartInfo.FileName}' (PID {p.Id}) did not exit after termination.");
					}
				}

				// Descendants may retain redirected pipes after the parent exits.
				if (exited && !Task.WhenAll(stdoutClosed.Task, stderrClosed.Task).Wait(TimeSpan.FromSeconds(10)))
				{
					WriteLine($"[ToolRunner] Output capture for process '{p.StartInfo.FileName}' (PID {p.Id}) did not finish within 10 seconds.", output);
				}

				lock (procOutput)
					return procOutput.ToString();
			}
		}

		static void WriteLine(string message, ITestOutputHelper? output)
		{
			if (output is null)
				Console.WriteLine(message);
			else
				output.WriteLine(message);
		}
	}
}
