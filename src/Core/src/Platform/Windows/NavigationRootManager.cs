using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public class NavigationRootManager
	{
		IMauiContext _mauiContext;
		NavigationRootView _navigationRootView;
		WindowHeader? _windowHeader;

		public NavigationRootManager(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
			_navigationRootView = new NavigationRootView();
			_navigationRootView.BackRequested += OnBackRequested;
			_navigationRootView.OnApplyTemplateFinished += OnApplyTemplateFinished;
		}

		void OnApplyTemplateFinished(object? sender, EventArgs e)
		{
			if (_navigationRootView.AppTitleBar != null)
			{
				var nativeWindow = _mauiContext.GetNativeWindow();
				nativeWindow.ExtendsContentIntoTitleBar = true;
				UpdateAppTitleBar(true);
			}

			if (_navigationRootView.NavigationViewControl != null)
			{
				_navigationRootView.NavigationViewControl.Header = _windowHeader;
			}
		}

		void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			_mauiContext.GetNativeWindow().GetWindow()?.BackButtonClicked();
		}

		public FrameworkElement RootView => _navigationRootView;

		public virtual void Connect(IView view)
		{
			_ = view.ToNative(_mauiContext);
			var nativeView = view.GetNative(true);
			_navigationRootView.Content = nativeView;

			var nativeWindow = _mauiContext.GetNativeWindow();
			nativeWindow.Activated += OnWindowActivated;

			UpdateAppTitleBar(true);
		}

		public virtual void Disconnect(IView view)
		{
			_mauiContext.GetNativeWindow().Activated -= OnWindowActivated;
		}

		internal void UpdateAppTitleBar(bool isActive)
		{
			var nativeWindow = _mauiContext.GetNativeWindow();
			if (_navigationRootView.AppTitleBar != null)
			{
				if (isActive)
				{
					_navigationRootView.Visibility = UI.Xaml.Visibility.Visible;
					nativeWindow.SetTitleBar(_navigationRootView.AppTitleBar);
				}
				else
				{
					_navigationRootView.Visibility = UI.Xaml.Visibility.Collapsed;
				}
			}
		}

		internal void SetWindowTitle(string? title)
		{
			_navigationRootView.SetWindowTitle(title);
		}

		internal void SetToolbar(FrameworkElement toolBar)
		{
			_windowHeader = toolBar as WindowHeader;
		}

		void OnWindowActivated(object sender, WindowActivatedEventArgs e)
		{
			if (_navigationRootView.AppTitle == null)
				return;

			SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
			SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

			if (e.WindowActivationState == WindowActivationState.Deactivated)
			{
				_navigationRootView.AppTitle.Foreground = inactiveForegroundBrush;
			}
			else
			{
				_navigationRootView.AppTitle.Foreground = defaultForegroundBrush;
			}
		}
	}
}
