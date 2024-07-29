#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WASDKListViewSelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode;
using WASDKSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
	{
		bool _ignorePlatformSelectionChange;

		protected override void ConnectHandler(WItemsView platformView)
		{
			base.ConnectHandler(platformView);

			if (ItemsView != null)
			{
				ItemsView.SelectionChanged += VirtualSelectionChanged;
			}

			var newListViewBase = ListViewBase;

			if (newListViewBase != null)
			{
				//newListViewBase.SetBinding(ListViewBase.SelectionModeProperty,
				//		new Microsoft.UI.Xaml.Data.Binding
				//		{
				//			Source = ItemsView,
				//			Path = new Microsoft.UI.Xaml.PropertyPath("SelectionMode"),
				//			Converter = new SelectionModeConvert(),
				//			Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay
				//		});
				//
				//newListViewBase.SelectionChanged += PlatformSelectionChanged;
			}

			UpdatePlatformSelection();
		}

		protected override void DisconnectHandler(WItemsView platformView)
		{
			var oldListViewBase = platformView;

			if (oldListViewBase != null)
			{
				//oldListViewBase.ClearValue(ListViewBase.SelectionModeProperty);
				//oldListViewBase.SelectionChanged -= PlatformSelectionChanged;
			}

			if (ItemsView != null)
			{
				ItemsView.SelectionChanged -= VirtualSelectionChanged;
			}

			base.DisconnectHandler(platformView);
		}

		public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		void UpdatePlatformSelection()
		{
			_ignorePlatformSelectionChange = true;

			var itemList = ListViewBase.ItemsSource as ICollectionView;
			switch (ListViewBase.SelectionMode)
			{
				case ItemsViewSelectionMode.None:
					break;
				case ItemsViewSelectionMode.Single:
					if (ItemsView != null)
					{
						if (ItemsView.SelectedItem == null)
						{
							ListViewBase.DeselectAll();
						}
						else
						{
							var selectedItem = itemList.FirstOrDefault(item =>
							{
								if (item is ItemTemplateContext itemPair)
								{
									return itemPair.Item == ItemsView.SelectedItem;
								}
								else
								{
									return item == ItemsView.SelectedItem;
								}
							});
							if (selectedItem is not null)
							{
								ListViewBase.Select(itemList.IndexOf(selectedItem));
							}	
						}
					}
			
					break;
				case ItemsViewSelectionMode.Multiple:
					ListViewBase.DeselectAll();
					for (int i = 0; i < itemList.Count; i++)
					{
						var nativeItem = itemList[i];
						if (nativeItem is ItemTemplateContext itemPair && ItemsView.SelectedItems.Contains(itemPair.Item))
						{
							ListViewBase.Select(i);
						}
						else if (ItemsView.SelectedItems.Contains(nativeItem))
						{
							ListViewBase.Select(i);
						}
					}
					break;
				case ItemsViewSelectionMode.Extended:
					break;
				default:
					break;
			}

			_ignorePlatformSelectionChange = false;
		}

		void VirtualSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdatePlatformSelection();
		}

		void PlatformSelectionChanged(object sender, WASDKSelectionChangedEventArgs args)
		{
			UpdateVirtualSelection();
		}

		void UpdateVirtualSelection()
		{
			if (_ignorePlatformSelectionChange || ItemsView == null)
			{
				return;
			}

			switch (ListViewBase.SelectionMode)
			{
				case ItemsViewSelectionMode.None:
					break;
				case ItemsViewSelectionMode.Single:
					UpdateVirtualSingleSelection();
					break;
				case ItemsViewSelectionMode.Multiple:
					UpdateVirtualMultipleSelection();
					break;
				default:
					break;
			}

			var formsItemContentControls = ListViewBase.GetChildren<ItemContentControl>();

			foreach (var formsItemContentControl in formsItemContentControls)
			{
				bool isSelected = ItemsView.SelectedItem == formsItemContentControl.FormsDataContext || ItemsView.SelectedItems.Contains(formsItemContentControl.FormsDataContext);
				formsItemContentControl.UpdateIsSelected(isSelected);
			}
		}

		void UpdateVirtualSingleSelection()
		{
			var selectedItem = ListViewBase.SelectedItem is ItemTemplateContext itemPair
				? itemPair.Item
				: ListViewBase.SelectedItem;

			if (ItemsView != null)
			{
				ItemsView.SelectionChanged -= VirtualSelectionChanged;
				ItemsView.SelectedItem = selectedItem;

				ItemsView.SelectionChanged += VirtualSelectionChanged;
			}
		}

		void UpdateVirtualMultipleSelection()
		{
			ItemsView.SelectionChanged -= VirtualSelectionChanged;

			var selection = new List<object>();
			for (int n = 0; n < ListViewBase.SelectedItems.Count; n++)
			{
				var item = ListViewBase.SelectedItems[n];
				selection.Add(item is ItemTemplateContext itc ? itc.Item : item);
			}

			ItemsView.UpdateSelectedItems(selection);

			ItemsView.SelectionChanged += VirtualSelectionChanged;
		}

		protected override void UpdateItemsSource()
		{
			_ignorePlatformSelectionChange = true;

			base.UpdateItemsSource();
			UpdatePlatformSelection();

			_ignorePlatformSelectionChange = false;
		}

		class SelectionModeConvert : Microsoft.UI.Xaml.Data.IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, string language)
			{
				var formSelectionMode = (SelectionMode)value;
				switch (formSelectionMode)
				{
					case SelectionMode.None:
						return ItemsViewSelectionMode.None;
					case SelectionMode.Single:
						return ItemsViewSelectionMode.Single;
					case SelectionMode.Multiple:
						return ItemsViewSelectionMode.Multiple;
					default:
						return WASDKListViewSelectionMode.None;
				}
			}

			public object ConvertBack(object value, Type targetType, object parameter, string language)
			{
				var uwpListViewSelectionMode = (ItemsViewSelectionMode)value;
				switch (uwpListViewSelectionMode)
				{
					case ItemsViewSelectionMode.None:
						return SelectionMode.None;
					case ItemsViewSelectionMode.Single:
						return SelectionMode.Single;
					case ItemsViewSelectionMode.Multiple:
						return SelectionMode.Multiple;
					case ItemsViewSelectionMode.Extended:
						return SelectionMode.None;
					default:
						return SelectionMode.None;
				}
			}
		}
	}
}
