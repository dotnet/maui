using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Samples
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();

			builder
				.UseMauiApp<App>()
				.ConfigureEssentials(essentials =>
				{
					essentials.UseVersionTracking();
#if WINDOWS
					var mapToken = Environment.GetEnvironmentVariable("BING_MAPS_API_KEY");
					if (!string.IsNullOrWhiteSpace(mapToken))
					{
						essentials.UseMapServiceToken(mapToken);
					}
					else
					{
						// Windows Maps/Geocoding requires a map service token. Set the
						// BING_MAPS_API_KEY environment variable before running the sample.
						Console.WriteLine("BING_MAPS_API_KEY is not set. Windows Maps/Geocoding features in this sample require a valid map service token.");
					}
#endif
					essentials.AddAppAction("app_info", "App Info", icon: "app_info_action_icon");
					essentials.AddAppAction("battery_info", "Battery Info");
					essentials.OnAppAction(App.HandleAppActions);
				});

			return builder.Build();
		}
	}
}
