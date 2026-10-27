using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using UITest.Core;

namespace UITest.Appium.NUnit
{
	internal sealed class WindowsProcessObservation : IDisposable
	{
		readonly Process _process;
		readonly string _processName;
		readonly DateTime _processStartUtc;
		readonly IntPtr _selectedWindow;
		readonly string _selectedWindowHandle;
		readonly Action<string> _log;

		WindowsProcessObservation(Process process, string processName, DateTime processStartUtc, IntPtr selectedWindow, string selectedWindowHandle, Action<string> log)
		{
			_process = process;
			_processName = processName;
			_processStartUtc = processStartUtc;
			_selectedWindow = selectedWindow;
			_selectedWindowHandle = selectedWindowHandle;
			_log = log;
		}

		public static WindowsProcessObservation? TryStart(IApp app, Action<string> log)
		{
			if (!OperatingSystem.IsWindows())
				return null;

			try
			{
				var driver = app.GetType().GetProperty("Driver")?.GetValue(app);
				var handle = driver?.GetType().GetProperty("CurrentWindowHandle")?.GetValue(driver) as string;
				if (string.IsNullOrEmpty(handle))
					throw new InvalidOperationException("The selected window handle is unavailable.");

				var digits = handle.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? handle[2..] : handle;
				var window = new IntPtr(unchecked((long)ulong.Parse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture)));
				if (GetWindowThreadProcessId(window, out var processId) == 0 || processId == 0)
					throw new InvalidOperationException("The selected window could not be mapped to a native PID.");

				var process = Process.GetProcessById(checked((int)processId));
				try
				{
					// Retain the native process handle so ExitCode remains available after termination.
					_ = process.Handle;
					var observation = new WindowsProcessObservation(process, process.ProcessName, process.StartTime.ToUniversalTime(), window, handle, log);
					observation.Capture("before-test");
					return observation;
				}
				catch
				{
					process.Dispose();
					throw;
				}
			}
			catch (Exception exception)
			{
				Write(log, JsonSerializer.Serialize(new { phase = "capture-unavailable", error = exception.ToString() }));
				return null;
			}
		}

		public void Capture(string phase, ApplicationState? windowHandleProbeState = null)
		{
			if (!OperatingSystem.IsWindows())
				return;

			try
			{
				_process.Refresh();
				var exited = _process.HasExited;
				int? exitCode = exited ? _process.ExitCode : null;
				GetWindowThreadProcessId(_selectedWindow, out var selectedWindowProcessId);
				var windows = new List<string>();
				var enumerationSucceeded = EnumWindows((window, _) =>
				{
					GetWindowThreadProcessId(window, out var processId);
					if (processId == _process.Id)
						windows.Add($"0x{window.ToInt64():X}");
					return true;
				}, IntPtr.Zero);

				Write(_log, JsonSerializer.Serialize(new
				{
					phase,
					utc = DateTimeOffset.UtcNow,
					pid = _process.Id,
					processName = _processName,
					processStartUtc = _processStartUtc,
					windowHandleProbeState = windowHandleProbeState?.ToString(),
					initialSelectedWindowHandle = _selectedWindowHandle,
					selectedWindowProcessId,
					selectedNativeWindowExists = IsWindow(_selectedWindow),
					hasExited = exited,
					exitCode,
					exitCodeHex = exitCode.HasValue ? $"0x{unchecked((uint)exitCode.Value):X8}" : null,
					nativeTopLevelWindows = windows,
					enumerationSucceeded
				}));
			}
			catch (Exception exception)
			{
				Write(_log, JsonSerializer.Serialize(new { phase, utc = DateTimeOffset.UtcNow, error = exception.ToString() }));
			}
		}

		public void Dispose()
		{
			try
			{
				_process.Dispose();
			}
			catch (Exception exception)
			{
				Write(_log, JsonSerializer.Serialize(new { phase = "dispose-error", error = exception.ToString() }));
			}
		}

		static void Write(Action<string> log, string message)
		{
			try
			{
				log($"[WindowsProcessObservation] {message}");
			}
			catch
			{
				// Diagnostic output must not change the test outcome or recovery path.
			}
		}

		delegate bool EnumWindowsCallback(IntPtr window, IntPtr parameter);

		[DllImport("user32.dll")]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

		[DllImport("user32.dll")]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool IsWindow(IntPtr window);

		[DllImport("user32.dll")]
		[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr parameter);
	}
}
