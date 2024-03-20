using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : GtkMenuItemHandler<IMenuBarItem, MauiMenuItem, IMenuElement, Gtk.MenuItem>, IMenuBarItemHandler
	{
		public static void MapText(IMenuBarItemHandler handler, IMenuBarItem view)
		{
			handler.PlatformView.UpdateText(view.Text);
		}

		public static void MapIsEnabled(IMenuBarItemHandler handler, IMenuBarItem view) =>
			handler.PlatformView.UpdateIsEnabled(view.IsEnabled);
	}
}