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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class TabbedPageHandler : ViewHandler<TabbedPage, FrameworkElement>
	{
		MauiNavigationView? _navigationView;
		NavigationRootManager? _navigationRootManager;
		TabbedPage? _previousView;
		WFrame? _navigationFrame;
		WFrame NavigationFrame => _navigationFrame ?? throw new ArgumentNullException(nameof(NavigationFrame));

		protected override FrameworkElement CreatePlatformView()
		{
			_navigationFrame = new WFrame();
			if (VirtualView.FindParentOfType<FlyoutPage>() != null)
			{
				_navigationView = new MauiNavigationView()
				{
					Content = _navigationFrame,
					PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal,
					IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed,
					IsSettingsVisible = false,
					IsPaneToggleButtonVisible = false
				};

				// Unset styles set by parent NavigationView
				_navigationView.SetApplicationResource("NavigationViewContentMargin", null);
				_navigationView.SetApplicationResource("NavigationViewMinimalHeaderMargin", null);
				_navigationView.SetApplicationResource("NavigationViewHeaderMargin", null);
				_navigationView.SetApplicationResource("NavigationViewMinimalContentGridBorderThickness", null);

				return _navigationView;
			}

			_navigationRootManager = MauiContext?.GetNavigationRootManager();
			return _navigationFrame;
		}

		private protected override void OnConnectHandler(FrameworkElement platformView)
		{
			base.OnConnectHandler(platformView);
			NavigationFrame.Navigated += OnNavigated;

			// If CreatePlatformView didn't set the NavigationView then that means we are using the
			// WindowRootView for our tabs
			if (_navigationView == null)
			{
				SetupNavigationLocals();
			}
			else
			{
				SetupNavigationView();
			}

			if (_navigationView == null && _navigationRootManager?.RootView is WindowRootView wrv)
			{
				wrv.ContentChanged += OnContentChanged;

				void OnContentChanged(object? sender, EventArgs e)
				{
					wrv.ContentChanged -= OnContentChanged;
					SetupNavigationLocals();
				}
			}

			void SetupNavigationLocals()
			{
				_navigationRootManager = MauiContext?.GetNavigationRootManager();
				_navigationView = (_navigationRootManager?.RootView as WindowRootView)?.NavigationViewControl;
				SetupNavigationView();
			}
		}

		private protected override void OnDisconnectHandler(FrameworkElement platformView)
		{
			if (_navigationView != null)
			{
				_navigationView.OnApplyTemplateFinished -= OnApplyTemplateFinished;
				_navigationView.SizeChanged -= OnNavigationViewSizeChanged;
			}

			((WFrame)platformView).Navigated -= OnNavigated;
			VirtualView.Appearing -= OnTabbedPageAppearing;
			VirtualView.Disappearing -= OnTabbedPageDisappearing;
			if (_navigationView != null)
				_navigationView.SelectionChanged -= OnSelectedMenuItemChanged;

			_navigationView = null;
			_navigationRootManager = null;
			_previousView = null;
			_navigationFrame = null;

			base.OnDisconnectHandler(platformView);
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
			UpdateValuesWaitingForNavigationView();
		}

		void OnNavigationViewSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (_navigationView != null)
				VirtualView.Arrange(_navigationView);
		}

		void SetupNavigationView()
		{
			if (_navigationView == null)
				return;

			if (_navigationView.PaneDisplayMode != NavigationViewPaneDisplayMode.Top)
				_navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;

			_navigationView.MenuItemTemplate = (UI.Xaml.DataTemplate)WApp.Current.Resources["TabBarNavigationViewMenuItem"];

			if (_navigationView.TopNavArea != null)
				UpdateValuesWaitingForNavigationView();
			else
			{
				_navigationView.OnApplyTemplateFinished += OnApplyTemplateFinished;
				_navigationView.SizeChanged += OnNavigationViewSizeChanged;
			}
		}

		void UpdateValuesWaitingForNavigationView()
		{
			if (_navigationView == null)
				return;

			UpdateValue(nameof(TabbedPage.BarBackground));
			UpdateValue(nameof(TabbedPage.ItemsSource));
			UpdateValue(nameof(TabbedPage.BarTextColor));
			UpdateValue(nameof(TabbedPage.SelectedTabColor));
			UpdateValue(nameof(TabbedPage.UnselectedTabColor));

			_navigationView.SelectionChanged += OnSelectedMenuItemChanged;

			if (_navigationView.SelectedItem is NavigationViewItemViewModel vm && vm.Data != VirtualView.CurrentPage)
				UpdateValue(nameof(TabbedPage.CurrentPage));
			else
				NavigateToPage(VirtualView.CurrentPage);
		}

		void OnSelectedMenuItemChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.SelectedItem is NavigationViewItemViewModel itemViewModel &&
				itemViewModel.Data is Page page)
			{
				NavigateToPage(page);
			}
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

			presenter.Content = _currentPage.ToPlatform(MauiContext);
		}

		void OnNavigated(object sender, UI.Xaml.Navigation.NavigationEventArgs e)
		{
			if (e.Content is WPage page)
				UpdateCurrentPageContent(page);
		}

		public static void MapBarBackground(TabbedPageHandler handler, TabbedPage view)
		{
			handler._navigationView?.UpdateTopNavAreaBackground(view.BarBackground ?? view.BarBackgroundColor?.AsPaint());
		}

		public static void MapBarBackgroundColor(TabbedPageHandler handler, TabbedPage view)
		{
			MapBarBackground(handler, view);
		}

		public static void MapBarTextColor(TabbedPageHandler handler, TabbedPage view)
		{
			handler._navigationView?.UpdateTopNavigationViewItemTextColor(view.BarTextColor?.AsPaint());
		}

		public static void MapUnselectedTabColor(TabbedPageHandler handler, TabbedPage view)
		{
			handler._navigationView?.UpdateTopNavigationViewItemBackgroundUnselectedColor(view.UnselectedTabColor?.AsPaint());
		}

		public static void MapSelectedTabColor(TabbedPageHandler handler, TabbedPage view)
		{
			handler._navigationView?.UpdateTopNavigationViewItemBackgroundSelectedColor(view.SelectedTabColor?.AsPaint());
		}

		public static void MapItemsSource(TabbedPageHandler handler, TabbedPage view)
		{
			if (handler._navigationView != null)
			{
				ObservableCollection<NavigationViewItemViewModel> items;

				if (handler._navigationView.MenuItemsSource is ObservableCollection<NavigationViewItemViewModel> source)
				{
					items = source;
				}
				else
				{
					items = new ObservableCollection<NavigationViewItemViewModel>();
					handler._navigationView.MenuItemsSource = items;
				}

				items.SyncItems(handler.VirtualView.Children,
					(vm, page) =>
					{
						vm.Icon = page.IconImageSource?.ToIconSource(handler.MauiContext!)?.CreateIconElement();
						vm.Content = page.Title;
						vm.Data = page;
						vm.Foreground = view.BarTextColor?.AsPaint()?.ToPlatform();
						vm.SelectedBackground = view.SelectedTabColor?.AsPaint()?.ToPlatform();
						vm.UnselectedBackground = view.UnselectedTabColor?.AsPaint()?.ToPlatform();
					});

				handler.UpdateValue(nameof(TabbedPage.CurrentPage));
			}
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
			if (handler._navigationView?.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					if (item.Data == view.CurrentPage)
					{
						handler._navigationView.SelectedItem = item;
					}
				}
			}
		}
	}
}
