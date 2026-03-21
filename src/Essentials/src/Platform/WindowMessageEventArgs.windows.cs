#nullable enable
using System;

namespace Microsoft.Maui.ApplicationModel
{
	class WindowMessageEventArgs : EventArgs
	{
		public WindowMessageEventArgs(IntPtr hwnd, uint messageId, IntPtr wParam, IntPtr lParam)
		{
			Hwnd = hwnd;
			MessageId = messageId;
			WParam = wParam;
			LParam = lParam;
		}

		public IntPtr Hwnd { get; }

		public uint MessageId { get; }

		public IntPtr WParam { get; }

		public IntPtr LParam { get; }

		public IntPtr Result { get; set; }

		public bool Handled { get; set; }
	}
}
