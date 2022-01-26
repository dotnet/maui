using System;

namespace Microsoft.Maui.Platform
{
	public class WindowsNativeWindowSubclassedEventArgs : EventArgs
	{
		public WindowsNativeWindowSubclassedEventArgs(IntPtr hwnd)
		{
			Hwnd = hwnd;
		}

		public IntPtr Hwnd { get; private set; }
	}
}