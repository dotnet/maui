#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	public abstract class BaseWindowMessageManager_Tests
	{
		protected const uint WM_APP = 0x8000;
		protected const uint TEST_MESSAGE = WM_APP + 1;

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		protected static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		protected async Task PostTestMessageAsync(UI.Xaml.Window window)
		{
			var handle = window.GetWindowHandle();

			PostMessage(handle, TEST_MESSAGE, IntPtr.Zero, IntPtr.Zero);

			await Task.Delay(100);
		}

		// PostMessage delivers asynchronously: the posted window message is only seen once the
		// message pump dispatches it. Poll the condition (yielding to the pump on each iteration)
		// up to a generous timeout instead of asserting after a single fixed delay, so a slow pump
		// under CI load no longer reads as a failure. A genuine "message never delivered" defect
		// still fails the caller's assertion once the timeout elapses.
		protected static async Task WaitForMessageAsync(Func<bool> condition, int timeoutMs = 5000, int pollMs = 50)
		{
			var stopwatch = Stopwatch.StartNew();
			while (!condition() && stopwatch.ElapsedMilliseconds < timeoutMs)
			{
				await Task.Delay(pollMs);
			}
		}
	}
}
