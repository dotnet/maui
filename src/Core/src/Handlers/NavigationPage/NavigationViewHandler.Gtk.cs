using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{

	public partial class NavigationViewHandler : ViewHandler<IStackNavigationView, NavigationView>
	{

		protected override NavigationView CreatePlatformView()
		{
			return new();
		}

		protected override void ConnectHandler(NavigationView nativeView)
		{
			base.ConnectHandler(nativeView);

			var virtualView = VirtualView;

		}

		protected override void DisconnectHandler(NavigationView nativeView)
		{
			base.DisconnectHandler(nativeView);

			var virtualView = VirtualView;

		}

		public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
		{
			throw new NotImplementedException();
		}
	}

}