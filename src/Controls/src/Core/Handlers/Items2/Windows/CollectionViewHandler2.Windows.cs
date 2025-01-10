#nullable disable
#pragma warning disable RS0016 // Add public types and members to the declared API

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WASDKListViewSelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class CollectionViewHandler2 : ItemsViewHandler2<ReorderableItemsView>
	{
		bool _ignorePlatformSelectionChange;

		protected override IItemsLayout Layout { get => ItemsView?.ItemsLayout; }

		public static void MapCanReorderItems(CollectionViewHandler2 handler, ReorderableItemsView itemsView)
		{
		}

		public static void MapIsGrouped(CollectionViewHandler2 handler, GroupableItemsView itemsView)
		{
			handler.UpdateItemsSource();
		}

		public static void MapItemsSource(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
			ItemsViewHandler2<ReorderableItemsView>.MapItemsSource(handler, itemsView);
		}

		public static void MapSelectedItem(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{

		}

		public static void MapSelectedItems(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectionMode(CollectionViewHandler2 handler, SelectableItemsView itemsView)
		{
		}

		public static void MapHeaderTemplate(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
		}

		public static void MapFooterTemplate(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
		}
		public static void MapItemSizingStrategy(CollectionViewHandler2 handler, StructuredItemsView itemsView)
		{
		}

		protected override void ConnectHandler(WItemsView platformView)
		{
			base.ConnectHandler(platformView);

			ItemsView.SelectionChanged += VirtualSelectionChanged;
			if (PlatformView != null)
			{
				PlatformView.SetBinding(WItemsView.SelectionModeProperty,
						new Microsoft.UI.Xaml.Data.Binding
						{
							Source = ItemsView,
							Path = new Microsoft.UI.Xaml.PropertyPath("SelectionMode"),
							Converter = new SelectionModeConvert(),
							Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay
						});

				PlatformView.SelectionChanged += PlatformSelectionChanged;
			}
		}

		protected override void DisconnectHandler(WItemsView platformView)
		{
			var oldListViewBase = platformView;

			if (oldListViewBase != null)
			{
				oldListViewBase.ClearValue(WItemsView.SelectionModeProperty);
				oldListViewBase.SelectionChanged -= PlatformSelectionChanged;
			}

			if (ItemsView != null)
			{
				ItemsView.SelectionChanged -= VirtualSelectionChanged;
			}

			base.DisconnectHandler(platformView);
		}

		protected override void UpdateItemsSource()
		{
			_ignorePlatformSelectionChange = true;

			base.UpdateItemsSource();
			UpdatePlatformSelection();

			_ignorePlatformSelectionChange = false;
		}

		void VirtualSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdatePlatformSelection();
		}

		void PlatformSelectionChanged(WItemsView sender, ItemsViewSelectionChangedEventArgs args)
		{
			UpdateVirtualSelection();
		}

		void UpdateVirtualSelection()
		{
			if (_ignorePlatformSelectionChange || ItemsView == null)
			{
				return;
			}

			switch (PlatformView.SelectionMode)
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

			var formsItemContentControls = PlatformView.GetChildren<ItemContentControl>();

			foreach (var formsItemContentControl in formsItemContentControls)
			{
				bool isSelected = ItemsView.SelectedItem == formsItemContentControl.FormsDataContext ||
					ItemsView.SelectedItems.Contains(formsItemContentControl.FormsDataContext);
				formsItemContentControl.UpdateIsSelected(isSelected);
			}
		}


		void UpdateVirtualSingleSelection()
		{
			var selectedItem = PlatformView.SelectedItem is ItemTemplateContext2 itemPair
				? itemPair.Item
				: PlatformView.SelectedItem;

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
			for (int n = 0; n < PlatformView.SelectedItems.Count; n++)
			{
				var item = PlatformView.SelectedItems[n];
				selection.Add(item is ItemTemplateContext2 itc ? itc.Item : item);
			}

			ItemsView.UpdateSelectedItems(selection);
			ItemsView.SelectionChanged += VirtualSelectionChanged;
		}

		void UpdatePlatformSelection()
		{
			_ignorePlatformSelectionChange = true;

			var itemList = PlatformView.ItemsSource as ICollectionView;
			if (itemList is null)
				return;

			switch (PlatformView.SelectionMode)
			{
				case ItemsViewSelectionMode.None:
					break;
				case ItemsViewSelectionMode.Single:
					if (ItemsView != null)
					{
						if (ItemsView.SelectedItem == null)
						{
							PlatformView.DeselectAll();
						}
						else
						{
							var selectedItem = itemList.FirstOrDefault(item =>
							{
								if (item is ItemTemplateContext2 itemPair)
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
								PlatformView.Select(itemList.IndexOf(selectedItem));
							}
						}
					}

					break;
				case ItemsViewSelectionMode.Multiple:
					PlatformView.DeselectAll();
					for (int i = 0; i < itemList.Count; i++)
					{
						var nativeItem = itemList[i];
						if (nativeItem is ItemTemplateContext2 itemPair && ItemsView.SelectedItems.Contains(itemPair.Item))
						{
							PlatformView.Select(i);
						}
						else if (ItemsView.SelectedItems.Contains(nativeItem))
						{
							PlatformView.Select(i);
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

#pragma warning restore RS0016 // Add public types and members to the declared API