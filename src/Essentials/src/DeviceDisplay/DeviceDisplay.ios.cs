#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Devices
{
	partial class DeviceDisplayImplementation : IDeviceDisplay
	{
		NSObject? observer;

#if !MACCATALYST
		IDisposable? geometryObserver;
		UIWindowScene? observedScene;
		NSObject? windowVisibleObserver;
		NSObject? sceneActivatedObserver;
		NSObject? sceneDeactivatedObserver;
		NSObject? sceneBackgroundObserver;
		NSObject? sceneForegroundObserver;
		NSObject? sceneDisconnectedObserver;
		NSObject? applicationActivatedObserver;
		NSObject? legacyOrientationObserver;
		NSObject? statusBarOrientationObserver;
		int listenerVersion;
#endif

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
		static extern nuint CGDisplayModeGetPixelWidth(IntPtr mode);

		[DllImport(Constants.CoreGraphicsLibrary)]
		static extern nuint CGDisplayModeGetPixelHeight(IntPtr mode);

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
				// DisplayInfo dimensions are physical pixels, not the mode's logical size.
				var width = (double)CGDisplayModeGetPixelWidth(mode);
				var height = (double)CGDisplayModeGetPixelHeight(mode);
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
#if !MACCATALYST
			if (OperatingSystem.IsIOSVersionAtLeast(27) &&
				GetCurrentScene() is { } scene)
			{
				var screen = scene.Screen;
				var interfaceOrientation = scene.EffectiveGeometry.InterfaceOrientation;
				var sceneBounds = GetOrientedBounds(screen.Bounds, screen.FixedCoordinateSpace.Bounds, interfaceOrientation);
				var sceneScale = screen.Scale;

				// The screen's current bounds can lag behind the scene orientation during a display handoff.
				return new DisplayInfo(
					width: sceneBounds.Width * sceneScale,
					height: sceneBounds.Height * sceneScale,
					density: sceneScale,
					orientation: ConvertInterfaceOrientation(interfaceOrientation),
					rotation: ConvertInterfaceRotation(interfaceOrientation),
					rate: screen.MaximumFramesPerSecond);
			}
#endif
			var mainScreen = UIScreen.MainScreen;
			var bounds = mainScreen.Bounds;
#if !MACCATALYST
			if (!OperatingSystem.IsIOSVersionAtLeast(27) && CanOrientMainScreen(mainScreen))
				bounds = GetOrientedBounds(bounds, mainScreen.FixedCoordinateSpace.Bounds, GetInterfaceOrientation());
#endif
			var scale = mainScreen.Scale;

			var rate = (OperatingSystem.IsIOSVersionAtLeast(10, 3) || OperatingSystem.IsMacCatalystVersionAtLeast(10, 3) || OperatingSystem.IsTvOSVersionAtLeast(10, 3))
				? mainScreen.MaximumFramesPerSecond
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
				listenerVersion++;
				// Interface geometry can change without a physical rotation (for example, when unfolding).
				observer = notificationCenter.AddObserver(UIWindow.DidBecomeKeyNotification, _ => ObserveSceneGeometry());
				windowVisibleObserver = notificationCenter.AddObserver(UIWindow.DidBecomeVisibleNotification, _ => ObserveSceneGeometry());
				sceneActivatedObserver = notificationCenter.AddObserver(UIScene.DidActivateNotification, _ => ObserveSceneGeometry());
				sceneDeactivatedObserver = notificationCenter.AddObserver(UIScene.WillDeactivateNotification, _ => QueueDisplayInfoChanged());
				sceneBackgroundObserver = notificationCenter.AddObserver(UIScene.DidEnterBackgroundNotification, _ => QueueDisplayInfoChanged());
				sceneForegroundObserver = notificationCenter.AddObserver(UIScene.WillEnterForegroundNotification, _ => QueueDisplayInfoChanged());
				applicationActivatedObserver = notificationCenter.AddObserver(UIApplication.DidBecomeActiveNotification, _ => ObserveSceneGeometry());
				sceneDisconnectedObserver = notificationCenter.AddObserver(UIScene.DidDisconnectNotification, notification =>
				{
					if (notification.Object == observedScene)
					{
						listenerVersion++;
						StopObservingSceneGeometry();
					}
					QueueDisplayInfoChanged();
				});
				ObserveSceneGeometry();
				return;
			}

			var notification = UIDevice.OrientationDidChangeNotification;
			observer = notificationCenter.AddObserver(notification, OnMainDisplayInfoChanged);
#pragma warning disable CA1422 // UIKit updates screen bounds by the status bar orientation notification.
			statusBarOrientationObserver = notificationCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification, OnMainDisplayInfoChanged);
#pragma warning restore CA1422
			UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
#endif
		}

		protected override void StopScreenMetricsListeners()
		{
#if !MACCATALYST
			listenerVersion++;
			StopObservingSceneGeometry();
			StopLegacyOrientationListeners();
			if (!OperatingSystem.IsIOSVersionAtLeast(27) && observer is not null)
				UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
			windowVisibleObserver?.Dispose();
			windowVisibleObserver = null;
			sceneActivatedObserver?.Dispose();
			sceneActivatedObserver = null;
			sceneDeactivatedObserver?.Dispose();
			sceneDeactivatedObserver = null;
			sceneBackgroundObserver?.Dispose();
			sceneBackgroundObserver = null;
			sceneForegroundObserver?.Dispose();
			sceneForegroundObserver = null;
			sceneDisconnectedObserver?.Dispose();
			sceneDisconnectedObserver = null;
			applicationActivatedObserver?.Dispose();
			applicationActivatedObserver = null;
#endif
			observer?.Dispose();
			observer = null;
		}

#if !MACCATALYST
		internal static CGRect GetOrientedBounds(CGRect bounds, CGRect fixedBounds, UIInterfaceOrientation orientation) =>
			orientation switch
			{
				UIInterfaceOrientation.LandscapeLeft or UIInterfaceOrientation.LandscapeRight =>
					new CGRect(fixedBounds.X, fixedBounds.Y, fixedBounds.Height, fixedBounds.Width),
				UIInterfaceOrientation.Portrait or UIInterfaceOrientation.PortraitUpsideDown => fixedBounds,
				_ => bounds,
			};

		static bool CanOrientMainScreen(UIScreen mainScreen)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return true;

			// A process-wide status bar orientation may describe a different scene or screen.
			using var scenes = UIApplication.SharedApplication.ConnectedScenes;
			if (scenes.Count > 1)
				return false;

			var scene = WindowStateManager.Default.GetCurrentUIWindow()?.WindowScene;
			return scene is null || scene.Screen == mainScreen;
		}

		void ObserveSceneGeometry()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(27) || observer is null)
				return;

			var scene = GetCurrentScene();
			if (scene != observedScene)
			{
				listenerVersion++;
				StopObservingSceneGeometry();
				StopLegacyOrientationListeners();
				observedScene = scene;
				var version = listenerVersion;
				geometryObserver = scene?.AddObserver("effectiveGeometry", NSKeyValueObservingOptions.New, _ =>
				{
					if (observer is not null && listenerVersion == version && observedScene == scene)
						OnMainDisplayInfoChanged();
				});
			}

			if (scene is null && legacyOrientationObserver is null)
			{
				// This also covers scene-enabled apps before their first window is available.
				var notificationCenter = NSNotificationCenter.DefaultCenter;
				legacyOrientationObserver = notificationCenter.AddObserver(UIDevice.OrientationDidChangeNotification, _ => QueueDisplayInfoChanged());
#pragma warning disable CA1422 // Legacy apps still use the status bar's interface orientation.
				statusBarOrientationObserver = notificationCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification, _ => QueueDisplayInfoChanged());
#pragma warning restore CA1422
				UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
			}

			OnMainDisplayInfoChanged();
		}

		void QueueDisplayInfoChanged()
		{
			var version = listenerVersion;
			UIApplication.SharedApplication.BeginInvokeOnMainThread(() =>
			{
				if (observer is not null && listenerVersion == version)
					ObserveSceneGeometry();
			});
		}

		void StopLegacyOrientationListeners()
		{
			if (legacyOrientationObserver is not null)
			{
				legacyOrientationObserver.Dispose();
				legacyOrientationObserver = null;
				UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
			}
			statusBarOrientationObserver?.Dispose();
			statusBarOrientationObserver = null;
		}

		void StopObservingSceneGeometry()
		{
			geometryObserver?.Dispose();
			geometryObserver = null;
			observedScene = null;
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios27.0")]
		static UIWindowScene? GetCurrentScene()
		{
			var manager = WindowStateManager.Default;
			var scene = manager.GetCurrentUIWindow()?.WindowScene;
			if (manager is not WindowStateManagerImplementation)
			{
				if (scene is null || scene.ActivationState != UISceneActivationState.Unattached)
					return scene;

				using var managerScenes = UIApplication.SharedApplication.ConnectedScenes;
				return scene.Windows.Length > 0 &&
					scene.EffectiveGeometry.InterfaceOrientation != UIInterfaceOrientation.Unknown &&
					managerScenes.ToArray().OfType<UIWindowScene>().Any(candidate => candidate == scene)
					? scene : null;
			}

			if (scene?.ActivationState == UISceneActivationState.ForegroundActive)
				return scene;

			using var connectedScenes = UIApplication.SharedApplication.ConnectedScenes;
			if (connectedScenes.Count == 0)
				return scene?.ActivationState == UISceneActivationState.Unattached ? null : scene;

			var candidates = connectedScenes.ToArray().OfType<UIWindowScene>()
				.Where(candidate => candidate.Session.Role == UIWindowSceneSessionRole.Application && candidate.Windows.Length > 0);
			var selected = SelectScene(scene, candidates, static candidate => candidate.ActivationState);
			return selected?.ActivationState == UISceneActivationState.Unattached &&
				selected.EffectiveGeometry.InterfaceOrientation == UIInterfaceOrientation.Unknown
				? null : selected;
		}

		internal static T? SelectScene<T>(T? current, IEnumerable<T> candidates, Func<T, UISceneActivationState> getState) where T : class
		{
			var currentState = current is null ? UISceneActivationState.Unattached : getState(current);
			if (currentState == UISceneActivationState.ForegroundActive)
				return current;

			T? active = null;
			T? inactive = null;
			T? onlyCandidate = null;
			var candidateCount = 0;
			var multipleActive = false;
			var multipleInactive = false;
			foreach (var candidate in candidates)
			{
				candidateCount++;
				onlyCandidate = candidate;
				switch (getState(candidate))
				{
					case UISceneActivationState.ForegroundActive:
						multipleActive |= active is not null;
						active = candidate;
						break;
					case UISceneActivationState.ForegroundInactive:
						multipleInactive |= inactive is not null;
						inactive = candidate;
						break;
				}
			}

			if (active is not null && !multipleActive)
				return active;
			if (active is null && inactive is not null && !multipleInactive)
				return inactive;
			if (current is not null && currentState != UISceneActivationState.Unattached)
				return current;
			if (candidateCount == 1 && EqualityComparer<T>.Default.Equals(current, onlyCandidate))
				return current;
			return null;
		}
#endif

		void OnMainDisplayInfoChanged(NSNotification obj) =>
			OnMainDisplayInfoChanged();

#pragma warning disable CA1416 // UIApplication.StatusBarOrientation has [UnsupportedOSPlatform("ios9.0")]. (Deprecated but still works)
#pragma warning disable CA1422 // Validate platform compatibility
		static UIInterfaceOrientation GetInterfaceOrientation()
		{
#if !MACCATALYST
			if (OperatingSystem.IsIOSVersionAtLeast(27))
			{
				if (GetCurrentScene() is { } scene)
					return scene.EffectiveGeometry.InterfaceOrientation;

				var orientation = UIApplication.SharedApplication.StatusBarOrientation;
				if (orientation != UIInterfaceOrientation.Unknown)
					return orientation;

				return UIDevice.CurrentDevice.Orientation switch
				{
					UIDeviceOrientation.Portrait => UIInterfaceOrientation.Portrait,
					UIDeviceOrientation.PortraitUpsideDown => UIInterfaceOrientation.PortraitUpsideDown,
					UIDeviceOrientation.LandscapeLeft => UIInterfaceOrientation.LandscapeRight,
					UIDeviceOrientation.LandscapeRight => UIInterfaceOrientation.LandscapeLeft,
					_ => UIInterfaceOrientation.Unknown,
				};
			}
#endif
			return UIApplication.SharedApplication.StatusBarOrientation;
		}

		static DisplayOrientation CalculateOrientation()
		{
#if !MACCATALYST
			if (OperatingSystem.IsIOSVersionAtLeast(27))
				return ConvertInterfaceOrientation(GetInterfaceOrientation());
#endif
			var orientation = UIDevice.CurrentDevice.Orientation;
#if !MACCATALYST
			if (orientation == UIDeviceOrientation.Unknown)
				return ConvertInterfaceOrientation(GetInterfaceOrientation());
#endif
			return orientation.IsLandscape()
				? DisplayOrientation.Landscape
				: orientation == UIDeviceOrientation.PortraitUpsideDown || orientation == UIDeviceOrientation.Portrait ? DisplayOrientation.Portrait : DisplayOrientation.Unknown;
		}

		static DisplayRotation CalculateRotation()
		{
#if !MACCATALYST
			if (OperatingSystem.IsIOSVersionAtLeast(27))
				return ConvertInterfaceRotation(GetInterfaceOrientation());
#endif
			var orientation = UIDevice.CurrentDevice.Orientation;
#if !MACCATALYST
			if (orientation == UIDeviceOrientation.Unknown)
				return ConvertInterfaceRotation(GetInterfaceOrientation());
#endif
			return orientation switch
			{
				UIDeviceOrientation.Portrait => DisplayRotation.Rotation0,
				UIDeviceOrientation.PortraitUpsideDown => DisplayRotation.Rotation180,
				UIDeviceOrientation.LandscapeRight => DisplayRotation.Rotation270,
				UIDeviceOrientation.LandscapeLeft => DisplayRotation.Rotation90,
				_ => DisplayRotation.Unknown,
			};
		}

#if !MACCATALYST
		static DisplayOrientation ConvertInterfaceOrientation(UIInterfaceOrientation orientation) =>
			orientation switch
			{
				UIInterfaceOrientation.LandscapeLeft or UIInterfaceOrientation.LandscapeRight => DisplayOrientation.Landscape,
				UIInterfaceOrientation.Portrait or UIInterfaceOrientation.PortraitUpsideDown => DisplayOrientation.Portrait,
				_ => DisplayOrientation.Unknown,
			};

		static DisplayRotation ConvertInterfaceRotation(UIInterfaceOrientation orientation) =>
			orientation switch
			{
				UIInterfaceOrientation.Portrait => DisplayRotation.Rotation0,
				UIInterfaceOrientation.PortraitUpsideDown => DisplayRotation.Rotation180,
				UIInterfaceOrientation.LandscapeLeft => DisplayRotation.Rotation270,
				UIInterfaceOrientation.LandscapeRight => DisplayRotation.Rotation90,
				_ => DisplayRotation.Unknown,
			};
#endif

#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
	}
}
