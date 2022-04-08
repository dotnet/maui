using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public class NavigationRootManager
	{
		IMauiContext _mauiContext;
		WindowRootView _rootView;

		public NavigationRootManager(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
			_rootView = new WindowRootView();
			_rootView.BackRequested += OnBackRequested;
			_rootView.OnApplyTemplateFinished += OnApplyTemplateFinished;
		}

		internal bool UseCustomAppTitleBar { get; set; } = true;
		internal FrameworkElement? AppTitleBar => _rootView.AppTitleBar;
		internal MauiToolbar? Toolbar => _rootView.Toolbar;

		void OnApplyTemplateFinished(object? sender, EventArgs e)
		{
			if (_rootView.AppTitleBar != null)
			{
				var platformWindow = _mauiContext.GetPlatformWindow();
				platformWindow.ExtendsContentIntoTitleBar = true;
				UpdateAppTitleBar(true);
			}
		}

		void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			_mauiContext
				.GetPlatformWindow()
				.GetWindow()?
				.BackButtonClicked();
		}

		public FrameworkElement RootView => _rootView;

		public virtual void Connect(IView view)
		{
			bool firstConnect = _rootView.Content == null;

			if (!firstConnect)
			{
				// We need to make sure to clear out the root view content 
				// before creating the new view.
				// The new view might try to act on the old content if we don't
				// It might have code in the handler that retrieves the rootmanager
				// in order to setup the toolbar or other components				
				_rootView.Content = null;
			}

			var platformView = view.ToPlatform(_mauiContext);

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

			if (firstConnect)
			{
				var platformWindow = _mauiContext.GetPlatformWindow();
				platformWindow.Activated += OnWindowActivated;

				UpdateAppTitleBar(true);
				SetWindowTitle(_mauiContext.GetPlatformWindow().GetWindow()?.Title);
			}
		}

		public virtual void Disconnect()
		{
			_mauiContext.GetPlatformWindow().Activated -= OnWindowActivated;
			_rootView.Content = null;
		}

		internal void UpdateAppTitleBar(bool isActive)
		{
			if (!UseCustomAppTitleBar)
				return;

			var platformWindow = _mauiContext.GetPlatformWindow();
			if (_rootView.AppTitleBar != null)
			{
				if (isActive)
				{
					_rootView.Visibility = UI.Xaml.Visibility.Visible;
					platformWindow.SetTitleBar(_rootView.AppTitleBar);
				}
				else
				{
					_rootView.Visibility = UI.Xaml.Visibility.Collapsed;
				}
			}
		}

		internal void SetWindowTitle(string? title)
		{
			_rootView.SetWindowTitle(title);
		}

		internal void SetMenuBar(IMenuBar? menuBar)
		{
			_rootView.MenuBar = menuBar?.ToPlatform(_mauiContext) as MenuBar;
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
				SetWindowTitle(_mauiContext.GetPlatformWindow().GetWindow()?.Title);
			}
		}
	}
}
