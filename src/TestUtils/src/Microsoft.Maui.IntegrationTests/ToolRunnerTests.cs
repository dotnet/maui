using System.Diagnostics;

namespace Microsoft.Maui.IntegrationTests;

[CollectionDefinition(nameof(ConsoleOutputCollection), DisableParallelization = true)]
public class ConsoleOutputCollection
{
}

[Collection(nameof(ConsoleOutputCollection))]
[Trait("Category", "Build")]
public class ToolRunnerTests
{
	readonly ITestOutputHelper _output;

	public ToolRunnerTests(ITestOutputHelper output) => _output = output;

	[Fact]
	public void CapturesOutputAndExitCode()
	{
		var info = CreateShell("echo stdout; echo stderr >&2; exit 7", "echo stdout & echo stderr >&2 & exit /b 7");
		var output = ToolRunner.Run(info, out var exitCode, output: _output);

		Assert.Equal(7, exitCode);
		Assert.Contains("stdout", output, StringComparison.Ordinal);
		Assert.Contains("stderr", output, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData(null)]
	[InlineData(true)]
	[InlineData(false)]
	public void TimeoutHonorsProcessLifetime(bool? killOnTimeout)
	{
		var info = CreateShell("sleep 60", "ping -n 60 127.0.0.1 > nul");
		int processId = 0;
		try
		{
			int exitCode;
			if (killOnTimeout is bool kill)
				ToolRunner.Run(info, out exitCode, timeoutInSeconds: 1,
					inputAction: process => processId = process.Id, output: _output, killOnTimeout: kill);
			else
				ToolRunner.Run(info, out exitCode, timeoutInSeconds: 1,
					inputAction: process => processId = process.Id, output: _output);

			Assert.Equal(-1, exitCode);
			Assert.Equal(killOnTimeout ?? true, HasExited(processId));
		}
		finally
		{
			TerminateProcess(processId);
		}
	}

	[Fact]
	public void TimeoutTerminatesChildProcesses()
	{
		var info = CreateShell("sleep 60 & child=$!; echo $child; wait \"$child\"", "");
		if (OperatingSystem.IsWindows())
		{
			info = new ProcessStartInfo("powershell.exe");
			info.ArgumentList.Add("-NoProfile");
			info.ArgumentList.Add("-NonInteractive");
			info.ArgumentList.Add("-Command");
			info.ArgumentList.Add("$child = Start-Process -FilePath $env:ComSpec -ArgumentList '/c ping -n 60 127.0.0.1 > nul' -NoNewWindow -PassThru; [Console]::WriteLine($child.Id); $child.WaitForExit()");
		}

		int parentId = 0;
		int childId = 0;
		try
		{
			var output = ToolRunner.Run(info, out var exitCode, timeoutInSeconds: 5,
				inputAction: process => parentId = process.Id, output: _output);

			Assert.True(int.TryParse(output.Trim(), out childId), $"Child process ID was not captured: {output}");
			Assert.Equal(-1, exitCode);
			Assert.True(HasExited(parentId), "Parent process survived the timeout.");
			Assert.True(SpinWait.SpinUntil(() => HasExited(childId), TimeSpan.FromSeconds(10)),
				"Child process survived the timeout.");
		}
		finally
		{
			TerminateProcess(parentId);
			TerminateProcess(childId);
		}
	}

	[Fact]
	public void TimeoutReportsDiagnosticsWithoutOutputHelper()
	{
		using var console = new ConsoleCapture();
		var info = CreateShell("sleep 60", "ping -n 60 127.0.0.1 > nul");
		int processId = 0;
		try
		{
			ToolRunner.Run(info, out var exitCode, timeoutInSeconds: 1,
				inputAction: process => processId = process.Id);

			Assert.Equal(-1, exitCode);
			Assert.Contains($"(PID {processId}) timed out after 1 seconds.", console.Text, StringComparison.Ordinal);
		}
		finally
		{
			TerminateProcess(processId);
		}
	}

	[Fact]
	public void BoundsOutputDrainWhenChildRetainsPipes()
	{
		using var console = new ConsoleCapture();
		var info = CreateShell("sleep 60 & echo $!; exit 0", "");
		if (OperatingSystem.IsWindows())
		{
			info = new ProcessStartInfo("powershell.exe");
			info.ArgumentList.Add("-NoProfile");
			info.ArgumentList.Add("-NonInteractive");
			info.ArgumentList.Add("-Command");
			info.ArgumentList.Add("$child = Start-Process -FilePath $env:ComSpec -ArgumentList '/c ping -n 60 127.0.0.1 > nul' -NoNewWindow -PassThru; [Console]::WriteLine($child.Id)");
		}

		int parentId = 0;
		int childId = 0;
		try
		{
			var watch = Stopwatch.StartNew();
			var output = ToolRunner.Run(info, out var exitCode, timeoutInSeconds: 10,
				inputAction: process => parentId = process.Id);
			watch.Stop();

			Assert.True(int.TryParse(output.Trim(), out childId), $"Child process ID was not captured: {output}");
			Assert.Equal(0, exitCode);
			Assert.True(HasExited(parentId), "The parent must exit normally before output draining.");
			Assert.False(HasExited(childId), "The child must still retain the redirected pipes.");
			Assert.InRange(watch.Elapsed, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
			Assert.Contains($"(PID {parentId}) did not finish within 10 seconds.", console.Text, StringComparison.Ordinal);
		}
		finally
		{
			TerminateProcess(parentId);
			TerminateProcess(childId);
		}
	}

	static void TerminateProcess(int processId)
	{
		if (processId != 0 && !HasExited(processId))
		{
			using var process = Process.GetProcessById(processId);
			process.Kill(entireProcessTree: true);
			Assert.True(process.WaitForExit(10000), "Test process did not terminate.");
		}
	}

	static ProcessStartInfo CreateShell(string unixCommand, string windowsCommand)
	{
		var info = new ProcessStartInfo(OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh");
		info.ArgumentList.Add(OperatingSystem.IsWindows() ? "/c" : "-c");
		info.ArgumentList.Add(OperatingSystem.IsWindows() ? windowsCommand : unixCommand);
		return info;
	}

	static bool HasExited(int processId)
	{
		try
		{
			using var process = Process.GetProcessById(processId);
			return process.HasExited;
		}
		catch (ArgumentException)
		{
			return true;
		}
	}

	sealed class ConsoleCapture : IDisposable
	{
		readonly TextWriter _original = Console.Out;
		readonly StringWriter _output = new();

		public ConsoleCapture() => Console.SetOut(_output);

		public string Text => _output.ToString();

		public void Dispose()
		{
			Console.SetOut(_original);
			_output.Dispose();
		}
	}
}
