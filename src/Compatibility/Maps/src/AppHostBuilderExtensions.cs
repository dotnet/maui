#nullable enable
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Extensions.DependencyInjection;

#if __ANDROID__
using MapRenderer = Microsoft.Maui.Controls.Compatibility.Maps.Android.MapRenderer;
#elif __IOS__
using MapRenderer = Microsoft.Maui.Controls.Compatibility.Maps.iOS.MapRenderer;
#endif

namespace Microsoft.Maui.Controls.Hosting
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureMaps(this IAppHostBuilder builder)
		{
			builder.ConfigureLifecycleEvents(events =>
			{
#if __ANDROID__
				events.AddAndroid(android => android
					.OnCreate((activity, bundle) =>
					{
						FormsMaps.Init(activity, bundle);

						var services = MauiApplication.Current.Services;
						var handlersCollection = services.GetRequiredService<IMauiHandlersServiceProvider>().GetCollection();
						handlersCollection.TryAddCompatibilityRenderer(typeof(Map), typeof(MapRenderer));
					}));
#elif __IOS__
				events.AddiOS(iOS => iOS
					.WillFinishLaunching((app, options) =>
					{
						FormsMaps.Init();

						var services = MauiUIApplicationDelegate.Current.Services;
						var handlersCollection = services.GetRequiredService<IMauiHandlersServiceProvider>().GetCollection();
						handlersCollection.TryAddCompatibilityRenderer(typeof(Map), typeof(MapRenderer));

						return true;
					}));
#endif
			});

			return builder;
		}
	}
}