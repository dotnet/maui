using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class WindowManager
	{
		MauiContext _mauiContext;
		MauiNavigationView _navigationView;
		IView? _content;
		IWindow? _window;

		public WindowManager(MauiContext mauiContext)
		{
			_mauiContext = mauiContext;
			_navigationView = new MauiNavigationView();
			_navigationView.BackRequested += OnBackRequested;
		}

		void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			_window?.BackButtonPressed();
		}

		public FrameworkElement RootView => _navigationView;

		public virtual void Connect(IWindow window)
		{
			_window = window;
			_content = window?.Content;
			_navigationView.Content = _content?.ToNative(_mauiContext);
		}

		internal CommandBar? GetCommandBar() =>
			(_navigationView.Header as WindowHeader)?.CommandBar;
	}
}
