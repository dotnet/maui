using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Maps.Handlers;

#if ANDROID
using Android.Gms.Common;
using Android.Gms.Maps;
#endif

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		/// <summary>
		/// Configures <see cref="MauiAppBuilder"/> to add support for the <see cref="Map"/> control.
		/// </summary>
		/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
		/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
		public static MauiAppBuilder UseMauiMaps(this MauiAppBuilder builder)
		{
			builder
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddMauiMaps();
				})
				.ConfigureLifecycleEvents(events =>
				{
#if __ANDROID__
					// Log everything in this one
					events.AddAndroid(android => android
						.OnCreate((a, b) =>
						{

							Microsoft.Maui.Maps.Handlers.MapHandler.Bundle = b;
							if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(a) == ConnectionResult.Success)
							{
								try
								{
									MapsInitializer.Initialize(a);
								}
								catch (Exception e)
								{
									Console.WriteLine("Google Play Services Not Found");
									Console.WriteLine("Exception: {0}", e);
								}
							}
						}));
#endif
				});

			return builder;
		}

		public static IMauiHandlersCollection AddMauiMaps(this IMauiHandlersCollection handlersCollection)
		{
#if __ANDROID__ || __IOS__
			handlersCollection.AddHandler<Map, MapHandler>();
			handlersCollection.AddHandler<Pin, MapPinHandler>();
			handlersCollection.AddHandler<MapElement, MapElementHandler>();
#endif

#if WINDOWS
			throw new NotImplementedException(".NET MAUI Maps is currently not implemented for Windows. For more information, please see: https://aka.ms/maui-maps-no-windows");
#else
			return handlersCollection;
#endif
		}
	}
}
