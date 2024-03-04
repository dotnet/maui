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

		StackNavigationManager? _stackNavigationManager;

		StackNavigationManager NavigationManager
			=> _stackNavigationManager ??= new StackNavigationManager(MauiContext);

		protected override NavigationView CreatePlatformView()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			return new();
		}

		protected override void ConnectHandler(NavigationView nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.Connect(VirtualView);
			NavigationManager.Connect(VirtualView, nativeView);
		}

		protected override void DisconnectHandler(NavigationView nativeView)
		{

			NavigationManager.Disconnect(VirtualView, nativeView);

			nativeView.Disconnect(VirtualView);
			base.DisconnectHandler(nativeView);
		}

		void RequestNavigation(NavigationRequest request)
		{
			NavigationManager.NavigateTo(request);
		}

		static void RequestNavigation(INavigationViewHandler viewHandler, IStackNavigation navigation, object? request)
		{
			if (viewHandler is NavigationViewHandler platformHandler && request is NavigationRequest ea)
				platformHandler.RequestNavigation(ea);
		}

	}

}