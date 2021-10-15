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
		IView? _content;
		IWindow? _window;

		public NavigationRootManager(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
			_navigationView = new MauiNavigationView();
			_navigationView.BackRequested += OnBackRequested;
		}

		void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			_window?.BackButtonClicked();
		}

		public FrameworkElement RootView => _navigationView;

		public virtual void Connect(IWindow window)
		{
			_window = window;
			_content = window?.Content;
			_navigationView.Content = _content?.ToNative(_mauiContext);
		}

		public virtual void Disconnect(IWindow window)
		{
			_window = null;
			_content = null;
			_navigationView.Content = null;
		}

		internal CommandBar? GetCommandBar() =>
			(_navigationView.Header as WindowHeader)?.CommandBar;
	}
}
