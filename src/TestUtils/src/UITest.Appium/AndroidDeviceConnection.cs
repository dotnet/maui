using System.ComponentModel;
using System.Diagnostics;
using UITest.Core;

namespace UITest.Appium
{
	internal sealed class AndroidDeviceConnection
	{
		readonly Func<string[], string> _runAdb;
		readonly Action<TimeSpan> _delay;
		readonly Action<string> _log;
		bool _recoveryAttempted;
		AndroidDeviceUnavailableException? _failure;

		public AndroidDeviceConnection() : this(RunAdb, Thread.Sleep, Console.Error.WriteLine)
		{
		}

		internal AndroidDeviceConnection(Func<string[], string> runAdb, Action<TimeSpan> delay, Action<string> log)
		{
			_runAdb = runAdb;
			_delay = delay;
			_log = log;
		}

		public void EnsureReady(string? serial)
		{
			if (_failure != null)
				throw _failure;

			var target = string.IsNullOrWhiteSpace(serial) ? Array.Empty<string>() : new[] { "-s", serial };
			if (IsReady(target))
				return;

			if (!_recoveryAttempted)
			{
				_recoveryAttempted = true;
				_log($"Android device '{serial ?? "default"}' is not ready; attempting one ADB reconnect.");
				TryAdb(target.Concat(new[] { "reconnect" }).ToArray());
				for (var attempt = 0; attempt < 3; attempt++)
				{
					_delay(TimeSpan.FromSeconds(5));
					if (IsReady(target))
						return;
				}
			}

			_failure = new AndroidDeviceUnavailableException(
				$"Android device '{serial ?? "default"}' is unavailable after bounded ADB recovery. Stopping UI test run.");
			_log(_failure.Message);
			throw _failure;
		}

		bool IsReady(string[] target) =>
			TryAdb(target.Concat(new[] { "shell", "getprop", "sys.boot_completed" }).ToArray())?.Trim() == "1";

		string? TryAdb(string[] arguments)
		{
			try
			{
				return _runAdb(arguments);
			}
			catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is Win32Exception)
			{
				_log($"ADB {string.Join(" ", arguments)} failed: {ex.Message}");
				return null;
			}
		}

		static string RunAdb(string[] arguments)
		{
			var sdkRoot = Environment.GetEnvironmentVariable("ANDROID_HOME")
				?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
			var startInfo = new ProcessStartInfo
			{
				FileName = string.IsNullOrWhiteSpace(sdkRoot) ? "adb" :
					Path.Combine(sdkRoot, "platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb"),
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};
			foreach (var argument in arguments)
				startInfo.ArgumentList.Add(argument);

			using var process = Process.Start(startInfo) ?? throw new IOException("Unable to start adb.");
			var output = process.StandardOutput.ReadToEndAsync();
			var error = process.StandardError.ReadToEndAsync();
			if (!process.WaitForExit(5000))
			{
				try
				{
					process.Kill(entireProcessTree: true);
				}
				catch (InvalidOperationException) when (process.HasExited)
				{
					// The command exited between the timeout and the kill request.
				}
				if (!process.WaitForExit(5000))
					throw new TimeoutException("ADB did not exit after being killed.");
				throw new TimeoutException("ADB command exceeded 5 seconds.");
			}

			if (!Task.WaitAll(new Task[] { output, error }, 5000))
				throw new TimeoutException("ADB output collection exceeded 5 seconds.");
			if (process.ExitCode != 0)
				throw new IOException($"ADB exited with code {process.ExitCode}: {error.Result.Trim()}");
			return output.Result;
		}
	}
}
