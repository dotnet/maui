using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class NavigationPageHandler :
		ViewHandler<NavigationPage, object>
	{
		protected override object CreateNativeView()
		{
			throw new NotImplementedException();
		}

		public static void MapPadding(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapBarTextColor(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapBarBackground(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleIcon(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleView(NavigationPageHandler handler, NavigationPage view) { }
	}
}
