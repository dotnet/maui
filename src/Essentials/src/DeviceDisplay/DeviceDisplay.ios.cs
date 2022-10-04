#nullable enable
using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Devices
{
	partial class DeviceDisplayImplementation : IDeviceDisplay
	{
		NSObject? observer;

		protected override bool GetKeepScreenOn() => UIApplication.SharedApplication.IdleTimerDisabled;

		protected override void SetKeepScreenOn(bool keepScreenOn) => UIApplication.SharedApplication.IdleTimerDisabled = keepScreenOn;

		protected override DisplayInfo GetMainDisplayInfo()
		{
			var bounds = UIScreen.MainScreen.Bounds;
			var scale = UIScreen.MainScreen.Scale;

			nint rate = 0;

			if (OperatingSystem.IsIOSVersionAtLeast(10, 3) || OperatingSystem.IsMacCatalystVersionAtLeast(10, 3)
#if TVOS
				|| OperatingSystem.IsTvOSVersionAtLeast(11)
#endif
			)
			{
				rate = UIScreen.MainScreen.MaximumFramesPerSecond;
			}

			return new DisplayInfo(
				width: bounds.Width * scale,
				height: bounds.Height * scale,
				density: scale,
				orientation: CalculateOrientation(),
				rotation: CalculateRotation(),
				rate: rate);
		}

		[System.Runtime.Versioning.UnsupportedOSPlatform("ios13.0")]
		protected override void StartScreenMetricsListeners()
		{
			var notificationCenter = NSNotificationCenter.DefaultCenter;
			var notification = UIApplication.DidChangeStatusBarOrientationNotification;
			observer = notificationCenter.AddObserver(notification, OnMainDisplayInfoChanged);
		}

		protected override void StopScreenMetricsListeners()
		{
			observer?.Dispose();
			observer = null;
		}

		void OnMainDisplayInfoChanged(NSNotification obj) =>
			OnMainDisplayInfoChanged();

#pragma warning disable CA1416 // UIApplication.StatusBarOrientation has [UnsupportedOSPlatform("ios9.0")]. (Deprecated but still works)
		static DisplayOrientation CalculateOrientation() =>
			UIApplication.SharedApplication.StatusBarOrientation.IsLandscape()
				? DisplayOrientation.Landscape
				: DisplayOrientation.Portrait;

		static DisplayRotation CalculateRotation() =>
			UIApplication.SharedApplication.StatusBarOrientation switch
			{
				UIInterfaceOrientation.Portrait => DisplayRotation.Rotation0,
				UIInterfaceOrientation.PortraitUpsideDown => DisplayRotation.Rotation180,
				UIInterfaceOrientation.LandscapeLeft => DisplayRotation.Rotation270,
				UIInterfaceOrientation.LandscapeRight => DisplayRotation.Rotation90,
				_ => DisplayRotation.Unknown,
			};
#pragma warning restore CA1416
	}
}
