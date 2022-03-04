using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler : ViewHandler<IStackNavigationView, Frame>
	{
		StackNavigationManager? _navigationManager;
		protected override Frame CreatePlatformView()
		{
			_navigationManager = CreateNavigationManager();
			return new Frame();
		}

		protected override void ConnectHandler(Frame platformView)
		{
			_navigationManager?.Connect(VirtualView, platformView);
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(Frame platformView)
		{
			_navigationManager?.Disconnect(VirtualView, platformView);
			base.DisconnectHandler(platformView);
		}

		public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
		{
			if (arg1 is NavigationViewHandler platformHandler && arg3 is NavigationRequest nr)
			{
				platformHandler._navigationManager?.NavigateTo(nr);
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

