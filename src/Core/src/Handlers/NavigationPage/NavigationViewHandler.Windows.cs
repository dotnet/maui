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
		StackNavigationManager? _stackNavigationManager;
		protected override Frame CreateNativeView()
		{
			return new Frame();
		}

		protected override void ConnectHandler(Frame nativeView)
		{
			_ = _stackNavigationManager ?? throw new InvalidOperationException($"StackNavigationManager cannot be null");
			_stackNavigationManager.Connect(VirtualView, nativeView);
			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(Frame nativeView)
		{
			_stackNavigationManager?.Disconnect(VirtualView, nativeView);
			base.DisconnectHandler(nativeView);
		}

		public void SetStackNavigationManager(StackNavigationManager stackNavigationManager)
		{
			_stackNavigationManager = stackNavigationManager;
		}

		public static void RequestNavigation(NavigationViewHandler arg1, INavigationView arg2, object? arg3)
		{
			if (arg3 is NavigationRequest nr)
			{
				arg1._stackNavigationManager?.NavigateTo(nr);
			}
			else
			{
				throw new InvalidOperationException("Args must be NavigationRequest");
			}
		}

		private static void MapStackNavigationManager(NavigationViewHandler handler, INavigationView view)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			handler.SetStackNavigationManager(handler._stackNavigationManager ?? new StackNavigationManager(handler.MauiContext));
		}
	}
}

