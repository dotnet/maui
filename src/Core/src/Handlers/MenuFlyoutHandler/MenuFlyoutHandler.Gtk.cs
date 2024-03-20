using System;
using System.Collections.Generic;
using PlatformView = Microsoft.Maui.Platform.MauiMenu;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutHandler : GtkMenuShellHandler<IMenuFlyout, PlatformView, IMenuElement, Gtk.MenuItem>, IMenuFlyoutHandler { }
}