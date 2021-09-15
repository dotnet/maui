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
		NavigationView _navigationView;
		WindowHeader _windowHeader;
		IView? _content;
		IWindow? _window;

		public WindowManager(MauiContext mauiContext)
		{
			_mauiContext = mauiContext;
			_navigationView = new NavigationView();
			_navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
			//_navigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
			_navigationView.IsPaneToggleButtonVisible = false;
			_navigationView.Header = (_windowHeader = new WindowHeader());
			_navigationView.BackRequested += OnBackRequested;

			_navigationView.IsBackEnabled = true;
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

		// TODO MAUI: This will need to change once we start having nested NavigationViews
		internal void SetVisibleContent(IView view)
		{
			_windowHeader.Title = (view as ITitledElement)?.Title ?? String.Empty;
		}

		internal CommandBar GetCommandBar() => (CommandBar)_navigationView.Header;
	}
}
