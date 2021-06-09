using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class NavigationPageHandler :
		ViewHandler<NavigationPage, Gtk.Widget>
	{
		protected override Gtk.Widget CreateNativeView()
		{
			throw new NotImplementedException();
		}

		[MissingMapper]
		public static void MapPadding(NavigationPageHandler handler, NavigationPage view) { }

		[MissingMapper]
		public static void MapBarTextColor(NavigationPageHandler handler, NavigationPage view) { }

		[MissingMapper]
		public static void MapBarBackground(NavigationPageHandler handler, NavigationPage view) { }

		[MissingMapper]
		public static void MapTitleIcon(NavigationPageHandler handler, NavigationPage view) { }

		[MissingMapper]
		public static void MapTitleView(NavigationPageHandler handler, NavigationPage view) { }
	}
}
