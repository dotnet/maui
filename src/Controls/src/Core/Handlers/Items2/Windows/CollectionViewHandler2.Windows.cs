using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;

namespace Microsoft.Maui.Controls.Handlers.Items2;

public partial class CollectionViewHandler2 : ItemsViewHandler2<ReorderableItemsView>
{
	bool _ignorePlatformSelectionChange;

	protected override IItemsLayout Layout { get => ItemsView.ItemsLayout; }

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

	protected override void ConnectHandler(WItemsView platformView)
	{
		base.ConnectHandler(platformView);

		ItemsView.SelectionChanged += VirtualSelectionChanged;
		if (PlatformView is not null)
		{
			PlatformView.SetBinding(WItemsView.SelectionModeProperty,
					new UI.Xaml.Data.Binding
					{
						Source = ItemsView,
						Path = new UI.Xaml.PropertyPath("SelectionMode"),
						Converter = new SelectionModeConvert(),
						Mode = UI.Xaml.Data.BindingMode.TwoWay
					});

			PlatformView.SelectionChanged += PlatformSelectionChanged;
		}
	}

	protected override void DisconnectHandler(WItemsView platformView)
	{
		var oldListViewBase = platformView;

		if (oldListViewBase is not null)
		{
			oldListViewBase.ClearValue(WItemsView.SelectionModeProperty);
			oldListViewBase.SelectionChanged -= PlatformSelectionChanged;
		}

		if (ItemsView is not null)
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

	void VirtualSelectionChanged(object? sender, SelectionChangedEventArgs? e)
	{
		UpdatePlatformSelection();
	}

	void PlatformSelectionChanged(WItemsView sender, ItemsViewSelectionChangedEventArgs args)
	{
		UpdateVirtualSelection();
	}

	void UpdateVirtualSelection()
	{
		if (_ignorePlatformSelectionChange || ItemsView is null)
		{
			return;
		}

		switch (PlatformView.SelectionMode)
		{
			case ItemsViewSelectionMode.Single:
				UpdateVirtualSingleSelection();
				break;
			case ItemsViewSelectionMode.Multiple:
				UpdateVirtualMultipleSelection();
				break;
			case ItemsViewSelectionMode.None:
			default:
				break;
		}

		var formsItemContentControls = PlatformView.GetChildren<ItemContentControl>();
		foreach (var formsItemContentControl in formsItemContentControls)
		{
			if (formsItemContentControl is not null)
			{
				bool isSelected = ItemsView.SelectedItem == formsItemContentControl.FormsDataContext ||
					ItemsView.SelectedItems.Contains(formsItemContentControl.FormsDataContext);
				formsItemContentControl.UpdateIsSelected(isSelected);
			}
		}
	}

	void UpdateVirtualSingleSelection()
	{
		var selectedItem = PlatformView.SelectedItem is ItemTemplateContext2 itemPair
			? itemPair.Item
			: PlatformView.SelectedItem;

		if (ItemsView is not null)
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
		for (int index = 0; index < PlatformView.SelectedItems.Count; index++)
		{
			var item = PlatformView.SelectedItems[index];
			var selectedItem = item is ItemTemplateContext2 itc ? itc.Item : item;
			if (selectedItem is not null)
				selection.Add(selectedItem);
		}

		ItemsView.UpdateSelectedItems(selection);
		ItemsView.SelectionChanged += VirtualSelectionChanged;
	}

	void UpdatePlatformSelection()
	{
		_ignorePlatformSelectionChange = true;

		var itemList = PlatformView.ItemsSource as ICollectionView;

		if (itemList is null)
		{
			return;
		}

		switch (PlatformView.SelectionMode)
		{
			case ItemsViewSelectionMode.Single:
				if (ItemsView is not null)
				{
					if (ItemsView.SelectedItem is null)
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
				
				// Use safe enumeration to avoid ArgumentOutOfRangeException during collection updates
				int index = 0;
				foreach (var nativeItem in itemList)
				{
					if (nativeItem is ItemTemplateContext2 itemPair && ItemsView.SelectedItems.Contains(itemPair.Item))
					{
						PlatformView.Select(index);
					}
					else if (ItemsView.SelectedItems.Contains(nativeItem))
					{
						PlatformView.Select(index);
					}
					index++;
				}
				break;
			case ItemsViewSelectionMode.None:	
			case ItemsViewSelectionMode.Extended:
			default:
				break;
		}

		_ignorePlatformSelectionChange = false;
	}
}

partial class SelectionModeConvert : UI.Xaml.Data.IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		var formSelectionMode = (SelectionMode)value;
		switch (formSelectionMode)
		{
			case SelectionMode.Single:
				return ItemsViewSelectionMode.Single;
			case SelectionMode.Multiple:
				return ItemsViewSelectionMode.Multiple;
			default:
				return ItemsViewSelectionMode.None;
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