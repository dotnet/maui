using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Hosting
{
	public static partial class GtkHandlerMap
	{
		// Navigation container handlers: stack navigation, toolbar (header bar), flyout and tabs.
		static void AddNavigationEntries()
		{
			Add<NavigationViewHandler, GtkNavigationViewHandler>();
			Add<ToolbarHandler, GtkToolbarHandler>();
			Add<FlyoutViewHandler, GtkFlyoutViewHandler>();
			Add<TabbedViewHandler, GtkTabbedViewHandler>();

			AddDefault(typeof(IStackNavigationView), typeof(GtkNavigationViewHandler));
			AddDefault(typeof(IToolbar), typeof(GtkToolbarHandler));
			AddDefault(typeof(IFlyoutView), typeof(GtkFlyoutViewHandler));
			AddDefault(typeof(ITabbedView), typeof(GtkTabbedViewHandler));
		}
	}
}
