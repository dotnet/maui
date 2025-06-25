#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.ApplicationModel
{
	static class PlatformMethods
	{
		public static class MessageIds
		{
			public const int WM_DPICHANGED = 0x02E0;
			public const int WM_DISPLAYCHANGE = 0x007E;
			public const int WM_SETTINGCHANGE = 0x001A;
			public const int WM_THEMECHANGE = 0x031A;
			public const int WM_GETMINMAXINFO = 0x0024;
			public const int WM_STYLECHANGING = 0x007C;
		}

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

		public static IntPtr SetWindowLongPtr(IntPtr hWnd, WindowLongFlags nIndex, IntPtr dwNewLong)
		{
			if (IntPtr.Size == 8)
				return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
			else
				return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong));

			[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
			static extern int SetWindowLong32(IntPtr hWnd, WindowLongFlags nIndex, IntPtr dwNewLong);

			[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
			static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, WindowLongFlags nIndex, IntPtr dwNewLong);
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
		public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, SpecialWindowHandles hWndInsertAfter, int x, int y, int width, int height, SetWindowPosFlags uFlags);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, ShowWindowFlags uFlags);

		[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern uint GetDpiForWindow(IntPtr hWnd);

		[DllImport("User32", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("dwmapi.dll")]
		static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out RECT pvAttribute, int cbAttribute);

		public static Maui.Graphics.Rect GetCaptionButtonsBound(IntPtr hWnd)
		{
			DwmGetWindowAttribute(hWnd, DwmWindowAttribute.DWMWA_CAPTION_BUTTON_BOUNDS, out RECT value, Marshal.SizeOf<RECT>());
			var density = GetDpiForWindow(hWnd) / 96f;
			return new Graphics.Rect(
				value.Left / density,
				value.Top / density,
				value.Right / density,
				value.Bottom / density);
		}

		public static bool HasStyle(uint currentStyle, WindowStyles styleMask)
		{
			return (currentStyle & (uint)styleMask) == (uint)styleMask;
		}

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
		public enum ShowWindowFlags : uint
		{
			SW_HIDE = 0,
			SW_NORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_MAXIMIZE = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_SHOWMINNOACTIVE = 7,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_SHOWDEFAULT = 10,
			SW_FORCEMINIMIZE = 11,
		}

		[Flags]
		public enum WindowStyles : uint
		{
			WS_BORDER = 0x00800000,
			WS_CAPTION = 0x00C00000,
			WS_SYSMENU = 0x00080000,
			WS_THICKFRAME = 0x00040000,
			WS_MINIMIZEBOX = 0x00020000,
			WS_MAXIMIZEBOX = 0x00020000,
			WS_OVERLAPPED = 0x00000000,
			WS_CAPTIONANDSYSTEMMENU = WS_CAPTION | WS_SYSMENU,
			WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX
		}

		[Flags]
		public enum ExtendedWindowStyles : uint
		{
			WS_EX_RTLREADING = 0x00002000,
			WS_EX_LAYOUTRTL = 0x00400000
		}

		public enum DwmWindowAttribute
		{
			DWMWA_NCRENDERING_ENABLED = 1,
			DWMWA_NCRENDERING_POLICY,
			DWMWA_TRANSITIONS_FORCEDISABLED,
			DWMWA_ALLOW_NCPAINT,
			DWMWA_CAPTION_BUTTON_BOUNDS,
			DWMWA_NONCLIENT_RTL_LAYOUT,
			DWMWA_FORCE_ICONIC_REPRESENTATION,
			DWMWA_FLIP3D_POLICY,
			DWMWA_EXTENDED_FRAME_BOUNDS,
			DWMWA_HAS_ICONIC_BITMAP,
			DWMWA_DISALLOW_PEEK,
			DWMWA_EXCLUDED_FROM_PEEK,
			DWMWA_CLOAK,
			DWMWA_CLOAKED,
			DWMWA_FREEZE_REPRESENTATION,
			DWMWA_LAST
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MinMaxInfo
		{
			public POINT Reserved;
			public POINT MaxSize;
			public POINT MaxPosition;
			public POINT MinTrackSize;
			public POINT MaxTrackSize;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct STYLESTRUCT
		{
			public uint StyleOld;
			public uint StyleNew;
		}
	}
}
