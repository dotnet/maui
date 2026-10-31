using Microsoft.Maui.Controls.Gtk.Handlers;
using Microsoft.Maui.Controls.Gtk.Items;
using Microsoft.Maui.Controls.Gtk.Platform;
using Microsoft.Maui.Controls.Platform;
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
			GtkControlsAlerts.Register(builder);

			// Backend hooks of the platform-less Controls build: gestures and modal pages.
			GesturePlatformBackend.Factory = GtkGestureConnection.Create;
			ModalNavigationBackend.Current = GtkModalNavigationBackend.Instance;

			builder.ConfigureMauiHandlers(handlers =>
			{
				// The handlers that the platform-less build of AddControlsHandlers skips.
				// Controls registers Label -> LabelHandler, which the Core rewrite maps to the plain GTK label handler: use the Controls-aware one.
				handlers.AddHandler<Label, GtkControlsLabelHandler>();
				handlers.AddHandler<NavigationPage, GtkNavigationViewHandler>();
				handlers.AddHandler<Toolbar, GtkControlsToolbarHandler>();
				handlers.AddHandler<FlyoutPage, GtkFlyoutViewHandler>();
				handlers.AddHandler<TabbedPage, GtkTabbedPageHandler>();
				handlers.AddHandler<CollectionView, GtkCollectionViewHandler>();
				handlers.AddHandler<CarouselView, GtkCarouselViewHandler>();
				handlers.AddHandler<Shell, GtkShellHandler>();
				handlers.AddHandler<Shapes.Line, GtkLineHandler>();
				handlers.AddHandler<Shapes.Path, GtkPathHandler>();
				handlers.AddHandler<Shapes.Polygon, GtkPolygonHandler>();
				handlers.AddHandler<Shapes.Polyline, GtkPolylineHandler>();
				handlers.AddHandler<Shapes.Rectangle, GtkRectangleHandler>();
				handlers.AddHandler<Shapes.RoundRectangle, GtkRoundRectangleHandler>();
				handlers.AddHandler<BoxView, GtkBoxViewHandler>();
#pragma warning disable CS0618 // Frame is obsolete but still supported.
				handlers.AddHandler<Frame, GtkFrameHandler>();
#pragma warning restore CS0618
#pragma warning disable CS0618 // ListView and TableView are obsolete but still supported.
				handlers.AddHandler<ListView, GtkListViewHandler>();
				handlers.AddHandler<TableView, GtkTableViewHandler>();
#pragma warning restore CS0618
			});
		}
	}
}
