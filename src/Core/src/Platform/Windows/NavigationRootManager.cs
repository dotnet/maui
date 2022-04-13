using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public partial class NavigationRootManager
	{
		Window _platformWindow;
		WindowRootView _rootView;
		bool _firstConnect;
		bool _disconnected;

		public NavigationRootManager(Window platformWindow)
		{
			_platformWindow = platformWindow;
			_rootView = new WindowRootView();
			_rootView.BackRequested += OnBackRequested;
			_rootView.OnApplyTemplateFinished += OnApplyTemplateFinished;
			_rootView.OnAppTitleBarChanged += OnAppTitleBarChanged;
		}

		void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			_platformWindow
				.GetWindow()?
				.BackButtonClicked();
		}

		internal FrameworkElement? AppTitleBar => _rootView.AppTitleBar;
		internal MauiToolbar? Toolbar => _rootView.Toolbar;

		void OnApplyTemplateFinished(object? sender, EventArgs e)
		{
			if (_rootView.AppTitleBar != null)
			{
				_platformWindow.ExtendsContentIntoTitleBar = true;
				UpdateAppTitleBar(true);
			}
		}

		void OnAppTitleBarChanged(object? sender, EventArgs e)
		{
			UpdateAppTitleBar(true);
			if (AppTitleBar != null)
			{
				var handle = _platformWindow.GetWindowHandle();
				var result = PlatformMethods.GetCaptionButtonsBound(handle);
				_rootView.UpdateAppTitleBar(result, _platformWindow.ExtendsContentIntoTitleBar);
			}
		}

		public FrameworkElement RootView => _rootView;

		public virtual void Connect(UIElement platformView)
		{
			bool firstConnect = _firstConnect;

			if (!firstConnect)
			{
				// We need to make sure to clear out the root view content 
				// before creating the new view.
				// Otherwise the new view might try to act on the old content.
				// It might have code in the handler that retrieves this class.
				_rootView.Content = null;
			}

			_firstConnect = false;

			NavigationView rootNavigationView;
			if (platformView is NavigationView nv)
			{
				rootNavigationView = nv;
				_rootView.Content = platformView;
			}
			else
			{
				if (_rootView.Content is RootNavigationView navView)
				{
					rootNavigationView = navView;
				}
				else
				{
					rootNavigationView = new RootNavigationView();
				}

				rootNavigationView.Content = platformView;
				_rootView.Content = rootNavigationView;
			}

			if (_disconnected)
			{
				_platformWindow.Activated += OnWindowActivated;
			}

			if (firstConnect)
			{
				if (_rootView.AppTitleBarContentControl != null && _platformWindow.ExtendsContentIntoTitleBar)
					UpdateAppTitleBar(true);

				SetWindowTitle(_platformWindow.GetWindow()?.Title);
			}

			_disconnected = false;
		}

		public virtual void Disconnect()
		{
			_platformWindow.Activated -= OnWindowActivated;
			_rootView.Content = null;
			_disconnected = true;
		}

		internal void UpdateAppTitleBar(bool isActive)
		{
			if (_rootView.AppTitleBarContentControl != null &&
				_platformWindow.ExtendsContentIntoTitleBar)
			{
				if (isActive)
				{
					_rootView.Visibility = UI.Xaml.Visibility.Visible;
					_platformWindow.SetTitleBar(_rootView.AppTitleBarContentControl);
				}
				else
				{
					_rootView.Visibility = UI.Xaml.Visibility.Collapsed;
				}
			}
			else
			{
				_platformWindow.SetTitleBar(null);
			}
		}

		internal void SetWindowTitle(string? title)
		{
			_rootView.SetWindowTitle(title);
		}

		internal void SetMenuBar(MenuBar? menuBar)
		{
			_rootView.MenuBar = menuBar;
		}

		internal void SetToolbar(FrameworkElement? toolBar)
		{
			_rootView.Toolbar = toolBar as MauiToolbar;
		}

		void OnWindowActivated(object sender, WindowActivatedEventArgs e)
		{
			if (_rootView.AppTitle == null)
				return;

			SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
			SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

			if (e.WindowActivationState == WindowActivationState.Deactivated)
			{
				_rootView.AppTitle.Foreground = inactiveForegroundBrush;
			}
			else
			{
				_rootView.AppTitle.Foreground = defaultForegroundBrush;
				SetWindowTitle(_platformWindow.GetWindow()?.Title);
			}
		}
	}
}