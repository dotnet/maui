using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;

namespace Microsoft.Maui.Controls.Handlers.Items2;

public partial class CollectionViewHandler2
{
	public CollectionViewHandler2() : base(Mapper)
	{
	}


	public CollectionViewHandler2(PropertyMapper? mapper = null) : base(mapper ?? Mapper)
	{
	}

	public static PropertyMapper<CollectionView, CollectionViewHandler2> Mapper = new(ItemsViewMapper)
	{
		[ReorderableItemsView.CanReorderItemsProperty.PropertyName] = MapCanReorderItems,
		[GroupableItemsView.IsGroupedProperty.PropertyName] = MapIsGrouped,
		[GroupableItemsView.GroupHeaderTemplateProperty.PropertyName] = MapGroupHeaderTemplate,
		[GroupableItemsView.GroupFooterTemplateProperty.PropertyName] = MapGroupFooterTemplate,
		[SelectableItemsView.SelectedItemProperty.PropertyName] = MapSelectedItem,
		[SelectableItemsView.SelectedItemsProperty.PropertyName] = MapSelectedItems,
		[SelectableItemsView.SelectionModeProperty.PropertyName] = MapSelectionMode,
		[StructuredItemsView.ItemSizingStrategyProperty.PropertyName] = MapItemSizingStrategy,
		
	};
}
public partial class CollectionViewHandler2 : ItemsViewHandler2<ReorderableItemsView>
{
	bool _ignorePlatformSelectionChange;

	protected override IItemsLayout Layout { get => ItemsView.ItemsLayout; }

	// Cache for MeasureFirstItem optimization
	global::Windows.Foundation.Size _firstItemMeasuredSize = global::Windows.Foundation.Size.Empty;

	public static void MapCanReorderItems(CollectionViewHandler2 handler, ReorderableItemsView itemsView)
	{
	}

	public static void MapIsGrouped(CollectionViewHandler2 handler, GroupableItemsView itemsView)
	{
		handler.UpdateItemsSource();
	}

	public static void MapGroupHeaderTemplate(CollectionViewHandler2 handler, GroupableItemsView itemsView)
	{
		handler.UpdateItemsSource();
	}

	public static void MapGroupFooterTemplate(CollectionViewHandler2 handler, GroupableItemsView itemsView)
	{
		handler.UpdateItemsSource();
	}

	public static void MapItemSizingStrategy(CollectionViewHandler2 handler, ItemsView itemsView)
	{
		handler.InvalidateFirstItemSize();
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

	/// <summary>
	/// Gets the cached first item measured size for MeasureFirstItem optimization.
	/// Returns Size.Empty if not cached or not using MeasureFirstItem strategy.
	/// </summary>
	internal global::Windows.Foundation.Size GetCachedFirstItemSize()
	{
		if (VirtualView is CollectionView cv && cv.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
		{
			return _firstItemMeasuredSize;
		}
		return global::Windows.Foundation.Size.Empty;
	}

	/// <summary>
	/// Sets the cached first item measured size for MeasureFirstItem optimization.
	/// </summary>
	internal void SetCachedFirstItemSize(global::Windows.Foundation.Size size)
	{
		if (VirtualView is CollectionView cv && cv.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
		{
			_firstItemMeasuredSize = size;
		}
	}

	/// <summary>
	/// Invalidates the cached first item size.
	/// </summary>
	internal void InvalidateFirstItemSize()
	{
		_firstItemMeasuredSize = global::Windows.Foundation.Size.Empty;
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
			oldListViewBase.SelectionChanged -= PlatformSelectionChanged;
			oldListViewBase.ClearValue(WItemsView.SelectionModeProperty);
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
		if (PlatformView is null)
			return;

		UpdateVirtualSelection();
	}

	void UpdateVirtualSelection()
	{
		if (_ignorePlatformSelectionChange || ItemsView is null || PlatformView is null)
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

		UpdateVisualStates();
	}

	void UpdateVisualStates()
	{
		if (PlatformView is null || ItemsView is null)
			return;

		foreach (var itemcontainer in PlatformView.GetChildren<ItemContainer>())
		{
			if (itemcontainer?.Child is ElementWrapper wrapper && wrapper.VirtualView is VisualElement visualElement)
			{
				var actualItem = visualElement.BindingContext;
				bool isSelected = ItemsView.SelectedItem == actualItem || ItemsView.SelectedItems.Contains(actualItem);
				VisualStateManager.GoToState(visualElement, isSelected ? VisualStateManager.CommonStates.Selected : VisualStateManager.CommonStates.Normal);
			}
		}
	}

	void UpdateVirtualSingleSelection()
	{
		if (PlatformView is null || ItemsView is null)
			return;

		var selectedItem = PlatformView.SelectedItem is ItemTemplateContext2 itemPair
			? itemPair.Item
			: PlatformView.SelectedItem;

		ItemsView.SelectionChanged -= VirtualSelectionChanged;
		ItemsView.SelectedItem = selectedItem;

		ItemsView.SelectionChanged += VirtualSelectionChanged;
	}

	void UpdateVirtualMultipleSelection()
	{
		if (PlatformView is null || ItemsView is null)
			return;

		ItemsView.SelectionChanged -= VirtualSelectionChanged;

		// Get current platform selection
		var currentPlatformSelection = new HashSet<object>();
		foreach (var item in PlatformView.SelectedItems)
		{
			var selectedItem = item is ItemTemplateContext2 itc ? itc.Item : item;
			if (selectedItem is not null)
				currentPlatformSelection.Add(selectedItem);
		}

		// Get previous virtual selection
		var previousSelection = new HashSet<object>(ItemsView.SelectedItems);

		// Find newly selected items (in platform but not in virtual)
		var newlySelected = new List<object>();
		foreach (var item in PlatformView.SelectedItems)
		{
			var selectedItem = item is ItemTemplateContext2 itc ? itc.Item : item;
			if (selectedItem is not null && !previousSelection.Contains(selectedItem))
				newlySelected.Add(selectedItem);
		}

		// Build new selection: existing items still selected + newly selected items
		var newSelection = new List<object>();

		// Keep existing items that are still selected (maintains their order)
		foreach (var existingItem in ItemsView.SelectedItems)
		{
			if (currentPlatformSelection.Contains(existingItem))
			{
				newSelection.Add(existingItem);
			}
		}

		// Add newly selected items at the end (in selection order)
		foreach (var item in newlySelected)
		{
			if (!newSelection.Contains(item))
			{
				newSelection.Add(item);
			}
		}

		ItemsView.UpdateSelectedItems(newSelection);
		ItemsView.SelectionChanged += VirtualSelectionChanged;
	}

	void UpdatePlatformSelection()
	{
		if (PlatformView is null || ItemsView is null)
		{
			return;
		}

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
		UpdateVisualStates();
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