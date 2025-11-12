using System.Diagnostics;
using System.Text;

namespace Microsoft.Maui.AppleIntegrationTests
{
	public static class ToolRunner
	{
		public static string Run(string tool, string args, out int exitCode,
			string workingDirectory = "",
			int timeoutInSeconds = 600)
		{
			var info = new ProcessStartInfo(tool, args);

			if (Directory.Exists(workingDirectory))
				info.WorkingDirectory = workingDirectory;

			return Run(info, out exitCode, timeoutInSeconds);
		}

		public static string Run(ProcessStartInfo info, out int exitCode,
			int timeoutInSeconds = 600, Action<Process>? inputAction = null)
		{
			var procOutput = new StringBuilder();
			using (var p = new Process())
			{
			p.StartInfo = info;
			TestOutput.WriteLine($"[ToolRunner] Running: {p.StartInfo.FileName} {p.StartInfo.Arguments}");
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
					if (!string.IsNullOrEmpty(o?.Data))
					{
						lock (procOutput)
							procOutput.AppendLine(o.Data);
					}
				};
				p.ErrorDataReceived += (sender, e) =>
				{
					if (!string.IsNullOrEmpty(e?.Data))
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

				if (p.WaitForExit(timeoutInSeconds * 1000))
				{
					exitCode = p.ExitCode;
					TestOutput.WriteLine($"[ToolRunner] Process '{Path.GetFileName(p.StartInfo.FileName)}' exited with code: {exitCode}");
				}
				else
				{
					exitCode = -1;
				}
			}

			return procOutput.ToString();
		}

	}
}
