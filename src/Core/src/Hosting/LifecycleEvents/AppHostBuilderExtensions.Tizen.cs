using System;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.LifecycleEvents
{
	public static partial class AppHostBuilderExtensions
	{
		internal static MauiAppBuilder ConfigureCrossPlatformLifecycleEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddTizen(OnConfigureLifeCycle));

		internal static MauiAppBuilder ConfigureWindowEvents(this MauiAppBuilder builder) =>
			builder;

		static void OnConfigureLifeCycle(ITizenLifecycleBuilder tizen)
		{
			tizen
				.OnCreate((app) =>
				{
					// OnCreate is only ever called once when the app is initally created
					app.GetWindow().Created();
				})
				.OnResume(app =>
				{
					app.GetWindow().Resumed();
					app.GetWindow().Activated();

				})
				.OnPause(app =>
				{
					app.GetWindow().Deactivated();
					app.GetWindow().Stopped();
				})
				.OnTerminate(app =>
				{
					app.GetWindow().Destroying();
				});
		}
	}
}
