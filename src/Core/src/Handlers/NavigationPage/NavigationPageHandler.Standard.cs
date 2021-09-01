using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Handlers
{
	internal partial class NavigationPageHandler :
		ViewHandler<INavigationView, object>
	{
		protected override object CreateNativeView()
		{
			throw new NotImplementedException();
		}

		private static void PushAsyncTo(NavigationPageHandler arg1, INavigationView arg2, object? arg3)
		{
			throw new NotImplementedException();
		}

		private static void PopAsyncTo(NavigationPageHandler arg1, INavigationView arg2, object? arg3)
		{
			throw new NotImplementedException();
		}

		//public static void MapPadding(NavigationPageHandler handler, INavigationView view) { }

		//public static void MapBarTextColor(NavigationPageHandler handler, INavigationView view) { }

		//public static void MapBarBackground(NavigationPageHandler handler, INavigationView view) { }

		//public static void MapTitleIcon(NavigationPageHandler handler, INavigationView view) { }

		//public static void MapTitleView(NavigationPageHandler handler, INavigationView view) { }
	}
}
