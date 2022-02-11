#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Platform
{
	static class NativeMethods
	{
		public delegate IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		public static IntPtr SetWindowLongPtr(IntPtr hWnd, WindowLongFlags nIndex, WindowProc dwNewLong)
		{
			if (IntPtr.Size == 8)
				return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
			else
				return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong));

			[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
			static extern int SetWindowLong32(IntPtr hWnd, WindowLongFlags nIndex, WindowProc dwNewLong);

			[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
			static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, WindowLongFlags nIndex, WindowProc dwNewLong);
		}

		[DllImport("user32.dll")]
		public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, SetWindowPosFlags uFlags);

		[DllImport("user32.dll")]
		public static extern uint GetDpiForWindow(IntPtr hWnd);

		public enum WindowLongFlags : int
		{
			GWL_EXSTYLE = -20,
			GWLP_HINSTANCE = -6,
			GWLP_HWNDPARENT = -8,
			GWL_ID = -12,
			GWL_STYLE = -16,
			GWL_USERDATA = -21,
			GWL_WNDPROC = -4,
			DWLP_USER = 0x8,
			DWLP_MSGRESULT = 0x0,
			DWLP_DLGPROC = 0x4
		}

		public enum SpecialWindowHandles
		{
			HWND_TOP = 0,
			HWND_BOTTOM = 1,
			HWND_TOPMOST = -1,
			HWND_NOTOPMOST = -2
		}

		[Flags]
		public enum SetWindowPosFlags : uint
		{
			SWP_ASYNCWINDOWPOS = 0x4000,
			SWP_DEFERERASE = 0x2000,
			SWP_DRAWFRAME = 0x0020,
			SWP_FRAMECHANGED = 0x0020,
			SWP_HIDEWINDOW = 0x0080,
			SWP_NOACTIVATE = 0x0010,
			SWP_NOCOPYBITS = 0x0100,
			SWP_NOMOVE = 0x0002,
			SWP_NOOWNERZORDER = 0x0200,
			SWP_NOREDRAW = 0x0008,
			SWP_NOREPOSITION = 0x0200,
			SWP_NOSENDCHANGING = 0x0400,
			SWP_NOSIZE = 0x0001,
			SWP_NOZORDER = 0x0004,
			SWP_SHOWWINDOW = 0x0040,
		}
	}
}