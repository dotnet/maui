#nullable enable
using System;
using AppKit;
using CoreVideo;
using Foundation;

namespace Microsoft.Maui.Devices
{
	class DeviceDisplayImplementation : IDeviceDisplay
	{
		uint keepScreenOnId = 0;
		NSObject? screenMetricsObserver;

		protected override bool GetKeepScreenOn() => keepScreenOnId != 0;

		protected override void SetKeepScreenOn(bool keepScreenOn)
		{
			if (KeepScreenOn == value)
				return;

			if (keepScreenOn)
			{
				IOKit.PreventUserIdleDisplaySleep("KeepScreenOn", out keepScreenOnId);
			}
			else
			{
				if (IOKit.AllowUserIdleDisplaySleep(keepScreenOnId))
					keepScreenOnId = 0;
			}
		}

		protected override DisplayInfo GetMainDisplayInfo()
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

		protected override void StartScreenMetricsListeners()
		{
			screenMetricsObserver ??= NSNotificationCenter.DefaultCenter.AddObserver(
				NSApplication.DidChangeScreenParametersNotification, OnDidChangeScreenParameters);
		}

		protected override void StopScreenMetricsListeners()
		{
			screenMetricsObserver?.Dispose();
			screenMetricsObserver = null;
		}

		void OnDidChangeScreenParameters(NSNotification notification) =>
			OnMainDisplayInfoChanged();
	}
}
