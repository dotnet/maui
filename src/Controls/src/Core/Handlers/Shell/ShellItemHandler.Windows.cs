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
	public partial class ShellItemHandler : ElementHandler<ShellItem, MauiNavigationView>
	{
		public static PropertyMapper<ShellItem, ShellItemHandler> Mapper =
				new PropertyMapper<ShellItem, ShellItemHandler>(ElementMapper)
				{
					[nameof(ShellItem.CurrentItem)] = MapCurrentItem
				};

		public static CommandMapper<ShellItem, ShellItemHandler> CommandMapper =
				new CommandMapper<ShellItem, ShellItemHandler>(ElementCommandMapper);


		ShellSectionHandler? _shellSectionHandler;
		ObservableCollection<NavigationViewItemViewModel> _mainLevelTabs;

		public ShellItemHandler() : base(Mapper, CommandMapper)
		{
			_mainLevelTabs = new ObservableCollection<NavigationViewItemViewModel>();
		}

		protected override MauiNavigationView CreateNativeElement()
		{
			var platformView =  new MauiNavigationView()
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

		protected override void ConnectHandler(MauiNavigationView nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.SelectionChanged += OnNavigationTabChanged;
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
				navItem.Content = baseShellItem.Title;

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
					navItem.MenuItemsSource = null;
				}
				else
				{
					navItem.MenuItemsSource ??= new ObservableCollection<NavigationViewItemViewModel>();
					navItem
						.MenuItemsSource
						.SyncItems(shellSectionItems, (shellConentNavItem, shellContent) =>
						{
							shellConentNavItem.Content = shellContent.Title;

							if (shellSection == VirtualView.CurrentItem &&
								shellContent == VirtualView.CurrentItem.CurrentItem)
							{
								selectedItem = shellConentNavItem;
							}
						});

					hasTabs = hasTabs || shellSectionItems.Count > 1;
				}
			});

			if (NativeView.SelectedItem != selectedItem)
				NativeView.SelectedItem = selectedItem;

			if (!hasTabs)
			{
				NativeView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
			}
			else
			{
				NativeView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
			}
		}

		public static void MapCurrentItem(ShellItemHandler handler, ShellItem item)
		{
			if (item.CurrentItem != null)
			{
				handler._shellSectionHandler ??= (ShellSectionHandler)item.CurrentItem.ToHandler(handler.MauiContext!);

				if (handler._shellSectionHandler.NativeView != (FrameworkElement)handler.NativeView.Content)
					handler.NativeView.Content = handler._shellSectionHandler.NativeView;

				if (handler._shellSectionHandler.VirtualView != item.CurrentItem)
					handler._shellSectionHandler.SetVirtualView(item.CurrentItem);
			}

			handler.MapMenuItems();
		}
	}
}
