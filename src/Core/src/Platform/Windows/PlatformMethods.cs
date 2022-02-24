#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Platform
{
	static class PlatformMethods
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

		public static IntPtr SetWindowLongPtr(IntPtr hWnd, WindowLongFlags nIndex, long dwNewLong)
		{
			if (IntPtr.Size == 8)
				return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
			else
				return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong));

			[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
			static extern int SetWindowLong32(IntPtr hWnd, WindowLongFlags nIndex, long dwNewLong);

			[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
			static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, WindowLongFlags nIndex, long dwNewLong);
		}

		public static long GetWindowLongPtr(IntPtr hWnd, WindowLongFlags nIndex)
		{
			if (IntPtr.Size == 8)
				return GetWindowLongPtr64(hWnd, nIndex);
			else
				return GetWindowLong32(hWnd, nIndex);

			[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
			static extern int GetWindowLong32(IntPtr hWnd, WindowLongFlags nIndex);

			[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
			static extern long GetWindowLongPtr64(IntPtr hWnd, WindowLongFlags nIndex);
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

		[Flags]
		public enum ExtendedWindowFlags : long
		{
			WS_EX_LAYOUTRTL = 0x00400000L
		}

		[Flags]
		public enum WindowStyle : int
		{
			Border = 0x00800000,
			Caption = 0x00C00000,
			Child = 0x40000000,
			ChildWindow = 0x40000000,
			ChildChildren = 0x02000000,
			ClipSiblings = 0x04000000,
			Disabled = 0x08000000,
			DlgFrame = 0x00400000,
			Group = 0x00020000,
			HScroll = 0x00100000,
			Iconic = 0x20000000,
			Maximize = 0x01000000,
			MaximizeBox = 0x00010000,
			Minimize = 0x20000000,
			MinimizeBox = 0x00020000,
			Overlapped = 0x00000000,
			OverlappedWindow = (Overlapped | Caption | SysMenu | ThickFrame | MinimizeBox | MaximizeBox),
			SizeBox = 0x00040000,
			SysMenu = 0x00080000,
			TabStop = 0x00010000,
			ThickFrame = 0x00040000,
			Tiled = 0x00000000,
			TiledWindow = (Overlapped | Caption | SysMenu | ThickFrame | MinimizeBox | MaximizeBox),
			Visible = 0x10000000,
			VScroll = 0x00200000,
			WS_EX_LAYOUTRTL = 0x00400000
		}
	}
}