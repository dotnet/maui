#nullable enable
using System;
using Foundation;
using UIKit;
#if MACCATALYST
using PlatformIOKit = Microsoft.Maui.ApplicationModel.IOKit;
#endif

namespace Microsoft.Maui.Devices
{
	partial class DeviceDisplayImplementation : IDeviceDisplay
	{
		NSObject? observer;
#if MACCATALYST
		uint keepScreenOnId = 0;

		protected override bool GetKeepScreenOn() => keepScreenOnId != 0;

		protected override void SetKeepScreenOn(bool keepScreenOn)
		{
			if (GetKeepScreenOn() == keepScreenOn)
			{
				return;
			}

			if (keepScreenOn)
			{
				// Create an IOKit assertion to prevent the display from sleeping
				if (!PlatformIOKit.PreventUserIdleDisplaySleep("KeepScreenOn", out keepScreenOnId))
				{
					keepScreenOnId = 0; // Reset on failure
				}
			}
			else
			{
				// Release the IOKit assertion to allow the display to sleep
				if (PlatformIOKit.AllowUserIdleDisplaySleep(keepScreenOnId))
				{
					keepScreenOnId = 0;
				}
			}
		}

		/// <summary>
		/// Finalizer to ensures IOKit assertion is released when object is disposed
		/// </summary>
		~DeviceDisplayImplementation()
		{
			// Check if there's an active IOKit assertion that needs to be released
			if (keepScreenOnId != 0)
			{
				// Release the IOKit assertion to allow the display to sleep
				PlatformIOKit.AllowUserIdleDisplaySleep(keepScreenOnId);
				keepScreenOnId = 0;
			}
		}
#else
		protected override bool GetKeepScreenOn() => UIApplication.SharedApplication.IdleTimerDisabled;

		protected override void SetKeepScreenOn(bool keepScreenOn) => UIApplication.SharedApplication.IdleTimerDisabled = keepScreenOn;
#endif

		protected override DisplayInfo GetMainDisplayInfo()
		{
			var bounds = UIScreen.MainScreen.Bounds;
			var scale = UIScreen.MainScreen.Scale;

			var rate = (OperatingSystem.IsIOSVersionAtLeast(10, 3) || OperatingSystem.IsMacCatalystVersionAtLeast(10, 3) || OperatingSystem.IsTvOSVersionAtLeast(10, 3))
				? UIScreen.MainScreen.MaximumFramesPerSecond
				: 0;

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
#pragma warning disable CA1422 // Validate platform compatibility
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

#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
	}
}
