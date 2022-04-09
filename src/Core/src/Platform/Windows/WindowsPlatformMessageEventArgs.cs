using System;

namespace Microsoft.Maui.Platform
{
	public class WindowsPlatformMessageEventArgs : EventArgs
	{
		public WindowsPlatformMessageEventArgs(IntPtr hwnd, uint messageId, IntPtr wParam, IntPtr lParam)
		{
			Hwnd = hwnd;
			MessageId = messageId;
			WParam = wParam;
			LParam = lParam;
		}

		public IntPtr Hwnd { get; private set; }
		public uint MessageId { get; private set; }
		public IntPtr WParam { get; private set; }
		public IntPtr LParam { get; private set; }
	}
}