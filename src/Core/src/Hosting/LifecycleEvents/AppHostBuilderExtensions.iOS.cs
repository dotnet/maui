using System;
using System.Runtime.Versioning;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using UIKit;

namespace Microsoft.Maui.LifecycleEvents
{
	public static partial class AppHostBuilderExtensions
	{
		internal static MauiAppBuilder ConfigureCrossPlatformLifecycleEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddiOS(OnConfigureLifeCycle));

		internal static MauiAppBuilder ConfigureWindowEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddiOS(OnConfigureWindow));

		static void OnConfigureLifeCycle(IiOSLifecycleBuilder iOS)
		{
			iOS = iOS
					.OnPlatformWindowCreated((window) =>
					{
						window.GetWindow()?.Created();
						if (!KeyboardAutoManagerScroll.ShouldDisconnectLifecycle)
							KeyboardAutoManagerScroll.Connect();
					})
					.WillTerminate(app =>
					{
						// By this point if we were a multi window app, the GetWindow would be null anyway
						app.GetWindow()?.Destroying();
						KeyboardAutoManagerScroll.Disconnect();
					})
					.WillEnterForeground(app =>
					{
						if (!app.Delegate.HasSceneManifest())
							app.GetWindow()?.Resumed();
					})
					.OnActivated(app =>
					{
						if (!app.Delegate.HasSceneManifest())
							app.GetWindow()?.Activated();
					})
					.OnResignActivation(app =>
					{
						if (!app.Delegate.HasSceneManifest())
							app.GetWindow()?.Deactivated();
					})
					.DidEnterBackground(app =>
					{
						if (!app.Delegate.HasSceneManifest())
							app.GetWindow()?.Stopped();
					});


			// Pre iOS 13 doesn't support scenes
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;


			iOS
				.SceneWillEnterForeground(scene =>
				{
					if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1)) {
						WindowSceneResumed(scene);
					}
				})
				.SceneOnActivated(scene =>
				{
					if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1)) {
						WindowSceneActivated(scene);
					}
				})
				.SceneOnResignActivation(scene =>
				{
					if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1)) {
						WindowSceneDeactivated(scene);
					}
				})
				.SceneDidEnterBackground(scene =>
				{
					if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1)) {
						WindowSceneStopped(scene);
					}
				})
				.SceneDidDisconnect(scene =>
				{
					if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1)) {
						WindowSceneDestroying(scene);
					}
				});
		}

		static void OnConfigureWindow(IiOSLifecycleBuilder iOS)
		{
			// Pre iOS 13 doesn't support scenes
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;

			iOS = iOS
				.WindowSceneDidUpdateCoordinateSpace((windowScene, _, _, _) =>
				{
					if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1)) {
						OnConfigureWindowImpl(windowScene);
					}
				});
		}
		
		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		static void OnConfigureWindowImpl(UIWindowScene windowScene)
		{
			if (windowScene.Delegate is not IUIWindowSceneDelegate wsd ||
				wsd.GetWindow() is not UIWindow platformWindow)
				return;

			var window = platformWindow.GetWindow();
			if (window is null)
				return;

			window.FrameChanged(platformWindow.Frame.ToRectangle());		
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		static void WindowSceneActivated(UIScene scene)
		{
			if (scene.Delegate is IUIWindowSceneDelegate sd)
				sd.GetWindow().GetWindow()?.Activated();
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		static void WindowSceneDeactivated(UIScene scene)
		{
			if (scene.Delegate is IUIWindowSceneDelegate sd)
				sd.GetWindow().GetWindow()?.Deactivated();
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		static void WindowSceneResumed(UIScene scene)
		{
			if (scene.Delegate is IUIWindowSceneDelegate windowScene &&
				scene.ActivationState != UISceneActivationState.Unattached)
			{
				windowScene.GetWindow().GetWindow()?.Resumed();
			}
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		static void WindowSceneStopped(UIScene scene)
		{
			if (scene.Delegate is IUIWindowSceneDelegate sd)
				sd.GetWindow().GetWindow()?.Stopped();
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		static void WindowSceneDestroying(UIScene scene)
		{
			if (scene.Delegate is IUIWindowSceneDelegate sd)
				sd.GetWindow().GetWindow()?.Destroying();
		}
	}
}
