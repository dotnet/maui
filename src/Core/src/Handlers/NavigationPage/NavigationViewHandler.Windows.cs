using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler :
		ViewHandler<INavigationView, Frame>
	{
		StackNavigationManager? _navigationManager;
		protected override Frame CreateNativeView()
		{
			_navigationManager = CreateNavigationManager();
			return new Frame();
		}

		protected override void ConnectHandler(Frame nativeView)
		{
			_navigationManager?.Connect(VirtualView, nativeView);
			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(Frame nativeView)
		{
			_navigationManager?.Disconnect(VirtualView, nativeView);
			base.DisconnectHandler(nativeView);
		}

		public static void RequestNavigation(NavigationViewHandler arg1, INavigationView arg2, object? arg3)
		{
			if (arg3 is NavigationRequest nr)
			{
				arg1._navigationManager?.NavigateTo(nr);
			}
			else
			{
				throw new InvalidOperationException("Args must be NavigationRequest");
			}
		}


		// this should move to a factory method
		protected virtual StackNavigationManager CreateNavigationManager() =>
			_navigationManager ??= new StackNavigationManager(MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null"));
	}
}

