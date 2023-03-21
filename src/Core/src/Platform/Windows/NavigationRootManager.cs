using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public partial class NavigationRootManager
	{
		Window _platformWindow;
		WindowRootView _rootView;
		bool _disconnected = true;
		bool _isActiveRootManager;
		bool _applyTemplateFinished;

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

		internal ContentControl? AppTitleBarContentControl => _rootView.AppTitleBarContentControl;
		internal FrameworkElement? AppTitleBar => _rootView.AppTitleBar;
		internal MauiToolbar? Toolbar => _rootView.Toolbar;

		void OnApplyTemplateFinished(object? sender, EventArgs e)
		{
			if (_rootView.AppTitleBar != null)
			{
				_platformWindow.ExtendsContentIntoTitleBar = true;
				UpdateAppTitleBar(true);
			}

			_applyTemplateFinished = true;
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
			if (_rootView.Content != null)
			{
				// Clear out the toolbar that was set from the previous content
				SetToolbar(null);

				// We need to make sure to clear out the root view content 
				// before creating the new view.
				// Otherwise the new view might try to act on the old content.
				// It might have code in the handler that retrieves this class.
				_rootView.Content = null;
			}

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
				_isActiveRootManager = true;
				_platformWindow.Activated += OnWindowActivated;
			}

			_disconnected = false;

			if (_applyTemplateFinished)
				OnApplyTemplateFinished(_rootView, EventArgs.Empty);
		}

		public virtual void Disconnect()
		{
			_platformWindow.Activated -= OnWindowActivated;
			SetToolbar(null);
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
					SetTitleBar(_rootView.AppTitleBarContentControl);

					SetWindowTitle(_platformWindow.GetWindow()?.Title);
				}
				else
				{
					_rootView.Visibility = UI.Xaml.Visibility.Collapsed;
				}
			}
			else
			{
				SetTitleBar(null);
			}

			if (!_isActiveRootManager && isActive)
			{
				_platformWindow.Activated += OnWindowActivated;
			}
			else if (!isActive)
			{
				_platformWindow.Activated -= OnWindowActivated;
			}

			_isActiveRootManager = isActive;
		}

		void SetTitleBar(UIElement? titleBar)
		{
			if (_platformWindow is MauiWinUIWindow mauiWindow)
				mauiWindow.MauiCustomTitleBar = titleBar;
			else
				_platformWindow.SetTitleBar(titleBar);
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
			if (!_isActiveRootManager)
			{
				_platformWindow.Activated -= OnWindowActivated;
			}

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