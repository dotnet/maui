using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Maui.IntegrationTests.Android;
using Xunit;

namespace Microsoft.Maui.IntegrationTests;

public static class AppLaunch
{
	public static bool RunAndroid(string packageName, string resultDir, string deviceId, string completionMarker,
		int timeoutSeconds = 120, ITestOutputHelper? output = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(completionMarker);
		Directory.CreateDirectory(resultDir);
		using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
		try
		{
			var text = Run(Adb.ToolPath,
				["-s", deviceId, "shell", "am", "instrument", "-w", $"{packageName}/com.microsoft.mauitemplate.Launch"],
				Path.Combine(resultDir, "instrumentation.log"), timeout.Token, output, out var exitCode);
			return AndroidRunCompleted(exitCode, text, completionMarker);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			output?.WriteLine($"Android smoke test timed out after {timeoutSeconds} seconds.");
			return false;
		}
		finally
		{
			Adb.Run($"shell am force-stop {packageName}", deviceId: deviceId, output: output);
			var logs = Adb.RunForOutput("logcat -d -t 2000", out _, deviceId: deviceId, output: output);
			File.WriteAllText(Path.Combine(resultDir, "device.log"), logs);
		}
	}

	internal static bool AndroidRunCompleted(int exitCode, string text, string completionMarker)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(completionMarker);
		return exitCode == 0 &&
			Regex.IsMatch(text, @"^INSTRUMENTATION_CODE: -1\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant) &&
			Regex.IsMatch(text, @"^INSTRUMENTATION_RESULT: completion-marker=" + Regex.Escape(completionMarker) + @"\s*$",
				RegexOptions.Multiline | RegexOptions.CultureInvariant) &&
			!Regex.IsMatch(text, @"^INSTRUMENTATION_(?:FAILED|ABORTED):", RegexOptions.Multiline | RegexOptions.CultureInvariant);
	}

	public static bool RunApple(string appPath, string resultDir, string deviceUdid, string completionMarker,
		int timeoutSeconds = 300, ITestOutputHelper? output = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(completionMarker);
		Directory.CreateDirectory(resultDir);
		using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
		string? bundleId = null;
		try
		{
			bundleId = Run("/usr/libexec/PlistBuddy", ["-c", "Print :CFBundleIdentifier", Path.Combine(appPath, "Info.plist")],
				Path.Combine(resultDir, "bundle-id.log"), timeout.Token, output, out var exitCode).Trim();
			if (exitCode != 0 || !Regex.IsMatch(bundleId, @"^[A-Za-z0-9._-]+$", RegexOptions.CultureInvariant))
				return false;

			Run("xcrun", ["simctl", "install", deviceUdid, appPath],
				Path.Combine(resultDir, "install.log"), timeout.Token, output, out exitCode);
			if (exitCode != 0)
				return false;

			var launchOutput = Run("xcrun", ["simctl", "launch", "--console", "--terminate-running-process", deviceUdid, bundleId],
				Path.Combine(resultDir, "application.log"), timeout.Token, output, out exitCode);
			var completed = AppleRunCompleted(exitCode, resultDir, completionMarker);

			// simctl's exit code describes the launch command, not the application.
			// Match runningboard's native wait status to this launch's PID; fail closed if unavailable.
			var pidMatch = Regex.Match(launchOutput, @"^" + Regex.Escape(bundleId) + @": (?<pid>\d+)\s*$",
				RegexOptions.Multiline | RegexOptions.CultureInvariant);
			if (!pidMatch.Success)
				return false;

			var pid = int.Parse(pidMatch.Groups["pid"].Value, System.Globalization.CultureInfo.InvariantCulture);
			for (var attempt = 0; attempt < 5; attempt++)
			{
				var terminationLog = Run("xcrun",
					["simctl", "spawn", deviceUdid, "log", "show", "--last", $"{timeoutSeconds}s", "--style", "compact",
						"--predicate", $"process == 'runningboardd' AND eventMessage CONTAINS '{bundleId}' AND eventMessage CONTAINS 'termination reported by launchd'"],
					Path.Combine(resultDir, "termination.log"), timeout.Token, output, out exitCode);
				if (exitCode != 0)
					return false;
				var status = AppleExitStatus(terminationLog, bundleId, pid);
				if (status is not null)
					return completed && status == 0;
				Task.Delay(TimeSpan.FromSeconds(1), timeout.Token).GetAwaiter().GetResult();
			}
			output?.WriteLine($"No native exit status found for {bundleId} PID {pid}.");
			return false;
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			output?.WriteLine($"iOS smoke test timed out after {timeoutSeconds} seconds; expected '{completionMarker}'.");
			return false;
		}
		finally
		{
			if (!string.IsNullOrWhiteSpace(bundleId))
			{
				ToolRunner.Run("xcrun", $"simctl terminate {deviceUdid} {bundleId}", out _, timeoutInSeconds: 30, output: output);
				ToolRunner.Run("xcrun", $"simctl uninstall {deviceUdid} {bundleId}", out _, timeoutInSeconds: 30, output: output);
			}
		}
	}

	internal static int? AppleExitStatus(string log, string bundleId, int pid)
	{
		var match = Regex.Match(log, @"\[app<" + Regex.Escape(bundleId) + @"(?:\([^>\r\n]*\))?>:" +
			pid.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			@"\] termination reported by launchd \((?<reason>\d+), (?<namespace>\d+), (?<status>\d+)\)",
			RegexOptions.CultureInvariant);
		if (!match.Success)
			return null;
		if (match.Groups["reason"].Value != "0" || match.Groups["namespace"].Value != "0")
			return -1;
		return int.Parse(match.Groups["status"].Value, System.Globalization.CultureInfo.InvariantCulture);
	}

	internal static bool AppleRunCompleted(int exitCode, string resultDir, string completionMarker)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(completionMarker);
		var markerLine = new Regex(
			@"^(?:\[\d{2}:\d{2}:\d{2}\.\d+\] )?(?:\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d+[+-]\d{4} [^\[\r\n]+\[\d+:\d+\] )?" +
			Regex.Escape(completionMarker) + @"\s*$", RegexOptions.CultureInvariant);
		return exitCode == 0 && Directory.Exists(resultDir) &&
			File.Exists(Path.Combine(resultDir, "application.log")) &&
			File.ReadLines(Path.Combine(resultDir, "application.log")).Any(markerLine.IsMatch);
	}

	internal static string Run(string tool, string[] arguments, string logPath, CancellationToken cancellationToken,
		ITestOutputHelper? output, out int exitCode)
	{
		cancellationToken.ThrowIfCancellationRequested();
		using var process = new Process { StartInfo = new ProcessStartInfo(tool) };
		foreach (var argument in arguments)
			process.StartInfo.ArgumentList.Add(argument);
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		var captured = new StringBuilder();
		using var log = new StreamWriter(logPath) { AutoFlush = true };
		var stdoutClosed = new TaskCompletionSource();
		var stderrClosed = new TaskCompletionSource();
		var capturing = true;
		void Capture(string? line, TaskCompletionSource closed)
		{
			if (line is null)
				closed.TrySetResult();
			else
			{
				lock (captured)
				{
					if (!capturing)
						return;
					captured.AppendLine(line);
					log.WriteLine(line);
				}
			}
		}
		process.OutputDataReceived += (_, e) => Capture(e.Data, stdoutClosed);
		process.ErrorDataReceived += (_, e) => Capture(e.Data, stderrClosed);
		output?.WriteLine($"Running: {tool} {string.Join(" ", arguments)}");
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		try
		{
			process.WaitForExitAsync(cancellationToken).GetAwaiter().GetResult();
		}
		finally
		{
			if (!process.HasExited)
			{
				try
				{
					process.Kill(entireProcessTree: true);
				}
				catch (InvalidOperationException) when (process.HasExited)
				{
				}
				if (!process.WaitForExit(10000))
					throw new TimeoutException($"Launch tool '{tool}' failed to terminate.");
			}
			// Preserve partial output even when execution is canceled or times out.
			if (!Task.WhenAll(stdoutClosed.Task, stderrClosed.Task).Wait(TimeSpan.FromSeconds(10)))
			{
				process.CancelOutputRead();
				process.CancelErrorRead();
				output?.WriteLine($"Output capture for '{tool}' did not finish within 10 seconds.");
			}
			lock (captured)
			{
				capturing = false;
				output?.WriteLine(captured.ToString());
			}
		}
		exitCode = process.ExitCode;
		output?.WriteLine($"Exit code: {exitCode}");
		lock (captured)
			return captured.ToString();
	}
}
