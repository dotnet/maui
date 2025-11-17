#nullable enable
using System;
using System.Runtime.InteropServices;
using CoreFoundation;
using Foundation;
using UIKit;

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
				IOKit.PreventUserIdleDisplaySleep("KeepScreenOn", out keepScreenOnId);
			}
			else
			{
				if (IOKit.AllowUserIdleDisplaySleep(keepScreenOnId))
				{
					keepScreenOnId = 0;
				}
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

#if MACCATALYST
	internal static class IOKit
	{
		const string IOKitLibrary = "/System/Library/Frameworks/IOKit.framework/IOKit";
		const uint kIOPMAssertionLevelOn = 255;
		static readonly CFString kIOPMAssertionTypePreventUserIdleDisplaySleep = "PreventUserIdleDisplaySleep";

		[DllImport(IOKitLibrary)]
		static extern uint IOPMAssertionCreateWithName(IntPtr type, uint level, IntPtr name, out uint id);

		[DllImport(IOKitLibrary)]
		static extern uint IOPMAssertionRelease(uint id);

		internal static bool PreventUserIdleDisplaySleep(CFString name, out uint id)
		{
			var result = IOPMAssertionCreateWithName(
				kIOPMAssertionTypePreventUserIdleDisplaySleep.Handle,
				kIOPMAssertionLevelOn,
				name.Handle,
				out var newId);

			id = result == 0 ? newId : 0;
			return result == 0;
		}

		internal static bool AllowUserIdleDisplaySleep(uint id)
		{
			var result = IOPMAssertionRelease(id);
			return result == 0;
		}
	}
#endif
}
