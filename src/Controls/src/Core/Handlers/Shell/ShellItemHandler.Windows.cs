#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using WApp = Microsoft.UI.Xaml.Application;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellItemHandler : ElementHandler<ShellItem, FrameworkElement>, IAppearanceObserver
	{
		public static PropertyMapper<ShellItem, ShellItemHandler> Mapper =
				new PropertyMapper<ShellItem, ShellItemHandler>(ElementMapper)
				{
					[nameof(ShellItem.CurrentItem)] = MapCurrentItem,
					[Shell.SearchHandlerProperty.PropertyName] = MapSearchHandler,
					[Shell.TabBarIsVisibleProperty.PropertyName] = MapTabBarIsVisible
				};

		public static CommandMapper<ShellItem, ShellItemHandler> CommandMapper =
				new CommandMapper<ShellItem, ShellItemHandler>(ElementCommandMapper);


		ShellSectionHandler? _shellSectionHandler;
		ObservableCollection<NavigationViewItemViewModel> _mainLevelTabs;
		ShellItem? _shellItem;
		SearchHandler? _currentSearchHandler;
		MauiNavigationView? _mauiNavigationView;
		MauiNavigationView ShellItemNavigationView => _mauiNavigationView!;


		public ShellItemHandler() : base(Mapper, CommandMapper)
		{
			_mainLevelTabs = new ObservableCollection<NavigationViewItemViewModel>();
		}

		protected override FrameworkElement CreatePlatformElement()
		{
			var platformView = new MauiNavigationView()
			{
				PaneDisplayMode = NavigationViewPaneDisplayMode.Top,
				IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed,
				IsSettingsVisible = false,
				IsPaneToggleButtonVisible = false,
				MenuItemTemplate = (UI.Xaml.DataTemplate)WApp.Current.Resources["TabBarNavigationViewMenuItem"],
				MenuItemsSource = _mainLevelTabs
			};

			_mauiNavigationView = platformView;
			platformView.SetApplicationResource("NavigationViewMinimalHeaderMargin", null);
			platformView.SetApplicationResource("NavigationViewHeaderMargin", null);
			platformView.SetApplicationResource("NavigationViewMinimalContentGridBorderThickness", null);

			return platformView;
		}

		protected override void ConnectHandler(FrameworkElement platformView)
		{
			base.ConnectHandler(platformView);
			ShellItemNavigationView.SelectionChanged += OnNavigationTabChanged;
		}

		protected override void DisconnectHandler(FrameworkElement platformView)
		{
			base.DisconnectHandler(platformView);
			ShellItemNavigationView.SelectionChanged -= OnNavigationTabChanged;
		}

		public override void SetVirtualView(Maui.IElement view)
		{
			if (view.Parent is IShellController controller)
			{
				if (_shellItem != null)
					controller.RemoveAppearanceObserver(this);

				_shellItem = (ShellItem)view;

				base.SetVirtualView(view);

				if (_shellItem != null)
					controller.AddAppearanceObserver(this, _shellItem);
			}
			else
				base.SetVirtualView(view);
		}

		private void OnNavigationTabChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.SelectedItem == null)
				return;

			var selectedItem = (NavigationViewItemViewModel)args.SelectedItem;

			if (selectedItem.Data is ShellSection shellSection)
			{
				((Shell)VirtualView.Parent).CurrentItem = shellSection;
			}
			else if (selectedItem.Data is ShellContent shellContent)
			{
				((Shell)VirtualView.Parent).CurrentItem = shellContent;
			}
		}

		void MapMenuItems()
		{
			List<BaseShellItem> items;

			IShellItemController shellItemController = VirtualView;
			if (Routing.IsImplicit(VirtualView))
			{
				items = new List<BaseShellItem>(((IShellSectionController)VirtualView.CurrentItem).GetItems());
			}
			else
			{
				items = new List<BaseShellItem>(shellItemController.GetItems());
			}

			bool hasTabs = shellItemController.ShowTabs;
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

					hasTabs = hasTabs || shellSectionItems.Count > 1;
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

			if (ShellItemNavigationView.SelectedItem != selectedItem)
				ShellItemNavigationView.SelectedItem = selectedItem;

			if (!hasTabs)
			{
				ShellItemNavigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
			}
			else
			{
				ShellItemNavigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
			}
		}

		void UpdateSearchHandler()
		{
			if (ShellItemNavigationView.AutoSuggestBox == null)
				ShellItemNavigationView.AutoSuggestBox = new Microsoft.UI.Xaml.Controls.AutoSuggestBox() { Width = 300 };

			if (VirtualView.Parent is not Shell shell)
				return;

			_currentSearchHandler = shell.GetEffectiveValue<SearchHandler?>(Shell.SearchHandlerProperty, null);

			var AutoSuggestBox = ShellItemNavigationView.AutoSuggestBox;
			AutoSuggestBox.TextChanged += OnSearchBoxTextChanged;
			AutoSuggestBox.QuerySubmitted += OnSearchBoxQuerySubmitted;
			AutoSuggestBox.SuggestionChosen += OnSearchBoxSuggestionChosen;

			if (AutoSuggestBox == null)
				return;

			if (_currentSearchHandler != null)
			{
				AutoSuggestBox.PlaceholderText = _currentSearchHandler.Placeholder;
				AutoSuggestBox.IsEnabled = _currentSearchHandler.IsSearchEnabled;
				AutoSuggestBox.ItemsSource = _currentSearchHandler.ItemsSource;
				AutoSuggestBox.Text = _currentSearchHandler.Query;
			}

			AutoSuggestBox.Visibility = _currentSearchHandler == null || _currentSearchHandler.SearchBoxVisibility == SearchBoxVisibility.Hidden ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
			if (_currentSearchHandler != null && _currentSearchHandler.SearchBoxVisibility != SearchBoxVisibility.Hidden)
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
			((ISearchHandlerController)_currentSearchHandler).ItemSelected(args.SelectedItem);
		}

		void OnSearchBoxQuerySubmitted(Microsoft.UI.Xaml.Controls.AutoSuggestBox sender, Microsoft.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (_currentSearchHandler == null)
				return;
			((ISearchHandlerController)_currentSearchHandler).QueryConfirmed();
		}

		void UpdateQueryIcon()
		{
			if (_currentSearchHandler != null)
			{
				if (_currentSearchHandler.QueryIcon is FileImageSource fis)
					ShellItemNavigationView.AutoSuggestBox.QueryIcon = new BitmapIcon() { UriSource = new Uri("ms-appx:///" + fis.File) };
				else
					ShellItemNavigationView.AutoSuggestBox.QueryIcon = new SymbolIcon(Symbol.Find);
			}
		}

		public static void MapSearchHandler(ShellItemHandler handler, ShellItem item)
		{
		}

		public static void MapTabBarIsVisible(ShellItemHandler handler, ShellItem item)
		{
			IShellItemController shellItemController = item;

			if (shellItemController.ShowTabs)
			{
				handler.ShellItemNavigationView.PaneDisplayMode
					= NavigationViewPaneDisplayMode.Top;
			}
			else
			{
				handler.ShellItemNavigationView.PaneDisplayMode
					= NavigationViewPaneDisplayMode.LeftMinimal;
			}
		}

		public static void MapCurrentItem(ShellItemHandler handler, ShellItem item)
		{
			if (item.CurrentItem != null)
			{
				handler._shellSectionHandler ??= (ShellSectionHandler)item.CurrentItem.ToHandler(handler.MauiContext!);

				if (handler._shellSectionHandler.PlatformView != (FrameworkElement)handler.ShellItemNavigationView.Content)
					handler.ShellItemNavigationView.Content = handler._shellSectionHandler.PlatformView;

				if (handler._shellSectionHandler.VirtualView != item.CurrentItem)
					handler._shellSectionHandler.SetVirtualView(item.CurrentItem);
			}

			handler.MapMenuItems();
		}

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance is IShellAppearanceElement a)
			{
				// This means the template hasn't been applied yet
				if (ShellItemNavigationView.TopNavArea == null)
				{
					ShellItemNavigationView.OnApplyTemplateFinished += OnApplyTemplateFinished;

					void OnApplyTemplateFinished(object? sender, EventArgs e)
					{
						ShellItemNavigationView.OnApplyTemplateFinished -= OnApplyTemplateFinished;
						ApplyAppearance();
					}
				}
				else
				{
					ApplyAppearance();
				}

				void ApplyAppearance()
				{
					ShellItemNavigationView.UpdateTopNavAreaBackground(a.EffectiveTabBarBackgroundColor?.AsPaint());
					ShellItemNavigationView.UpdateTopNavigationViewItemTextColor(a.EffectiveTabBarForegroundColor?.AsPaint());
				}
			}
		}
	}
}
