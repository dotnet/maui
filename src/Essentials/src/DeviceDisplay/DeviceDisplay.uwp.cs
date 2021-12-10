#nullable enable
using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Windows.Graphics.Display;
using Windows.Graphics.Display.Core;
using Windows.System.Display;

namespace Microsoft.Maui.Essentials
{
	partial class DeviceDisplayImplementation : IDeviceDisplay
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

		public DisplayInfo GetMainDisplayInfo()
		{
			var appWindow = GetAppWindowForCurrentWindow();
			DEVMODE vDevMode = new DEVMODE();
			EnumDisplaySettings(null!, -1, ref vDevMode);

			var rotation = CalculateRotation(vDevMode);
			var perpendicular =
				rotation == DisplayRotation.Rotation90 ||
				rotation == DisplayRotation.Rotation270;

			var w = appWindow.Size.Width;
			var h = appWindow.Size.Height;


			var hdi = HdmiDisplayInformation.GetForCurrentView();
			var hdm = hdi?.GetCurrentDisplayMode();

			return new DisplayInfo(
				width: perpendicular ? h : w,
				height: perpendicular ? w : h,
				density: 999,//di.LogicalDpi / 96.0, TODO FIX
				orientation: GetWindowOrientationWin32() == DisplayOrientations.Landscape ? DisplayOrientation.Landscape : DisplayOrientation.Portrait,
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

			//var metrics = GetMainDisplayInfo(di);

			// TODO this is just so it compiles
			MainDisplayInfoChanged?.Invoke(this, new DisplayInfoChangedEventArgs(new DisplayInfo()));
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

		static DisplayRotation CalculateRotation(DEVMODE devMode)
		{
			DisplayOrientations native = DisplayOrientations.Portrait;
			switch (devMode.dmDisplayOrientation)
			{
				case 0:
					native = DisplayOrientations.Portrait;
					break;
				case 1:
					native = DisplayOrientations.Landscape;
					break;
				case 2:
					native = DisplayOrientations.LandscapeFlipped;
					break;
				case 3:
					native = DisplayOrientations.PortraitFlipped;
					break;
			}

			var current = GetWindowOrientationWin32();

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

		static AppWindow GetAppWindowForCurrentWindow()
		{
			var myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(Essentials.Platform.CurrentWindowHandle);
			return AppWindow.GetFromWindowId(myWndId);
		}

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern Boolean EnumDisplaySettings(
			byte[] lpszDeviceName, 
			[param: MarshalAs(UnmanagedType.U4)] int iModeNum,
			[In, Out] ref DEVMODE lpDevMode);

		static DisplayOrientations GetWindowOrientationWin32()
		{
			var appWindow = GetAppWindowForCurrentWindow();
			DisplayOrientations orientationEnum;
			int theScreenWidth = appWindow.Size.Width;
			int theScreenHeight = appWindow.Size.Height;
			if (theScreenWidth > theScreenHeight)
				orientationEnum = DisplayOrientations.Landscape;
			else
				orientationEnum = DisplayOrientations.Portrait;

			return orientationEnum;
		}

		public struct DEVMODE
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
	}
}
