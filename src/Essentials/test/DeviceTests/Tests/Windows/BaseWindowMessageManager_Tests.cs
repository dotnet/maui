#nullable enable
using System;
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
	}
}
