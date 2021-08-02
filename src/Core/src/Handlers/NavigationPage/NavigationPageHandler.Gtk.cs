using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Handlers
{

	internal partial class NavigationPageHandler : ViewHandler<INavigationView, NavigationView>
	{

		protected override NavigationView CreateNativeView()
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
		
		
		private static void PushAsyncTo(NavigationPageHandler arg1, INavigationView arg2, object? arg3)
		{
			throw new NotImplementedException();
		}

		private static void PopAsyncTo(NavigationPageHandler arg1, INavigationView arg2, object? arg3)
		{
			throw new NotImplementedException();
		}

	}

}