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

namespace Microsoft.Maui.Controls
{
	public partial class TabbedPage
	{
		MauiNavigationView? _navigationView;
		NavigationRootManager? _navigationRootManager;
		WFrame? _navigationFrame;
		WFrame NavigationFrame => _navigationFrame ?? throw new ArgumentNullException(nameof(NavigationFrame));
		IMauiContext MauiContext => this.Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null here");

		FrameworkElement CreatePlatformView()
		{
			_navigationFrame = new WFrame();
			if (this.FindParentOfType<FlyoutPage>() != null)
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

			_navigationRootManager = MauiContext.GetNavigationRootManager();

			return _navigationFrame;
		}

		static FrameworkElement? OnCreatePlatformView(ViewHandler<ITabbedView, FrameworkElement> arg)
		{
			if (arg.VirtualView is TabbedPage tabbedPage)
				return tabbedPage.CreatePlatformView();

			return null;
		}

		protected override void OnHandlerChanging(HandlerChangingEventArgs args)
		{
			base.OnHandlerChanging(args);

			if (args.OldHandler == null && args.NewHandler != null)
				OnConnectHandler();
			else if (args.OldHandler != null && args.NewHandler == null)
				OnDisconnectHandler(args.OldHandler.PlatformView as FrameworkElement);
		}

		void OnConnectHandler()
		{
			Appearing += OnTabbedPageAppearing;
			Disappearing += OnTabbedPageDisappearing;

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

		void OnDisconnectHandler(FrameworkElement? platformView)
		{
			if (_navigationView != null)
			{
				_navigationView.OnApplyTemplateFinished -= OnApplyTemplateFinished;
				_navigationView.SizeChanged -= OnNavigationViewSizeChanged;
			}

			if (platformView is WFrame wFrame)
				wFrame.Navigated -= OnNavigated;

			Appearing -= OnTabbedPageAppearing;
			Disappearing -= OnTabbedPageDisappearing;
			if (_navigationView != null)
				_navigationView.SelectionChanged -= OnSelectedMenuItemChanged;

			_navigationView = null;
			_navigationRootManager = null;
			_navigationFrame = null;
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
				this.Arrange(_navigationView);
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

			Handler?.UpdateValue(nameof(TabbedPage.BarBackground));
			Handler?.UpdateValue(nameof(TabbedPage.ItemsSource));
			Handler?.UpdateValue(nameof(TabbedPage.BarTextColor));
			Handler?.UpdateValue(nameof(TabbedPage.SelectedTabColor));
			Handler?.UpdateValue(nameof(TabbedPage.UnselectedTabColor));

			_navigationView.SelectionChanged += OnSelectedMenuItemChanged;

			if (_navigationView.SelectedItem is NavigationViewItemViewModel vm && vm.Data != CurrentPage)
			{
				Handler?.UpdateValue(nameof(TabbedPage.CurrentPage));
			}
			else
				NavigateToPage(CurrentPage);
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
			CurrentPage = page;
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
			IView _currentPage = CurrentPage;

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

		internal static void MapBarBackground(ITabbedViewHandler handler, TabbedPage view)
		{
			view._navigationView?.UpdateTopNavAreaBackground(view.BarBackground ?? view.BarBackgroundColor?.AsPaint());
		}

		internal static void MapBarBackgroundColor(ITabbedViewHandler handler, TabbedPage view)
		{
			MapBarBackground(handler, view);
		}

		internal static void MapBarTextColor(ITabbedViewHandler handler, TabbedPage view)
		{
			view._navigationView?.UpdateTopNavigationViewItemTextColor(view.BarTextColor?.AsPaint());
		}

		internal static void MapUnselectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
			view._navigationView?.UpdateTopNavigationViewItemBackgroundUnselectedColor(view.UnselectedTabColor?.AsPaint());
		}

		internal static void MapSelectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
			view._navigationView?.UpdateTopNavigationViewItemBackgroundSelectedColor(view.SelectedTabColor?.AsPaint());
		}

		internal static void MapItemsSource(ITabbedViewHandler handler, TabbedPage view)
		{
			if (view._navigationView != null)
			{
				ObservableCollection<NavigationViewItemViewModel> items;

				if (view._navigationView.MenuItemsSource is ObservableCollection<NavigationViewItemViewModel> source)
				{
					items = source;
				}
				else
				{
					items = new ObservableCollection<NavigationViewItemViewModel>();
					view._navigationView.MenuItemsSource = items;
				}

				items.SyncItems(view.Children,
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

		internal static void MapItemTemplate(ITabbedViewHandler handler, TabbedPage view)
		{
			view.UpdateCurrentPageContent();
		}

		internal static void MapSelectedItem(ITabbedViewHandler handler, TabbedPage view)
		{
			view.UpdateCurrentPageContent();
		}

		internal static void MapCurrentPage(ITabbedViewHandler handler, TabbedPage view)
		{
			if (view._navigationView?.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					if (item.Data == view.CurrentPage)
					{
						view._navigationView.SelectedItem = item;
					}
				}
			}
		}
	}
}
