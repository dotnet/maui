#nullable enable
using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class DeviceDisplayImplementation : IDeviceDisplay
	{
		NSObject? observer;

		public event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChanged;

		public bool KeepScreenOn
		{
			get => UIApplication.SharedApplication.IdleTimerDisabled;
			set => UIApplication.SharedApplication.IdleTimerDisabled = value;
		}

		public DisplayInfo GetMainDisplayInfo()
		{
			var bounds = UIScreen.MainScreen.Bounds;
			var scale = UIScreen.MainScreen.Scale;

			return new DisplayInfo(
				width: bounds.Width * scale,
				height: bounds.Height * scale,
				density: scale,
				orientation: CalculateOrientation(),
				rotation: CalculateRotation(),
				rate: Platform.HasOSVersion(10, 3) ? UIScreen.MainScreen.MaximumFramesPerSecond : 0);
		}

		public void StartScreenMetricsListeners()
		{
			var notificationCenter = NSNotificationCenter.DefaultCenter;
			var notification = UIApplication.DidChangeStatusBarOrientationNotification;
			observer = notificationCenter.AddObserver(notification, OnScreenMetricsChanged);
		}

		public void StopScreenMetricsListeners()
		{
			observer?.Dispose();
			observer = null;
		}

		void OnScreenMetricsChanged(NSNotification obj)
		{
			var metrics = GetMainDisplayInfo();
			MainDisplayInfoChanged?.Invoke(this, new DisplayInfoChangedEventArgs(metrics));
		}

		DisplayOrientation CalculateOrientation()
		{
			var orientation = UIApplication.SharedApplication.StatusBarOrientation;

			if (orientation.IsLandscape())
				return DisplayOrientation.Landscape;

			return DisplayOrientation.Portrait;
		}

		DisplayRotation CalculateRotation()
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
