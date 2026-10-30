using Microsoft.Maui.Controls.Gtk.Handlers;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Gtk.Hosting
{
	/// <summary>Registers the Controls-level GTK handlers, services and platform hooks.</summary>
	static class GtkControlsRegistration
	{
		public static void Register(MauiAppBuilder builder)
		{
			// Controls registers Label -> LabelHandler; the Core rewrite then picks the Controls-aware GTK label handler.
			GtkHandlerMap.Add<LabelHandler, GtkControlsLabelHandler>();

			GtkControlsAlerts.Register(builder);

			builder.ConfigureMauiHandlers(handlers =>
			{
				// The handlers that the platform-less build of AddControlsHandlers skips.
				handlers.AddHandler<NavigationPage, GtkNavigationViewHandler>();
				handlers.AddHandler<Toolbar, GtkControlsToolbarHandler>();
				handlers.AddHandler<FlyoutPage, GtkFlyoutViewHandler>();
				handlers.AddHandler<TabbedPage, GtkTabbedPageHandler>();
			});
		}
	}
}
