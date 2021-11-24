using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class NavigationRootManager
	{
		IMauiContext _mauiContext;
		MauiNavigationView _navigationView;

		public NavigationRootManager(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
			_navigationView = new MauiNavigationView();
			_navigationView.BackRequested += OnBackRequested;
		}

		void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			_mauiContext.GetNativeWindow().GetWindow()?.BackButtonClicked();
		}

		public FrameworkElement RootView => _navigationView;

		public virtual void Connect(IView view)
		{
			_ = view.ToNative(_mauiContext);
			var nativeView = view.GetNative(true);
			_navigationView.Content = nativeView;
		}

		public virtual void Disconnect(IView view)
		{
			_navigationView.Content = null;
		}

		internal CommandBar? GetCommandBar() =>
			(_navigationView.Header as WindowHeader)?.CommandBar;
	}
}
