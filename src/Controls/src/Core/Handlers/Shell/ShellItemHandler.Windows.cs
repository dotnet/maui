#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using WApp = Microsoft.UI.Xaml.Application;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellItemHandler : ElementHandler<ShellItem, MauiNavigationView>, IAppearanceObserver
	{
		public static PropertyMapper<ShellItem, ShellItemHandler> Mapper =
				new PropertyMapper<ShellItem, ShellItemHandler>(ElementMapper)
				{
					[nameof(ShellItem.CurrentItem)] = MapCurrentItem,
					[Shell.SearchHandlerProperty.PropertyName] = MapSearchHandler
				};

		public static CommandMapper<ShellItem, ShellItemHandler> CommandMapper =
				new CommandMapper<ShellItem, ShellItemHandler>(ElementCommandMapper);


		ShellSectionHandler? _shellSectionHandler;
		ObservableCollection<NavigationViewItemViewModel> _mainLevelTabs;
		ShellItem? _shellItem;
		SearchHandler? _currentSearchHandler;

		public ShellItemHandler() : base(Mapper, CommandMapper)
		{
			_mainLevelTabs = new ObservableCollection<NavigationViewItemViewModel>();
		}

		protected override MauiNavigationView CreatePlatformElement()
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

			platformView.SetApplicationResource("NavigationViewContentMargin", null);
			platformView.SetApplicationResource("NavigationViewMinimalHeaderMargin", null);
			platformView.SetApplicationResource("NavigationViewHeaderMargin", null);
			platformView.SetApplicationResource("NavigationViewMinimalContentGridBorderThickness", null);

			return platformView;
		}

		protected override void ConnectHandler(MauiNavigationView platformView)
		{
			base.ConnectHandler(platformView);
			platformView.SelectionChanged += OnNavigationTabChanged;
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

			if (Routing.IsImplicit(VirtualView))
			{
				items = new List<BaseShellItem>(((IShellSectionController)VirtualView.CurrentItem).GetItems());
			}
			else
			{
				items = new List<BaseShellItem>(((IShellItemController)VirtualView).GetItems());
			}

			bool hasTabs = items.Count > 1;
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
					vm.Icon = bsi.Icon?.ToIconSource(MauiContext!)?.CreateIconElement();
				}
			});

			if (PlatformView.SelectedItem != selectedItem)
				PlatformView.SelectedItem = selectedItem;

			if (!hasTabs)
			{
				PlatformView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
			}
			else
			{
				PlatformView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
			}
		}

		void UpdateSearchHandler()
		{
			if(PlatformView.AutoSuggestBox == null)
				PlatformView.AutoSuggestBox = new Microsoft.UI.Xaml.Controls.AutoSuggestBox() { Width = 300 };

			if (VirtualView.Parent is not Shell shell)
				return;

			_currentSearchHandler = shell.GetEffectiveValue<SearchHandler?>(Shell.SearchHandlerProperty, null);

			var AutoSuggestBox = PlatformView.AutoSuggestBox;
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
					PlatformView.AutoSuggestBox.QueryIcon = new BitmapIcon() { UriSource = new Uri("ms-appx:///" + fis.File) };
				else
					PlatformView.AutoSuggestBox.QueryIcon = new SymbolIcon(Symbol.Find);
			}
		}


		

		public static void MapSearchHandler(ShellItemHandler handler, ShellItem item)
		{
		}

		public static void MapCurrentItem(ShellItemHandler handler, ShellItem item)
		{
			if (item.CurrentItem != null)
			{
				handler._shellSectionHandler ??= (ShellSectionHandler)item.CurrentItem.ToHandler(handler.MauiContext!);

				if (handler._shellSectionHandler.PlatformView != (FrameworkElement)handler.PlatformView.Content)
					handler.PlatformView.Content = handler._shellSectionHandler.PlatformView;

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
				if (PlatformView.TopNavArea == null)
				{
					PlatformView.OnApplyTemplateFinished += OnApplyTemplateFinished;

					void OnApplyTemplateFinished(object? sender, EventArgs e)
					{
						PlatformView.OnApplyTemplateFinished -= OnApplyTemplateFinished;
						ApplyAppearance();
					}
				}
				else
				{
					ApplyAppearance();
				}

				void ApplyAppearance()
				{
					PlatformView.UpdateTopNavAreaBackground(a.EffectiveTabBarBackgroundColor?.AsPaint());
					PlatformView.UpdateTopNavigationViewItemTextColor(a.EffectiveTabBarForegroundColor?.AsPaint());
				}
			}
		}
	}
}
