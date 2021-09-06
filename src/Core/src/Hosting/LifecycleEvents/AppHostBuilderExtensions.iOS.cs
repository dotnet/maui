using System;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.LifecycleEvents
{
	public static partial class AppHostBuilderExtensions
	{
		internal static MauiAppBuilder ConfigureCrossPlatformLifecycleEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddiOS(OnConfigureLifeCycle));

		static void OnConfigureLifeCycle(IiOSLifecycleBuilder iOS)
		{
			iOS
				.FinishedLaunching((app, launchOptions) =>
				{
					app.GetWindow()?.Created();
					return true;
				})
				.WillEnterForeground(app =>
				{
					app.GetWindow()?.Resumed();
				})
				.OnActivated(app =>
				{
					app.GetWindow()?.Activated();
				})
				.OnResignActivation(app =>
				{
					app.GetWindow()?.Deactivated();
				})
				.DidEnterBackground(app =>
				{
					app.GetWindow()?.Stopped();
				})
				.WillTerminate(app =>
				{
					app.GetWindow()?.Destroying();
				});
		}
	}
}
