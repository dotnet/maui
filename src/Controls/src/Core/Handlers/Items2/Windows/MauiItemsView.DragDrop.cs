using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
		bool isHeaderOrFooter = itemContainer.Child is ElementWrapper wrapper && wrapper.IsHeaderOrFooter;

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
		}
	}

	#endregion

	#region Drag Event Handlers

	void ItemContainer_DragStarting(UIElement sender, UI.Xaml.DragStartingEventArgs args)
	{
		var itemContainer = (ItemContainer)sender;

		// Use the container's currently bound item first. The Tag/index can become
		// stale after a reorder because the element is reused without being recreated.
		var sourceList = GetSourceList();
		object? item = GetContainerItem(itemContainer);
		if (item is null && itemContainer.Tag is int index && sourceList is not null && index >= 0 && index < sourceList.Count)
		{
			item = GetItemAtIndex(index, sourceList);
		}

		if (item is null || sourceList is null)
		{
			args.Cancel = true;
			return;
		}

		_draggedItem = item;
		args.Data.Properties.Add("DragSource", "MauiItemsView");
		args.Data.RequestedOperation = WDataTransfer.DataPackageOperation.Move;
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

		e.AcceptedOperation = WDataTransfer.DataPackageOperation.Move;
		e.Handled = true;
	}

	void ScrollViewer_DragLeave(object sender, UI.Xaml.DragEventArgs e)
	{
		StopAutoScroll();
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
			if (_mauiVirtualView.ItemsSource is not IList itemsList)
			{
				CleanupDragState();
				return;
			}

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

		var finalIndex = adjustedInsertionIndex;
		DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
		{
			ItemsRepeaterControl?.UpdateLayout();
			UpdateAllContainerIndices();

			var container = FindContainerByIndex(finalIndex);
			container?.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = true });
		});

		return true;
	}

	/// <summary>
	/// Performs a reorder operation on grouped data, respecting CanMixGroups.
	/// Maps the flat insertion index to the correct group and position within the group.
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
		int sourceGroupIndex = -1;
		int sourceItemIndex = -1;
		IList? sourceGroup = null;

		for (int g = 0; g < groupsList.Count; g++)
		{
			if (groupsList[g] is IList group)
			{
				for (int i = 0; i < group.Count; i++)
				{
					if (Equals(group[i], _draggedItem))
					{
						sourceGroupIndex = g;
						sourceItemIndex = i;
						sourceGroup = group;
						break;
					}
				}
				if (sourceGroup is not null)
				{
					break;
				}
			}
		}

		if (sourceGroup is null || sourceGroupIndex < 0 || sourceItemIndex < 0)
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
			if (groupsList[g] is IList group)
			{
				int groupStart = flatPos;
				if (hasHeaders)
				{
					flatPos++; // skip header
				}

				int itemsStart = flatPos;
				flatPos += group.Count;

				if (hasFooters)
				{
					flatPos++; // skip footer
				}

				if (_insertionIndex >= itemsStart && _insertionIndex <= itemsStart + group.Count)
				{
					targetGroupIndex = g;
					targetItemIndex = _insertionIndex - itemsStart;
					targetGroup = group;
					break;
				}

				if (hasHeaders && _insertionIndex == groupStart)
				{
					targetGroupIndex = g;
					targetItemIndex = 0;
					targetGroup = group;
					break;
				}
			}
		}

		// If we didn't find a target (e.g., dragged past the end), use the last group.
		if (targetGroup is null && groupsList.Count > 0)
		{
			for (int g = groupsList.Count - 1; g >= 0; g--)
			{
				if (groupsList[g] is IList group)
				{
					targetGroupIndex = g;
					targetItemIndex = group.Count;
					targetGroup = group;
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
		return _mauiVirtualView?.ItemsSource as IList;
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

		var containers = FindAllContainers().ToList();

		for (int i = 0; i < containers.Count && i < sourceList.Count; i++)
		{
			containers[i].Tag = i;
		}
	}

	#endregion

	#region Cleanup

	void CleanupDragState()
	{
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
		if (_scrollViewer is null)
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
			newOffset = _scrollViewer.HorizontalOffset + _currentScrollVelocity;
			newOffset = Math.Clamp(newOffset, 0, _scrollViewer.ScrollableWidth);
			_scrollViewer.ChangeView(newOffset, _scrollViewer.VerticalOffset, null, disableAnimation: true);
		}
		else
		{
			newOffset = _scrollViewer.VerticalOffset + _currentScrollVelocity;
			newOffset = Math.Clamp(newOffset, 0, _scrollViewer.ScrollableHeight);
			_scrollViewer.ChangeView(_scrollViewer.HorizontalOffset, newOffset, null, disableAnimation: true);
		}
	}

	static double Lerp(double start, double end, double amount)
	{
		return start + (end - start) * amount;
	}

	#endregion
}
