// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text;

namespace Microsoft.Maui.DevTools.Utils;

/// <summary>
/// Result of a process execution.
/// </summary>
public record ProcessResult
{
	public int ExitCode { get; init; }
	public string StandardOutput { get; init; } = string.Empty;
	public string StandardError { get; init; } = string.Empty;
	public bool Success => ExitCode == 0;
	public TimeSpan Duration { get; init; }
}

/// <summary>
/// Utility for running external processes with output capture.
/// </summary>
public static class ProcessRunner
{
	/// <summary>
	/// Runs a process synchronously and captures output.
	/// </summary>
	public static ProcessResult RunSync(
		string fileName,
		string arguments = "",
		string? workingDirectory = null,
		Dictionary<string, string>? environmentVariables = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var stopwatch = Stopwatch.StartNew();
		var stdoutBuilder = new StringBuilder();
		var stderrBuilder = new StringBuilder();

		using var process = new Process();
		process.StartInfo = new ProcessStartInfo
		{
			FileName = fileName,
			Arguments = arguments,
			WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		if (environmentVariables != null)
		{
			foreach (var kvp in environmentVariables)
			{
				process.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
			}
		}

		process.OutputDataReceived += (_, e) =>
		{
			if (e.Data != null)
				stdoutBuilder.AppendLine(e.Data);
		};

		process.ErrorDataReceived += (_, e) =>
		{
			if (e.Data != null)
				stderrBuilder.AppendLine(e.Data);
		};

		try
		{
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(5);
			var completed = process.WaitForExit((int)effectiveTimeout.TotalMilliseconds);

			if (!completed)
			{
				try { process.Kill(entireProcessTree: true); } catch { }
				throw new TimeoutException($"Process '{fileName}' timed out after {effectiveTimeout.TotalSeconds}s");
			}

			// Ensure all output is flushed
			process.WaitForExit();

			stopwatch.Stop();

			return new ProcessResult
			{
				ExitCode = process.ExitCode,
				StandardOutput = stdoutBuilder.ToString(),
				StandardError = stderrBuilder.ToString(),
				Duration = stopwatch.Elapsed
			};
		}
		catch (Exception ex) when (ex is not TimeoutException)
		{
			stopwatch.Stop();
			return new ProcessResult
			{
				ExitCode = -1,
				StandardOutput = stdoutBuilder.ToString(),
				StandardError = ex.Message,
				Duration = stopwatch.Elapsed
			};
		}
	}

	/// <summary>
	/// Runs a process asynchronously and captures output.
	/// </summary>
	public static async Task<ProcessResult> RunAsync(
		string fileName,
		string arguments = "",
		string? workingDirectory = null,
		Dictionary<string, string>? environmentVariables = null,
		TimeSpan? timeout = null,
		string? continuousInput = null,
		CancellationToken cancellationToken = default)
	{
		var stopwatch = Stopwatch.StartNew();
		var stdoutBuilder = new StringBuilder();
		var stderrBuilder = new StringBuilder();

		using var process = new Process();
		process.StartInfo = new ProcessStartInfo
		{
			FileName = fileName,
			Arguments = arguments,
			WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			RedirectStandardInput = continuousInput != null,
			CreateNoWindow = true
		};

		if (environmentVariables != null)
		{
			foreach (var kvp in environmentVariables)
			{
				process.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
			}
		}

		var outputTcs = new TaskCompletionSource<bool>();
		var errorTcs = new TaskCompletionSource<bool>();

		process.OutputDataReceived += (_, e) =>
		{
			if (e.Data != null)
				stdoutBuilder.AppendLine(e.Data);
			else
				outputTcs.TrySetResult(true);
		};

		process.ErrorDataReceived += (_, e) =>
		{
			if (e.Data != null)
				stderrBuilder.AppendLine(e.Data);
			else
				errorTcs.TrySetResult(true);
		};

		try
		{
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(5);
			
			using var timeoutCts = new CancellationTokenSource(effectiveTimeout);
			using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
				cancellationToken, timeoutCts.Token);

			// If continuous input is requested, start a background task to write it
			Task? inputTask = null;
			if (continuousInput != null)
			{
				inputTask = Task.Run(async () =>
				{
					while (!process.HasExited && !linkedCts.Token.IsCancellationRequested)
					{
						try
						{
							await process.StandardInput.WriteLineAsync(continuousInput);
							await process.StandardInput.FlushAsync();
						}
						catch
						{
							break; // Process may have exited
						}
						await Task.Delay(250, linkedCts.Token);
					}
				}, linkedCts.Token);
			}

			try
			{
				await process.WaitForExitAsync(linkedCts.Token);
			}
			catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
			{
				try { process.Kill(entireProcessTree: true); } catch { }
				throw new TimeoutException($"Process '{fileName}' timed out after {effectiveTimeout.TotalSeconds}s");
			}

			// Wait for input task to complete if present
			if (inputTask != null)
			{
				try { await inputTask.WaitAsync(TimeSpan.FromSeconds(2)); } catch { }
			}

			// Wait for output streams to complete
			await Task.WhenAll(outputTcs.Task, errorTcs.Task).WaitAsync(TimeSpan.FromSeconds(5));

			stopwatch.Stop();

			return new ProcessResult
			{
				ExitCode = process.ExitCode,
				StandardOutput = stdoutBuilder.ToString(),
				StandardError = stderrBuilder.ToString(),
				Duration = stopwatch.Elapsed
			};
		}
		catch (Exception ex) when (ex is not TimeoutException && ex is not OperationCanceledException)
		{
			stopwatch.Stop();
			return new ProcessResult
			{
				ExitCode = -1,
				StandardOutput = stdoutBuilder.ToString(),
				StandardError = ex.Message,
				Duration = stopwatch.Elapsed
			};
		}
	}

	/// <summary>
	/// Runs a process asynchronously with continuous stdin input until exit.
	/// Convenience overload for backward compatibility.
	/// </summary>
	[Obsolete("Use RunAsync with continuousInput parameter instead.")]
	public static Task<ProcessResult> RunWithContinuousInputAsync(
		string fileName,
		string arguments = "",
		string inputToWrite = "y",
		string? workingDirectory = null,
		Dictionary<string, string>? environmentVariables = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		return RunAsync(fileName, arguments, workingDirectory, environmentVariables,
			timeout, continuousInput: inputToWrite, cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Checks if a command exists in PATH.
	/// </summary>
	public static bool CommandExists(string command)
	{
		try
		{
			var whichCommand = PlatformDetector.IsWindows ? "where" : "which";
			var result = RunSync(whichCommand, command, timeout: TimeSpan.FromSeconds(5));
			return result.ExitCode == 0;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Gets the full path of a command.
	/// </summary>
	public static string? GetCommandPath(string command)
	{
		try
		{
			var whichCommand = PlatformDetector.IsWindows ? "where" : "which";
			var result = RunSync(whichCommand, command, timeout: TimeSpan.FromSeconds(5));
			if (result.ExitCode == 0)
			{
				var path = result.StandardOutput.Trim().Split('\n').FirstOrDefault()?.Trim();
				if (!string.IsNullOrEmpty(path) && File.Exists(path))
					return path;
			}
		}
		catch { }
		return null;
	}
}
