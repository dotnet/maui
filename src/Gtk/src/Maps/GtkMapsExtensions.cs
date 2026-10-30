using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Maps.Gtk;
using Microsoft.Maui.Maps.Handlers;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// Registers the GTK 4 implementation of the .NET MAUI <c>Map</c> control.
	/// </summary>
	public static class GtkMapsExtensions
	{
		/// <summary>
		/// Registers the <c>Map</c> control (as <c>UseMauiMaps()</c> does), its GTK handler and a Nominatim geocoder.
		/// </summary>
		/// <param name="builder">The app builder.</param>
		/// <param name="configure">Optional callback to set <see cref="GtkMapOptions"/> (tile servers, <c>User-Agent</c>, disk cache, geocoder URL).</param>
		/// <returns>The same <paramref name="builder"/>.</returns>
		/// <remarks>
		/// <para>Call it after <c>UseMauiGtk()</c> / <c>UseMauiGtkCore()</c>; the registrations of <c>MapHandler</c> are rewritten to
		/// <see cref="GtkMapHandler"/>, whatever the order of the two calls.</para>
		/// <para>The geocoder is registered as <see cref="IGeocoding"/> unless the app (or Essentials) already registered one, so
		/// <c>Geocoding.Default</c> uses it.</para>
		/// </remarks>
		public static MauiAppBuilder UseMauiMapsGtk(this MauiAppBuilder builder, Action<GtkMapOptions>? configure = null)
		{
			if (builder is null)
				throw new ArgumentNullException(nameof(builder));

			var options = new GtkMapOptions();
			configure?.Invoke(options);

			GtkHandlerMap.Add<MapHandler, GtkMapHandler>();

			builder.UseMauiMaps();

			// Handler callbacks run in registration order and GtkHandlerMap.Apply rewrites what is registered so far: run it again after the
			// map registrations so this call works before or after UseMauiGtkCore() (Apply is idempotent).
			builder.ConfigureMauiHandlers(GtkHandlerMap.Apply);
			builder.Services.TryAddSingleton(options);
			builder.Services.TryAddSingleton<IGeocoding>(sp => new NominatimGeocoding(sp.GetRequiredService<GtkMapOptions>()));

			return builder;
		}
	}
}
