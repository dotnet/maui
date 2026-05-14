using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WDataTransfer = Windows.ApplicationModel.DataTransfer;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// Drag-and-drop / reordering implementation for <see cref="MauiItemsView"/>.
/// Supports <see cref="ReorderableItemsView.CanReorderItems"/> and
/// <see cref="ReorderableItemsView.CanMixGroups"/> for both flat and grouped sources.
/// </summary>
internal partial class MauiItemsView
{
	// Drag and drop fields
	object? _draggedItem;
	ItemContainer? _sourceContainer;
	readonly HashSet<ItemContainer> _shiftedContainers = new();
	int _insertionIndex = -1;
	bool _insertAfter;
	bool _canReorderItems;
	bool _dragDropWired;
	ItemsView? _mauiVirtualView;

	// Auto-scroll fields
	Microsoft.UI.Dispatching.DispatcherQueueTimer? _autoScrollTimer;
	double _targetScrollVelocity;
	double _currentScrollVelocity;
	const double AutoScrollThreshold = 60.0;
	const double AutoScrollMaxSpeed = 25.0;
	const double AutoScrollMinSpeed = 3.0;
	const double ScrollAcceleration = 0.3;

	RoutedEventHandler? _deferredWireHandler;

	/// <summary>
	/// Event fired when a reorder operation completes successfully.
	/// </summary>
	public event EventHandler? ReorderCompleted;

	/// <summary>
	/// Convenience accessor that exposes the templated ItemsRepeater.
	/// </summary>
	ItemsRepeater? ItemsRepeaterControl => _itemsRepeater as ItemsRepeater;

	/// <summary>
	/// Sets a reference to the MAUI ItemsView for accessing the original ItemsSource
	/// during drag/drop reorder operations. The MAUI source is mutated directly so
	/// reorder events propagate back through the normal data binding pipeline.
	/// </summary>
	public void SetMauiVirtualView(ItemsView? itemsView)
	{
		_mauiVirtualView = itemsView;
	}

	/// <summary>
	/// Updates drag and drop capabilities for reordering items.
	/// </summary>
	public void UpdateCanReorderItems(bool canReorderItems)
	{
		_canReorderItems = canReorderItems;

		if (_scrollViewer is null)
		{
			// Template hasn't applied yet — defer wiring until Loaded.
			if (_deferredWireHandler is null)
			{
				_deferredWireHandler = OnLoadedForDragDrop;
				Loaded += _deferredWireHandler;
			}
			return;
		}

		ApplyDragDropState();
	}

	void OnLoadedForDragDrop(object sender, RoutedEventArgs e)
	{
		if (_deferredWireHandler is not null)
		{
			Loaded -= _deferredWireHandler;
			_deferredWireHandler = null;
		}

		ApplyDragDropState();
	}

	void ApplyDragDropState()
	{
		if (_canReorderItems)
		{
			WireUpDragDropEvents();
		}
		else
		{
			UnwireDragDropEvents();
		}
	}

	#region Drag and Drop Event Wiring

	void WireUpDragDropEvents()
	{
		if (_dragDropWired || _scrollViewer is null)
		{
			return;
		}

		_scrollViewer.AllowDrop = true;
		_scrollViewer.DragEnter -= ScrollViewer_DragEnter;
		_scrollViewer.DragOver -= ScrollViewer_DragOver;
		_scrollViewer.DragLeave -= ScrollViewer_DragLeave;
		_scrollViewer.Drop -= ScrollViewer_Drop;

		_scrollViewer.DragEnter += ScrollViewer_DragEnter;
		_scrollViewer.DragOver += ScrollViewer_DragOver;
		_scrollViewer.DragLeave += ScrollViewer_DragLeave;
		_scrollViewer.Drop += ScrollViewer_Drop;

		var repeater = ItemsRepeaterControl;
		if (repeater is not null)
		{
			repeater.ElementPrepared -= ItemsRepeater_ElementPrepared;
			repeater.ElementClearing -= ItemsRepeater_ElementClearing;
			repeater.ElementPrepared += ItemsRepeater_ElementPrepared;
			repeater.ElementClearing += ItemsRepeater_ElementClearing;

			// Apply drag affordance to already-realized containers. ItemsRepeater
			// virtualizes containers, so TryGetElement returns null for unrealized
			// indices — we must iterate over the full source range to avoid missing
			// realized containers that sit past an unrealized gap.
			var sourceList = GetSourceList();
			if (sourceList is not null)
			{
				for (int i = 0; i < sourceList.Count; i++)
				{
					if (repeater.TryGetElement(i) is ItemContainer ic)
					{
						ApplyDragAffordance(ic, i);
					}
				}
			}
		}

		_dragDropWired = true;
	}

	void UnwireDragDropEvents()
	{
		if (!_dragDropWired)
		{
			return;
		}

		if (_scrollViewer is not null)
		{
			_scrollViewer.AllowDrop = false;
			_scrollViewer.DragEnter -= ScrollViewer_DragEnter;
			_scrollViewer.DragOver -= ScrollViewer_DragOver;
			_scrollViewer.DragLeave -= ScrollViewer_DragLeave;
			_scrollViewer.Drop -= ScrollViewer_Drop;
		}

		var repeater = ItemsRepeaterControl;
		if (repeater is not null)
		{
			repeater.ElementPrepared -= ItemsRepeater_ElementPrepared;
			repeater.ElementClearing -= ItemsRepeater_ElementClearing;

			// Iterate the full source range; TryGetElement returns null for unrealized
			// indices so we can't stop at the first gap and assume we've cleared every
			// realized container.
			var sourceList = GetSourceList();
			if (sourceList is not null)
			{
				for (int i = 0; i < sourceList.Count; i++)
				{
					if (repeater.TryGetElement(i) is ItemContainer ic)
					{
						ic.CanDrag = false;
						ic.DragStarting -= ItemContainer_DragStarting;
					}
				}
			}
		}

		StopAutoScroll();
		_dragDropWired = false;
	}

	internal void DisconnectDragDrop()
	{
		UnwireDragDropEvents();
		if (_deferredWireHandler is not null)
		{
			Loaded -= _deferredWireHandler;
			_deferredWireHandler = null;
		}

		// Fully tear down the auto-scroll timer. StopAutoScroll only calls Stop(),
		// which leaves the Tick delegate (and therefore this instance) rooted by
		// the dispatcher queue.
		if (_autoScrollTimer is not null)
		{
			_autoScrollTimer.Stop();
			_autoScrollTimer.Tick -= AutoScrollTimer_Tick;
			_autoScrollTimer = null;
		}

		// Clear any remaining ReorderCompleted subscribers so a stray subscriber
		// can't keep this instance alive past disconnect.
		ReorderCompleted = null;

		_mauiVirtualView = null;
		CleanupDragState();
	}

	#endregion

	#region ItemsRepeater Element Management

	void ItemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
	{
		if (!_canReorderItems)
		{
			return;
		}

		if (args.Element is ItemContainer itemContainer)
		{
			ApplyDragAffordance(itemContainer, args.Index);
		}
	}

	void ApplyDragAffordance(ItemContainer itemContainer, int index)
	{
		itemContainer.Tag = index;

		// Don't allow dragging headers, footers, or group headers/footers.
		bool isHeaderOrFooter = itemContainer.Child is ElementWrapper wrapper &&
			wrapper.IsHeaderOrFooter;

		itemContainer.CanDrag = !isHeaderOrFooter;
		itemContainer.DragStarting -= ItemContainer_DragStarting;
		if (!isHeaderOrFooter)
		{
			itemContainer.DragStarting += ItemContainer_DragStarting;
		}

		// Enable an implicit Offset animation so when the data source is reordered
		// on drop, ItemsRepeater repositions the surrounding containers smoothly
		// instead of snapping. Header/footer containers benefit from the same
		// animation when neighbors shift, so we apply it unconditionally.
		ConfigureContainerReorderAnimation(itemContainer);
	}

	/// <summary>
	/// Installs implicit Composition animations on the container's Offset and
	/// Translation properties so layout changes and hover-shuffle shifts animate
	/// smoothly instead of snapping. Idempotent.
	/// </summary>
	static void ConfigureContainerReorderAnimation(ItemContainer container)
	{
		ElementCompositionPreview.SetIsTranslationEnabled(container, true);

		var visual = ElementCompositionPreview.GetElementVisual(container);
		var compositor = visual.Compositor;

		var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
		offsetAnimation.Target = "Offset";
		offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
		offsetAnimation.Duration = TimeSpan.FromMilliseconds(250);

		var translationAnimation = compositor.CreateVector3KeyFrameAnimation();
		translationAnimation.Target = "Translation";
		translationAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
		translationAnimation.Duration = TimeSpan.FromMilliseconds(200);

		var animations = compositor.CreateImplicitAnimationCollection();
		animations["Offset"] = offsetAnimation;
		animations["Translation"] = translationAnimation;
		visual.ImplicitAnimations = animations;
	}

	void ItemsRepeater_ElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
	{
		if (!_canReorderItems)
		{
			return;
		}

		if (args.Element is ItemContainer itemContainer)
		{
			itemContainer.CanDrag = false;
			itemContainer.DragStarting -= ItemContainer_DragStarting;
			itemContainer.Tag = null;

			// Detach the implicit animation when the container is recycled so the
			// Composition visual doesn't keep state from a previous item.
			var visual = ElementCompositionPreview.GetElementVisual(itemContainer);
			visual.ImplicitAnimations = null;

			// If the container being cleared is the one currently hidden as the drag
			// source (e.g. recycled mid-drag), restore its opacity so it doesn't get
			// reused while invisible.
			itemContainer.Opacity = 1;
			if (ReferenceEquals(_sourceContainer, itemContainer))
			{
				_sourceContainer = null;
			}

			// Reset any hover-shuffle translation and drop the tracking entry so a
			// recycled container doesn't carry stale offset state to its next item.
			if (_shiftedContainers.Remove(itemContainer))
			{
				ResetTranslation(itemContainer);
			}
		}
	}

	#endregion

	#region Drag Event Handlers

	void ItemContainer_DragStarting(UIElement sender, UI.Xaml.DragStartingEventArgs args)
	{
		var itemContainer = (ItemContainer)sender;

		// Use the container's currently bound item first. The Tag/index can become
		// stale after a reorder because the element is reused without being recreated.
		object? item = GetContainerItem(itemContainer);

		// Fallback: look up by index from the source (works for IList and IEnumerable).
		if (item is null && itemContainer.Tag is int index && index >= 0)
		{
			var sourceList = GetSourceList();
			if (sourceList is not null && index < sourceList.Count)
			{
				item = GetItemAtIndex(index, sourceList);
			}
		}

		if (item is null)
		{
			args.Cancel = true;
			return;
		}

		_draggedItem = item;
		_sourceContainer = itemContainer;
		args.Data.Properties.Add("DragSource", "MauiItemsView");
		args.Data.RequestedOperation = WDataTransfer.DataPackageOperation.Move;

		// Make sure the drop-completed handler is wired exactly once so the source
		// container's opacity is restored on success, cancel, or escape.
		itemContainer.DropCompleted -= ItemContainer_DropCompleted;
		itemContainer.DropCompleted += ItemContainer_DropCompleted;

		// Hide the source slot so the user perceives the item as "lifted out" of
		// the list, leaving a visible gap. Opacity (not Visibility.Collapsed) keeps
		// the element in the visual tree so WinUI captures a valid drag preview and
		// the layout slot is preserved. Deferred so the platform snapshot for the
		// drag ghost is taken from a fully opaque container.
		DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
		{
			if (_sourceContainer is not null)
			{
				_sourceContainer.Opacity = 0;
			}
		});
	}

	void ItemContainer_DropCompleted(UIElement sender, UI.Xaml.DropCompletedEventArgs args)
	{
		if (sender is ItemContainer itemContainer)
		{
			itemContainer.DropCompleted -= ItemContainer_DropCompleted;
			itemContainer.Opacity = 1;
		}

		// Defensive: if the source container was recycled during reorder, make sure
		// no realized container is left hidden.
		foreach (var container in FindAllContainers())
		{
			if (container.Opacity < 1)
			{
				container.Opacity = 1;
			}
		}

		_sourceContainer = null;
	}

	void ScrollViewer_DragEnter(object sender, UI.Xaml.DragEventArgs e)
	{
		if (!_canReorderItems)
		{
			e.AcceptedOperation = WDataTransfer.DataPackageOperation.None;
			return;
		}

		e.AcceptedOperation = WDataTransfer.DataPackageOperation.Move;
		e.DragUIOverride.IsGlyphVisible = false;
		e.DragUIOverride.IsCaptionVisible = false;
	}

	void ScrollViewer_DragOver(object sender, UI.Xaml.DragEventArgs e)
	{
		if (!_canReorderItems)
		{
			e.AcceptedOperation = WDataTransfer.DataPackageOperation.None;
			return;
		}

		e.DragUIOverride.IsGlyphVisible = false;
		e.DragUIOverride.IsCaptionVisible = false;

		HandleAutoScroll(e);

		var targetContainer = FindContainerUnderPointer(e);
		if (targetContainer is null)
		{
			return;
		}

		var pt = e.GetPosition(targetContainer);

		int targetIndex = GetContainerIndex(targetContainer);
		if (targetIndex < 0)
		{
			return;
		}

		if (_isHorizontalLayout)
		{
			_insertAfter = pt.X >= targetContainer.ActualWidth / 2;
		}
		else
		{
			_insertAfter = pt.Y >= targetContainer.ActualHeight / 2;
		}

		_insertionIndex = _insertAfter ? targetIndex + 1 : targetIndex;

		// Visually shuffle the items between the source slot and the prospective
		// drop position so the user sees where the item will land. Data is NOT
		// mutated — only Composition Translation is adjusted, and the implicit
		// Translation animation smooths the motion.
		UpdateHoverShift();

		e.AcceptedOperation = WDataTransfer.DataPackageOperation.Move;
		e.Handled = true;
	}

	void ScrollViewer_DragLeave(object sender, UI.Xaml.DragEventArgs e)
	{
		StopAutoScroll();
		ResetAllHoverShifts();
	}

	void ScrollViewer_Drop(object sender, UI.Xaml.DragEventArgs e)
	{
		if (!_canReorderItems || _draggedItem is null || _insertionIndex < 0 || _mauiVirtualView is null)
		{
			CleanupDragState();
			return;
		}

		bool isGrouped = _mauiVirtualView is GroupableItemsView giv && giv.IsGrouped;

		if (isGrouped)
		{
			if (_mauiVirtualView.ItemsSource is not IList groupsList)
			{
				CleanupDragState();
				return;
			}

			try
			{
				bool reordered = PerformGroupedReorder(groupsList);
				if (reordered)
				{
					ReorderCompleted?.Invoke(this, EventArgs.Empty);
				}
			}
			finally
			{
				CleanupDragState();
			}
		}
		else
		{
			if (_mauiVirtualView.ItemsSource is IList itemsList)
			{
				try
				{
					bool reordered = PerformReorder(itemsList);
					if (reordered)
					{
						ReorderCompleted?.Invoke(this, EventArgs.Empty);
					}
				}
				finally
				{
					CleanupDragState();
				}
			}
			else if (_mauiVirtualView.ItemsSource is IEnumerable itemsEnumerable)
			{
				// For plain IEnumerable sources (non-IList), materialize into a new
				// list, reorder it, then reassign ItemsSource so the change propagates
				// back through the normal data-binding pipeline.
				try
				{
					var materializedList = new System.Collections.Generic.List<object?>(itemsEnumerable.Cast<object?>());
					bool reordered = PerformReorder(materializedList);
					if (reordered)
					{
						_mauiVirtualView.ItemsSource = materializedList;
						ReorderCompleted?.Invoke(this, EventArgs.Empty);
					}
				}
				finally
				{
					CleanupDragState();
				}
			}
			else
			{
				CleanupDragState();
			}
		}
	}

	#endregion

	#region Reordering Logic

	bool PerformReorder(IList itemsList)
	{
		if (_draggedItem is null)
		{
			return false;
		}

		int oldIndex = IndexOfItem(_draggedItem, itemsList);
		if (oldIndex < 0)
		{
			return false;
		}

		int adjustedInsertionIndex = _insertionIndex;
		if (oldIndex < adjustedInsertionIndex)
		{
			adjustedInsertionIndex--;
		}

		if (oldIndex == adjustedInsertionIndex)
		{
			return false;
		}

		var itemToMove = itemsList[oldIndex];
		itemsList.RemoveAt(oldIndex);
		adjustedInsertionIndex = Math.Clamp(adjustedInsertionIndex, 0, itemsList.Count);
		itemsList.Insert(adjustedInsertionIndex, itemToMove);

		// Capture the moved item by reference so the async callback can find its
		// container by identity. Using the index is unsafe — another reorder or
		// collection change between now and the callback makes it stale.
		var movedItem = itemToMove is ItemTemplateContext2 itc ? itc.Item : itemToMove;
		DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
		{
			ItemsRepeaterControl?.UpdateLayout();
			UpdateAllContainerIndices();

			var container = FindContainerByItem(movedItem);
			container?.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = true });
		});

		return true;
	}

	/// <summary>
	/// Performs a reorder operation on grouped data, respecting CanMixGroups.
	/// Maps the flat insertion index to the correct group and position within the group.
	/// Groups that implement only IEnumerable (not IList) are traversed for item lookup
	/// but cannot be mutated — a reorder into or out of such a group returns false.
	/// </summary>
	bool PerformGroupedReorder(IList groupsList)
	{
		if (_draggedItem is null || _mauiVirtualView is not GroupableItemsView groupableView)
		{
			return false;
		}

		bool hasHeaders = groupableView.GroupHeaderTemplate is not null;
		bool hasFooters = groupableView.GroupFooterTemplate is not null;

		// Find which group the dragged item belongs to.
		// Groups may be IEnumerable-only (e.g., IGrouping<K,V>), so enumerate rather
		// than requiring IList for the search. IList is still required for mutation.
		int sourceGroupIndex = -1;
		int sourceItemIndex = -1;
		IList? sourceGroup = null;

		for (int g = 0; g < groupsList.Count; g++)
		{
			if (groupsList[g] is not IEnumerable groupItems)
			{
				continue;
			}

			int i = 0;
			foreach (var groupItem in groupItems)
			{
				if (ReferenceEquals(groupItem, _draggedItem) || Equals(groupItem, _draggedItem))
				{
					sourceGroupIndex = g;
					sourceItemIndex = i;
					sourceGroup = groupsList[g] as IList;
					break;
				}

				i++;
			}

			if (sourceGroupIndex >= 0)
			{
				break;
			}
		}

		// sourceGroup being null means the group is not mutable — reorder not possible.
		if (sourceGroupIndex < 0 || sourceItemIndex < 0 || sourceGroup is null)
		{
			return false;
		}

		// Map the flat _insertionIndex to a target group and position within that group.
		int targetGroupIndex = -1;
		int targetItemIndex = -1;
		IList? targetGroup = null;
		int flatPos = 0;

		for (int g = 0; g < groupsList.Count; g++)
		{
			if (groupsList[g] is not IEnumerable groupItems)
			{
				continue;
			}

			// Use ICollection.Count when available (O(1)); otherwise enumerate (O(n)).
			int groupItemCount = groupsList[g] is ICollection coll
				? coll.Count
				: groupItems.Cast<object>().Count();

			int groupStart = flatPos;

			if (hasHeaders)
			{
				flatPos++; // skip header
			}

			int itemsStart = flatPos;
			flatPos += groupItemCount;

			if (hasFooters)
			{
				flatPos++; // skip footer
			}

			if (_insertionIndex >= itemsStart && _insertionIndex <= itemsStart + groupItemCount)
			{
				targetGroupIndex = g;
				targetItemIndex = _insertionIndex - itemsStart;
				targetGroup = groupsList[g] as IList;
				break;
			}

			if (hasHeaders && _insertionIndex == groupStart)
			{
				targetGroupIndex = g;
				targetItemIndex = 0;
				targetGroup = groupsList[g] as IList;
				break;
			}
		}

		// If we didn't find a target (e.g., dragged past the end), use the last group.
		if (targetGroup is null && groupsList.Count > 0)
		{
			for (int g = groupsList.Count - 1; g >= 0; g--)
			{
				if (groupsList[g] is IEnumerable groupItems)
				{
					int groupItemCount = groupsList[g] is ICollection coll
						? coll.Count
						: groupItems.Cast<object>().Count();

					targetGroupIndex = g;
					targetItemIndex = groupItemCount;
					targetGroup = groupsList[g] as IList;
					break;
				}
			}
		}

		if (targetGroup is null || targetGroupIndex < 0)
		{
			return false;
		}

		// Honor CanMixGroups: reject cross-group moves when disabled.
		if (sourceGroupIndex != targetGroupIndex)
		{
			if (_mauiVirtualView is ReorderableItemsView riv && !riv.CanMixGroups)
			{
				return false;
			}
		}

		if (sourceGroupIndex == targetGroupIndex)
		{
			int adjustedTargetIndex = targetItemIndex;
			if (sourceItemIndex < adjustedTargetIndex)
			{
				adjustedTargetIndex--;
			}

			if (sourceItemIndex == adjustedTargetIndex)
			{
				return false;
			}

			var item = sourceGroup[sourceItemIndex];
			sourceGroup.RemoveAt(sourceItemIndex);
			adjustedTargetIndex = Math.Clamp(adjustedTargetIndex, 0, sourceGroup.Count);
			sourceGroup.Insert(adjustedTargetIndex, item);
		}
		else
		{
			var item = sourceGroup[sourceItemIndex];
			sourceGroup.RemoveAt(sourceItemIndex);
			targetItemIndex = Math.Clamp(targetItemIndex, 0, targetGroup.Count);
			targetGroup.Insert(targetItemIndex, item);
		}

		DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
		{
			ItemsRepeaterControl?.UpdateLayout();
			UpdateAllContainerIndices();
		});

		return true;
	}

	#endregion

	#region Container and Item Management

	IList? GetSourceList()
	{
		// Prefer the WinUI-side bound source so indices map to realized containers.
		if (ItemsSource is IList list)
		{
			return list;
		}

		var mauiSource = _mauiVirtualView?.ItemsSource;
		if (mauiSource is IList mauiList)
		{
			return mauiList;
		}

		// For plain IEnumerable sources (non-IList), materialize a snapshot so that
		// all index/count operations (wiring affordances, finding containers, etc.)
		// work correctly. This snapshot is read-only — mutation reassigns ItemsSource
		// directly in ScrollViewer_Drop for IEnumerable sources.
		if (mauiSource is IEnumerable enumerable)
		{
			return enumerable.Cast<object>().ToList();
		}

		return null;
	}

	FrameworkElement? FindContainerUnderPointer(UI.Xaml.DragEventArgs e)
	{
		var repeater = ItemsRepeaterControl;
		if (repeater is null)
		{
			return null;
		}

		var position = e.GetPosition(repeater);

		var elements = VisualTreeHelper.FindElementsInHostCoordinates(
			repeater.TransformToVisual(null).TransformPoint(position),
			repeater,
			false);

		foreach (var element in elements)
		{
			if (element is ItemContainer itemContainer && itemContainer.Tag is int)
			{
				return itemContainer;
			}
		}

		// Fallback: find by axis-aligned position.
		var sourceList = GetSourceList();
		if (sourceList is not null && sourceList.Count > 0)
		{
			var allContainers = FindAllContainers().ToList();

			foreach (var container in allContainers)
			{
				var containerPosition = container.TransformToVisual(repeater).TransformPoint(new global::Windows.Foundation.Point(0, 0));

				bool isInBounds;
				if (_isHorizontalLayout)
				{
					isInBounds = position.X >= containerPosition.X &&
								 position.X <= containerPosition.X + container.ActualWidth;
				}
				else
				{
					isInBounds = position.Y >= containerPosition.Y &&
								 position.Y <= containerPosition.Y + container.ActualHeight;
				}

				if (isInBounds)
				{
					return container;
				}
			}

			if (allContainers.Count > 0)
			{
				var lastContainer = allContainers[allContainers.Count - 1];
				var lastPos = lastContainer.TransformToVisual(repeater).TransformPoint(new global::Windows.Foundation.Point(0, 0));

				bool isBeyondLast = _isHorizontalLayout
					? position.X > lastPos.X + lastContainer.ActualWidth
					: position.Y > lastPos.Y + lastContainer.ActualHeight;

				if (isBeyondLast)
				{
					return lastContainer;
				}
			}
		}

		return null;
	}

	FrameworkElement? FindContainerByIndex(int index)
	{
		var sourceList = GetSourceList();
		if (sourceList is null || index < 0 || index >= sourceList.Count)
		{
			return null;
		}

		return FindAllContainers().FirstOrDefault(c => GetContainerIndex(c) == index);
	}

	/// <summary>
	/// Finds the realized container whose binding context equals <paramref name="targetItem"/>
	/// by identity. Unlike <see cref="FindContainerByIndex"/>, this is safe to call from
	/// an async callback because it does not rely on a captured index that may have been
	/// invalidated by a subsequent collection change.
	/// </summary>
	FrameworkElement? FindContainerByItem(object? targetItem)
	{
		if (targetItem is null)
		{
			return null;
		}

		// Prefer reference equality so duplicate value-equal items resolve to the
		// correct container. Fall back to value equality for value types.
		FrameworkElement? valueEqualFallback = null;
		foreach (var container in FindAllContainers())
		{
			var item = GetContainerItem(container);
			if (ReferenceEquals(item, targetItem))
			{
				return container;
			}

			if (valueEqualFallback is null && Equals(item, targetItem))
			{
				valueEqualFallback = container;
			}
		}

		return valueEqualFallback;
	}

	IEnumerable<FrameworkElement> FindAllContainers()
	{
		var repeater = ItemsRepeaterControl;
		var sourceList = GetSourceList();
		if (repeater is not null && sourceList is not null)
		{
			for (int i = 0; i < sourceList.Count; i++)
			{
				var container = repeater.TryGetElement(i);
				if (container is FrameworkElement fe)
				{
					yield return fe;
				}
			}
		}
	}

	object? GetContainerItem(FrameworkElement container)
	{
		if (container is ItemContainer itemContainer &&
			itemContainer.Child is ElementWrapper wrapper &&
			wrapper.VirtualView is View view)
		{
			return view.BindingContext;
		}

		return null;
	}

	int GetContainerIndex(FrameworkElement container)
	{
		var sourceList = GetSourceList();
		var containerItem = GetContainerItem(container);

		// Prefer the Tag set during ElementPrepared — it is the authoritative flat
		// index and avoids the ambiguity where group headers and footers share the
		// same underlying Item (the group object). Validate the tag by checking that
		// the item at that index still matches the container's current item.
		if (container.Tag is int tagIndex && sourceList is not null &&
			tagIndex >= 0 && tagIndex < sourceList.Count)
		{
			var tagItem = GetItemAtIndex(tagIndex, sourceList);
			if (containerItem is not null && Equals(tagItem, containerItem))
			{
				return tagIndex;
			}
		}

		// Tag is stale — fall back to a linear search.
		if (sourceList is not null && containerItem is not null)
		{
			var liveIndex = IndexOfItem(containerItem, sourceList);
			if (liveIndex >= 0)
			{
				return liveIndex;
			}
		}

		// Last resort: use the raw tag even if unvalidated.
		if (container.Tag is int index)
		{
			return index;
		}

		var allContainers = FindAllContainers().ToList();
		return allContainers.IndexOf(container);
	}

	int IndexOfItem(object item, IList itemsList)
	{
		// First pass: reference equality — correctly distinguishes two items that are
		// value-equal but distinct objects (e.g., duplicate records in the list).
		for (int i = 0; i < itemsList.Count; i++)
		{
			var currentItem = GetItemAtIndex(i, itemsList);
			if (ReferenceEquals(currentItem, item))
			{
				return i;
			}
		}

		// Second pass: value equality fallback for value types (structs, primitives)
		// where ReferenceEquals is always false.
		for (int i = 0; i < itemsList.Count; i++)
		{
			var currentItem = GetItemAtIndex(i, itemsList);
			if (Equals(currentItem, item))
			{
				return i;
			}
		}

		return -1;
	}

	object? GetItemAtIndex(int index, IList itemsList)
	{
		var item = itemsList[index];

		if (item is ItemTemplateContext2 itc)
		{
			return itc.Item;
		}

		return item;
	}

	void UpdateAllContainerIndices()
	{
		var sourceList = GetSourceList();
		if (sourceList is null)
		{
			return;
		}

		// Derive each container's Tag from its item's actual position in the source.
		// A positional loop (containers[i].Tag = i) is wrong when ItemsRepeater
		// virtualizes: FindAllContainers skips unrealized slots, so containers[i]
		// does not necessarily correspond to sourceList[i].
		foreach (var container in FindAllContainers())
		{
			var item = GetContainerItem(container);
			if (item is not null)
			{
				int actualIndex = IndexOfItem(item, sourceList);
				if (actualIndex >= 0)
				{
					container.Tag = actualIndex;
				}
			}
		}
	}

	#endregion

	#region Cleanup

	void CleanupDragState()
	{
		// Reset hover shifts BEFORE clearing _sourceContainer so the shifted
		// containers' Translation snaps back; the implicit Translation animation
		// will smooth this. After that, any real layout reorder (Drop case) is
		// animated via the implicit Offset animation.
		ResetAllHoverShifts();

		// Restore the source container synchronously in case DropCompleted does not
		// fire (e.g. drop handled outside the source element, or disconnect).
		if (_sourceContainer is not null)
		{
			_sourceContainer.Opacity = 1;
			_sourceContainer.DropCompleted -= ItemContainer_DropCompleted;
			_sourceContainer = null;
		}

		_draggedItem = null;
		_insertionIndex = -1;
		_insertAfter = false;
		StopAutoScroll();
	}

	#endregion

	#region Hover Shuffle (visual-only)

	/// <summary>
	/// Visually shifts the containers between the source slot and the prospective
	/// drop position to reveal where the item will land. Operates purely on the
	/// Composition Translation property — the underlying data source is not
	/// modified until the drop is committed.
	/// </summary>
	void UpdateHoverShift()
	{
		if (_draggedItem is null || _sourceContainer is null)
		{
			ResetAllHoverShifts();
			return;
		}

		var sourceList = GetSourceList();
		var repeater = ItemsRepeaterControl;
		if (sourceList is null || repeater is null)
		{
			ResetAllHoverShifts();
			return;
		}

		int sourceIndex = IndexOfItem(_draggedItem, sourceList);
		if (sourceIndex < 0)
		{
			ResetAllHoverShifts();
			return;
		}

		// Map _insertionIndex (where the item would be inserted) to the final index
		// it would occupy after a Remove+Insert. When dragging forward, the removal
		// shifts everything down by one.
		int targetIndex = _insertionIndex;
		if (sourceIndex < targetIndex)
		{
			targetIndex--;
		}
		targetIndex = Math.Clamp(targetIndex, 0, sourceList.Count - 1);

		// Measure the actual stride between two adjacent realized containers in the
		// repeater. Using just _sourceContainer.ActualHeight/Width misses any
		// inter-item spacing the layout applies, which causes neighbors to land
		// short of their target slot — appearing to overlap.
		float shiftSize = ComputeContainerStride(repeater, sourceList.Count);
		if (shiftSize <= 0)
		{
			// Fall back to the source container size if we can't measure a stride
			// (e.g. only one realized container).
			shiftSize = _isHorizontalLayout
				? (float)_sourceContainer.ActualWidth
				: (float)_sourceContainer.ActualHeight;
		}

		if (shiftSize <= 0)
		{
			return;
		}

		int rangeStart, rangeEnd;
		float direction;
		if (sourceIndex < targetIndex)
		{
			// Dragging forward: items in (source, target] slide back toward the source.
			rangeStart = sourceIndex + 1;
			rangeEnd = targetIndex;
			direction = -1f;
		}
		else if (sourceIndex > targetIndex)
		{
			// Dragging backward: items in [target, source) slide forward.
			rangeStart = targetIndex;
			rangeEnd = sourceIndex - 1;
			direction = +1f;
		}
		else
		{
			ResetAllHoverShifts();
			return;
		}

		var newShifted = new HashSet<ItemContainer>();
		for (int i = rangeStart; i <= rangeEnd; i++)
		{
			if (repeater.TryGetElement(i) is not ItemContainer ic)
			{
				continue;
			}

			if (ReferenceEquals(ic, _sourceContainer))
			{
				continue;
			}

			// Don't shift headers/footers/group headers/footers.
			if (ic.Child is ElementWrapper wrapper && wrapper.IsHeaderOrFooter)
			{
				continue;
			}

			ApplyTranslation(ic, direction * shiftSize);
			newShifted.Add(ic);
		}

		// Reset containers that were shifted previously but no longer should be.
		foreach (var c in _shiftedContainers)
		{
			if (!newShifted.Contains(c))
			{
				ResetTranslation(c);
			}
		}

		_shiftedContainers.Clear();
		foreach (var c in newShifted)
		{
			_shiftedContainers.Add(c);
		}
	}

	void ApplyTranslation(ItemContainer container, float distance)
	{
		ElementCompositionPreview.SetIsTranslationEnabled(container, true);
		var visual = ElementCompositionPreview.GetElementVisual(container);
		var offset = _isHorizontalLayout
			? new System.Numerics.Vector3(distance, 0f, 0f)
			: new System.Numerics.Vector3(0f, distance, 0f);
		visual.Properties.InsertVector3("Translation", offset);
	}

	static void ResetTranslation(ItemContainer container)
	{
		var visual = ElementCompositionPreview.GetElementVisual(container);
		visual.Properties.InsertVector3("Translation", System.Numerics.Vector3.Zero);
	}

	void ResetAllHoverShifts()
	{
		if (_shiftedContainers.Count == 0)
		{
			return;
		}

		foreach (var c in _shiftedContainers)
		{
			ResetTranslation(c);
		}
		_shiftedContainers.Clear();
	}

	/// <summary>
	/// Returns the on-axis distance between the layout positions of two adjacent
	/// realized containers, including any inter-item spacing applied by the layout.
	/// Returns 0 if fewer than two adjacent realized containers can be found.
	/// </summary>
	float ComputeContainerStride(ItemsRepeater repeater, int itemCount)
	{
		FrameworkElement? prev = null;
		int prevIndex = -1;

		for (int i = 0; i < itemCount; i++)
		{
			if (repeater.TryGetElement(i) is not FrameworkElement cur)
			{
				continue;
			}

			if (prev is not null && i == prevIndex + 1)
			{
				var p1 = prev.TransformToVisual(repeater)
					.TransformPoint(new global::Windows.Foundation.Point(0, 0));
				var p2 = cur.TransformToVisual(repeater)
					.TransformPoint(new global::Windows.Foundation.Point(0, 0));

				float stride = _isHorizontalLayout
					? (float)(p2.X - p1.X)
					: (float)(p2.Y - p1.Y);

				if (stride > 0)
				{
					return stride;
				}
			}

			prev = cur;
			prevIndex = i;
		}

		return 0f;
	}

	#endregion

	#region Auto-Scroll During Drag

	void HandleAutoScroll(UI.Xaml.DragEventArgs e)
	{
		if (_scrollViewer is null)
		{
			return;
		}

		var position = e.GetPosition(_scrollViewer);

		if (_isHorizontalLayout)
		{
			HandleHorizontalAutoScroll(position);
		}
		else
		{
			HandleVerticalAutoScroll(position);
		}
	}

	void HandleVerticalAutoScroll(global::Windows.Foundation.Point position)
	{
		if (_scrollViewer is null)
		{
			return;
		}

		var height = _scrollViewer.ActualHeight;
		var distanceFromTop = position.Y;
		var distanceFromBottom = height - position.Y;

		if (distanceFromTop < AutoScrollThreshold && _scrollViewer.VerticalOffset > 0)
		{
			var normalizedDistance = 1.0 - (distanceFromTop / AutoScrollThreshold);
			_targetScrollVelocity = -(AutoScrollMinSpeed + (normalizedDistance * (AutoScrollMaxSpeed - AutoScrollMinSpeed)));
			StartAutoScroll();
		}
		else if (distanceFromBottom < AutoScrollThreshold &&
				 _scrollViewer.VerticalOffset < _scrollViewer.ScrollableHeight)
		{
			var normalizedDistance = 1.0 - (distanceFromBottom / AutoScrollThreshold);
			_targetScrollVelocity = AutoScrollMinSpeed + (normalizedDistance * (AutoScrollMaxSpeed - AutoScrollMinSpeed));
			StartAutoScroll();
		}
		else
		{
			_targetScrollVelocity = 0;
			if (Math.Abs(_currentScrollVelocity) < 0.1)
			{
				StopAutoScroll();
			}
		}
	}

	void HandleHorizontalAutoScroll(global::Windows.Foundation.Point position)
	{
		if (_scrollViewer is null)
		{
			return;
		}

		var width = _scrollViewer.ActualWidth;
		var distanceFromLeft = position.X;
		var distanceFromRight = width - position.X;

		if (distanceFromLeft < AutoScrollThreshold && _scrollViewer.HorizontalOffset > 0)
		{
			var normalizedDistance = 1.0 - (distanceFromLeft / AutoScrollThreshold);
			_targetScrollVelocity = -(AutoScrollMinSpeed + (normalizedDistance * (AutoScrollMaxSpeed - AutoScrollMinSpeed)));
			StartAutoScroll();
		}
		else if (distanceFromRight < AutoScrollThreshold &&
				 _scrollViewer.HorizontalOffset < _scrollViewer.ScrollableWidth)
		{
			var normalizedDistance = 1.0 - (distanceFromRight / AutoScrollThreshold);
			_targetScrollVelocity = AutoScrollMinSpeed + (normalizedDistance * (AutoScrollMaxSpeed - AutoScrollMinSpeed));
			StartAutoScroll();
		}
		else
		{
			_targetScrollVelocity = 0;
			if (Math.Abs(_currentScrollVelocity) < 0.1)
			{
				StopAutoScroll();
			}
		}
	}

	void StartAutoScroll()
	{
		if (_autoScrollTimer is null)
		{
			_autoScrollTimer = DispatcherQueue.CreateTimer();
			_autoScrollTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
			_autoScrollTimer.Tick += AutoScrollTimer_Tick;
			_autoScrollTimer.Start();
		}
		else if (!_autoScrollTimer.IsRunning)
		{
			_autoScrollTimer.Start();
		}
	}

	void StopAutoScroll()
	{
		_autoScrollTimer?.Stop();
		_targetScrollVelocity = 0;
		_currentScrollVelocity = 0;
	}

	void AutoScrollTimer_Tick(object? sender, object e)
	{
		// Cache to a local so that a concurrent DisconnectHandler nulling _scrollViewer
		// cannot produce a NullReferenceException between the null-check and ChangeView.
		var scrollViewer = _scrollViewer;
		if (scrollViewer is null)
		{
			StopAutoScroll();
			return;
		}

		_currentScrollVelocity = Lerp(_currentScrollVelocity, _targetScrollVelocity, ScrollAcceleration);

		if (Math.Abs(_currentScrollVelocity) < 0.01)
		{
			if (_targetScrollVelocity == 0)
			{
				StopAutoScroll();
			}
			return;
		}

		double newOffset;
		if (_isHorizontalLayout)
		{
			newOffset = scrollViewer.HorizontalOffset + _currentScrollVelocity;
			newOffset = Math.Clamp(newOffset, 0, scrollViewer.ScrollableWidth);
			scrollViewer.ChangeView(newOffset, scrollViewer.VerticalOffset, null, disableAnimation: true);
		}
		else
		{
			newOffset = scrollViewer.VerticalOffset + _currentScrollVelocity;
			newOffset = Math.Clamp(newOffset, 0, scrollViewer.ScrollableHeight);
			scrollViewer.ChangeView(scrollViewer.HorizontalOffset, newOffset, null, disableAnimation: true);
		}
	}

	static double Lerp(double start, double end, double amount)
	{
		return start + (end - start) * amount;
	}

	#endregion
}
