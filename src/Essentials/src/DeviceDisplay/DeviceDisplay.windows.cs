#nullable enable
using System;
using System.Runtime.InteropServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.UI.Windowing;
using Windows.Graphics.Display;
using Windows.System.Display;

namespace Microsoft.Maui.Devices
{
	partial class DeviceDisplayImplementation
	{
		readonly object locker = new object();
		readonly ActiveWindowTracker _activeWindowTracker;

		DisplayRequest? displayRequest;

		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceDisplayImplementation"/> class.
		/// </summary>
		public DeviceDisplayImplementation()
		{
			_activeWindowTracker = new(WindowStateManager.Default);
			_activeWindowTracker.WindowMessage += OnWindowMessage;
		}

		protected override bool GetKeepScreenOn()
		{
			lock (locker)
			{
				return displayRequest != null;
			}
		}

		protected override void SetKeepScreenOn(bool keepScreenOn)
		{
			lock (locker)
			{
				if (keepScreenOn)
				{
					if (displayRequest == null)
					{
						displayRequest = new DisplayRequest();
						displayRequest.RequestActive();
					}
				}
				else
				{
					displayRequest?.RequestRelease();
					displayRequest = null;
				}
			}
		}

		protected override DisplayInfo GetMainDisplayInfo()
		{
			if (WindowStateManager.Default.GetActiveAppWindow(false) is not AppWindow appWindow)
				return default;

			var windowHandle = UI.Win32Interop.GetWindowFromWindowId(appWindow.Id);
			var mi = GetDisplay(windowHandle);

			if (mi == null)
				return default;

			var vDevMode = new DEVMODE();
			EnumDisplaySettings(mi.Value.DeviceNameToLPTStr(), -1, ref vDevMode);

			var w = vDevMode.dmPelsWidth;
			var h = vDevMode.dmPelsHeight;

			var dpi = (double)GetDpiForWindow(windowHandle);
			if (dpi != 0)
				dpi /= DeviceDisplay.BaseLogicalDpi;
			else
				dpi = 1.0;

			var displayOrientation = GetDisplayOrientation(vDevMode);
			var rotation = CalculateRotation(displayOrientation);

			var orientation = displayOrientation == DisplayOrientations.Landscape || displayOrientation == DisplayOrientations.LandscapeFlipped
				? DisplayOrientation.Landscape
				: DisplayOrientation.Portrait;

			return new DisplayInfo(
				width: w,
				height: h,
				density: dpi,
				orientation: orientation,
				rotation: rotation,
				rate: vDevMode.dmDisplayFrequency);
		}

		static MONITORINFOEX? GetDisplay(IntPtr hwnd)
		{
			IntPtr hMonitor;
			RECT rc;
			GetWindowRect(hwnd, out rc);
			hMonitor = MonitorFromRect(ref rc, MonitorOptions.MONITOR_DEFAULTTONEAREST);

			MONITORINFOEX mi = new MONITORINFOEX();
			mi.Size = Marshal.SizeOf(mi);
			bool success = GetMonitorInfo(hMonitor, ref mi);
			if (success)
			{
				return mi;
			}
			return null;
		}

		protected override void StartScreenMetricsListeners() =>
			MainThread.BeginInvokeOnMainThread(_activeWindowTracker.Start);

		protected override void StopScreenMetricsListeners() =>
			MainThread.BeginInvokeOnMainThread(_activeWindowTracker.Stop);

		// Currently there isn't a way to detect Orientation Changes unless you subclass the WinUI.Window and watch the messages.
		// This is the "subtlest" way to currently wire this together.
		// Hopefully there will be a more public API for this down the road so we can just use that directly from Essentials
		void OnWindowMessage(object? sender, WindowMessageEventArgs e)
		{
			if (e.MessageId == PlatformMethods.MessageIds.WM_DISPLAYCHANGE ||
				e.MessageId == PlatformMethods.MessageIds.WM_DPICHANGED)
				OnMainDisplayInfoChanged();
		}

		static DisplayRotation CalculateRotation(DisplayOrientations orientation) =>
			orientation switch
			{
				DisplayOrientations.Landscape => DisplayRotation.Rotation0,
				DisplayOrientations.Portrait => DisplayRotation.Rotation270,
				DisplayOrientations.LandscapeFlipped => DisplayRotation.Rotation180,
				DisplayOrientations.PortraitFlipped => DisplayRotation.Rotation90,
				_ => DisplayRotation.Rotation0,
			};

		static DisplayOrientations GetDisplayOrientation(DEVMODE devMode) =>
			devMode.dmDisplayOrientation switch
			{
				0 => DisplayOrientations.Landscape,
				1 => DisplayOrientations.Portrait,
				2 => DisplayOrientations.LandscapeFlipped,
				3 => DisplayOrientations.PortraitFlipped,
				_ => DisplayOrientations.Landscape,
			};

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern int GetDpiForWindow(IntPtr hwnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern IntPtr MonitorFromRect(ref RECT lprc, MonitorOptions dwFlags);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool EnumDisplaySettings(
			byte[] lpszDeviceName,
			[param: MarshalAs(UnmanagedType.U4)] int iModeNum,
			[In, Out] ref DEVMODE lpDevMode);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

		enum MonitorOptions : uint
		{
			MONITOR_DEFAULTTONULL,
			MONITOR_DEFAULTTOPRIMARY,
			MONITOR_DEFAULTTONEAREST
		}

		[StructLayout(LayoutKind.Sequential)]
		struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		struct MONITORINFOEX
		{
			public int Size;
			public RECT Monitor;
			public RECT WorkArea;
			public uint Flags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			public string DeviceName;

			public byte[] DeviceNameToLPTStr()
			{
				var lptArray = new byte[DeviceName.Length + 1];

				var index = 0;
				foreach (char c in DeviceName.ToCharArray())
					lptArray[index++] = Convert.ToByte(c);

				lptArray[index] = Convert.ToByte('\0');

				return lptArray;
			}
		}

		struct DEVMODE
		{
			const int CCHDEVICENAME = 0x20;
			const int CCHFORMNAME = 0x20;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
			public string dmDeviceName;
			public short dmSpecVersion;
			public short dmDriverVersion;
			public short dmSize;
			public short dmDriverExtra;
			public int dmFields;
			public int dmPositionX;
			public int dmPositionY;
			public int dmDisplayOrientation;
			public int dmDisplayFixedOutput;
			public short dmColor;
			public short dmDuplex;
			public short dmYResolution;
			public short dmTTOption;
			public short dmCollate;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
			public string dmFormName;
			public short dmLogPixels;
			public int dmBitsPerPel;
			public int dmPelsWidth;
			public int dmPelsHeight;
			public int dmDisplayFlags;
			public int dmDisplayFrequency;
			public int dmICMMethod;
			public int dmICMIntent;
			public int dmMediaType;
			public int dmDitherType;
			public int dmReserved1;
			public int dmReserved2;
			public int dmPanningWidth;
			public int dmPanningHeight;
		}
	}
}
