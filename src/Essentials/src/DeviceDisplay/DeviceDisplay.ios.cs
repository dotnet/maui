#nullable enable
using System;
using System.Runtime.InteropServices;
using Foundation;
using UIKit;
using ObjCRuntime;

namespace Microsoft.Maui.Devices
{
	partial class DeviceDisplayImplementation : IDeviceDisplay
	{
		NSObject? observer;

#if MACCATALYST
		// Core Graphics P/Invoke declarations for Mac Catalyst
		// Returns the display ID of the main display
		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern uint CGMainDisplayID();

		// Returns information about a display’s current configuration
		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern IntPtr CGDisplayCopyDisplayMode(uint display);

		// Releases a Core Graphics display mode
		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern void CGDisplayModeRelease(IntPtr mode);

		// Returns the width of the specified display mode
		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern nuint CGDisplayModeGetWidth(IntPtr mode);

		// Returns the height of the specified display mode
		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern nuint CGDisplayModeGetHeight(IntPtr mode);

		// Returns the refresh rate of the specified display mode
		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern double CGDisplayModeGetRefreshRate(IntPtr mode);

		// Returns the rotation angle of a display in degrees
		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern double CGDisplayRotation(uint display);

#endif

		protected override bool GetKeepScreenOn() => UIApplication.SharedApplication.IdleTimerDisabled;

		protected override void SetKeepScreenOn(bool keepScreenOn) => UIApplication.SharedApplication.IdleTimerDisabled = keepScreenOn;

		protected override DisplayInfo GetMainDisplayInfo()
		{
#if MACCATALYST
			// On Mac Catalyst, bypass UIScreen entirely and use Core Graphics APIs
			// This gets fresh, non-cached screen information directly from the system
			var displayId = CGMainDisplayID();
			var mode = CGDisplayCopyDisplayMode(displayId);
			
			if (mode == IntPtr.Zero)
			{
				return GetFallbackDisplayInfo();
			}

			var width = (double)CGDisplayModeGetWidth(mode);
			var height = (double)CGDisplayModeGetHeight(mode);
			var refreshRate = CGDisplayModeGetRefreshRate(mode);

			// Release the display mode to avoid memory leaks
			CGDisplayModeRelease(mode);

			// Get rotation from Core Graphics
			var rotationDegrees = CGDisplayRotation(displayId);
			var rotation = ConvertRotationDegreesToDisplayRotation(rotationDegrees);

			// Get scale factor from UIScreen as a fallback (this is usually stable)
			var scale = UIScreen.MainScreen.Scale;

			// For Mac Catalyst, calculate orientation based on actual dimensions and rotation
			var orientation = CalculateOrientationFromDimensionsAndRotation(width, height, rotationDegrees);

			return new DisplayInfo(
				width: width,
				height: height,
				density: scale,
				orientation: orientation,
				rotation: rotation,
				rate: (float)refreshRate);
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

		static DisplayOrientation CalculateOrientationFromDimensionsAndRotation(double width, double height, double rotationDegrees)
		{
			// For 90° and 270° rotations, the effective orientation is swapped
			if (rotationDegrees == 90 || rotationDegrees == 270)
			{
				// Swap width and height for orientation calculation
				return height >= width ? DisplayOrientation.Landscape : DisplayOrientation.Portrait;
			}
			else
			{
				// 0° and 180° rotations don't change the orientation
				return width >= height ? DisplayOrientation.Landscape : DisplayOrientation.Portrait;
			}
		}

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
			observer = notificationCenter.AddObserver(new NSString("NSApplicationDidChangeScreenParametersNotification"), OnMainDisplayInfoChanged);
#else
			// On iOS, use status bar orientation changes (deprecated but still works)
			var notification = UIApplication.DidChangeStatusBarOrientationNotification;
			observer = notificationCenter.AddObserver(notification, OnMainDisplayInfoChanged);
#endif
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
