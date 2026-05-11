using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WDataTransfer = Windows.ApplicationModel.DataTransfer;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// Partial of <see cref="MauiItemsView"/> that implements drag-and-drop reordering
/// for <see cref="ReorderableItemsView"/>. Items are drag sources; the host
/// <see cref="ScrollViewer"/> is the drop target. Edge-proximity auto-scroll keeps
/// dragging usable when the list extends past the viewport.
/// </summary>
internal partial class MauiItemsView
{
	// Drag state
	object? _draggedItem;
	int _insertionIndex = -1;
	bool _insertAfter;
	// Default to true so the drag/drop pipeline is armed by the time OnApplyTemplate
	// wires events; otherwise the initial batch of ItemContainers is realized before
	// CanReorderItems mapping runs and never receives CanDrag = true.
	bool _canReorderItems = true;

	// Reference to the MAUI virtual view so we can consult IsGrouped / CanMixGroups
	// and mutate the original items source (rather than the wrapped WinUI flat list).
	ReorderableItemsView? _reorderableView;

	// Auto-scroll state
	Microsoft.UI.Dispatching.DispatcherQueueTimer? _autoScrollTimer;
	double _targetScrollVelocity;
	double _currentScrollVelocity;
	const double AutoScrollThreshold = 60.0;
	const double AutoScrollMaxSpeed = 25.0;
	const double AutoScrollMinSpeed = 3.0;
	const double ScrollAcceleration = 0.3;

	/// <summary>
	/// Raised after a drag-drop reorder successfully mutates the items source.
	/// </summary>
	public event EventHandler? ReorderCompleted;

	/// <summary>
	/// Attaches (or detaches with <c>null</c>) the MAUI <see cref="ReorderableItemsView"/> that owns
	/// this platform view. Required so the drag-drop pipeline can read <see cref="GroupableItemsView.IsGrouped"/>
	/// / <see cref="ReorderableItemsView.CanMixGroups"/> and mutate the original items source.
	/// </summary>
	internal void AttachReorderableView(ReorderableItemsView? view)
	{
		_reorderableView = view;
		UpdateCanReorderItems(view?.CanReorderItems ?? false);
	}

	/// <summary>
	/// Enables or disables drag-drop reordering. Safe to call before <see cref="OnApplyTemplate"/>;
	/// wiring is deferred until the template parts (<c>_itemsRepeater</c>/<c>_scrollViewer</c>) exist.
	/// </summary>
	public void UpdateCanReorderItems(bool canReorderItems)
	{
		_canReorderItems = canReorderItems;

		if (_itemsRepeater is null)
		{
			// Template not applied yet — OnApplyTemplate will wire up if enabled.
			return;
		}

		if (canReorderItems)
		{
			WireUpDragDropEvents();
		}
		else
		{
			UnwireDragDropEvents();
		}
	}

	#region Event Wiring

	// Drop events are wired on _itemsRepeater (not _scrollViewer) because the legacy
	// WinUI ScrollViewer's pointer/manipulation pipeline swallows drag events, which
	// prevents DragOver/Drop from reaching us reliably. The ItemsRepeater sits inside
	// the ScrollViewer and forwards drag events normally. Auto-scroll still drives the
	// outer ScrollViewer via ChangeView(...).
	void WireUpDragDropEvents()
	{
		if (_itemsRepeater is null)
			return;

		_itemsRepeater.AllowDrop = true;

		_itemsRepeater.DragEnter -= ScrollView_DragEnter;
		_itemsRepeater.DragOver -= ScrollView_DragOver;
		_itemsRepeater.DragLeave -= ScrollView_DragLeave;
		_itemsRepeater.Drop -= ScrollView_Drop;

		_itemsRepeater.DragEnter += ScrollView_DragEnter;
		_itemsRepeater.DragOver += ScrollView_DragOver;
		_itemsRepeater.DragLeave += ScrollView_DragLeave;
		_itemsRepeater.Drop += ScrollView_Drop;

		_itemsRepeater.ElementPrepared -= ItemsRepeater_ElementPrepared;
		_itemsRepeater.ElementClearing -= ItemsRepeater_ElementClearing;
		_itemsRepeater.ElementPrepared += ItemsRepeater_ElementPrepared;
		_itemsRepeater.ElementClearing += ItemsRepeater_ElementClearing;
	}

	void UnwireDragDropEvents()
	{
		if (_itemsRepeater is not null)
		{
			_itemsRepeater.AllowDrop = false;
			_itemsRepeater.DragEnter -= ScrollView_DragEnter;
			_itemsRepeater.DragOver -= ScrollView_DragOver;
			_itemsRepeater.DragLeave -= ScrollView_DragLeave;
			_itemsRepeater.Drop -= ScrollView_Drop;

			_itemsRepeater.ElementPrepared -= ItemsRepeater_ElementPrepared;
			_itemsRepeater.ElementClearing -= ItemsRepeater_ElementClearing;
		}

		StopAutoScroll();
	}

	#endregion

	#region ItemsRepeater Element Lifecycle

	void ItemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
	{
		if (!_canReorderItems)
			return;

		if (args.Element is not ItemContainer itemContainer)
			return;

		itemContainer.Tag = args.Index;

		// Group headers and footers are not draggable. They appear inline in the flat
		// WinUI items source as ItemTemplateContext2 instances flagged IsHeader/IsFooter.
		bool isHeaderOrFooter = ItemsSource is IList flatList
			&& args.Index >= 0 && args.Index < flatList.Count
			&& flatList[args.Index] is ItemTemplateContext2 itc
			&& (itc.IsHeader || itc.IsFooter);

		itemContainer.CanDrag = !isHeaderOrFooter;
		itemContainer.DragStarting -= ItemContainer_DragStarting;
		if (!isHeaderOrFooter)
		{
			itemContainer.DragStarting += ItemContainer_DragStarting;
		}
	}

	void ItemsRepeater_ElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
	{
		if (args.Element is ItemContainer itemContainer)
		{
			itemContainer.CanDrag = false;
			itemContainer.DragStarting -= ItemContainer_DragStarting;
			itemContainer.Tag = null;
		}
	}

	#endregion

	#region Drag Source / Drop Target Handlers

	void ItemContainer_DragStarting(UIElement sender, UI.Xaml.DragStartingEventArgs args)
	{
		var itemContainer = (ItemContainer)sender;

		object? item = null;
		if (itemContainer.Tag is int index && index >= 0 && ItemsSource is IList itemsList && index < itemsList.Count)
		{
			item = GetItemAtIndex(index, itemsList);
		}

		if (item is null || ItemsSource is null)
		{
			args.Cancel = true;
			return;
		}

		_draggedItem = item;
		args.Data.Properties.Add("DragSource", "MauiItemsView");
		args.Data.RequestedOperation = WDataTransfer.DataPackageOperation.Move;
	}

	void ScrollView_DragEnter(object sender, UI.Xaml.DragEventArgs e)
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

	void ScrollView_DragOver(object sender, UI.Xaml.DragEventArgs e)
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
			return;

		int targetIndex = GetContainerIndex(targetContainer);
		if (targetIndex < 0)
			return;

		var pt = e.GetPosition(targetContainer);

		// Insert before or after the target based on which half of the container the pointer is in.
		_insertAfter = _isHorizontalLayout
			? pt.X >= targetContainer.ActualWidth / 2
			: pt.Y >= targetContainer.ActualHeight / 2;

		_insertionIndex = _insertAfter ? targetIndex + 1 : targetIndex;

		e.AcceptedOperation = WDataTransfer.DataPackageOperation.Move;
		e.Handled = true;
	}

	void ScrollView_DragLeave(object sender, UI.Xaml.DragEventArgs e)
	{
		StopAutoScroll();
	}

	void ScrollView_Drop(object sender, UI.Xaml.DragEventArgs e)
	{
		if (!_canReorderItems || _draggedItem is null || _insertionIndex < 0)
		{
			CleanupDragState();
			return;
		}

		try
		{
			bool reordered;

			// Grouped reorder: walk the original MAUI items source (List<IList>) and
			// route the move to the right group, honoring CanMixGroups.
			if (_reorderableView is GroupableItemsView { IsGrouped: true } &&
				_reorderableView.ItemsSource is IList groupsList)
			{
				reordered = PerformGroupedReorder(groupsList);
			}
			else if (ItemsSource is IList itemsList)
			{
				reordered = PerformReorder(itemsList);
			}
			else
			{
				reordered = false;
			}

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

	#endregion

	#region Reorder Logic

	bool PerformReorder(IList itemsList)
	{
		if (_draggedItem is null)
			return false;

		int oldIndex = IndexOfItem(_draggedItem, itemsList);
		if (oldIndex < 0)
			return false;

		int newIndex = _insertionIndex;

		// Removing the dragged item shifts subsequent indices left by one.
		if (oldIndex < newIndex)
			newIndex--;

		if (oldIndex == newIndex)
			return false;

		// Move the wrapper (ItemTemplateContext2) — not the unwrapped item — so the
		// underlying CollectionView item-template binding stays intact.
		var wrapperToMove = itemsList[oldIndex];
		itemsList.RemoveAt(oldIndex);
		newIndex = Math.Clamp(newIndex, 0, itemsList.Count);
		itemsList.Insert(newIndex, wrapperToMove);

		var finalIndex = newIndex;
		DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
		{
			_itemsRepeater?.UpdateLayout();
			UpdateAllContainerIndices();
			FindContainerByIndex(finalIndex)?.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = true });
		});

		return true;
	}

	/// <summary>
	/// Performs a reorder on a grouped items source. Maps the flat insertion index
	/// (computed against the WinUI items repeater) back to a (group, item) coordinate
	/// in the original <c>List&lt;IList&gt;</c>, honoring <see cref="ReorderableItemsView.CanMixGroups"/>.
	/// </summary>
	bool PerformGroupedReorder(IList groupsList)
	{
		if (_draggedItem is null || _reorderableView is not GroupableItemsView groupableView)
		{
			return false;
		}

		bool hasHeaders = groupableView.GroupHeaderTemplate is not null;
		bool hasFooters = groupableView.GroupFooterTemplate is not null;

		// Locate the dragged item within its group.
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
					break;
			}
		}

		if (sourceGroup is null || sourceGroupIndex < 0 || sourceItemIndex < 0)
			return false;

		// Map the flat insertion index to a (target group, target position-within-group)
		// by walking groups and accounting for header/footer rows.
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
					flatPos++;

				int itemsStart = flatPos;
				flatPos += group.Count;

				if (hasFooters)
					flatPos++;

				// Insertion within (or at the end of) this group's items range.
				if (_insertionIndex >= itemsStart && _insertionIndex <= itemsStart + group.Count)
				{
					targetGroupIndex = g;
					targetItemIndex = _insertionIndex - itemsStart;
					targetGroup = group;
					break;
				}

				// Insertion lands on this group's header — treat as "top of this group".
				if (hasHeaders && _insertionIndex == groupStart)
				{
					targetGroupIndex = g;
					targetItemIndex = 0;
					targetGroup = group;
					break;
				}
			}
		}

		// Dragged past the end — append to the last group.
		if (targetGroup is null)
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
			return false;

		// Reject cross-group moves when CanMixGroups is false.
		if (sourceGroupIndex != targetGroupIndex &&
			_reorderableView is ReorderableItemsView { CanMixGroups: false })
		{
			return false;
		}

		if (sourceGroupIndex == targetGroupIndex)
		{
			// In-group move: account for the removal shifting the target index left.
			int adjustedTargetIndex = targetItemIndex;
			if (sourceItemIndex < adjustedTargetIndex)
				adjustedTargetIndex--;

			if (sourceItemIndex == adjustedTargetIndex)
				return false;

			var item = sourceGroup[sourceItemIndex];
			sourceGroup.RemoveAt(sourceItemIndex);
			adjustedTargetIndex = Math.Clamp(adjustedTargetIndex, 0, sourceGroup.Count);
			sourceGroup.Insert(adjustedTargetIndex, item);
		}
		else
		{
			// Cross-group move (CanMixGroups == true).
			var item = sourceGroup[sourceItemIndex];
			sourceGroup.RemoveAt(sourceItemIndex);
			targetItemIndex = Math.Clamp(targetItemIndex, 0, targetGroup.Count);
			targetGroup.Insert(targetItemIndex, item);
		}

		DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
		{
			_itemsRepeater?.UpdateLayout();
			UpdateAllContainerIndices();
		});

		return true;
	}

	#endregion

	#region Container / Item Lookup

	FrameworkElement? FindContainerUnderPointer(UI.Xaml.DragEventArgs e)
	{
		if (_itemsRepeater is null)
			return null;

		var position = e.GetPosition(_itemsRepeater);

		var elements = VisualTreeHelper.FindElementsInHostCoordinates(
			_itemsRepeater.TransformToVisual(null).TransformPoint(position),
			_itemsRepeater,
			false);

		foreach (var element in elements)
		{
			if (element is ItemContainer itemContainer && itemContainer.Tag is int)
				return itemContainer;
		}

		// Fallback: hit-test by transformed bounds (handles edge cases where hit-testing misses, e.g. spacing).
		if (ItemsSource is IList itemsList && itemsList.Count > 0)
		{
			var allContainers = FindAllContainers().ToList();

			foreach (var container in allContainers)
			{
				try
				{
					var origin = container.TransformToVisual(_itemsRepeater).TransformPoint(new global::Windows.Foundation.Point(0, 0));

					bool isInBounds = _isHorizontalLayout
						? position.X >= origin.X && position.X <= origin.X + container.ActualWidth
						: position.Y >= origin.Y && position.Y <= origin.Y + container.ActualHeight;

					if (isInBounds)
						return container;
				}
				catch
				{
					// TransformToVisual can throw if the container has been recycled mid-drag; skip it.
				}
			}

			// Pointer past the last container — drop at end.
			if (allContainers.Count > 0)
			{
				var last = allContainers.Last();
				var lastOrigin = last.TransformToVisual(_itemsRepeater).TransformPoint(new global::Windows.Foundation.Point(0, 0));

				bool beyondLast = _isHorizontalLayout
					? position.X > lastOrigin.X + last.ActualWidth
					: position.Y > lastOrigin.Y + last.ActualHeight;

				if (beyondLast)
					return last;
			}
		}

		return null;
	}

	FrameworkElement? FindContainerByIndex(int index)
	{
		if (ItemsSource is not IList itemsList || index < 0 || index >= itemsList.Count)
			return null;

		return FindAllContainers().FirstOrDefault(c => GetContainerIndex(c) == index);
	}

	IEnumerable<FrameworkElement> FindAllContainers()
	{
		if (_itemsRepeater is null || ItemsSource is not IList itemsList)
			yield break;

		for (int i = 0; i < itemsList.Count; i++)
		{
			if (_itemsRepeater.TryGetElement(i) is FrameworkElement fe)
				yield return fe;
		}
	}

	int GetContainerIndex(FrameworkElement container)
	{
		if (container.Tag is int index)
			return index;

		return FindAllContainers().ToList().IndexOf(container);
	}

	int IndexOfItem(object item, IList itemsList)
	{
		for (int i = 0; i < itemsList.Count; i++)
		{
			if (Equals(GetItemAtIndex(i, itemsList), item))
				return i;
		}
		return -1;
	}

	static object? GetItemAtIndex(int index, IList itemsList)
	{
		var item = itemsList[index];
		return item is ItemTemplateContext2 itc ? itc.Item : item;
	}

	void UpdateAllContainerIndices()
	{
		if (ItemsSource is not IList itemsList)
			return;

		var containers = FindAllContainers().ToList();
		for (int i = 0; i < containers.Count && i < itemsList.Count; i++)
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
			return;

		var position = e.GetPosition(_scrollViewer);

		if (_isHorizontalLayout)
			HandleHorizontalAutoScroll(position);
		else
			HandleVerticalAutoScroll(position);
	}

	void HandleVerticalAutoScroll(global::Windows.Foundation.Point position)
	{
		if (_scrollViewer is null)
			return;

		var height = _scrollViewer.ActualHeight;
		var distanceFromTop = position.Y;
		var distanceFromBottom = height - position.Y;

		if (distanceFromTop < AutoScrollThreshold && _scrollViewer.VerticalOffset > 0)
		{
			var normalized = 1.0 - (distanceFromTop / AutoScrollThreshold);
			_targetScrollVelocity = -(AutoScrollMinSpeed + (normalized * (AutoScrollMaxSpeed - AutoScrollMinSpeed)));
			StartAutoScroll();
		}
		else if (distanceFromBottom < AutoScrollThreshold && _scrollViewer.VerticalOffset < _scrollViewer.ScrollableHeight)
		{
			var normalized = 1.0 - (distanceFromBottom / AutoScrollThreshold);
			_targetScrollVelocity = AutoScrollMinSpeed + (normalized * (AutoScrollMaxSpeed - AutoScrollMinSpeed));
			StartAutoScroll();
		}
		else
		{
			_targetScrollVelocity = 0;
			if (Math.Abs(_currentScrollVelocity) < 0.1)
				StopAutoScroll();
		}
	}

	void HandleHorizontalAutoScroll(global::Windows.Foundation.Point position)
	{
		if (_scrollViewer is null)
			return;

		var width = _scrollViewer.ActualWidth;
		var distanceFromLeft = position.X;
		var distanceFromRight = width - position.X;

		if (distanceFromLeft < AutoScrollThreshold && _scrollViewer.HorizontalOffset > 0)
		{
			var normalized = 1.0 - (distanceFromLeft / AutoScrollThreshold);
			_targetScrollVelocity = -(AutoScrollMinSpeed + (normalized * (AutoScrollMaxSpeed - AutoScrollMinSpeed)));
			StartAutoScroll();
		}
		else if (distanceFromRight < AutoScrollThreshold && _scrollViewer.HorizontalOffset < _scrollViewer.ScrollableWidth)
		{
			var normalized = 1.0 - (distanceFromRight / AutoScrollThreshold);
			_targetScrollVelocity = AutoScrollMinSpeed + (normalized * (AutoScrollMaxSpeed - AutoScrollMinSpeed));
			StartAutoScroll();
		}
		else
		{
			_targetScrollVelocity = 0;
			if (Math.Abs(_currentScrollVelocity) < 0.1)
				StopAutoScroll();
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

		// Linear interpolation toward the target velocity for smooth accel/decel.
		_currentScrollVelocity = Lerp(_currentScrollVelocity, _targetScrollVelocity, ScrollAcceleration);

		if (Math.Abs(_currentScrollVelocity) < 0.01)
		{
			if (_targetScrollVelocity == 0)
				StopAutoScroll();
			return;
		}

		try
		{
			if (_isHorizontalLayout)
			{
				var newOffset = Math.Clamp(_scrollViewer.HorizontalOffset + _currentScrollVelocity, 0, _scrollViewer.ScrollableWidth);
				_scrollViewer.ChangeView(newOffset, null, null, disableAnimation: true);
			}
			else
			{
				var newOffset = Math.Clamp(_scrollViewer.VerticalOffset + _currentScrollVelocity, 0, _scrollViewer.ScrollableHeight);
				_scrollViewer.ChangeView(null, newOffset, null, disableAnimation: true);
			}
		}
		catch
		{
			StopAutoScroll();
		}
	}

	static double Lerp(double start, double end, double amount) => start + (end - start) * amount;

	#endregion
}
