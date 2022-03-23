#nullable enable
using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Windows.Graphics.Display;
using Windows.Graphics.Display.Core;
using Windows.System.Display;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class DeviceDisplayImplementation : IDeviceDisplay
	{
		readonly object locker = new object();
		DisplayRequest? displayRequest;

		public event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChanged;

		public bool KeepScreenOn
		{
			get
			{
				lock (locker)
				{
					return displayRequest != null;
				}
			}

			set
			{
				lock (locker)
				{
					if (value)
					{
						if (displayRequest == null)
						{
							displayRequest = new DisplayRequest();
							displayRequest.RequestActive();
						}
					}
					else
					{
						if (displayRequest != null)
						{
							displayRequest.RequestRelease();
							displayRequest = null;
						}
					}
				}
			}
		}

		static DisplayRotation CalculateRotation(DisplayOrientations native, DisplayOrientations current)
		{
			if (native == DisplayOrientations.Portrait)
			{
				switch (current)
				{
					case DisplayOrientations.Landscape:
						return DisplayRotation.Rotation90;
					case DisplayOrientations.Portrait:
						return DisplayRotation.Rotation0;
					case DisplayOrientations.LandscapeFlipped:
						return DisplayRotation.Rotation270;
					case DisplayOrientations.PortraitFlipped:
						return DisplayRotation.Rotation180;
				}
			}
			else if (native == DisplayOrientations.Landscape)
			{
				switch (current)
				{
					case DisplayOrientations.Landscape:
						return DisplayRotation.Rotation0;
					case DisplayOrientations.Portrait:
						return DisplayRotation.Rotation270;
					case DisplayOrientations.LandscapeFlipped:
						return DisplayRotation.Rotation180;
					case DisplayOrientations.PortraitFlipped:
						return DisplayRotation.Rotation90;
				}
			}

			return DisplayRotation.Unknown;
		}

#if WINDOWS

		AppWindow? _currentAppWindowListeningTo;
		public DisplayInfo GetMainDisplayInfo()
		{
			if (Platform.CurrentWindow == null)
				return new DisplayInfo();

			var appWindow = Platform.CurrentAppWindow;
			var windowHandler = Platform.CurrentWindowHandle;
			var mi = GetDisplay(windowHandler);

			if (mi == null)
				return new DisplayInfo();

			DEVMODE vDevMode = new DEVMODE();
			EnumDisplaySettings(mi.Value.DeviceNameToLPTStr(), -1, ref vDevMode);

			var rotation = CalculateRotation(vDevMode, appWindow);
			var perpendicular =
				rotation == DisplayRotation.Rotation90 ||
				rotation == DisplayRotation.Rotation270;

			var w = vDevMode.dmPelsWidth;
			var h = vDevMode.dmPelsHeight;
			var dpi = GetDpiForWindow(windowHandler) / DeviceDisplay.BaseLogicalDpi;

			return new DisplayInfo(
				width: perpendicular ? h : w,
				height: perpendicular ? w : h,
				density: dpi,
				orientation: GetWindowOrientationWin32(appWindow) == DisplayOrientations.Landscape ? DisplayOrientation.Landscape : DisplayOrientation.Portrait,
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

		public void StartScreenMetricsListeners()
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				Platform.CurrentWindowDisplayChanged += OnWindowDisplayChanged;
				Platform.CurrentWindowChanged += OnCurrentWindowChanged;

				if (Platform.CurrentWindow != null)
				{
					_currentAppWindowListeningTo = Platform.CurrentAppWindow;
					_currentAppWindowListeningTo.Changed += OnAppWindowChanged;
				}
			});
		}

		public void StopScreenMetricsListeners()
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				Platform.CurrentWindowChanged -= OnCurrentWindowChanged;
				Platform.CurrentWindowDisplayChanged -= OnWindowDisplayChanged;

				if (_currentAppWindowListeningTo != null)
					_currentAppWindowListeningTo.Changed -= OnAppWindowChanged;

				_currentAppWindowListeningTo = null;
			});
		}

		void OnCurrentWindowChanged(object? sender, EventArgs e)
		{
			if (_currentAppWindowListeningTo != null)
			{
				_currentAppWindowListeningTo.Changed -= OnAppWindowChanged;
			}

			_currentAppWindowListeningTo = Platform.CurrentAppWindow;
			_currentAppWindowListeningTo.Changed += OnAppWindowChanged;
		}

		void OnWindowDisplayChanged(object? sender, EventArgs e)
		{
			WindowDisplayPropertiesChanged();
		}

		void WindowDisplayPropertiesChanged()
		{
			var metrics = GetMainDisplayInfo();
			MainDisplayInfoChanged?.Invoke(this, new DisplayInfoChangedEventArgs(metrics));
		}

		void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args) =>
			WindowDisplayPropertiesChanged();

		static DisplayRotation CalculateRotation(DEVMODE devMode, AppWindow appWindow)
		{
			DisplayOrientations native = DisplayOrientations.Portrait;
			switch (devMode.dmDisplayOrientation)
			{
				case 0:
					native = DisplayOrientations.Landscape;
					break;
				case 1:
					native = DisplayOrientations.Portrait;
					break;
				case 2:
					native = DisplayOrientations.LandscapeFlipped;
					break;
				case 3:
					native = DisplayOrientations.PortraitFlipped;
					break;
			}

			var current = GetWindowOrientationWin32(appWindow);
			return CalculateRotation(native, current);
		}

		// https://github.com/marb2000/DesktopWindow/blob/abb21b797767bb24da09c066514117d5f1aabd75/WindowExtensions/DesktopWindow.cs#L407
		static DisplayOrientations GetWindowOrientationWin32(AppWindow appWindow)
		{
			DisplayOrientations orientationEnum;
			int theScreenWidth = appWindow.Size.Width;
			int theScreenHeight = appWindow.Size.Height;
			if (theScreenWidth > theScreenHeight)
				orientationEnum = DisplayOrientations.Landscape;
			else
				orientationEnum = DisplayOrientations.Portrait;

			return orientationEnum;
		}

		[DllImport("User32", CharSet = CharSet.Unicode)]
		static extern int GetDpiForWindow(IntPtr hwnd);

		[DllImport("User32", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern IntPtr MonitorFromRect(ref RECT lprc, MonitorOptions dwFlags);

		[DllImport("User32", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern Boolean EnumDisplaySettings(
			byte[] lpszDeviceName,
			[param: MarshalAs(UnmanagedType.U4)] int iModeNum,
			[In, Out] ref DEVMODE lpDevMode);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

		[DllImport("user32.dll")]
		static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);
		delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

		static bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
		{
			MONITORINFOEX mi = new MONITORINFOEX();
			mi.Size = Marshal.SizeOf(typeof(MONITORINFOEX));
			if (GetMonitorInfo(hMonitor, ref mi))
				Console.WriteLine(mi.DeviceName);

			return true;
		}

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
			private const int CCHDEVICENAME = 0x20;
			private const int CCHFORMNAME = 0x20;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
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
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
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
#elif WINDOWS_UWP
		public DisplayInfo GetMainDisplayInfo() =>
			GetMainDisplayInfo(null);

		DisplayInfo GetMainDisplayInfo(DisplayInformation? di = null)
		{
			di ??= DisplayInformation.GetForCurrentView();

			var rotation = CalculateRotation(di);
			var perpendicular =
				rotation == DisplayRotation.Rotation90 ||
				rotation == DisplayRotation.Rotation270;

			var w = di.ScreenWidthInRawPixels;
			var h = di.ScreenHeightInRawPixels;

			var hdi = HdmiDisplayInformation.GetForCurrentView();
			var hdm = hdi?.GetCurrentDisplayMode();

			return new DisplayInfo(
				width: perpendicular ? h : w,
				height: perpendicular ? w : h,
				density: di.LogicalDpi / DeviceDisplay.BaseLogicalDpi,
				orientation: CalculateOrientation(di),
				rotation: rotation,
				rate: (float)(hdm?.RefreshRate ?? 0));
		}

		public void StartScreenMetricsListeners()
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				var di = DisplayInformation.GetForCurrentView();

				di.DpiChanged += OnDisplayInformationChanged;
				di.OrientationChanged += OnDisplayInformationChanged;
			});
		}

		public void StopScreenMetricsListeners()
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				var di = DisplayInformation.GetForCurrentView();

				di.DpiChanged -= OnDisplayInformationChanged;
				di.OrientationChanged -= OnDisplayInformationChanged;
			});
		}

		void OnDisplayInformationChanged(DisplayInformation di, object args)
		{
			var metrics = GetMainDisplayInfo(di);
			MainDisplayInfoChanged?.Invoke(this, new DisplayInfoChangedEventArgs(metrics));
		}

		DisplayOrientation CalculateOrientation(DisplayInformation di)
		{
			switch (di.CurrentOrientation)
			{
				case DisplayOrientations.Landscape:
				case DisplayOrientations.LandscapeFlipped:
					return DisplayOrientation.Landscape;
				case DisplayOrientations.Portrait:
				case DisplayOrientations.PortraitFlipped:
					return DisplayOrientation.Portrait;
			}

			return DisplayOrientation.Unknown;
		}

		static DisplayRotation CalculateRotation(DisplayInformation di)
		{
			var native = di.NativeOrientation;
			var current = di.CurrentOrientation;

			return CalculateRotation(native, current);
		}
	}
#endif
	}
}
