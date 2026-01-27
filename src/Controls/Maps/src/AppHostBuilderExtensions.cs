using System;
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
	/// <summary>
	/// This class contains the Map's <see cref="MauiAppBuilder"/> extensions.
	/// </summary>
	public static partial class AppHostBuilderExtensions
	{
		/// <summary>
		/// Configures <see cref="MauiAppBuilder"/> to add support for the <see cref="Map"/> control.
		/// </summary>
		/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
		/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
		/// <remarks>
		/// <para>
		/// <b>Windows (Azure Maps):</b> Set your Azure Maps subscription key using <c>ConfigureEssentials</c>:
		/// <code>
		/// builder.ConfigureEssentials(essentials =&gt; essentials.UseMapServiceToken("YOUR_AZURE_MAPS_KEY"));
		/// </code>
		/// Get a key from the Azure Portal: https://portal.azure.com → Azure Maps account → Authentication
		/// </para>
		/// <para>
		/// <b>Windows Platform Limitations:</b>
		/// The Windows implementation uses the WinUI 3 MapControl backed by Azure Maps, which has some limitations
		/// compared to iOS (Apple Maps) and Android (Google Maps):
		/// <list type="bullet">
		/// <item><description><b>MapType:</b> Programmatic map type switching is not supported. Users can change styles via the built-in picker.</description></item>
		/// <item><description><b>Traffic:</b> Traffic layer is not supported by the basic MapControl.</description></item>
		/// <item><description><b>User Location:</b> Not built-in; requires manual Geolocation API integration.</description></item>
		/// <item><description><b>Shapes:</b> Polylines, polygons, and circles have limited support.</description></item>
		/// <item><description><b>Pin Labels:</b> MapIcon does not support labels or info windows directly.</description></item>
		/// </list>
		/// See <see cref="MapHandler"/> documentation for detailed platform limitations.
		/// </para>
		/// </remarks>
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

							MapHandler.Bundle = b;
							if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(a) == ConnectionResult.Success)
							{
								try
								{
									MapsInitializer.Initialize(a, MapsInitializer.Renderer.Latest, null);
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

		/// <summary>
		/// Registers the .NET MAUI Maps handlers that are needed to render the map control.
		/// </summary>
		/// <param name="handlersCollection">An instance of <see cref="IMauiHandlersCollection"/> on which to register the map handlers.</param>
		/// <returns>The provided <see cref="IMauiHandlersCollection"/> object with the registered map handlers for subsequent registration calls.</returns>
		public static IMauiHandlersCollection AddMauiMaps(this IMauiHandlersCollection handlersCollection)
		{
			handlersCollection.AddHandler<Map, MapHandler>();
			handlersCollection.AddHandler<Pin, MapPinHandler>();
			handlersCollection.AddHandler<MapElement, MapElementHandler>();

			return handlersCollection;
		}
	}
}
