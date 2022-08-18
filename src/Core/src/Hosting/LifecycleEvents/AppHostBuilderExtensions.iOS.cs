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

		static void OnConfigureLifeCycle(IiOSLifecycleBuilder iOS)
		{
			iOS = iOS
					.OnPlatformWindowCreated((window) =>
					{
						window.GetWindow()?.Created();
					})
					.WillTerminate(app =>
					{
						// By this point if we were a multi window app, the GetWindow would be null anyway
						app.GetWindow()?.Destroying();
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

			// Scenes
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
			{
				// Pre iOS 13 doesn't support scenes
			}
			else
			{
				iOS
					.SceneWillEnterForeground(scene =>
					{
						if (scene is UIWindowScene windowScene)
							windowScene.GetWindow()?.Resumed();
					})
					.SceneOnActivated(scene =>
					{
						if (scene is UIWindowScene windowScene)
							windowScene.GetWindow()?.Activated();
					})
					.SceneOnResignActivation(scene =>
					{
						if (scene is UIWindowScene windowScene)
							windowScene.GetWindow()?.Deactivated();
					})
					.SceneDidEnterBackground(scene =>
					{
						if (scene is UIWindowScene windowScene)
							windowScene.GetWindow()?.Stopped();
					})
					.SceneDidDisconnect(scene =>
					{
						if (scene is UIWindowScene windowScene)
							windowScene.GetWindow()?.Destroying();
					});
			}
		}
	}
}
