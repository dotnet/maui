using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
	public static partial class DeviceDisplay
	{
		static NSObject observer;

		static bool PlatformKeepScreenOn
		{
			get => UIApplication.SharedApplication.IdleTimerDisabled;
			set => UIApplication.SharedApplication.IdleTimerDisabled = value;
		}

		static DisplayInfo GetMainDisplayInfo()
		{
			var bounds = UIScreen.MainScreen.Bounds;
			var scale = UIScreen.MainScreen.Scale;

			return new DisplayInfo(
				width: bounds.Width * scale,
				height: bounds.Height * scale,
				density: scale,
				orientation: CalculateOrientation(),
				rotation: CalculateRotation(),
				rate: UIScreen.MainScreen.MaximumFramesPerSecond);
		}

		static void StartScreenMetricsListeners()
		{
			var notificationCenter = NSNotificationCenter.DefaultCenter;
			var notification = UIApplication.DidChangeStatusBarOrientationNotification;
			observer = notificationCenter.AddObserver(notification, OnScreenMetricsChanged);
		}

		static void StopScreenMetricsListeners()
		{
			observer?.Dispose();
			observer = null;
		}

		static void OnScreenMetricsChanged(NSNotification obj)
		{
			var metrics = GetMainDisplayInfo();
			OnMainDisplayInfoChanged(metrics);
		}

		static DisplayOrientation CalculateOrientation()
		{
			var orientation = UIApplication.SharedApplication.StatusBarOrientation;

			if (orientation.IsLandscape())
				return DisplayOrientation.Landscape;

			return DisplayOrientation.Portrait;
		}

		static DisplayRotation CalculateRotation()
		{
			var orientation = UIApplication.SharedApplication.StatusBarOrientation;

			switch (orientation)
			{
				case UIInterfaceOrientation.Portrait:
					return DisplayRotation.Rotation0;
				case UIInterfaceOrientation.PortraitUpsideDown:
					return DisplayRotation.Rotation180;
				case UIInterfaceOrientation.LandscapeLeft:
					return DisplayRotation.Rotation270;
				case UIInterfaceOrientation.LandscapeRight:
					return DisplayRotation.Rotation90;
			}

			return DisplayRotation.Unknown;
		}
	}
}
