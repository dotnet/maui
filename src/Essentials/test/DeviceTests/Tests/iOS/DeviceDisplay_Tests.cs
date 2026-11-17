#if IOS
#nullable enable
using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using UIKit;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public sealed class IOS27FactAttribute : FactAttribute
	{
		public IOS27FactAttribute()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(27))
				Skip = "Requires scene geometry on iOS 27 or later.";
		}
	}

	public sealed class IOS27TheoryAttribute : TheoryAttribute
	{
		public IOS27TheoryAttribute()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(27))
				Skip = "Requires scene geometry on iOS 27 or later.";
		}
	}

	public sealed class LegacyDisplayFactAttribute : FactAttribute
	{
		public LegacyDisplayFactAttribute()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(16) || OperatingSystem.IsIOSVersionAtLeast(27))
				Skip = "Requires the legacy display listener and scene geometry requests.";
		}
	}

	[CollectionDefinition(CollectionName, DisableParallelization = true)]
	public sealed class DeviceDisplayGeometryCollection
	{
		public const string CollectionName = "DeviceDisplay geometry";
	}

	[Collection(DeviceDisplayGeometryCollection.CollectionName)]
	[Category("DeviceDisplay")]
	public class DeviceDisplay_iOS_Tests
	{
		[Theory(DisplayName = nameof(InterfaceOrientationCorrectsStaleBounds))]
		[InlineData(UIInterfaceOrientation.Portrait, 874, 402, 402, 874)]
		[InlineData(UIInterfaceOrientation.PortraitUpsideDown, 874, 402, 402, 874)]
		[InlineData(UIInterfaceOrientation.LandscapeLeft, 402, 874, 874, 402)]
		[InlineData(UIInterfaceOrientation.LandscapeRight, 402, 874, 874, 402)]
		[InlineData(UIInterfaceOrientation.Unknown, 874, 402, 874, 402)]
		public void InterfaceOrientationCorrectsStaleBounds(
			UIInterfaceOrientation orientation, int currentWidth, int currentHeight, int expectedWidth, int expectedHeight)
		{
			var current = new CGRect(0, 0, currentWidth, currentHeight);
			var fixedBounds = new CGRect(0, 0, 402, 874);
			var result = DeviceDisplayImplementation.GetOrientedBounds(current, fixedBounds, orientation);
			Assert.Equal((double)expectedWidth, (double)result.Width);
			Assert.Equal((double)expectedHeight, (double)result.Height);
		}

		[Fact(DisplayName = nameof(SceneSelectionPrefersAvailableForegroundScene))]
		public void SceneSelectionPrefersAvailableForegroundScene()
		{
			var active = new SceneState(UISceneActivationState.ForegroundActive);
			var anotherActive = new SceneState(UISceneActivationState.ForegroundActive);
			var inactive = new SceneState(UISceneActivationState.ForegroundInactive);
			var anotherInactive = new SceneState(UISceneActivationState.ForegroundInactive);
			var background = new SceneState(UISceneActivationState.Background);
			var unattached = new SceneState(UISceneActivationState.Unattached);
			var anotherUnattached = new SceneState(UISceneActivationState.Unattached);

			Assert.Same(active, SelectScene(background, background, active));
			Assert.Same(inactive, SelectScene(unattached, unattached, inactive));
			Assert.Same(inactive, SelectScene(background, background, inactive));
			Assert.Same(active, SelectScene(active, active, anotherActive));
			Assert.Same(background, SelectScene(background, background, active, anotherActive));
			Assert.Null(SelectScene(unattached, unattached, inactive, anotherInactive));
			Assert.Same(unattached, SelectScene(unattached, unattached));
			Assert.Null(SelectScene(unattached, unattached, anotherUnattached));
			Assert.Null(SelectScene(null));

			static SceneState? SelectScene(SceneState? current, params SceneState[] candidates) =>
				DeviceDisplayImplementation.SelectScene(current, candidates, static candidate => candidate.ActivationState);
		}

		[IOS27Fact(DisplayName = nameof(MainDisplayInfoUsesActiveSceneScreen))]
		public Task MainDisplayInfoUsesActiveSceneScreen() =>
			Utils.OnMainThread(() =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(27))
					throw new PlatformNotSupportedException("Requires scene geometry on iOS 27 or later.");

				var scene = WindowStateManager.Default.GetCurrentUIWindow()?.WindowScene;
				Assert.NotNull(scene);
				AssertMatchesSceneScreen(scene, new DeviceDisplayImplementation().MainDisplayInfo);
			});

		[LegacyDisplayFact(DisplayName = nameof(LegacyStatusBarNotificationUpdatesScreenBounds))]
		public Task LegacyStatusBarNotificationUpdatesScreenBounds() =>
			Utils.OnMainThread(async () =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(16) || OperatingSystem.IsIOSVersionAtLeast(27))
					throw new PlatformNotSupportedException("Requires the legacy display listener and scene geometry requests.");

				var scene = WindowStateManager.Default.GetCurrentUIWindow()?.WindowScene;
				Assert.NotNull(scene);
#pragma warning disable CA1422 // The legacy runtime exposes interface orientation on the scene.
				var originalOrientation = scene.InterfaceOrientation;
#pragma warning restore CA1422
				var display = new DeviceDisplayImplementation();
				DisplayInfo? received = null;
				var eventCount = 0;
				var physicalNotifications = 0;
				using var physicalObserver = NSNotificationCenter.DefaultCenter.AddObserver(
					UIDevice.OrientationDidChangeNotification, _ => physicalNotifications++);
				void OnDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs args)
				{
					eventCount++;
					received = args.DisplayInfo;
				}

				try
				{
					await RequestLegacyOrientation(scene, UIInterfaceOrientation.Portrait);
					display.MainDisplayInfoChanged += OnDisplayInfoChanged;
					var initial = display.MainDisplayInfo;
					var initialPhysicalNotifications = physicalNotifications;
					await RequestLegacyOrientation(scene, UIInterfaceOrientation.LandscapeRight);
					var current = display.MainDisplayInfo;
					Assert.NotEqual(initial.Width, current.Width);
					Assert.Equal(initialPhysicalNotifications, physicalNotifications);
#pragma warning disable CA1422 // Exercise the notification used by legacy applications.
					NSNotificationCenter.DefaultCenter.PostNotificationName(UIApplication.DidChangeStatusBarOrientationNotification, null);
#pragma warning restore CA1422
					Assert.NotNull(received);
					Assert.Equal(current, received.Value);

					var updatedEvents = eventCount;
					display.MainDisplayInfoChanged -= OnDisplayInfoChanged;
					await RequestLegacyOrientation(scene, UIInterfaceOrientation.Portrait);
#pragma warning disable CA1422
					NSNotificationCenter.DefaultCenter.PostNotificationName(UIApplication.DidChangeStatusBarOrientationNotification, null);
#pragma warning restore CA1422
					Assert.Equal(updatedEvents, eventCount);
				}
				finally
				{
					display.MainDisplayInfoChanged -= OnDisplayInfoChanged;
					if (originalOrientation != UIInterfaceOrientation.Unknown)
						await RequestLegacyOrientation(scene, originalOrientation);
				}
			});

		[IOS27Fact(DisplayName = nameof(ListenerAttachesWhenWindowBecomesAvailable))]
		public Task ListenerAttachesWhenWindowBecomesAvailable() =>
			Utils.OnMainThread(async () =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(27))
					throw new PlatformNotSupportedException("Requires scene geometry on iOS 27 or later.");

				var originalManager = WindowStateManager.Default;
				var window = originalManager.GetCurrentUIWindow();
				Assert.NotNull(window);
				var scene = window.WindowScene;
				Assert.NotNull(scene);
				var originalOrientation = scene.EffectiveGeometry.InterfaceOrientation;
				var manager = new TestWindowStateManager();
				var display = new DeviceDisplayImplementation();
				DisplayInfo? received = null;
				TaskCompletionSource<DisplayInfo>? changed = null;
				var eventCount = 0;
				void OnDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs args)
				{
					eventCount++;
					received = args.DisplayInfo;
					changed?.TrySetResult(args.DisplayInfo);
				}

				try
				{
					await RequestOrientation(scene, UIInterfaceOrientation.Portrait);
					WindowStateManager.SetDefault(manager);
					display.MainDisplayInfoChanged += OnDisplayInfoChanged;
					var fallback = display.MainDisplayInfo;

					manager.Window = window;
					NSNotificationCenter.DefaultCenter.PostNotificationName(UIWindow.DidBecomeKeyNotification, window);
					var initialEvents = fallback.Equals(display.MainDisplayInfo) ? 0 : 1;
					Assert.Equal(initialEvents, eventCount);
					AssertMatchesSceneScreen(scene, display.MainDisplayInfo);
					if (received is { } info)
						AssertMatchesSceneScreen(scene, info);

					changed = new TaskCompletionSource<DisplayInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
					await RequestOrientation(scene, UIInterfaceOrientation.LandscapeRight);
					var completed = await Task.WhenAny(changed.Task, Task.Delay(TimeSpan.FromSeconds(10)));
					Assert.True(completed == changed.Task, "No MainDisplayInfoChanged event after attaching to the scene.");
					Assert.Equal(initialEvents + 1, eventCount);
					AssertMatchesSceneScreen(scene, await changed.Task);

					manager.Window = null;
					NSNotificationCenter.DefaultCenter.PostNotificationName(UIScene.DidDisconnectNotification, scene);
					await Task.Delay(100);
					AssertMatchesLegacyScreen(display.MainDisplayInfo);

					changed = new TaskCompletionSource<DisplayInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
					await RequestOrientation(scene, UIInterfaceOrientation.Portrait);
					NSNotificationCenter.DefaultCenter.PostNotificationName(UIDevice.OrientationDidChangeNotification, UIDevice.CurrentDevice);
					completed = await Task.WhenAny(changed.Task, Task.Delay(TimeSpan.FromSeconds(10)));
					Assert.True(completed == changed.Task, "No legacy display event after the scene disconnected.");
					AssertMatchesLegacyScreen(await changed.Task);

					// Queue a fallback update, then reconnect before it can run.
					NSNotificationCenter.DefaultCenter.PostNotificationName(UIDevice.OrientationDidChangeNotification, UIDevice.CurrentDevice);
					manager.Window = window;
					NSNotificationCenter.DefaultCenter.PostNotificationName(UIWindow.DidBecomeKeyNotification, window);
					var reconnectedEvents = eventCount;
					await Task.Delay(100);
					Assert.Equal(reconnectedEvents, eventCount);
					AssertMatchesSceneScreen(scene, display.MainDisplayInfo);

					changed = new TaskCompletionSource<DisplayInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
					await RequestOrientation(scene, UIInterfaceOrientation.LandscapeLeft);
					completed = await Task.WhenAny(changed.Task, Task.Delay(TimeSpan.FromSeconds(10)));
					Assert.True(completed == changed.Task, "No scene geometry event after reconnection.");
					AssertMatchesSceneScreen(scene, await changed.Task);
					Assert.Equal(reconnectedEvents + 1, eventCount);
				}
				finally
				{
					display.MainDisplayInfoChanged -= OnDisplayInfoChanged;
					WindowStateManager.SetDefault(originalManager);
					if (scene.EffectiveGeometry.InterfaceOrientation != originalOrientation)
						await RequestOrientation(scene, originalOrientation);
				}
			});

		[IOS27Theory(DisplayName = nameof(ListenerRebindsOnSceneLifecycleChanges))]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(4)]
		public Task ListenerRebindsOnSceneLifecycleChanges(int notificationKind) =>
			Utils.OnMainThread(async () =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(27))
					throw new PlatformNotSupportedException("Requires scene geometry on iOS 27 or later.");

				var originalManager = WindowStateManager.Default;
				var window = originalManager.GetCurrentUIWindow();
				Assert.NotNull(window);
				var scene = window.WindowScene;
				Assert.NotNull(scene);
				var originalOrientation = scene.EffectiveGeometry.InterfaceOrientation;
				var manager = new TestWindowStateManager();
				var display = new DeviceDisplayImplementation();
				var changed = new TaskCompletionSource<DisplayInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
				var physicalNotifications = 0;
				var eventCount = 0;
				using var physicalObserver = NSNotificationCenter.DefaultCenter.AddObserver(
					UIDevice.OrientationDidChangeNotification, _ => physicalNotifications++);
				void OnDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs args)
				{
					eventCount++;
					changed.TrySetResult(args.DisplayInfo);
				}

				try
				{
					await RequestOrientation(scene, UIInterfaceOrientation.Portrait);
					WindowStateManager.SetDefault(manager);
					display.MainDisplayInfoChanged += OnDisplayInfoChanged;
					await Task.Delay(500);
					manager.Window = window;
					var notification = notificationKind switch
					{
						0 => UIScene.WillDeactivateNotification,
						1 => UIScene.DidEnterBackgroundNotification,
						2 => UIScene.WillEnterForegroundNotification,
						3 => UIScene.DidDisconnectNotification,
						_ => UIWindow.DidBecomeVisibleNotification,
					};
					var source = notificationKind == 4 ? (NSObject)window : scene;
					NSNotificationCenter.DefaultCenter.PostNotificationName(notification, source);
					await Task.Delay(500);
					changed = new TaskCompletionSource<DisplayInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
					var initialPhysicalNotifications = physicalNotifications;
					await RequestOrientation(scene, UIInterfaceOrientation.LandscapeRight);
					var completed = await Task.WhenAny(changed.Task, Task.Delay(TimeSpan.FromSeconds(2)));
					Assert.True(completed == changed.Task, $"No scene geometry event after {notification}.");
					AssertMatchesSceneScreen(scene, await changed.Task);
					Assert.Equal(initialPhysicalNotifications, physicalNotifications);
					var stoppedCount = eventCount;
					NSNotificationCenter.DefaultCenter.PostNotificationName(notification, source);
					display.MainDisplayInfoChanged -= OnDisplayInfoChanged;
					await RequestOrientation(scene, UIInterfaceOrientation.Portrait);
					Assert.Equal(stoppedCount, eventCount);
				}
				finally
				{
					display.MainDisplayInfoChanged -= OnDisplayInfoChanged;
					WindowStateManager.SetDefault(originalManager);
					if (scene.EffectiveGeometry.InterfaceOrientation != originalOrientation)
						await RequestOrientation(scene, originalOrientation);
				}
			});

		[IOS27Theory(DisplayName = nameof(LegacyFallbackTracksInterfaceOrientation))]
		[InlineData(false)]
		[InlineData(true)]
		public Task LegacyFallbackTracksInterfaceOrientation(bool restartListeners) =>
			Utils.OnMainThread(async () =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(27))
					throw new PlatformNotSupportedException("Requires scene geometry on iOS 27 or later.");

				var originalManager = WindowStateManager.Default;
				var scene = originalManager.GetCurrentUIWindow()?.WindowScene;
				Assert.NotNull(scene);
				var originalOrientation = scene.EffectiveGeometry.InterfaceOrientation;
				var display = new DeviceDisplayImplementation();
				TaskCompletionSource<DisplayInfo>? changed = null;
				var eventCount = 0;
				void OnDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs args)
				{
					eventCount++;
					changed?.TrySetResult(args.DisplayInfo);
				}

				try
				{
					await RequestOrientation(scene, UIInterfaceOrientation.Portrait);
					WindowStateManager.SetDefault(new TestWindowStateManager());
					AssertMatchesLegacyScreen(display.MainDisplayInfo);
					display.MainDisplayInfoChanged += OnDisplayInfoChanged;
					if (restartListeners)
					{
						NSNotificationCenter.DefaultCenter.PostNotificationName(UIDevice.OrientationDidChangeNotification, UIDevice.CurrentDevice);
						display.MainDisplayInfoChanged -= OnDisplayInfoChanged;
						display.MainDisplayInfoChanged += OnDisplayInfoChanged;
					}

					foreach (var orientation in new[]
					{
						UIInterfaceOrientation.LandscapeLeft,
						UIInterfaceOrientation.Portrait,
						UIInterfaceOrientation.LandscapeRight,
						UIInterfaceOrientation.Portrait,
					})
					{
						changed = new TaskCompletionSource<DisplayInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
						var previousCount = eventCount;
						var previousInfo = display.MainDisplayInfo;
						await RequestOrientation(scene, orientation);
						// Exercise the legacy notification path while the managed window is unavailable.
						NSNotificationCenter.DefaultCenter.PostNotificationName(UIDevice.OrientationDidChangeNotification, UIDevice.CurrentDevice);
						var completed = await Task.WhenAny(changed.Task, Task.Delay(TimeSpan.FromSeconds(10)));
						Assert.True(completed == changed.Task,
							$"No display event from the legacy fallback for {orientation}; before: {previousInfo}; after: {display.MainDisplayInfo}; device orientation: {UIDevice.CurrentDevice.Orientation}.");
						var info = await changed.Task;
						Assert.Equal(display.MainDisplayInfo, info);
						AssertMatchesLegacyScreen(info);
						Assert.Equal(previousCount + 1, eventCount);
					}

					display.MainDisplayInfoChanged -= OnDisplayInfoChanged;
					var stoppedCount = eventCount;
					await RequestOrientation(scene, UIInterfaceOrientation.LandscapeLeft);
					NSNotificationCenter.DefaultCenter.PostNotificationName(UIDevice.OrientationDidChangeNotification, UIDevice.CurrentDevice);
					await Task.Delay(100);
					Assert.Equal(stoppedCount, eventCount);
				}
				finally
				{
					display.MainDisplayInfoChanged -= OnDisplayInfoChanged;
					WindowStateManager.SetDefault(originalManager);
					if (scene.EffectiveGeometry.InterfaceOrientation != originalOrientation)
						await RequestOrientation(scene, originalOrientation);
				}
			});

		[IOS27Theory(DisplayName = nameof(SceneGeometryChangesRaiseDisplayInfoChanged))]
		[InlineData(false)]
		[InlineData(true)]
		public Task SceneGeometryChangesRaiseDisplayInfoChanged(bool restartListeners) =>
			Utils.OnMainThread(async () =>
			{
				if (!OperatingSystem.IsIOSVersionAtLeast(27))
					throw new PlatformNotSupportedException("Requires scene geometry on iOS 27 or later.");

				var scene = WindowStateManager.Default.GetCurrentUIWindow()?.WindowScene;
				Assert.NotNull(scene);
				var originalOrientation = scene.EffectiveGeometry.InterfaceOrientation;
				var display = new DeviceDisplayImplementation();
				var physicalNotifications = 0;
				using var physicalObserver = NSNotificationCenter.DefaultCenter.AddObserver(
					UIDevice.OrientationDidChangeNotification, _ => physicalNotifications++);
				TaskCompletionSource<DisplayInfo>? changed = null;
				var eventCount = 0;
				void OnDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs args)
				{
					eventCount++;
					changed?.TrySetResult(args.DisplayInfo);
				}

				try
				{
					await RequestOrientation(scene, UIInterfaceOrientation.Portrait);
					display.MainDisplayInfoChanged += OnDisplayInfoChanged;
					if (restartListeners)
					{
						display.MainDisplayInfoChanged -= OnDisplayInfoChanged;
						display.MainDisplayInfoChanged += OnDisplayInfoChanged;
					}

					foreach (var orientation in new[]
					{
						UIInterfaceOrientation.LandscapeRight,
						UIInterfaceOrientation.Portrait,
						UIInterfaceOrientation.LandscapeLeft,
						UIInterfaceOrientation.Portrait,
					})
					{
						changed = new TaskCompletionSource<DisplayInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
						var previousCount = eventCount;
						var previousPhysicalNotifications = physicalNotifications;
						var physicalOrientation = UIDevice.CurrentDevice.Orientation;

						// A geometry request does not rotate the physical device or post its orientation notification.
						await RequestOrientation(scene, orientation);
						var completed = await Task.WhenAny(changed.Task, Task.Delay(TimeSpan.FromSeconds(10)));
						Assert.True(completed == changed.Task,
							$"No MainDisplayInfoChanged event for scene orientation {orientation}.");
						var info = await changed.Task;
						Assert.Equal(display.MainDisplayInfo, info);
						AssertMatchesSceneScreen(scene, info);
						Assert.Equal(previousCount + 1, eventCount);
						Assert.Equal(previousPhysicalNotifications, physicalNotifications);
						Assert.Equal(physicalOrientation, UIDevice.CurrentDevice.Orientation);
						Assert.Equal(orientation == UIInterfaceOrientation.Portrait
							? DisplayOrientation.Portrait : DisplayOrientation.Landscape, info.Orientation);
						Assert.Equal(orientation switch
						{
							UIInterfaceOrientation.LandscapeRight => DisplayRotation.Rotation90,
							UIInterfaceOrientation.LandscapeLeft => DisplayRotation.Rotation270,
							_ => DisplayRotation.Rotation0,
						}, info.Rotation);

						await RequestOrientation(scene, orientation);
						Assert.Equal(previousCount + 1, eventCount);
					}
				}
				finally
				{
					display.MainDisplayInfoChanged -= OnDisplayInfoChanged;
					if (scene.EffectiveGeometry.InterfaceOrientation != originalOrientation)
						await RequestOrientation(scene, originalOrientation);
				}
			});

		static void AssertMatchesLegacyScreen(DisplayInfo info)
		{
#pragma warning disable CA1422 // Verify the orientation source used by legacy applications.
			var orientation = UIApplication.SharedApplication.StatusBarOrientation;
#pragma warning restore CA1422
			var expectedOrientation = orientation switch
			{
				UIInterfaceOrientation.Portrait or UIInterfaceOrientation.PortraitUpsideDown => DisplayOrientation.Portrait,
				UIInterfaceOrientation.LandscapeLeft or UIInterfaceOrientation.LandscapeRight => DisplayOrientation.Landscape,
				_ => UIDevice.CurrentDevice.Orientation switch
				{
					UIDeviceOrientation.Portrait or UIDeviceOrientation.PortraitUpsideDown => DisplayOrientation.Portrait,
					UIDeviceOrientation.LandscapeLeft or UIDeviceOrientation.LandscapeRight => DisplayOrientation.Landscape,
					_ => DisplayOrientation.Unknown,
				},
			};
			var expectedRotation = orientation switch
			{
				UIInterfaceOrientation.Portrait => DisplayRotation.Rotation0,
				UIInterfaceOrientation.PortraitUpsideDown => DisplayRotation.Rotation180,
				UIInterfaceOrientation.LandscapeLeft => DisplayRotation.Rotation270,
				UIInterfaceOrientation.LandscapeRight => DisplayRotation.Rotation90,
				_ => UIDevice.CurrentDevice.Orientation switch
				{
					UIDeviceOrientation.Portrait => DisplayRotation.Rotation0,
					UIDeviceOrientation.PortraitUpsideDown => DisplayRotation.Rotation180,
					UIDeviceOrientation.LandscapeLeft => DisplayRotation.Rotation90,
					UIDeviceOrientation.LandscapeRight => DisplayRotation.Rotation270,
					_ => DisplayRotation.Unknown,
				},
			};
			Assert.Equal(expectedOrientation, info.Orientation);
			Assert.Equal(expectedRotation, info.Rotation);
			Assert.Equal((double)(UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale), info.Width);
			Assert.Equal((double)(UIScreen.MainScreen.Bounds.Height * UIScreen.MainScreen.Scale), info.Height);
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios27.0")]
		internal static void AssertMatchesSceneScreen(UIWindowScene scene, DisplayInfo info)
		{
			var screen = scene.Screen;
			var bounds = screen.Bounds;
			Assert.Equal((double)(bounds.Width * screen.Scale), info.Width);
			Assert.Equal((double)(bounds.Height * screen.Scale), info.Height);
			Assert.Equal((double)screen.Scale, info.Density);
			Assert.Equal((float)screen.MaximumFramesPerSecond, info.RefreshRate);
			var orientation = scene.EffectiveGeometry.InterfaceOrientation;
			Assert.Equal(orientation switch
			{
				UIInterfaceOrientation.Portrait or UIInterfaceOrientation.PortraitUpsideDown => DisplayOrientation.Portrait,
				UIInterfaceOrientation.LandscapeLeft or UIInterfaceOrientation.LandscapeRight => DisplayOrientation.Landscape,
				_ => DisplayOrientation.Unknown,
			}, info.Orientation);
			Assert.Equal(orientation switch
			{
				UIInterfaceOrientation.Portrait => DisplayRotation.Rotation0,
				UIInterfaceOrientation.PortraitUpsideDown => DisplayRotation.Rotation180,
				UIInterfaceOrientation.LandscapeRight => DisplayRotation.Rotation90,
				UIInterfaceOrientation.LandscapeLeft => DisplayRotation.Rotation270,
				_ => DisplayRotation.Unknown,
			}, info.Rotation);
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios27.0")]
		static async Task RequestOrientation(UIWindowScene scene, UIInterfaceOrientation orientation)
		{
			using var preferences = new UIWindowSceneGeometryPreferencesIOS(orientation switch
			{
				UIInterfaceOrientation.LandscapeRight => UIInterfaceOrientationMask.LandscapeRight,
				UIInterfaceOrientation.LandscapeLeft => UIInterfaceOrientationMask.LandscapeLeft,
				UIInterfaceOrientation.PortraitUpsideDown => UIInterfaceOrientationMask.PortraitUpsideDown,
				_ => UIInterfaceOrientationMask.Portrait,
			});
			var request = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			scene.RequestGeometryUpdate(preferences, error => request.TrySetException(
				new InvalidOperationException($"Unable to request {orientation}: {error}, details: {error.UserInfo}")));
			var deadline = DateTime.UtcNow.AddSeconds(10);
			while (scene.EffectiveGeometry.InterfaceOrientation != orientation && !request.Task.IsFaulted && DateTime.UtcNow < deadline)
				await Task.Delay(50);
			if (request.Task.IsFaulted)
				await request.Task;
			Assert.Equal(orientation, scene.EffectiveGeometry.InterfaceOrientation);
			await Task.Delay(500);
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios16.0")]
		static async Task RequestLegacyOrientation(UIWindowScene scene, UIInterfaceOrientation orientation)
		{
			using var preferences = new UIWindowSceneGeometryPreferencesIOS(orientation switch
			{
				UIInterfaceOrientation.LandscapeRight => UIInterfaceOrientationMask.LandscapeRight,
				UIInterfaceOrientation.LandscapeLeft => UIInterfaceOrientationMask.LandscapeLeft,
				UIInterfaceOrientation.PortraitUpsideDown => UIInterfaceOrientationMask.PortraitUpsideDown,
				_ => UIInterfaceOrientationMask.Portrait,
			});
			var request = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			scene.RequestGeometryUpdate(preferences, error => request.TrySetException(new InvalidOperationException(error.ToString())));
			var deadline = DateTime.UtcNow.AddSeconds(10);
#pragma warning disable CA1422 // The legacy runtime exposes interface orientation on the scene.
			while (scene.InterfaceOrientation != orientation && !request.Task.IsFaulted && DateTime.UtcNow < deadline)
				await Task.Delay(50);
			if (request.Task.IsFaulted)
				await request.Task;
			Assert.Equal(orientation, scene.InterfaceOrientation);
#pragma warning restore CA1422
			await Task.Delay(500);
		}

		sealed class TestWindowStateManager : IWindowStateManager
		{
			public UIWindow? Window { get; set; }

			public void Init(Func<UIViewController?>? getCurrentUIViewController) { }

			public UIViewController? GetCurrentUIViewController() => Window?.RootViewController;

			public UIWindow? GetCurrentUIWindow() => Window;
		}

		sealed class SceneState
		{
			public SceneState(UISceneActivationState activationState) => ActivationState = activationState;

			public UISceneActivationState ActivationState { get; }
		}
	}
}
#endif
