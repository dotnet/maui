using AppKit;
using CoreVideo;
using Foundation;

namespace Microsoft.Maui.Essentials
{
	partial class PlatformDeviceDisplay
	{
		uint keepScreenOnId = 0;
		NSObject screenMetricsObserver;

		bool PlatformKeepScreenOn
		{
			get
			{
				return keepScreenOnId != 0;
			}

			set
			{
				if (KeepScreenOn == value)
					return;

				if (value)
				{
					IOKit.PreventUserIdleDisplaySleep("KeepScreenOn", out keepScreenOnId);
				}
				else
				{
					if (IOKit.AllowUserIdleDisplaySleep(keepScreenOnId))
						keepScreenOnId = 0;
				}
			}
		}

		DisplayInfo GetMainDisplayInfo()
		{
			var mainScreen = NSScreen.MainScreen;
			var frame = mainScreen.Frame;
			var scale = mainScreen.BackingScaleFactor;

			var mainDisplayId = CoreGraphicsInterop.MainDisplayId;

			// try determine the refresh rate, but fall back to 60Hz
			var refreshRate = CoreGraphicsInterop.GetRefreshRate(mainDisplayId);
			if (refreshRate == 0)
				refreshRate = CVDisplayLinkInterop.GetRefreshRate(mainDisplayId);
			if (refreshRate == 0)
				refreshRate = 60.0;

			return new DisplayInfo(
				width: frame.Width,
				height: frame.Height,
				density: scale,
				orientation: DisplayOrientation.Portrait,
				rotation: DisplayRotation.Rotation0,
				rate: (float)refreshRate);
		}

		void StartScreenMetricsListeners()
		{
			if (screenMetricsObserver == null)
			{
				screenMetricsObserver = NSNotificationCenter.DefaultCenter.AddObserver(NSApplication.DidChangeScreenParametersNotification, OnDidChangeScreenParameters);
			}
		}

		void StopScreenMetricsListeners()
		{
			screenMetricsObserver?.Dispose();
		}

		void OnDidChangeScreenParameters(NSNotification notification)
		{
			var metrics = GetMainDisplayInfo();
			OnMainDisplayInfoChanged(metrics);
		}
	}
}
