using System;

namespace Microsoft.Maui.Platform
{
	public class WindowsPlatformWindowSubclassedEventArgs : EventArgs
	{
		public WindowsPlatformWindowSubclassedEventArgs(IntPtr hwnd)
		{
			Hwnd = hwnd;
		}

		public IntPtr Hwnd { get; private set; }
	}
}