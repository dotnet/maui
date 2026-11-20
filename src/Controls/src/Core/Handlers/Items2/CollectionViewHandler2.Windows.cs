using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Controls.Handlers.Items2;

public partial class CollectionViewHandler2
{
	public CollectionViewHandler2() : base(Mapper)
	{
	}


	public CollectionViewHandler2(PropertyMapper? mapper = null) : base(mapper ?? Mapper)
	{
	}

	public static PropertyMapper<CollectionView, CollectionViewHandler2> Mapper = new(ReorderableItemsViewMapper)
	{
		[GroupableItemsView.IsGroupedProperty.PropertyName] = MapIsGrouped,
		[GroupableItemsView.GroupHeaderTemplateProperty.PropertyName] = MapGroupHeaderTemplate,
		[GroupableItemsView.GroupFooterTemplateProperty.PropertyName] = MapGroupFooterTemplate,
		[SelectableItemsView.SelectedItemProperty.PropertyName] = MapSelectedItem,
		[SelectableItemsView.SelectedItemsProperty.PropertyName] = MapSelectedItems,
		[SelectableItemsView.SelectionModeProperty.PropertyName] = MapSelectionMode,
		[SelectableItemsView.IsItemTapEnabledMapperKey] = MapIsItemTapEnabled,
		[StructuredItemsView.ItemSizingStrategyProperty.PropertyName] = MapItemSizingStrategy,

	};
}
public partial class CollectionViewHandler2 : ReorderableItemsViewHandler2<ReorderableItemsView>
{
	bool _ignorePlatformSelectionChange;
	bool _selectionDirty;
	bool _selectionUpdateQueued;
	bool _platformSelectionUpdateQueued;
	Action<int>? _containerPreparedHandler;
	UI.Xaml.Input.KeyEventHandler? _keyDownHandler;

	// Cache for MeasureFirstItem optimization
	global::Windows.Foundation.Size _firstItemMeasuredSize = global::Windows.Foundation.Size.Empty;

	public static void MapIsGrouped(CollectionViewHandler2 handler, GroupableItemsView itemsView)
	{
		// When IsGrouped changes with GridItemsLayout, we need to recreate the layout
		// because grouped grids use GroupableUniformGridLayout while ungrouped use UniformGridLayout
		if (handler.Layout is GridItemsLayout)
		{
			handler.UpdateItemsLayout();
		}

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
		handler.UpdatePlatformSelection();
	}

	public static void MapSelectedItems(CollectionViewHandler2 handler, SelectableItemsView itemsView)
	{
		handler.UpdatePlatformSelection();
	}

	/// <summary>
	/// Left margin applied to item content when in multi-select mode to avoid overlapping
	/// the WinUI ItemContainer checkbox. Matches WinUI's ListViewItemMultiselectCheckBoxMargin.
	/// </summary>
	internal static readonly WThickness MultiSelectContentMargin = new WThickness(32, 0, 0, 0);
	static readonly WThickness ZeroMargin = new WThickness(0);

	internal static void MapIsItemTapEnabled(CollectionViewHandler2 handler, SelectableItemsView itemsView)
	{
		handler.UpdateItemInvokedEnabled();
	}

	public static void MapSelectionMode(CollectionViewHandler2 handler, SelectableItemsView itemsView)
	{
		handler.UpdateItemInvokedEnabled();
		handler.UpdatePlatformSelection();
		handler.UpdateMultiSelectContentMargin();
		// The margin change affects item sizes, so invalidate the MeasureFirstItem cache
		// to avoid stale sizes being applied to items realized after the mode switch.
		handler.InvalidateFirstItemSize();
	}

	/// <summary>
	/// WinUI raises ItemInvoked for the Enter key and UI Automation invoke in every mode; for taps and the Space key
	/// only in <see cref="SelectionMode.None"/>; and for double taps only when selection is enabled. The remaining
	/// cases are reported by <see cref="PlatformTapped"/>, <see cref="PlatformDoubleTapped"/> and
	/// <see cref="PlatformKeyDown"/>. Taps that a control inside the item handled are not reported.
	/// </summary>
	void UpdateItemInvokedEnabled()
	{
		if (PlatformView is null || ItemsView is null)
			return;

		PlatformView.IsItemInvokedEnabled = ItemsView.IsItemTapEnabled;
	}

	// The rows shown by the platform view, indexed like its item containers (see UpdatePlatformSelection)
	ICollectionView? PlatformRows => PlatformView?.ItemsSource as ICollectionView;

	void PlatformItemInvoked(WItemsView sender, ItemsViewItemInvokedEventArgs args)
	{
		if (ItemsView is null || !ItemsView.IsItemTapEnabled || PlatformRows is not ICollectionView rows)
			return;

		var index = -1;

		// Without an item template the items themselves are shown, so prefer the invoked row, which WinUI makes current;
		// item contexts are unique per row, so matching them is exact even when the items repeat
		var current = sender.CurrentItemIndex;
		if (current >= 0 && current < rows.Count && IsSameRow(rows[current], args.InvokedItem))
		{
			index = current;
		}
		else
		{
			for (var row = 0; row < rows.Count; row++)
			{
				if (IsSameRow(rows[row], args.InvokedItem))
				{
					index = row;
					break;
				}
			}
		}

		ReportItemTapped(index);

		static bool IsSameRow(object row, object invokedItem) =>
			row is ItemTemplateContext2 ? ReferenceEquals(row, invokedItem) : Equals(row, invokedItem);
	}

	void PlatformTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
	{
		// In None mode ItemInvoked reports single taps
		if (ItemsView?.SelectionMode != SelectionMode.None)
		{
			ReportItemTapped(e.OriginalSource);
		}
	}

	void PlatformDoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
	{
		// The second tap of a double tap raises DoubleTapped instead of Tapped. ItemInvoked reports it when
		// selection is enabled, but not in None mode.
		if (ItemsView?.SelectionMode == SelectionMode.None)
		{
			ReportItemTapped(e.OriginalSource);
		}
	}

	void PlatformKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		// With selection enabled, Space selects the focused item without raising ItemInvoked; the item container
		// has already handled the key, so this handler also runs for handled events
		if (e.Key == global::Windows.System.VirtualKey.Space && !e.KeyStatus.WasKeyDown && e.OriginalSource is ItemContainer &&
			ItemsView is not null && ItemsView.SelectionMode != SelectionMode.None)
		{
			ReportItemTapped(e.OriginalSource);
		}
	}

	void ReportItemTapped(object? originalSource)
	{
		if (ItemsView is null || !ItemsView.IsItemTapEnabled ||
			PlatformView is not MauiItemsView platformView || platformView.ItemsRepeaterControl is not ItemsRepeater repeater)
		{
			return;
		}

		// Only items count, not the header, footer or empty view
		for (var current = originalSource as Microsoft.UI.Xaml.DependencyObject;
			current is not null && !ReferenceEquals(current, platformView);
			current = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(current))
		{
			if (current is ItemContainer container)
			{
				ReportItemTapped(repeater.GetElementIndex(container));
				return;
			}
		}
	}

	void ReportItemTapped(int index)
	{
		if (ItemsView is not SelectableItemsView itemsView || PlatformRows is not ICollectionView rows ||
			index < 0 || index >= rows.Count)
		{
			return;
		}

		var entry = rows[index];

		if (entry is ItemTemplateContext2 { IsHeader: true } or ItemTemplateContext2 { IsFooter: true })
		{
			return;
		}

		var item = entry is ItemTemplateContext2 context ? context.Item : entry;
		var tap = TryGetItemPosition(itemsView, index, out var groupIndex, out var indexInGroup)
			? itemsView.CreateItemTappedEventArgs(item, null, groupIndex, indexInGroup)
			: itemsView.CreateItemTappedEventArgs(item, null, -1, -1);

		// The platform selection is synced to the MAUI selection on the dispatcher; report the tap after that
		var dispatcherQueue = PlatformView?.DispatcherQueue;
		if (dispatcherQueue is null || !dispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
			{
				if (itemsView.Handler is not null)
					itemsView.SendItemTapped(tap);
			}))
		{
			itemsView.SendItemTapped(tap);
		}
	}

	/// <summary>
	/// Maps a row of the flattened platform list to the item's position in the items source. When grouped,
	/// the list holds each group's header (if there is a header template), its items and its footer (if there is a
	/// footer template); without an item template only the items are shown.
	/// </summary>
	static bool TryGetItemPosition(SelectableItemsView itemsView, int flatIndex, out int groupIndex, out int indexInGroup)
	{
		groupIndex = -1;
		indexInGroup = flatIndex;

		if (itemsView is not GroupableItemsView { IsGrouped: true } groupable)
		{
			return true;
		}

		indexInGroup = -1;

		if (groupable.ItemsSource is not System.Collections.IEnumerable groups)
		{
			return false;
		}

		var templated = groupable.ItemTemplate is not null;
		var hasHeader = templated && groupable.GroupHeaderTemplate is not null;
		var hasFooter = templated && groupable.GroupFooterTemplate is not null;
		var position = 0;
		var currentGroup = 0;

		foreach (var group in groups)
		{
			if (group is string || group is not System.Collections.IEnumerable groupItems)
			{
				continue;
			}

			if (hasHeader)
			{
				position++;
			}

			var count = 0;
			foreach (var _ in groupItems)
			{
				count++;
			}

			if (flatIndex >= position && flatIndex < position + count)
			{
				groupIndex = currentGroup;
				indexInGroup = flatIndex - position;
				return true;
			}

			position += count + (hasFooter ? 1 : 0);
			currentGroup++;
		}

		return false;
	}

	/// <summary>
	/// When in multi-select mode the WinUI ItemContainer renders a checkbox overlay
	/// on the left. Add left margin to the content (ElementWrapper) so that user
	/// content is not hidden behind the checkbox.
	/// </summary>
	void UpdateMultiSelectContentMargin()
	{
		if (PlatformView is null || ItemsView is null)
			return;

		var margin = ItemsView.SelectionMode == SelectionMode.Multiple
			? MultiSelectContentMargin
			: ZeroMargin;

		foreach (var container in PlatformView.GetChildren<ItemContainer>())
		{
			if (container?.Child is ElementWrapper wrapper && !wrapper.IsHeaderOrFooter)
			{
				wrapper.Margin = margin;
			}
		}
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
			PlatformView.IsTabStop = false;
			PlatformView.SetBinding(WItemsView.SelectionModeProperty,
					new UI.Xaml.Data.Binding
					{
						Source = ItemsView,
						Path = new UI.Xaml.PropertyPath("SelectionMode"),
						Converter = new SelectionModeConverter(),
						Mode = UI.Xaml.Data.BindingMode.TwoWay
					});

			PlatformView.SelectionChanged += PlatformSelectionChanged;
			PlatformView.ItemInvoked += PlatformItemInvoked;
			PlatformView.Tapped += PlatformTapped;
			PlatformView.DoubleTapped += PlatformDoubleTapped;
			PlatformView.AddHandler(UI.Xaml.UIElement.KeyDownEvent, _keyDownHandler ??= new UI.Xaml.Input.KeyEventHandler(PlatformKeyDown), true);
			PlatformView.Loaded += OnPlatformViewLoaded;

			if (PlatformView is MauiItemsView mauiItemsViewConnect)
			{
				mauiItemsViewConnect.ReattachAccessibilityHelper();
			}
		}
	}

	void OnPlatformViewLoaded(object? sender, UI.Xaml.RoutedEventArgs e)
	{
		if (sender is not MauiItemsView platformView || _selectionUpdateQueued)
			return;

		_selectionUpdateQueued = true;

		void OnReady()
		{
			if (!_selectionUpdateQueued || !ReferenceEquals(PlatformView, platformView))
			{
				return;
			}

			_selectionUpdateQueued = false;
			if (_containerPreparedHandler is not null)
			{
				platformView.ContainerPrepared -= _containerPreparedHandler;
				_containerPreparedHandler = null;
			}

			PlatformView.IsTabStop = true;
			if (_selectionDirty)
			{
				_selectionDirty = false;
				UpdatePlatformSelection();
			}
		}

		_containerPreparedHandler = _ => OnReady();
		platformView.ContainerPrepared += _containerPreparedHandler;
		if (!platformView.DispatcherQueue.TryEnqueue(OnReady))
		{
			OnReady();
		}
	}


	protected override void DisconnectHandler(WItemsView platformView)
	{
		if (platformView is null)
		{
			return;
		}

		platformView.SelectionChanged -= PlatformSelectionChanged;
		platformView.ItemInvoked -= PlatformItemInvoked;
		platformView.Tapped -= PlatformTapped;
		platformView.DoubleTapped -= PlatformDoubleTapped;
		if (_keyDownHandler is not null)
		{
			platformView.RemoveHandler(UI.Xaml.UIElement.KeyDownEvent, _keyDownHandler);
		}
		platformView.Loaded -= OnPlatformViewLoaded;
		platformView.ClearValue(WItemsView.SelectionModeProperty);
		if (platformView is MauiItemsView mauiItemsView)
		{
			if (_containerPreparedHandler is not null)
			{
				mauiItemsView.ContainerPrepared -= _containerPreparedHandler;
				_containerPreparedHandler = null;
			}

			mauiItemsView.CleanUpAccessibilityHelper();
			mauiItemsView.CleanUpAutomationEvents();
		}
		_selectionUpdateQueued = false;
		_platformSelectionUpdateQueued = false;

		if (ItemsView is not null)
		{
			ItemsView.SelectionChanged -= VirtualSelectionChanged;
		}


		base.DisconnectHandler(platformView);
	}

	protected override void UpdateItemsSource()
	{
		if (PlatformView is not null && PlatformView is MauiItemsView mauiItemsView)
		{
			mauiItemsView.CancelPendingAccessibilityFocus();
		}

		_ignorePlatformSelectionChange = true;
		try
		{
			base.UpdateItemsSource();
			UpdatePlatformSelection();
		}
		finally
		{
			_ignorePlatformSelectionChange = false;
		}
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
		if (_ignorePlatformSelectionChange ||
			!ReferenceEquals(((IElementHandler)this).PlatformView, sender) ||
			_platformSelectionUpdateQueued)
			return;

		_platformSelectionUpdateQueued = true;
		if (!sender.DispatcherQueue.TryEnqueue(() =>
		{
			_platformSelectionUpdateQueued = false;
			if (ReferenceEquals(((IElementHandler)this).PlatformView, sender))
			{
				UpdateVirtualSelection(sender);
			}
		}))
		{
			_platformSelectionUpdateQueued = false;
		}
	}

	/// <summary>
	/// Reads the current platform selection state and updates the virtual (MAUI) selection accordingly.
	/// </summary>
	void UpdateVirtualSelection(WItemsView platformView)
	{
		if (_ignorePlatformSelectionChange ||
			!ReferenceEquals(((IElementHandler)this).PlatformView, platformView) ||
			((IElementHandler)this).VirtualView is null)
		{
			return;
		}

		switch (platformView.SelectionMode)
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

		if (ReferenceEquals(((IElementHandler)this).PlatformView, platformView))
		{
			UpdateVisualStates();
		}
	}

	void UpdateVisualStates()
	{
		if (PlatformView is null || ItemsView is null)
			return;

		var transparent = new WSolidColorBrush(Microsoft.UI.Colors.Transparent);

		foreach (var itemContainer in PlatformView.GetChildren<ItemContainer>())
		{
			if (itemContainer?.Child is ElementWrapper wrapper && wrapper.VirtualView is VisualElement visualElement)
			{
				var actualItem = visualElement.BindingContext;
				bool isSelected = object.Equals(ItemsView.SelectedItem, actualItem) || ItemsView.SelectedItems.Contains(actualItem);
				// Use IsItemSelected instead of GoToState directly so that ChangeVisualState()
				// has the correct selected state when a pointer-enter/leave or IsEnabled change
				// fires later. IsElementInSelectedState() reads IsItemSelected, so bypassing it
				// here (as was done before PR #35421) caused PointerOver-exit and re-enable
				// events to incorrectly transition the item to Normal instead of Selected.
				visualElement.IsItemSelected = isSelected;

				// When the item template defines a "Selected" visual state, MAUI
				// handles the selection appearance. Suppress the native WinUI
				// selection border (PART_SelectionVisual) and selected background
				// fill on that container so the two don't overlap. Items without
				// a Selected visual state keep the default WinUI selection visuals.
				if (visualElement.HasVisualState(VisualStateManager.CommonStates.Selected))
				{
					itemContainer.Resources["ItemContainerSelectedBorderBrush"] = transparent;
					itemContainer.Resources["ItemContainerSelectedPointerOverBorderBrush"] = transparent;
					itemContainer.Resources["ItemContainerSelectedPressedBorderBrush"] = transparent;
					itemContainer.Resources["ItemContainerSelectionVisualBackground"] = transparent;
					itemContainer.Resources["ItemContainerSelectionVisualPointerOverBackground"] = transparent;
					itemContainer.Resources["ItemContainerSelectionVisualPressedBackground"] = transparent;
					itemContainer.Resources["ItemContainerSelectedInnerBorderBrush"] = transparent;
					// Also suppress the selected background fill so MAUI's VSM
					// is the only thing painting the selection appearance.
					itemContainer.Resources["ItemContainerSelectedBackground"] = transparent;
					itemContainer.Resources["ItemContainerSelectedPointerOverBackground"] = transparent;
					itemContainer.Resources["ItemContainerSelectedPressedBackground"] = transparent;
				}
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

		if (((IElementHandler)this).PlatformView is not null &&
			((IElementHandler)this).VirtualView is not null)
		{
			ItemsView.SelectionChanged += VirtualSelectionChanged;
		}
	}

	void UpdateVirtualMultipleSelection()
	{
		if (PlatformView is null || ItemsView is null)
			return;

		ItemsView.SelectionChanged -= VirtualSelectionChanged;

		var newSelection = ComputeNewMultipleSelection();
		if (!SelectionListsAreEqual(newSelection, ItemsView.SelectedItems))
		{
			ItemsView.UpdateSelectedItems(newSelection);
		}

		if (((IElementHandler)this).PlatformView is not null &&
			((IElementHandler)this).VirtualView is not null)
		{
			ItemsView.SelectionChanged += VirtualSelectionChanged;
		}
	}

	static bool SelectionListsAreEqual(IList<object> list1, IList<object> list2)
	{
		if (list1.Count != list2.Count)
			return false;

		var set2 = new HashSet<object>(list2);
		for (int i = 0; i < list1.Count; i++)
		{
			if (!set2.Contains(list1[i]))
				return false;
		}
		return true;
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
		if (((IElementHandler)this).PlatformView is null || ((IElementHandler)this).VirtualView is null)
		{
			return;
		}

		// When the WinUI ItemsView is not loaded (e.g. the page is hidden during navigation),
		// Select(index) calls silently fail because item containers are not realized.
		// Defer the selection update until the view is loaded again.
		if (!PlatformView.IsLoaded)
		{
			_selectionDirty = true;
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

				// Use index-based access to avoid issues while the collection is updating.
				for (int index = 0; index < itemList.Count; index++)
				{
					var nativeItem = itemList[index];
					if (nativeItem is ItemTemplateContext2 itemPair && ItemsView.SelectedItems.Contains(itemPair.Item))
					{
						PlatformView.Select(index);
					}
					else if (ItemsView.SelectedItems.Contains(nativeItem))
					{
						PlatformView.Select(index);
					}
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
	/// Finds the index without enumerating the projected <see cref="ICollectionView"/>. Returns -1 if the item is not found.
	/// </summary>
	static int FindItemIndexInSource(ICollectionView itemList, object targetItem)
	{
		for (int index = 0; index < itemList.Count; index++)
		{
			var nativeItem = itemList[index];
			var actualItem = nativeItem is ItemTemplateContext2 itc ? itc.Item : nativeItem;
			if (object.Equals(actualItem, targetItem))
			{
				return index;
			}
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
		return selectionMode switch
		{
			SelectionMode.Single => ItemsViewSelectionMode.Single,
			SelectionMode.Multiple => ItemsViewSelectionMode.Multiple,
			_ => ItemsViewSelectionMode.None,
		};
	}

	/// <inheritdoc />
	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		var winUISelectionMode = (ItemsViewSelectionMode)value;
		return winUISelectionMode switch
		{
			ItemsViewSelectionMode.None => SelectionMode.None,
			ItemsViewSelectionMode.Single => SelectionMode.Single,
			ItemsViewSelectionMode.Multiple => SelectionMode.Multiple,
			_ => SelectionMode.None,
		};
	}
}