#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Maui.Controls.Sample.Platform
{
	static class PlatformMethods
	{
		[DllImport("user32.dll")]
		public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
	}
}
