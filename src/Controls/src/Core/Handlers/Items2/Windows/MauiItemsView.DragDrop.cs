using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
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
	int _insertionIndex = -1;
	bool _insertAfter;
	bool _canReorderItems;
	bool _dragDropWired;
	ItemsView? _mauiVirtualView;

	// Between-items drop indicator — circle head with "+" and a colored line on _dropIndicatorCanvas.
	Border? _dropIndicatorHead;    // filled circle with "+" at the leading edge
	Rectangle? _dropIndicatorLine; // accent-colored line extending from the circle

	// Dim overlay opacity applied to non-source containers during a drag so the
	// list visually enters "reorder mode" (same pattern as iOS drag-reorder).
	const double DragDimOpacity = 0.4;

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

		// Remove indicator visuals from the canvas so they don't linger.
		if (_dropIndicatorCanvas is not null)
		{
			if (_dropIndicatorHead is not null)
			{
				_dropIndicatorCanvas.Children.Remove(_dropIndicatorHead);
				_dropIndicatorHead = null;
			}
			if (_dropIndicatorLine is not null)
			{
				_dropIndicatorCanvas.Children.Remove(_dropIndicatorLine);
				_dropIndicatorLine = null;
			}
		}

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

			// If a live-reorder happens to recycle a container onto the dragged
			// item's new position, hide it immediately. This is what makes the
			// "empty source slot" follow the dragged item without us having to
			// chase a moving _sourceContainer reference through dispatcher races.
			if (_draggedItem is not null && IsContainerBoundToDraggedItem(itemContainer))
			{
				itemContainer.Opacity = 0;
				itemContainer.IsHitTestVisible = false;
				_sourceContainer = itemContainer;
			}
			else
			{
				// Apply dim if a drag is in progress and this is not the source.
				itemContainer.Opacity = _draggedItem is not null ? DragDimOpacity : 1;
				itemContainer.IsHitTestVisible = true;
			}
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

			// If the container being cleared is the one currently hidden as the drag
			// source (e.g. recycled mid-drag), restore its opacity and hit-testability
			// so it doesn't get reused while invisible.
			itemContainer.Opacity = 1;
			itemContainer.IsHitTestVisible = true;
			if (ReferenceEquals(_sourceContainer, itemContainer))
			{
				_sourceContainer = null;
			}

			// Reset any stale Translation so the recycled container starts clean.
			itemContainer.Translation = System.Numerics.Vector3.Zero;
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

		// Apply card appearance SYNCHRONOUSLY before this handler returns.
		// WinUI captures the drag ghost visual immediately after DragStarting fires,
		// so this runs before the snapshot is taken. This gives the ghost a solid
		// card background + shadow (matching CV1/ListViewBase native behaviour) instead
		// of being transparent (which made it look like "only the item is dragged").
		ApplyDragGhostAppearance(itemContainer);

		// Deferred: remove ghost styling, hide the source slot, dim others.
		// The ghost has already been captured at this point so removing the card
		// background here does not affect the visual that floats under the pointer.
		DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
		{
			RemoveDragGhostAppearance(itemContainer);

			if (_sourceContainer is not null)
			{
				_sourceContainer.Opacity = 0;
				// Disable hit testing so the invisible container does not intercept
				// pointer events and is excluded from FindElementsInHostCoordinates.
				_sourceContainer.IsHitTestVisible = false;
			}

			// Dim all non-source containers to signal "reorder mode" to the user.
			DimNonSourceContainers();
		});
	}

	void ItemContainer_DropCompleted(UIElement sender, UI.Xaml.DropCompletedEventArgs args)
	{
		if (sender is ItemContainer itemContainer)
		{
			itemContainer.DropCompleted -= ItemContainer_DropCompleted;
			itemContainer.Opacity = 1;
			itemContainer.IsHitTestVisible = true;
		}

		// Restore all containers — dim + source hide
		foreach (var container in FindAllContainers())
		{
			container.Opacity = 1;
			container.IsHitTestVisible = true;
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

		if (targetContainer is ItemContainer ic)
		{
			UpdateInsertionIndicator(ic, _insertAfter);
		}

		e.AcceptedOperation = WDataTransfer.DataPackageOperation.Move;
		e.Handled = true;
	}

	/// <summary>
	/// Walks every realized container and sets Opacity = 0 on whichever one is
	/// currently bound to the dragged item, Opacity = 1 on all others. Idempotent
	/// and self-healing — a stale hidden container left over from a previous
	/// reorder is restored automatically.
	/// </summary>
	void ReconcileSourceOpacity(object? draggedItem)
	{
		if (draggedItem is null)
		{
			return;
		}

		// Always walk every realized container so that any container left at
		// Opacity < 1 from a previous pass (e.g. a race between ElementPrepared
		// and a rapid live-reorder) is unconditionally restored. A fast-path that
		// returns early would silently skip this restoration and leave items
		// permanently invisible.
		ItemContainer? newSource = null;
		foreach (var c in FindAllContainers())
		{
			if (c is not ItemContainer ic)
			{
				continue;
			}

			if (IsContainerBoundToItem(ic, draggedItem))
			{
				ic.Opacity = 0;
				ic.IsHitTestVisible = false;
				newSource = ic;
			}
			else if (ic.Opacity < 1)
			{
				ic.Opacity = 1;
				ic.IsHitTestVisible = true;
			}
		}

		if (newSource is not null)
		{
			_sourceContainer = newSource;
		}
	}

	bool IsContainerBoundToDraggedItem(ItemContainer container)
	{
		return _draggedItem is not null && IsContainerBoundToItem(container, _draggedItem);
	}

	static bool IsContainerBoundToItem(ItemContainer container, object item)
	{
		if (container.Child is not ElementWrapper wrapper || wrapper.VirtualView is not View view)
		{
			return false;
		}

		var bound = view.BindingContext;
		if (bound is null)
		{
			return false;
		}

		return ReferenceEquals(bound, item) || Equals(bound, item);
	}

	/// <summary>
	/// Attempts to call <see cref="System.Collections.ObjectModel.ObservableCollection{T}.Move(int, int)"/>
	/// without reflection. We pattern-match against the closed generic types the
	/// MAUI codebase realistically encounters (object and common reference types)
	/// so the call is trim-safe — no dynamic member lookup is needed.
	///
	/// Returning <c>false</c> is harmless: the caller falls back to
	/// <c>RemoveAt</c> + <c>Insert</c>, which produces correct results but emits
	/// two NotifyCollectionChanged events instead of one.
	/// </summary>
	static bool TryMoveObservableCollection(IList list, int oldIndex, int newIndex)
	{
		switch (list)
		{
			case System.Collections.ObjectModel.ObservableCollection<object> oc:
				oc.Move(oldIndex, newIndex);
				return true;
			default:
				return false;
		}
	}

	void ScrollViewer_DragLeave(object sender, UI.Xaml.DragEventArgs e)
	{
		StopAutoScroll();
		HideInsertionIndicator();
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
					// Visual-only shuffle never mutates the collection during drag.
					// Always call PerformReorder here to commit the actual move.
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
			if (element is ItemContainer itemContainer &&
				itemContainer.Tag is int &&
				!ReferenceEquals(itemContainer, _sourceContainer))
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
				// Skip the invisible source container — its layout slot is occupied
				// but should not be a valid drop target while the drag is active.
				if (ReferenceEquals(container, _sourceContainer))
				{
					continue;
				}

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
		// Hide insertion indicator before restoring containers.
		HideInsertionIndicator();

		// Restore the source container synchronously in case DropCompleted does not
		// fire (e.g. drop handled outside the source element, or disconnect).
		if (_sourceContainer is not null)
		{
			// Remove ghost appearance defensively — the deferred block in DragStarting
			// may not have run yet if drag was cancelled immediately.
			RemoveDragGhostAppearance(_sourceContainer);
			_sourceContainer.Opacity = 1;
			_sourceContainer.IsHitTestVisible = true;
			_sourceContainer.DropCompleted -= ItemContainer_DropCompleted;
			_sourceContainer = null;
		}

		// Restore all dimmed containers.
		RestoreAllContainerOpacity();

		_draggedItem = null;
		_insertionIndex = -1;
		_insertAfter = false;
		StopAutoScroll();
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

	#region Drop Target Indicator

	/// <summary>
	/// Shows a 2 px accent-coloured line on <see cref="_dropIndicatorCanvas"/> at the
	/// boundary between items — "insert before <paramref name="target"/>" or "insert
	/// after <paramref name="target"/>".  The line is positioned in the canvas coordinate
	/// space so it is always correct regardless of scroll position.
	/// </summary>
	void UpdateInsertionIndicator(ItemContainer target, bool insertAfter)
	{
		if (_dropIndicatorCanvas is null)
			return;

		// Skip the source slot itself.
		if (ReferenceEquals(target, _sourceContainer))
		{
			HideInsertionIndicator();
			return;
		}

		var accentBrush = TryGetAccentBrush();

		// ── Lazily build the indicator visuals once ───────────────────────────
		if (_dropIndicatorHead is null || _dropIndicatorLine is null)
		{
			// Hollow circle (outline only, transparent fill) — matches Teams/Notion drag indicator style.
			_dropIndicatorHead = new Border
			{
				Width = IndicatorHeadSize,
				Height = IndicatorHeadSize,
				CornerRadius = new CornerRadius(IndicatorHeadSize / 2),
				Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
				BorderBrush = accentBrush,
				BorderThickness = new Thickness(IndicatorHeadStroke),
				IsHitTestVisible = false,
			};

			_dropIndicatorLine = new Rectangle
			{
				Fill = accentBrush,
				Height = IndicatorLineThickness,
				IsHitTestVisible = false,
			};

			_dropIndicatorCanvas.Children.Add(_dropIndicatorHead);
			_dropIndicatorCanvas.Children.Add(_dropIndicatorLine);
		}
		else
		{
			// Refresh brushes on every update so theme changes are reflected.
			_dropIndicatorHead.BorderBrush = accentBrush;
			_dropIndicatorLine.Fill = accentBrush;
		}

		// ── Calculate position in canvas coordinates ──────────────────────────
		var origin = target.TransformToVisual(_dropIndicatorCanvas)
			.TransformPoint(new Windows.Foundation.Point(0, 0));

		if (_isHorizontalLayout)
		{
			// Vertical indicator: hollow circle at top-center, line extending downward.
			double lineX = insertAfter
				? origin.X + target.ActualWidth
				: origin.X;

			double lineHeight = target.ActualHeight - IndicatorHeadSize - IndicatorHeadGap;
			if (lineHeight < 0) lineHeight = 0;

			// Head at top-center of the insertion edge.
			Canvas.SetLeft(_dropIndicatorHead, lineX - IndicatorHeadSize / 2);
			Canvas.SetTop(_dropIndicatorHead, origin.Y);

			// Line: starts below the head, centered on the insertion edge.
			_dropIndicatorLine.Width = IndicatorLineThickness;
			_dropIndicatorLine.Height = lineHeight;
			Canvas.SetLeft(_dropIndicatorLine, lineX - IndicatorLineThickness / 2);
			Canvas.SetTop(_dropIndicatorLine, origin.Y + IndicatorHeadSize + IndicatorHeadGap);
		}
		else
		{
			// Horizontal indicator: hollow circle on left, line extending to the right edge.
			double lineY = insertAfter
				? origin.Y + target.ActualHeight
				: origin.Y;

			// Circle sits at the left edge of the item; line fills the remaining width.
			double lineWidth = target.ActualWidth - IndicatorHeadSize - IndicatorHeadGap;
			if (lineWidth < 0) lineWidth = 0;

			// Head: vertically centered on the insertion line, pinned to item left edge.
			Canvas.SetLeft(_dropIndicatorHead, origin.X);
			Canvas.SetTop(_dropIndicatorHead, lineY - IndicatorHeadSize / 2);

			// Line: immediately right of circle, 2 px tall, runs to the right edge.
			_dropIndicatorLine.Width = lineWidth;
			_dropIndicatorLine.Height = IndicatorLineThickness;
			Canvas.SetLeft(_dropIndicatorLine, origin.X + IndicatorHeadSize + IndicatorHeadGap);
			Canvas.SetTop(_dropIndicatorLine, lineY - IndicatorLineThickness / 2);
		}

		// ── Make visible; only animate on first appearance to avoid flicker ─────
		// Starting a new Storyboard on every DragOver mouse-move (while already visible)
		// causes multiple animations to compete on Opacity, producing a visible flicker.
		// Only fade in when transitioning from Collapsed → Visible.
		bool wasCollapsed = _dropIndicatorHead.Visibility == Visibility.Collapsed;

		if (wasCollapsed)
		{
			_dropIndicatorHead.Opacity = 0;
			_dropIndicatorLine.Opacity = 0;
		}

		_dropIndicatorHead.Visibility = Visibility.Visible;
		_dropIndicatorLine.Visibility = Visibility.Visible;

		if (wasCollapsed)
		{
			// One-shot fade-in only on first show.
			var fadeIn = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
			{
				To = 1.0,
				Duration = new Duration(TimeSpan.FromMilliseconds(80)),
				EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase
					{ EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut },
			};
			var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
			storyboard.Children.Add(fadeIn);
			Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeIn, _dropIndicatorHead);
			Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(fadeIn, "Opacity");

			var fadeIn2 = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
			{
				To = 1.0,
				Duration = new Duration(TimeSpan.FromMilliseconds(80)),
				EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase
					{ EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut },
			};
			storyboard.Children.Add(fadeIn2);
			Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeIn2, _dropIndicatorLine);
			Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(fadeIn2, "Opacity");

			storyboard.Begin();
		}
		else
		{
			// Already visible — ensure full opacity without starting another animation.
			_dropIndicatorHead.Opacity = 1;
			_dropIndicatorLine.Opacity = 1;
		}
	}

	/// <summary>
	/// Hides the between-items drop indicator. Safe to call when no indicator is shown.
	/// </summary>
	void HideInsertionIndicator()
	{
		if (_dropIndicatorHead is not null)
			_dropIndicatorHead.Visibility = Visibility.Collapsed;
		if (_dropIndicatorLine is not null)
			_dropIndicatorLine.Visibility = Visibility.Collapsed;
	}

	// Indicator geometry constants.
	const double IndicatorHeadSize = 12.0;    // hollow circle outer diameter in px
	const double IndicatorHeadStroke = 2.0;   // circle border/stroke thickness
	const double IndicatorHeadGap = 2.0;      // gap between circle and line
	const double IndicatorLineThickness = 2.0;

	static Microsoft.UI.Xaml.Media.Brush TryGetAccentBrush()
	{
		try
		{
			var resources = Microsoft.UI.Xaml.Application.Current.Resources;

			// Prefer the WinUI 3 semantic brush — automatically adapts to the user's
			// accent color and light/dark theme (Windows 11 design language).
			if (resources.TryGetValue("AccentFillColorDefaultBrush", out var brush) &&
				brush is Microsoft.UI.Xaml.Media.SolidColorBrush accentBrush)
			{
				return accentBrush;
			}

			// Secondary fallback: raw accent color token.
			if (resources.TryGetValue("SystemAccentColor", out var raw) &&
				raw is global::Windows.UI.Color accentColor)
			{
				return new Microsoft.UI.Xaml.Media.SolidColorBrush(accentColor);
			}
		}
		catch { }

		// Final fallback: Windows 11 default accent blue.
		return new Microsoft.UI.Xaml.Media.SolidColorBrush(
			global::Windows.UI.Color.FromArgb(255, 0, 95, 184)); // #005FB8
	}

	#endregion

	#region Dim / Restore During Drag

	/// <summary>
	/// Applies a card-style background and shadow elevation to <paramref name="container"/>
	/// synchronously so that WinUI's drag-ghost snapshot (captured immediately after
	/// <c>DragStarting</c> returns) looks like a full elevated row — matching the
	/// native CV1 / ListViewBase ghost behaviour.
	/// <para>
	/// Root cause of the difference: <see cref="MauiItemsView"/> sets
	/// <c>ItemContainerBackground = Transparent</c> to suppress WinUI hover/press
	/// overlays. This makes the ghost transparent, so only the inner MAUI content
	/// was visible during drag. Temporarily restoring a solid background here fixes
	/// that without affecting the live visual states during normal interaction.
	/// </para>
	/// </summary>
	static void ApplyDragGhostAppearance(ItemContainer container)
	{
		// Resolve the theme-appropriate card/layer background.
		// "LayerFillColorDefaultBrush" is the WinUI 3 system resource for card surfaces
		// (white in Light theme, #2C2C2C in Dark theme).  Fall back to solid white.
		Microsoft.UI.Xaml.Media.Brush cardBrush;
		if (Microsoft.UI.Xaml.Application.Current.Resources
			.TryGetValue("LayerFillColorDefaultBrush", out var raw)
			&& raw is Microsoft.UI.Xaml.Media.Brush b)
		{
			cardBrush = b;
		}
		else
		{
			cardBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
				global::Windows.UI.Color.FromArgb(255, 255, 255, 255));
		}

		container.Background = cardBrush;

		// Z-elevation via ThemeShadow creates the raised-card shadow on the ghost.
		// Translation.Z > 0 is required for ThemeShadow to render in WinUI 3.
		var shadow = new Microsoft.UI.Xaml.Media.ThemeShadow();
		container.Shadow = shadow;
		container.Translation = new System.Numerics.Vector3(0, 0, 8);
	}

	/// <summary>
	/// Removes the temporary card appearance applied by
	/// <see cref="ApplyDragGhostAppearance"/> so the source slot returns to its
	/// normal (transparent-background) state before being hidden.
	/// </summary>
	static void RemoveDragGhostAppearance(ItemContainer container)
	{
		container.Background = null;
		container.Shadow = null;
		container.Translation = System.Numerics.Vector3.Zero;
	}

	/// <summary>
	/// Dims all realized containers except the source container to the
	/// <see cref="DragDimOpacity"/> level, visually signalling reorder mode.
	/// Called after the source container is hidden so it doesn't accidentally
	/// receive DragDimOpacity on top of Opacity=0.
	/// </summary>
	void DimNonSourceContainers()
	{
		if (_draggedItem is null)
		{
			return;
		}

		foreach (var container in FindAllContainers())
		{
			// Skip the source slot (already at Opacity=0).
			if (ReferenceEquals(container, _sourceContainer))
			{
				continue;
			}

			container.Opacity = DragDimOpacity;
		}
	}

	/// <summary>
	/// Restores all realized containers to full opacity. Called from
	/// <see cref="CleanupDragState"/> and <see cref="ItemContainer_DropCompleted"/>.
	/// </summary>
	void RestoreAllContainerOpacity()
	{
		foreach (var container in FindAllContainers())
		{
			container.Opacity = 1;
			container.IsHitTestVisible = true;
		}
	}

	#endregion
}
