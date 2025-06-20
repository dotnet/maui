using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WApp = Microsoft.UI.Xaml.Application;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellItemHandler : ElementHandler<ShellItem, FrameworkElement>, IAppearanceObserver
	{
		public static PropertyMapper<ShellItem, ShellItemHandler> Mapper =
				new PropertyMapper<ShellItem, ShellItemHandler>(ElementMapper)
				{
					[nameof(ShellItem.Title)] = MapTitle,
					[nameof(ShellItem.CurrentItem)] = MapCurrentItem,
					[Shell.TabBarIsVisibleProperty.PropertyName] = MapTabBarIsVisible
				};

		public static CommandMapper<ShellItem, ShellItemHandler> CommandMapper =
				new CommandMapper<ShellItem, ShellItemHandler>(ElementCommandMapper);

		ShellSectionHandler? _shellSectionHandler;
		ObservableCollection<NavigationViewItemViewModel> _mainLevelTabs;
		ShellItem? _shellItem;
		SearchHandler? _currentSearchHandler;
		IShellAppearanceElement? _shellAppearanceElement;

		public ShellItemHandler() : base(Mapper, CommandMapper)
		{
			_mainLevelTabs = new ObservableCollection<NavigationViewItemViewModel>();
		}

		protected override FrameworkElement CreatePlatformElement()
		{
			return new MauiNavigationView();
		}

		void OnNavigationViewLoaded(object sender, RoutedEventArgs e)
		{
			if (PlatformView is not null)
				PlatformView.Loaded -= OnNavigationViewLoaded;

			UpdateSearchHandler();
			MapMenuItems();
		}

		protected override void ConnectHandler(FrameworkElement platformView)
		{
			var mauiNavView = platformView as MauiNavigationView;

			if (mauiNavView is not null)
			{
				mauiNavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
				mauiNavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
				mauiNavView.IsSettingsVisible = false;
				mauiNavView.IsPaneToggleButtonVisible = false;
				mauiNavView.MenuItemTemplate = (UI.Xaml.DataTemplate)WApp.Current.Resources["TabBarNavigationViewMenuItem"];
				mauiNavView.MenuItemsSource = _mainLevelTabs;

				platformView.SetApplicationResource("NavigationViewMinimalHeaderMargin", null);
				platformView.SetApplicationResource("NavigationViewHeaderMargin", null);
				platformView.SetApplicationResource("NavigationViewContentMargin", null);
				platformView.SetApplicationResource("NavigationViewMinimalContentMargin", null);
			}

			if (platformView.IsLoaded)
				OnNavigationViewLoaded(platformView, new RoutedEventArgs());
			else
				platformView.Loaded += OnNavigationViewLoaded;

			base.ConnectHandler(platformView);

			if (mauiNavView is not null)
				mauiNavView.SelectionChanged += OnNavigationTabChanged;

			if (VirtualView.Parent is Shell shell)
			{
				shell.Navigated += OnShellNavigated;
			}
		}

		protected override void DisconnectHandler(FrameworkElement platformView)
		{
			base.DisconnectHandler(platformView);

			if (platformView is MauiNavigationView mnv)
			{
				mnv.SelectionChanged -= OnNavigationTabChanged;
				if (mnv.AutoSuggestBox is { } autoSuggestBox)
				{
					autoSuggestBox.TextChanged -= OnSearchBoxTextChanged;
					autoSuggestBox.QuerySubmitted -= OnSearchBoxQuerySubmitted;
					autoSuggestBox.SuggestionChosen -= OnSearchBoxSuggestionChosen;
					autoSuggestBox.GotFocus -= OnSearchBoxGotFocus;
					autoSuggestBox.LostFocus -= OnSearchBoxLostFocus;
				}
			}

			platformView.Loaded -= OnNavigationViewLoaded;

			if (_currentShellSection != null)
				_currentShellSection.PropertyChanged -= OnCurrentShellSectionPropertyChanged;

			if (_currentSearchHandler != null)
				_currentSearchHandler.PropertyChanged -= OnCurrentSearchHandlerPropertyChanged;

			if (_shellItem?.Parent is IShellController controller)
			{
				controller.RemoveAppearanceObserver(this);
			}

			if (_shellItem is IShellItemController shellItemController)
				shellItemController.ItemsCollectionChanged -= OnItemsChanged;

			if (VirtualView.Parent is Shell shell)
			{
				shell.Navigated -= OnShellNavigated;
			}
		}

		public override void SetVirtualView(Maui.IElement view)
		{
			if (view.Parent is IShellController controller)
			{
				if (_shellItem != null)
				{
					controller.RemoveAppearanceObserver(this);
					((IShellItemController)_shellItem).ItemsCollectionChanged -= OnItemsChanged;
				}

				_shellItem = (ShellItem)view;

				base.SetVirtualView(view);

				if (_shellItem != null)
				{
					controller.AddAppearanceObserver(this, _shellItem);
					((IShellItemController)_shellItem).ItemsCollectionChanged += OnItemsChanged;
				}
			}
			else
			{
				base.SetVirtualView(view);
			}
		}

		void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
		{
			UpdateSearchHandler();
		}

		private void OnItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			MapMenuItems();
		}

		private void OnNavigationTabChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.SelectedItem == null)
				return;

			var selectedItem = (NavigationViewItemViewModel)args.SelectedItem;

			if (selectedItem.Data is ShellSection shellSection && VirtualView.Parent is Shell shell)
			{
				NavigationViewItemViewModel? currentItem = null;
				foreach (var item in _mainLevelTabs)
				{
					if (shell.CurrentItem?.CurrentItem is not null && item.Data == shell.CurrentItem.CurrentItem)
					{
						currentItem = item;
						break;
					}
				}
				if (PlatformView is NavigationView navView && navView?.SelectedItem is not null && navView.SelectedItem != currentItem)
				{
					((IShellItemController)shell.CurrentItem!).ProposeSection(shellSection);
				}

				((Shell)VirtualView.Parent).CurrentItem = shellSection;
			}
			else if (selectedItem.Data is ShellContent shellContent && VirtualView.Parent is Shell parentShell)
			{
				// We need to invoke ProposeSection for TabBar items navigation for ShellContent
				var currentItem = parentShell.CurrentItem?.CurrentItem;
				if (currentItem?.Title != shellContent.Title && currentItem != shellContent.Parent)
				{
					(parentShell.CurrentItem as IShellItemController)?.ProposeSection(shellContent);
				}
				parentShell.CurrentItem = shellContent;
			}
		}

		internal void MapMenuItems()
		{
			IShellItemController shellItemController = VirtualView;
			var items = new List<BaseShellItem>();

			foreach (var item in shellItemController.GetItems())
			{
				if (Routing.IsImplicit(item))
					items.Add(item.CurrentItem);
				else
					items.Add(item);
			}

			object? selectedItem = null;

			_mainLevelTabs.SyncItems(items, (navItem, baseShellItem) =>
			{
				SetValues(baseShellItem, navItem);

				if (baseShellItem is not ShellSection shellSection)
				{
					navItem.MenuItemsSource = null;
					if (baseShellItem.Parent == VirtualView.CurrentItem)
						selectedItem = navItem;

					return;
				}

				var shellSectionItems = ((IShellSectionController)shellSection).GetItems();

				if (shellSection == VirtualView.CurrentItem)
					selectedItem = navItem;

				if (shellSectionItems.Count <= 1)
				{
					if (navItem.MenuItemsSource != null)
						navItem.MenuItemsSource = null;
				}
				else
				{
					navItem.MenuItemsSource ??= new ObservableCollection<NavigationViewItemViewModel>();
					navItem
						.MenuItemsSource
						.SyncItems(shellSectionItems, (shellContentNavItem, shellContent) =>
						{
							SetValues(shellContent, shellContentNavItem);

							if (shellSection == VirtualView.CurrentItem &&
								shellContent == VirtualView.CurrentItem.CurrentItem)
							{
								selectedItem = shellContentNavItem;
							}
						});
				}

				void SetValues(BaseShellItem bsi, NavigationViewItemViewModel vm)
				{
					vm.Content = bsi.Title;
					var iconSource = bsi.Icon?.ToIconSource(MauiContext!);

					if (iconSource != null)
					{
						if (vm.Foreground != null)
						{
							iconSource.Foreground = vm.Foreground;
						}
						else if (PlatformView.Resources.TryGetValue("NavigationViewItemForeground", out object nviForeground) &&
							nviForeground is WBrush brush)
						{
							iconSource.Foreground = brush;
						}
					}

					vm.Icon = iconSource?.CreateIconElement();
				}
			});

			if (PlatformView is NavigationView navView && navView.SelectedItem != selectedItem)
				navView.SelectedItem = selectedItem;

			UpdateValue(Shell.TabBarIsVisibleProperty.PropertyName);
		}

		void UpdateSearchHandler()
		{
			if (VirtualView.Parent is not Shell shell)
				return;

			if (PlatformView is not NavigationView mauiNavView)
				return;

			var newSearchHandler = shell.GetEffectiveValue<SearchHandler?>(Shell.SearchHandlerProperty, null);
			if (newSearchHandler != _currentSearchHandler)
			{
				if (_currentSearchHandler is not null)
				{
					_currentSearchHandler.PropertyChanged -= OnCurrentSearchHandlerPropertyChanged;
				}

				_currentSearchHandler = newSearchHandler;

				var autoSuggestBox = mauiNavView.AutoSuggestBox;
				if (_currentSearchHandler is not null)
				{
					if (autoSuggestBox == null)
					{
						autoSuggestBox = new Microsoft.UI.Xaml.Controls.AutoSuggestBox() { Width = 300 };
						autoSuggestBox.TextChanged += OnSearchBoxTextChanged;
						autoSuggestBox.QuerySubmitted += OnSearchBoxQuerySubmitted;
						autoSuggestBox.SuggestionChosen += OnSearchBoxSuggestionChosen;
						autoSuggestBox.GotFocus += OnSearchBoxGotFocus;
						autoSuggestBox.LostFocus += OnSearchBoxLostFocus;
						mauiNavView.AutoSuggestBox = autoSuggestBox;
					}

					autoSuggestBox.PlaceholderText = _currentSearchHandler.Placeholder;
					autoSuggestBox.IsEnabled = _currentSearchHandler.IsSearchEnabled;
					autoSuggestBox.ItemsSource = CreateSearchHandlerItemsSource();
					autoSuggestBox.ItemTemplate = _currentSearchHandler.ItemTemplate is null ? null : (UI.Xaml.DataTemplate)WApp.Current.Resources["SearchHandlerItemTemplate"];
					autoSuggestBox.Text = _currentSearchHandler.Query;
					autoSuggestBox.UpdateTextOnSelect = false;

					_currentSearchHandler.PropertyChanged += OnCurrentSearchHandlerPropertyChanged;

					autoSuggestBox.Visibility = _currentSearchHandler.SearchBoxVisibility == SearchBoxVisibility.Hidden ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
					if (_currentSearchHandler.SearchBoxVisibility != SearchBoxVisibility.Hidden)
					{
						if (_currentSearchHandler.SearchBoxVisibility == SearchBoxVisibility.Expanded)
						{
							// TODO: Expand search
						}
						else
						{
							// TODO: Collapse search
						}
					}

					UpdateQueryIcon();
				}
				else if (autoSuggestBox is not null)
				{
					// there is no current search handler, so hide the autoSuggestBox
					autoSuggestBox.Visibility = UI.Xaml.Visibility.Collapsed;
				}
			}
		}

		void OnSearchBoxGotFocus(object sender, RoutedEventArgs e)
		{
			_currentSearchHandler?.SetIsFocused(true);
		}

		void OnSearchBoxLostFocus(object sender, RoutedEventArgs e)
		{
			_currentSearchHandler?.SetIsFocused(false);
		}

		void OnSearchBoxTextChanged(Microsoft.UI.Xaml.Controls.AutoSuggestBox sender, Microsoft.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs args)
		{
			if (_currentSearchHandler == null)
				return;

			if (args.Reason != Microsoft.UI.Xaml.Controls.AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
				_currentSearchHandler.Query = sender.Text;
		}

		void OnSearchBoxSuggestionChosen(Microsoft.UI.Xaml.Controls.AutoSuggestBox sender, Microsoft.UI.Xaml.Controls.AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			if (_currentSearchHandler == null)
				return;

			object selectedItem = args.SelectedItem;

			if (selectedItem is ItemTemplateContext itemTemplateContext)
				selectedItem = itemTemplateContext.Item;

			// Currently the search handler on each platform clears out the text when an answer is chosen
			// Ideally we'd have a "TextMemberPath" property that could bind to a property in the item source
			// to indicate what to display
			if (String.IsNullOrEmpty(sender.TextMemberPath))
				sender.Text = String.Empty;

			((ISearchHandlerController)_currentSearchHandler).ItemSelected(selectedItem);

		}

		void OnSearchBoxQuerySubmitted(Microsoft.UI.Xaml.Controls.AutoSuggestBox sender, Microsoft.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (_currentSearchHandler == null)
				return;

			((ISearchHandlerController)_currentSearchHandler).QueryConfirmed();
		}

		object? CreateSearchHandlerItemsSource()
		{
			if (_currentSearchHandler == null)
				return null;

			var itemsSource = _currentSearchHandler.ItemsSource;
			var itemTemplate = _currentSearchHandler.ItemTemplate;

			if (itemTemplate is not null && itemsSource is not null)
			{
				return TemplatedItemSourceFactory.Create(itemsSource, itemTemplate, _currentSearchHandler,
				null, null, null, MauiContext);
			}
			else
			{
				return itemsSource;
			}
		}

		void OnCurrentSearchHandlerPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (_currentSearchHandler is null)
				return;

			if (PlatformView is not NavigationView mauiNavView)
				return;

			switch (e.PropertyName)
			{
				case nameof(SearchHandler.Placeholder):
					mauiNavView.AutoSuggestBox.PlaceholderText = _currentSearchHandler.Placeholder;
					break;
				case nameof(SearchHandler.IsSearchEnabled):
					mauiNavView.AutoSuggestBox.IsEnabled = _currentSearchHandler.IsSearchEnabled;
					break;
				case nameof(SearchHandler.ItemsSource):
					mauiNavView.AutoSuggestBox.ItemsSource = CreateSearchHandlerItemsSource();
					break;
				case nameof(SearchHandler.Query):
					mauiNavView.AutoSuggestBox.Text = _currentSearchHandler.Query;
					break;
			}
		}

		void UpdateQueryIcon()
		{
			if (PlatformView is not NavigationView mauiNavView)
				return;

			if (_currentSearchHandler != null)
			{
				if (_currentSearchHandler.QueryIcon is FileImageSource fis)
					mauiNavView.AutoSuggestBox.QueryIcon = new BitmapIcon() { UriSource = new Uri("ms-appx:///" + fis.File) };
				else
					mauiNavView.AutoSuggestBox.QueryIcon = new SymbolIcon(Symbol.Find);
			}
		}

		ShellSection? _currentShellSection;

		internal void UpdateTitle()
		{
			MapMenuItems();
		}

		void UpdateCurrentItem()
		{
			if (_currentShellSection == VirtualView.CurrentItem)
				return;

			if (_currentShellSection != null)
			{
				_currentShellSection.PropertyChanged -= OnCurrentShellSectionPropertyChanged;
			}

			_currentShellSection = VirtualView.CurrentItem;

			if (VirtualView.CurrentItem != null)
			{
				_shellSectionHandler ??= (ShellSectionHandler)VirtualView.CurrentItem.ToHandler(MauiContext!);

				if (PlatformView is NavigationView navView &&
					_shellSectionHandler.PlatformView != (FrameworkElement)navView.Content)
				{
					navView.Content = _shellSectionHandler.PlatformView;
				}

				if (_shellSectionHandler.VirtualView != VirtualView.CurrentItem)
					_shellSectionHandler.SetVirtualView(VirtualView.CurrentItem);
			}

			UpdateSearchHandler();

			MapMenuItems();

			if (_currentShellSection != null)
			{
				_currentShellSection.PropertyChanged += OnCurrentShellSectionPropertyChanged;
			}
		}

		void OnCurrentShellSectionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (_mainLevelTabs == null)
				return;

			var currentItem = VirtualView.CurrentItem.CurrentItem;
			NavigationViewItemViewModel? navigationViewItemViewModel = null;

			foreach (var item in _mainLevelTabs)
			{
				if (item.Data == currentItem)
				{
					navigationViewItemViewModel = item;
					break;
				}

				if (item.MenuItemsSource != null)
				{
					foreach (var subItem in item.MenuItemsSource)
					{
						if (subItem.Data == currentItem)
						{
							navigationViewItemViewModel = subItem;
							break;
						}

					}
				}

				if (navigationViewItemViewModel != null)
					break;
			}

			if (navigationViewItemViewModel != null &&
				PlatformView is NavigationView navView &&
				navView.SelectedItem != navigationViewItemViewModel)
			{
				navView.SelectedItem = navigationViewItemViewModel;
			}
		}

		void UpdateTabBarVisibility(IShellItemController item)
		{
			if (PlatformView is not MauiNavigationView mauiNavView)
				return;

			var paneDisplayMode = GetNavigationViewPaneDisplayMode(item);
			mauiNavView.PaneDisplayMode = paneDisplayMode;
			mauiNavView.PinPaneDisplayModeTo = paneDisplayMode;
		}

		public static void MapTabBarIsVisible(ShellItemHandler handler, ShellItem item)
		{
			handler.UpdateTabBarVisibility(item);
		}

		NavigationViewPaneDisplayMode GetNavigationViewPaneDisplayMode(IShellItemController shellItemController)
		{
			return shellItemController.ShowTabs || _currentSearchHandler is not null ?
				NavigationViewPaneDisplayMode.Top :
				NavigationViewPaneDisplayMode.LeftMinimal;
		}

		public static void MapTitle(ShellItemHandler handler, ShellItem item)
		{
			handler.UpdateTitle();
		}

		public static void MapCurrentItem(ShellItemHandler handler, ShellItem item)
		{
			handler.UpdateCurrentItem();
		}

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance is IShellAppearanceElement a)
			{
				_shellAppearanceElement = a;
				// This means the template hasn't been applied yet
				if (PlatformView is MauiNavigationView mauiNavView && mauiNavView.TopNavArea is null)
				{
					mauiNavView.OnApplyTemplateFinished += OnApplyTemplateFinished;
				}

				UpdateAppearance(_shellAppearanceElement);
			}
		}

		protected virtual void UpdateAppearance(IShellAppearanceElement appearance)
		{
			if (_shellAppearanceElement is null)
				return;

			if (PlatformView is not MauiNavigationView mauiNavView)
				return;

			var backgroundColor = _shellAppearanceElement.EffectiveTabBarBackgroundColor?.AsPaint();
			var foregroundColor = _shellAppearanceElement.EffectiveTabBarForegroundColor?.AsPaint();
			var unselectedColor = _shellAppearanceElement.EffectiveTabBarUnselectedColor?.AsPaint();
			var titleColor = _shellAppearanceElement.EffectiveTabBarTitleColor?.AsPaint();

			mauiNavView.UpdateTopNavAreaBackground(backgroundColor);
			mauiNavView.UpdateTopNavigationViewItemUnselectedColor(unselectedColor);
			mauiNavView.UpdateTopNavigationViewItemTextSelectedColor(titleColor ?? foregroundColor);
			mauiNavView.UpdateTopNavigationViewItemTextColor(unselectedColor);
			mauiNavView.UpdateTopNavigationViewItemSelectedColor(foregroundColor ?? titleColor);
		}

		void OnApplyTemplateFinished(object? sender, EventArgs e)
		{
			if (PlatformView is MauiNavigationView mauiNavView)
				mauiNavView.OnApplyTemplateFinished -= OnApplyTemplateFinished;

			if (_shellAppearanceElement is not null)
				UpdateAppearance(_shellAppearanceElement);
		}
	}
}