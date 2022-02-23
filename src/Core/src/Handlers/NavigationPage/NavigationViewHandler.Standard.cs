using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler :
		ViewHandler<IStackNavigationView, object>
	{
		protected override object CreatePlatformView()
		{
			throw new NotImplementedException();
		}

		public static void RequestNavigation(NavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
		{
			throw new NotImplementedException();
		}
	}
}
