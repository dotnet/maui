#nullable enable
using System;
using System.Runtime.InteropServices;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Devices
{
	partial class DeviceDisplayImplementation : IDeviceDisplay
	{
		NSObject? observer;

#if MACCATALYST
		static readonly NSString ScreenParametersChangedNotification =
			new NSString("NSApplicationDidChangeScreenParametersNotification");

		// Core Graphics P/Invoke declarations for Mac Catalyst
		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern uint CGMainDisplayID();

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern IntPtr CGDisplayCopyDisplayMode(uint display);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern void CGDisplayModeRelease(IntPtr mode);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern nuint CGDisplayModeGetWidth(IntPtr mode);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern nuint CGDisplayModeGetHeight(IntPtr mode);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern double CGDisplayModeGetRefreshRate(IntPtr mode);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern double CGDisplayRotation(uint display);

		readonly object locker = new object();
		NSObject? keepScreenOnActivity;

		protected override bool GetKeepScreenOn()
		{
			lock (locker)
			{
				return keepScreenOnActivity is not null;
			}
		}

		protected override void SetKeepScreenOn(bool keepScreenOn)
		{
			lock (locker)
			{
				if ((keepScreenOnActivity is not null) == keepScreenOn)
				{
					return;
				}

				if (keepScreenOn)
				{
					keepScreenOnActivity = NSProcessInfo.ProcessInfo.BeginActivity(
						NSActivityOptions.IdleDisplaySleepDisabled | NSActivityOptions.UserInitiated,
						"KeepScreenOn");
				}
				else
				{
					NSProcessInfo.ProcessInfo.EndActivity(keepScreenOnActivity!);
					keepScreenOnActivity = null;
				}
			}
		}
#else
		protected override bool GetKeepScreenOn() => UIApplication.SharedApplication.IdleTimerDisabled;

		protected override void SetKeepScreenOn(bool keepScreenOn) => UIApplication.SharedApplication.IdleTimerDisabled = keepScreenOn;
#endif

		protected override DisplayInfo GetMainDisplayInfo()
		{
#if MACCATALYST
			// On Mac Catalyst, bypass UIScreen entirely and use Core Graphics APIs
			// This gets fresh, non-cached screen information directly from the system
			// Note: CGMainDisplayID returns the primary display (with menu bar).
			// In multi-monitor setups, this may not be the display the app window is on.
			var displayId = CGMainDisplayID();
			var mode = CGDisplayCopyDisplayMode(displayId);
			
			if (mode == IntPtr.Zero)
			{
				return GetFallbackDisplayInfo();
			}

			try
			{
				var width = (double)CGDisplayModeGetWidth(mode);
				var height = (double)CGDisplayModeGetHeight(mode);
				var refreshRate = CGDisplayModeGetRefreshRate(mode);

				// Get rotation from Core Graphics
				var rotationDegrees = CGDisplayRotation(displayId);
				var rotation = ConvertRotationDegreesToDisplayRotation(rotationDegrees);

				// Get scale factor from UIScreen as a fallback (this is usually stable)
				var scale = UIScreen.MainScreen.Scale;

				return new DisplayInfo(
					width: width,
					height: height,
					density: scale,
					// Orientation is intentionally hardcoded to Portrait to match Xamarin's Mac Catalyst
					// behavior. Deriving orientation from dimensions/rotation breaks existing tests and
					// Mac desktop apps don't have a meaningful orientation concept.
					orientation: DisplayOrientation.Portrait,
					rotation: rotation,
					rate: (float)refreshRate);
			}
			finally
			{
				CGDisplayModeRelease(mode);
			}
#else
			// iOS implementation
			return GetFallbackDisplayInfo();
#endif
		}

		static DisplayRotation ConvertRotationDegreesToDisplayRotation(double degrees) =>
			degrees switch
			{
				0 => DisplayRotation.Rotation0,
				90 => DisplayRotation.Rotation90,
				180 => DisplayRotation.Rotation180,
				270 => DisplayRotation.Rotation270,
				_ => DisplayRotation.Rotation0
			};

		DisplayInfo GetFallbackDisplayInfo()
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

#if MACCATALYST
			// On Mac Catalyst, use multiple notifications to cover all display changes
			// NSApplicationDidChangeScreenParametersNotification - for resolution/refresh rate changes
			observer = notificationCenter.AddObserver(ScreenParametersChangedNotification, OnMainDisplayInfoChanged);
#else
			if (OperatingSystem.IsIOSVersionAtLeast(27))
			{
				// Status bar orientation notifications are no longer delivered on iOS 27.
				// Queue the read so the window scene has applied the new interface orientation.
				observer = notificationCenter.AddObserver(UIDevice.OrientationDidChangeNotification,
					_ => UIApplication.SharedApplication.BeginInvokeOnMainThread(OnMainDisplayInfoChanged));
				UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
				return;
			}

			var notification = UIDevice.OrientationDidChangeNotification;
			observer = notificationCenter.AddObserver(notification, OnMainDisplayInfoChanged);
#endif
		}

		protected override void StopScreenMetricsListeners()
		{
#if !MACCATALYST
			if (OperatingSystem.IsIOSVersionAtLeast(27) && observer is not null)
				UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
#endif
			observer?.Dispose();
			observer = null;
		}

		void OnMainDisplayInfoChanged(NSNotification obj) =>
			OnMainDisplayInfoChanged();

#pragma warning disable CA1416 // UIApplication.StatusBarOrientation has [UnsupportedOSPlatform("ios9.0")]. (Deprecated but still works)
#pragma warning disable CA1422 // Validate platform compatibility
		static UIInterfaceOrientation GetInterfaceOrientation()
		{
#if !MACCATALYST
			if (OperatingSystem.IsIOSVersionAtLeast(27))
				return WindowStateManager.Default.GetCurrentUIWindow()?.WindowScene?.EffectiveGeometry.InterfaceOrientation
					?? UIInterfaceOrientation.Unknown;
#endif
			return UIApplication.SharedApplication.StatusBarOrientation;
		}

		static DisplayOrientation CalculateOrientation()
		{
#if !MACCATALYST
			if (OperatingSystem.IsIOSVersionAtLeast(27))
				return GetInterfaceOrientation() switch
				{
					UIInterfaceOrientation.LandscapeLeft or UIInterfaceOrientation.LandscapeRight => DisplayOrientation.Landscape,
					UIInterfaceOrientation.Portrait or UIInterfaceOrientation.PortraitUpsideDown => DisplayOrientation.Portrait,
					_ => DisplayOrientation.Unknown,
				};
#endif
			return UIDevice.CurrentDevice.Orientation.IsLandscape()
				? DisplayOrientation.Landscape
				: UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.PortraitUpsideDown || UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.Portrait ? DisplayOrientation.Portrait : DisplayOrientation.Unknown;
		}

		static DisplayRotation CalculateRotation()
		{
#if !MACCATALYST
			if (OperatingSystem.IsIOSVersionAtLeast(27))
				return GetInterfaceOrientation() switch
				{
					UIInterfaceOrientation.Portrait => DisplayRotation.Rotation0,
					UIInterfaceOrientation.PortraitUpsideDown => DisplayRotation.Rotation180,
					UIInterfaceOrientation.LandscapeLeft => DisplayRotation.Rotation270,
					UIInterfaceOrientation.LandscapeRight => DisplayRotation.Rotation90,
					_ => DisplayRotation.Unknown,
				};
#endif
			return UIDevice.CurrentDevice.Orientation switch
			{
				UIDeviceOrientation.Portrait => DisplayRotation.Rotation0,
				UIDeviceOrientation.PortraitUpsideDown => DisplayRotation.Rotation180,
				UIDeviceOrientation.LandscapeRight => DisplayRotation.Rotation270,
				UIDeviceOrientation.LandscapeLeft => DisplayRotation.Rotation90,
				_ => DisplayRotation.Unknown,
			};
		}

#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
	}
}
