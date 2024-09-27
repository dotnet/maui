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
					Console.WriteLine("TJ SceneWillEnterForeground");
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
					Console.WriteLine("TJ SceneOnActivated");
					if (!OperatingSystem.IsIOSVersionAtLeast(13))
						return;

					if (scene.Delegate is IUIWindowSceneDelegate sd)
						sd.GetWindow().GetWindow()?.Activated();
				})
				.SceneOnResignActivation(scene =>
				{
					Console.WriteLine("TJ SceneOnResignActivation");
					if (!OperatingSystem.IsIOSVersionAtLeast(13))
						return;

					if (scene.Delegate is IUIWindowSceneDelegate sd)
						sd.GetWindow().GetWindow()?.Deactivated();
				})
				.SceneDidEnterBackground(scene =>
				{
					Console.WriteLine("TJ SceneDidEnterBackground");
					if (!OperatingSystem.IsIOSVersionAtLeast(13))
						return;

					if (scene.Delegate is IUIWindowSceneDelegate sd)
						sd.GetWindow().GetWindow()?.Stopped();
				})
				.SceneDidDisconnect(scene =>
				{
					Console.WriteLine("TJ SceneDidDisconnect");
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
					Console.WriteLine("TJ in AppHostBuilderExtensions");
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
