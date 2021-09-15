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
		ViewHandler<INavigationView, NavigationFrame>
	{
		NavigationManager? _navigationManager;
		protected override NavigationFrame CreateNativeView()
		{
			_navigationManager = CreateNavigationManager();
			return new NavigationFrame(_navigationManager);
		}

		protected override void ConnectHandler(NavigationFrame nativeView)
		{
			_navigationManager?.Connect(VirtualView, nativeView);
			base.ConnectHandler(nativeView);
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
		protected virtual NavigationManager CreateNavigationManager() =>
			_navigationManager ??= new NavigationManager(MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null"));
	}
}

