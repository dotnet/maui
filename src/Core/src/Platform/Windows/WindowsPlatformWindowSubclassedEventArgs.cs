// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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