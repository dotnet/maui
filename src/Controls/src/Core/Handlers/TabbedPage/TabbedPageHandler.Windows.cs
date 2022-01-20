#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WFrame = Microsoft.UI.Xaml.Controls.Frame;
using WApp = Microsoft.UI.Xaml.Application;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;
using WContentPresenter = Microsoft.UI.Xaml.Controls.ContentPresenter;
using WPage = Microsoft.UI.Xaml.Controls.Page;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class TabbedPageHandler : ViewHandler<TabbedPage, FrameworkElement>
	{
		NavigationView? _navigationView;
		NavigationRootManager? _navigationRootManager;
		TabbedPage? _previousView;
		WFrame? _navigationFrame;
		WFrame NavigationFrame => _navigationFrame ?? throw new ArgumentNullException(nameof(NavigationFrame));

		protected override FrameworkElement CreateNativeView()
		{
			_navigationFrame = new WFrame();
			if (VirtualView.FindParentOfType<FlyoutPage>() != null)
			{
				_navigationView = new NavigationView()
				{
					Content = _navigationFrame,
					PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal,
					IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed,
					IsSettingsVisible = false,
					IsPaneToggleButtonVisible = false
				};

				// Unset styles set by parent NavigationView
				_navigationView.Resources["NavigationViewContentMargin"] = new WThickness(0, 0, 0, 0);
				_navigationView.Resources["NavigationViewMinimalHeaderMargin"] = new WThickness(-24, 44, 0, 0);
				_navigationView.Resources["NavigationViewHeaderMargin"] = new WThickness(56, 44, 0, 0);
				_navigationView.Resources["NavigationViewMinimalContentGridBorderThickness"] = new WThickness(0, 1, 0, 0);
				_navigationView.Resources["TopNavigationViewTopNavGridMargin"] = new WThickness(0, 4, 0, 4);

				return _navigationView;
			}

			_navigationRootManager = MauiContext?.GetNavigationRootManager();
			return _navigationFrame;
		}

		private protected override void OnConnectHandler(FrameworkElement nativeView)
		{
			base.OnConnectHandler(nativeView);
			_navigationRootManager = MauiContext?.GetNavigationRootManager();

			if (_navigationRootManager?.RootView is NavigationRootView nrv)
				nrv.OnApplyTemplateFinished += OnApplyTemplateFinished;

			NavigationFrame.Navigated += OnNavigated;
			UpdateNavigationView();
		}

		private protected override void OnDisconnectHandler(FrameworkElement nativeView)
		{
			((WFrame)NativeView).Navigated -= OnNavigated;
			VirtualView.Appearing -= OnTabbedPageAppearing;
			VirtualView.Disappearing -= OnTabbedPageDisappearing;
			base.OnDisconnectHandler(nativeView);
		}

		public override void SetVirtualView(IView view)
		{
			if (_previousView != null)
			{
				_previousView.Appearing -= OnTabbedPageAppearing;
				_previousView.Disappearing -= OnTabbedPageDisappearing;
			}

			base.SetVirtualView(view);

			_previousView = VirtualView;
			VirtualView.Appearing += OnTabbedPageAppearing;
			VirtualView.Disappearing += OnTabbedPageDisappearing;
		}

		void OnTabbedPageAppearing(object? sender, EventArgs e)
		{
			if (_navigationView != null)
				_navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
		}

		void OnTabbedPageDisappearing(object? sender, EventArgs e)
		{
			if (_navigationView != null)
				_navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
		}

		void OnApplyTemplateFinished(object? sender, EventArgs e)
		{
			UpdateNavigationView();
		}

		void UpdateNavigationView()
		{
			_navigationRootManager = MauiContext?.GetNavigationRootManager();

			if (_navigationView == null)
				_navigationView = (_navigationRootManager?.RootView as NavigationRootView)?.NavigationViewControl;

			if (_navigationView != null)
			{
				_navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
				_navigationView.MenuItemsSource = VirtualView.Children;
				_navigationView.MenuItemTemplate = (UI.Xaml.DataTemplate)WApp.Current.Resources["TabBarNavigationViewMenuItem"];
				_navigationView.SelectionChanged += OnSelectedMenuItemChanged;
				_navigationView.SelectedItem = VirtualView.CurrentPage;
			}
		}

		void OnSelectedMenuItemChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.SelectedItem is Page page)
				NavigateToPage(page);
		}

		void NavigateToPage(Page page)
		{
			FrameNavigationOptions navOptions = new FrameNavigationOptions();
			VirtualView.CurrentPage = page;
			navOptions.IsNavigationStackEnabled = false;
			NavigationFrame.NavigateToType(typeof(WPage), null, navOptions);
		}


		void UpdateCurrentPageContent()
		{
			if (NavigationFrame.Content is WPage page)
				UpdateCurrentPageContent(page);
		}

		void UpdateCurrentPageContent(WPage page)
		{
			if (MauiContext == null)
				return;

			WContentPresenter? presenter;
			IView _currentPage = VirtualView.CurrentPage;

			if (page.Content == null)
			{
				presenter = new WContentPresenter()
				{
					HorizontalAlignment = UI.Xaml.HorizontalAlignment.Stretch,
					VerticalAlignment = UI.Xaml.VerticalAlignment.Stretch
				};

				page.Content = presenter;
			}
			else
			{
				presenter = page.Content as WContentPresenter;
			}

			// At this point if the Content isn't a ContentPresenter the user has replaced
			// the conent so we just let them take control
			if (presenter == null || _currentPage == null)
				return;

			presenter.Content = _currentPage.ToNative(MauiContext);
		}

		void OnNavigated(object sender, UI.Xaml.Navigation.NavigationEventArgs e)
		{
			if (e.Content is WPage page)
				UpdateCurrentPageContent(page);
		}

		public static void MapBarBackground(TabbedPageHandler handler, TabbedPage view)
		{
		}

		public static void MapBarBackgroundColor(TabbedPageHandler handler, TabbedPage view)
		{
		}

		public static void MapBarTextColor(TabbedPageHandler handler, TabbedPage view)
		{
		}

		public static void MapUnselectedTabColor(TabbedPageHandler handler, TabbedPage view)
		{
		}

		public static void MapSelectedTabColor(TabbedPageHandler handler, TabbedPage view)
		{
		}

		public static void MapItemsSource(TabbedPageHandler handler, TabbedPage view)
		{
			handler.UpdateNavigationView();
		}

		public static void MapItemTemplate(TabbedPageHandler handler, TabbedPage view)
		{
			handler.UpdateCurrentPageContent();
		}

		public static void MapSelectedItem(TabbedPageHandler handler, TabbedPage view)
		{
			handler.UpdateCurrentPageContent();
		}

		public static void MapCurrentPage(TabbedPageHandler handler, TabbedPage view)
		{
			if (handler._navigationView != null && handler._navigationView.SelectedItem != view.CurrentPage)
				handler._navigationView.SelectedItem = view.CurrentPage;
		}
	}
}
