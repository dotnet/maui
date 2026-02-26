using System;
using System.Collections.Generic;
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
	Page? _parentPage;

	protected override IItemsLayout Layout { get => ItemsView.ItemsLayout; }

	// Cache for MeasureFirstItem optimization
	global::Windows.Foundation.Size _firstItemMeasuredSize = global::Windows.Foundation.Size.Empty;

	public static void MapCanReorderItems(CollectionViewHandler2 handler, ReorderableItemsView itemsView)
	{
	}

	public static void MapIsGrouped(CollectionViewHandler2 handler, GroupableItemsView itemsView)
	{
		// When IsGrouped changes with GridItemsLayout, we need to recreate the layout
		// because grouped grids use GroupableUniformGridLayout while ungrouped use UniformGridLayout
		if (handler.Layout is GridItemsLayout)
		{
			handler.UpdateItemsLayout();
		}
		else
		{
			handler.UpdateItemsSource();
		}
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
		handler.UpdatePlatformSelection();
	}

	public static void MapSelectedItems(CollectionViewHandler2 handler, SelectableItemsView itemsView)
	{
		handler.UpdatePlatformSelection();
	}

	public static void MapSelectionMode(CollectionViewHandler2 handler, SelectableItemsView itemsView)
	{
		handler.UpdatePlatformSelection();
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
		
		// Subscribe to parent page lifecycle events to clear selection on navigation
		_parentPage = ItemsView.FindParentOfType<Page>();
		if (_parentPage is not null)
		{
			_parentPage.Disappearing += OnPageDisappearing;
		}
		
		if (PlatformView is not null)
		{
			PlatformView.SetBinding(WItemsView.SelectionModeProperty,
					new UI.Xaml.Data.Binding
					{
						Source = ItemsView,
						Path = new UI.Xaml.PropertyPath("SelectionMode"),
						Converter = new SelectionModeConverter(),
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
		
		// Unsubscribe from page lifecycle events
		if (_parentPage is not null)
		{
			_parentPage.Disappearing -= OnPageDisappearing;
			_parentPage = null;
		}

		base.DisconnectHandler(platformView);
	}

	void OnPageDisappearing(object? sender, EventArgs e)
	{
		// Clear selection when navigating away from the page
		// This allows re-selecting the same item when returning to the page
		if (ItemsView is null || PlatformView is null)
			return;

		_ignorePlatformSelectionChange = true;

		// Clear platform selection
		PlatformView.DeselectAll();

		// Clear virtual selection without firing SelectionChanged event
		ItemsView.SelectionChanged -= VirtualSelectionChanged;
		ItemsView.SelectedItem = null;
		ItemsView.SelectionChanged += VirtualSelectionChanged;

		_ignorePlatformSelectionChange = false;
	}

	protected override void UpdateItemsSource()
	{
		_ignorePlatformSelectionChange = true;

		base.UpdateItemsSource();
		UpdatePlatformSelection();

		_ignorePlatformSelectionChange = false;
	}

	/// <summary>
	/// Handles changes to the virtual (MAUI) selection and synchronizes them to the platform.
	/// </summary>
	void VirtualSelectionChanged(object? sender, SelectionChangedEventArgs? e)
	{
		UpdatePlatformSelection();
	}

	/// <summary>
	/// Handles changes to the platform (WinUI) selection and synchronizes them to the virtual view.
	/// </summary>
	void PlatformSelectionChanged(WItemsView sender, ItemsViewSelectionChangedEventArgs args)
	{
		if (PlatformView is null)
			return;

		UpdateVirtualSelection();
	}

	/// <summary>
	/// Reads the current platform selection state and updates the virtual (MAUI) selection accordingly.
	/// </summary>
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
				bool isSelected = object.Equals(ItemsView.SelectedItem, actualItem) || ItemsView.SelectedItems.Contains(actualItem);
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

		var newSelection = ComputeNewMultipleSelection();
		ItemsView.UpdateSelectedItems(newSelection);

		ItemsView.SelectionChanged += VirtualSelectionChanged;
	}

	/// <summary>
	/// Computes the new multiple selection list by merging the current platform selection
	/// with the existing virtual selection, preserving the order of previously selected items
	/// and appending newly selected items at the end.
	/// </summary>
	List<object> ComputeNewMultipleSelection()
	{
		// Extract actual items from platform selection (unwrapping ItemTemplateContext2)
		var currentPlatformSelection = ExtractPlatformSelectedItems();
		var previousSelection = new HashSet<object>(ItemsView.SelectedItems);
		var newSelection = new List<object>();
		var addedToSelection = new HashSet<object>();

		// Keep existing items that are still selected (maintains their order)
		foreach (var existingItem in ItemsView.SelectedItems)
		{
			if (currentPlatformSelection.Contains(existingItem))
			{
				newSelection.Add(existingItem);
				addedToSelection.Add(existingItem);
			}
		}

		// Append newly selected items (in platform but not in previous virtual selection)
		foreach (var item in currentPlatformSelection)
		{
			if (!previousSelection.Contains(item) && !addedToSelection.Contains(item))
			{
				newSelection.Add(item);
			}
		}

		return newSelection;
	}

	/// <summary>
	/// Extracts the actual data items from the platform's selected items,
	/// unwrapping <see cref="ItemTemplateContext2"/> wrappers when present.
	/// </summary>
	HashSet<object> ExtractPlatformSelectedItems()
	{
		var result = new HashSet<object>();
		foreach (var item in PlatformView.SelectedItems)
		{
			var selectedItem = item is ItemTemplateContext2 itc ? itc.Item : item;
			if (selectedItem is not null)
				result.Add(selectedItem);
		}
		return result;
	}

	/// <summary>
	/// Reads the current virtual (MAUI) selection state and updates the platform (WinUI) selection accordingly.
	/// </summary>
	void UpdatePlatformSelection()
	{
		if (PlatformView is null || ItemsView is null)
		{
			return;
		}

		var itemList = PlatformView.ItemsSource as ICollectionView;

		if (itemList is null)
		{
			return;
		}

		_ignorePlatformSelectionChange = true;

		switch (PlatformView.SelectionMode)
		{
			case ItemsViewSelectionMode.Single:
				if (ItemsView.SelectedItem is null)
				{
					PlatformView.DeselectAll();
				}
				else
				{
					var selectedIndex = FindItemIndexInSource(itemList, ItemsView.SelectedItem);
					if (selectedIndex >= 0)
					{
						PlatformView.Select(selectedIndex);
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

	/// <summary>
	/// Finds the index of the specified item in the collection view source, using a single-pass
	/// iteration. Returns -1 if the item is not found.
	/// </summary>
	static int FindItemIndexInSource(ICollectionView itemList, object targetItem)
	{
		int index = 0;
		foreach (var nativeItem in itemList)
		{
			var actualItem = nativeItem is ItemTemplateContext2 itc ? itc.Item : nativeItem;
			if (object.Equals(actualItem, targetItem))
			{
				return index;
			}
			index++;
		}
		return -1;
	}
}

/// <summary>
/// Converts between MAUI <see cref="SelectionMode"/> and WinUI <see cref="ItemsViewSelectionMode"/> values.
/// </summary>
partial class SelectionModeConverter : UI.Xaml.Data.IValueConverter
{
	/// <inheritdoc />
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		var selectionMode = (SelectionMode)value;
		switch (selectionMode)
		{
			case SelectionMode.Single:
				return ItemsViewSelectionMode.Single;
			case SelectionMode.Multiple:
				return ItemsViewSelectionMode.Multiple;
			default:
				return ItemsViewSelectionMode.None;
		}
	}

	/// <inheritdoc />
	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		var winUISelectionMode = (ItemsViewSelectionMode)value;
		switch (winUISelectionMode)
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