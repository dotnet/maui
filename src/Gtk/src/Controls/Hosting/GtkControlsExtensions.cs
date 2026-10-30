using System;
using Microsoft.Maui.Controls.Gtk.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// The single call a MAUI app makes to run on GTK 4: Core backend, Controls handlers and Essentials.
	/// </summary>
	public static class GtkControlsExtensions
	{
		/// <summary>
		/// Registers the GTK 4 backend for an app that uses <c>Microsoft.Maui.Controls</c>: <c>UseMauiGtkCore()</c>, the Controls
		/// handlers that the platform-less build skips (NavigationPage, Toolbar, FlyoutPage, TabbedPage, Shell, CollectionView,
		/// CarouselView, ListView, TableView), the alert dialogs, gestures and modal pages, and <c>UseMauiGtkEssentials()</c>.
		/// </summary>
		/// <remarks>
		/// Call it right after <c>UseMauiApp&lt;T&gt;()</c>, on the GTK main thread (<c>CreateMauiApp</c> runs in <c>Main</c>).
		/// The property mappers are per handler and sit in front of the stock ones, so stock-mapper
		/// customisations (<c>Append</c>/<c>Prepend</c>/<c>Replace</c>) still run but always after the GTK implementation.
		/// </remarks>
		/// <param name="builder">The app builder.</param>
		/// <param name="configureEssentials">Optional Essentials options.</param>
		/// <returns>The same <paramref name="builder"/>.</returns>
		public static MauiAppBuilder UseMauiGtk(this MauiAppBuilder builder, Action<GtkEssentialsOptions>? configureEssentials = null)
		{
			if (builder is null)
				throw new ArgumentNullException(nameof(builder));

			builder.UseMauiGtkCore();
			builder.UseMauiGtkEssentials(configureEssentials);
			GtkControlsRegistration.Register(builder);

			return builder;
		}
	}
}
