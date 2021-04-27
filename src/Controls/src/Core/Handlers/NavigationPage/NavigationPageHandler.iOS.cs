using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class NavigationPageHandler :
		ViewHandler<NavigationPage, UIView>
	{
		protected override UIView CreateNativeView()
		{
			throw new NotImplementedException();
		}

		public static void MapPadding(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleColor(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapNavigationBarBackground(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleIcon(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleView(NavigationPageHandler handler, NavigationPage view) { }
	}
}
