using System;
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
						app.GetFirstWindow()?.Destroying();
						KeyboardAutoManagerScroll.Disconnect();
					})
					.WillEnterForeground(app =>
					{
						if (!app.Delegate.HasSceneManifest())
							app.GetFirstWindow()?.Resumed();
					})
					.OnActivated(app =>
					{
						if (!app.Delegate.HasSceneManifest())
							app.GetFirstWindow()?.Activated();
					})
					.OnResignActivation(app =>
					{
						if (!app.Delegate.HasSceneManifest())
							app.GetFirstWindow()?.Deactivated();
					})
					.DidEnterBackground(app =>
					{
						if (!app.Delegate.HasSceneManifest())
							app.GetFirstWindow()?.Stopped();
					});


			// Pre iOS 13 doesn't support scenes
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;


			iOS
				.SceneWillEnterForeground(scene =>
				{
					if (!OperatingSystem.IsIOSVersionAtLeast(13))
						return;

					if (scene.Delegate is IUIWindowSceneDelegate windowScene &&
						scene.ActivationState != UISceneActivationState.Unattached)
					{
						windowScene.GetWindow().GetWindow()?.Resumed();
					}
				})
				.SceneOnActivated(scene =>
				{
					if (!OperatingSystem.IsIOSVersionAtLeast(13))
						return;

					if (scene.Delegate is IUIWindowSceneDelegate sd)
						sd.GetWindow().GetWindow()?.Activated();
				})
				.SceneOnResignActivation(scene =>
				{
					if (!OperatingSystem.IsIOSVersionAtLeast(13))
						return;

					if (scene.Delegate is IUIWindowSceneDelegate sd)
						sd.GetWindow().GetWindow()?.Deactivated();
				})
				.SceneDidEnterBackground(scene =>
				{
					if (!OperatingSystem.IsIOSVersionAtLeast(13))
						return;

					if (scene.Delegate is IUIWindowSceneDelegate sd)
						sd.GetWindow().GetWindow()?.Stopped();
				})
				.SceneDidDisconnect(scene =>
				{
					if (!OperatingSystem.IsIOSVersionAtLeast(13))
						return;

					if (scene.Delegate is IUIWindowSceneDelegate sd)
						sd.GetWindow().GetWindow()?.Destroying();
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
					if (!OperatingSystem.IsIOSVersionAtLeast(13))
						return;

					if (windowScene.Delegate is not IUIWindowSceneDelegate wsd ||
						wsd.GetWindow() is not UIWindow platformWindow)
						return;

					var window = platformWindow.GetWindow();
					if (window is null)
						return;

					window.FrameChanged(platformWindow.Frame.ToRectangle());
				});
		}
	}
}
