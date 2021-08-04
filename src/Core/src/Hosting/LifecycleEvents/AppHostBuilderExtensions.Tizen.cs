using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using System;

namespace Microsoft.Maui.LifecycleEvents
{ 
	public static partial class AppHostBuilderExtensions
	{
		internal static IAppHostBuilder ConfigureCrossPlatformLifecycleEvents(this IAppHostBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddTizen(OnConfigureLifeCycle));

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
