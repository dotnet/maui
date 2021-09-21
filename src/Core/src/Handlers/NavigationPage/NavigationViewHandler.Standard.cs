using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler :
		ViewHandler<INavigationView, object>
	{
		protected override object CreateNativeView()
		{
			throw new NotImplementedException();
		}

		public static void RequestNavigation(NavigationViewHandler arg1, INavigationView arg2, object? arg3)
		{
			throw new NotImplementedException();
		}
		//public static void MapPadding(NavigationViewHandler handler, INavigationView view) { }

		//public static void MapBarTextColor(NavigationViewHandler handler, INavigationView view) { }

		//public static void MapBarBackground(NavigationViewHandler handler, INavigationView view) { }

		//public static void MapTitleIcon(NavigationViewHandler handler, INavigationView view) { }

		//public static void MapTitleView(NavigationViewHandler handler, INavigationView view) { }
	}
}
