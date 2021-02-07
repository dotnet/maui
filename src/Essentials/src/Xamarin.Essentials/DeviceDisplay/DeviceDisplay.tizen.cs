using System;
using System.Runtime.InteropServices;

namespace Xamarin.Essentials
{
	public static partial class DeviceDisplay
	{
		[DllImport("libcapi-system-device.so.0", EntryPoint = "device_power_request_lock")]
		static extern void RequestKeepScreenOn(int type = 1, int timeout = 0);

		[DllImport("libcapi-system-device.so.0", EntryPoint = "device_power_release_lock")]
		static extern void ReleaseKeepScreenOn(int type = 1);

		static bool keepScreenOn = false;

		static bool PlatformKeepScreenOn
		{
			get => keepScreenOn;
			set
			{
				if (value)
					RequestKeepScreenOn();
				else
					ReleaseKeepScreenOn();
				keepScreenOn = value;
			}
		}

		static DisplayInfo GetMainDisplayInfo()
		{
			var display = Platform.MainWindow;
			return new DisplayInfo(
				width: display.ScreenSize.Width,
				height: display.ScreenSize.Height,
				density: display.ScreenDpi.X / (DeviceInfo.Idiom == DeviceIdiom.TV ? 72.0 : 160.0),
				orientation: GetOrientation(),
				rotation: GetRotation());
		}

		static DisplayOrientation GetOrientation()
		{
			return Platform.MainWindow.Rotation switch
			{
				0 => DisplayOrientation.Portrait,
				90 => DisplayOrientation.Landscape,
				180 => DisplayOrientation.Portrait,
				270 => DisplayOrientation.Landscape,
				_ => DisplayOrientation.Unknown,
			};
		}

		static DisplayRotation GetRotation()
		{
			return Platform.MainWindow.Rotation switch
			{
				0 => DisplayRotation.Rotation0,
				90 => DisplayRotation.Rotation90,
				180 => DisplayRotation.Rotation180,
				270 => DisplayRotation.Rotation270,
				_ => DisplayRotation.Unknown,
			};
		}

		static void StartScreenMetricsListeners()
		{
			Platform.MainWindow.RotationChanged += OnRotationChanged;
		}

		static void StopScreenMetricsListeners()
		{
			Platform.MainWindow.RotationChanged -= OnRotationChanged;
		}

		static void OnRotationChanged(object s, EventArgs e)
		{
			OnMainDisplayInfoChanged(GetMainDisplayInfo());
		}
	}
}
