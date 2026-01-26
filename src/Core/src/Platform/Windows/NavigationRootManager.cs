using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public partial class NavigationRootManager
	{
		readonly WeakReference<Window> _platformWindow;
		WindowRootView _rootView;
		bool _disconnected = true;
		internal event EventHandler? OnApplyTemplateFinished;

		public NavigationRootManager(Window platformWindow)
		{
			_platformWindow = new(platformWindow);
			_rootView = new WindowRootView();
			_rootView.BackRequested += OnBackRequested;
			_rootView.OnApplyTemplateFinished += WindowRootViewOnApplyTemplateFinished;

			var titleBar = platformWindow.GetAppWindow()?.TitleBar;
			if (titleBar is not null)
			{
				SetTitleBarVisibility(titleBar.ExtendsContentIntoTitleBar);
			}
		}

		internal void SetTitleBarVisibility(bool isVisible)
		{
			var platformWindow = _platformWindow.GetTargetOrDefault();
			if (platformWindow is null)
				return;

			// https://learn.microsoft.com/en-us/windows/apps/design/basics/titlebar-design
			// Standard title bar height is 32px
			// This should always get set by the code after but
			// we are setting it just in case
			var appbarHeight = isVisible ? 32 : 0;
			var titlebarMargins = new UI.Xaml.Thickness(0, 0, 0, 0);
			if (isVisible && UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
			{
				var titleBar = platformWindow.GetAppWindow()?.TitleBar;
				if (titleBar is not null)
				{
					var density = platformWindow.GetDisplayDensity();
					appbarHeight = (int)(titleBar.Height / density);
					titlebarMargins = new UI.Xaml.Thickness(titleBar.LeftInset, 0, titleBar.RightInset, 0);
				}
			}

			_rootView.UpdateAppTitleBar(
					appbarHeight,
					UI.Windowing.AppWindowTitleBar.IsCustomizationSupported() &&
					isVisible,
					titlebarMargins
				);
		}

		void WindowRootViewOnWindowTitleBarContentSizeChanged(object? sender, EventArgs e)
		{
			if (_disconnected)
				return;

			_platformWindow.GetTargetOrDefault()?
				.GetWindow()?
				.Handler?
				.UpdateValue(nameof(IWindow.TitleBarDragRectangles));
		}

		void WindowRootViewOnApplyTemplateFinished(object? sender, System.EventArgs e) =>
			OnApplyTemplateFinished?.Invoke(this, EventArgs.Empty);

		void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			_platformWindow.GetTargetOrDefault()?
				.GetWindow()?
				.BackButtonClicked();
		}

		internal FrameworkElement? AppTitleBar => _rootView.AppTitleBar;
		internal MauiToolbar? Toolbar => _rootView.Toolbar;
		public FrameworkElement RootView => _rootView;

		internal void Connect(IWindowHandler handler)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var platformWindow = _platformWindow.GetTargetOrDefault();
			if (platformWindow is null)
			{
				return;
			}

			var previousRootView = RootView;

			Disconnect();
			Connect(handler.VirtualView.Content?.ToPlatform(handler.MauiContext));

			if (platformWindow.Content is WindowRootViewContainer container)
			{
				if (previousRootView is not null && previousRootView != RootView)
				{
					container.RemovePage(previousRootView);
				}

				container.AddPage(RootView);
			}
		}

		public virtual void Connect(UIElement? platformView)
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

			_rootView.Content = platformView is NavigationView ? platformView : new RootNavigationView()
			{
				Content = platformView
			};

			if (_disconnected && _platformWindow.TryGetTarget(out var platformWindow))
			{
				platformWindow.Activated += OnWindowActivated;
				_disconnected = false;
			}

			_rootView.OnWindowTitleBarContentSizeChanged += WindowRootViewOnWindowTitleBarContentSizeChanged;
		}

		public virtual void Disconnect()
		{
			_rootView.OnWindowTitleBarContentSizeChanged -= WindowRootViewOnWindowTitleBarContentSizeChanged;

			if (_platformWindow.TryGetTarget(out var platformWindow))
			{
				platformWindow.Activated -= OnWindowActivated;
			}

			if (_rootView.Content is RootNavigationView navView)
				navView.Content = null;

			_rootView.Content = null;
			SetToolbar(null);
			_disconnected = true;
		}

		internal void SetMenuBar(MenuBar? menuBar)
		{
			_rootView.MenuBar = menuBar;
		}

		internal void SetToolbar(FrameworkElement? toolBar)
		{
			_rootView.Toolbar = toolBar as MauiToolbar;
		}

		internal string? WindowTitle
		{
			get => _rootView.WindowTitle;
			set => _rootView.WindowTitle = value;
		}

		internal void SetTitle(string? title) =>
			_rootView.WindowTitle = title;

		internal void SetTitleBar(ITitleBar? titlebar, IMauiContext? mauiContext)
		{
			if (_platformWindow.TryGetTarget(out var window))
			{
				_rootView.AppWindowId = window.GetAppWindow()?.Id;
				_rootView.SetTitleBar(titlebar, mauiContext);
			}
		}

		void OnWindowActivated(object sender, WindowActivatedEventArgs e)
		{
			SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
			SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

			if (e.WindowActivationState == WindowActivationState.Deactivated)
			{
				_rootView.WindowTitleForeground = inactiveForegroundBrush;
			}
			else
			{
				_rootView.WindowTitleForeground = defaultForegroundBrush;
			}
		}
	}
}