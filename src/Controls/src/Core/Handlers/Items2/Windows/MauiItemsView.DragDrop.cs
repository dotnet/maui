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
using WApp = Microsoft.UI.Xaml.Application;
using WBorder = Microsoft.UI.Xaml.Controls.Border;
using WDataTransfer = Windows.ApplicationModel.DataTransfer;
using WVisibility = Microsoft.UI.Xaml.Visibility;

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
	// Flat source index of the dragged container, captured at DragStarting.
	// Used by PerformReorder to disambiguate value-equal duplicates (record structs,
	// boxed primitives, repeated entries) where IndexOfItem would return the first match.
	int _draggedSourceIndex = -1;
	ItemContainer? _sourceContainer;
	int _insertionIndex = -1;
	bool _insertAfter;
	bool _canReorderItems;
	bool _dragDropWired;
	ItemsView? _mauiVirtualView;

	/// <summary>
	/// Set by <see cref="ItemsViewHandler2{TItemsView}.UpdateItemsSource"/> when a flat (non-grouped)
	/// <see cref="ObservableItemTemplateCollection2"/> is active. Allows <see cref="PerformReorder"/>
	/// to call <see cref="ObservableItemTemplateCollection2.MoveItemAndSyncSource"/> — which atomically
	/// moves the item in both the source and the template collection without reflection — instead of
	/// falling back to plain <c>RemoveAt</c> + <c>Insert</c> on the raw source.
	/// </summary>
	internal ObservableItemTemplateCollection2? FlatTemplateCollection { get; set; }

	/// <summary>
	/// True while a drag/drop reorder mutation is in progress. Used by
	/// <see cref="ItemsViewHandler2{TItemsView}"/> to skip <c>ApplyItemsUpdatingScrollMode</c>
	/// during the collection change that results from the reorder, so the scroll position
	/// is not reset to the first or last item by the items-updating scroll mode logic.
	/// </summary>
	internal bool IsReordering { get; private set; }

	// Between-items drop indicator — circle head with "+" and a colored line on _dropIndicatorCanvas.
	WBorder? _dropIndicatorHead;    // hollow circle at the leading edge
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

	// Cached insertion-indicator fade-in animation (created once in OnApplyTemplate).
	Microsoft.UI.Xaml.Media.Animation.Storyboard? _insertionFadeStoryboard;
	Microsoft.UI.Xaml.Media.Animation.DoubleAnimation? _insertionHeadFadeIn;
	Microsoft.UI.Xaml.Media.Animation.DoubleAnimation? _insertionLineFadeIn;

	// Per-type cache for ObservableCollection<T>.Move(int,int) MethodInfo lookups.
	// Keyed by concrete collection type so each closed generic variant is cached once.
	static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, System.Reflection.MethodInfo?>
		s_moveMethodCache = new();

	/// <summary>
	/// Event fired when a reorder operation completes successfully.
	/// </summary>
	public event EventHandler? ReorderCompleted;

	/// <summary>
	/// Convenience accessor that exposes the templated ItemsRepeater.
	/// </summary>
	internal ItemsRepeater? ItemsRepeaterControl => _itemsRepeater as ItemsRepeater;

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

			// Apply drag affordance to already-realized containers. Walk the
			// ItemsRepeater's visual children directly — only realized containers
			// exist in the visual tree, so this is O(realized) rather than
			// O(total items). GetElementIndex returns the authoritative flat index
			// for each realized element (consistent with how FindAllContainers works).
			int childCount = VisualTreeHelper.GetChildrenCount(repeater);
			for (int i = 0; i < childCount; i++)
			{
				if (VisualTreeHelper.GetChild(repeater, i) is ItemContainer ic)
				{
					int index = repeater.GetElementIndex(ic);
					if (index >= 0)
						ApplyDragAffordance(ic, index);
				}
			}

			// When the page is off-screen (e.g. an Options page is on top via
			// PushAsync), the repeater has no realized children because MAUI's
			// StackNavigationManager clears the ContentPresenter on navigation.
			// Subscribe a persistent Loaded handler so affordance is re-applied
			// every time the page re-enters the visual tree. This must be
			// unconditional (not gated on childCount == 0) because Phase 3's
			// deferred SetContent means containers may exist as lightweight
			// shells at wiring time but only become fully realized after the
			// next MeasureOverride — which only runs after Loaded.
			Loaded -= OnLoadedForAffordanceReapply;
			Loaded += OnLoadedForAffordanceReapply;
		}

		_dragDropWired = true;
	}

	void OnLoadedForAffordanceReapply(object sender, RoutedEventArgs e)
	{
		if (!_canReorderItems || !_dragDropWired)
			return;

		var repeater = ItemsRepeaterControl;
		if (repeater is null)
			return;

		int childCount = VisualTreeHelper.GetChildrenCount(repeater);
		for (int i = 0; i < childCount; i++)
		{
			if (VisualTreeHelper.GetChild(repeater, i) is ItemContainer ic)
			{
				int index = repeater.GetElementIndex(ic);
				if (index >= 0)
					ApplyDragAffordance(ic, index);
			}
		}
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

			// Walk the ItemsRepeater's visual children directly (O(realized))
			// to clear drag affordance from every realized container. Only
			// realized containers exist in the visual tree, so no index loop needed.
			int childCount = VisualTreeHelper.GetChildrenCount(repeater);
			for (int i = 0; i < childCount; i++)
			{
				if (VisualTreeHelper.GetChild(repeater, i) is ItemContainer ic)
				{
					ic.CanDrag = false;
					ic.DragStarting -= ItemContainer_DragStarting;
					ic.DropCompleted -= ItemContainer_DropCompleted;
					// Clear the card Background set in ApplyDragAffordance so the
					// container falls back to the transparent ThemeResource (#13197).
					RemoveDragGhostAppearance(ic);
				}
			}
		}

		StopAutoScroll();
		_dragDropWired = false;
		Loaded -= OnLoadedForAffordanceReapply;
	}

	internal void DisconnectDragDrop()
	{
		Loaded -= OnLoadedForAffordanceReapply;
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

		// Hide the indicator visuals. They are template parts owned by the control
		// template (declared in XAML), so we only need to collapse them — not remove.
		HideInsertionIndicator();

		_mauiVirtualView = null;
		FlatTemplateCollection = null;
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
			if (_draggedSourceIndex >= 0 && IsContainerBoundToDraggedItem(itemContainer))
			{
				itemContainer.Opacity = 0;
				itemContainer.IsHitTestVisible = false;
				_sourceContainer = itemContainer;
			}
			else
			{
				// Apply dim if a drag is in progress and this is not the source.
				itemContainer.Opacity = _draggedSourceIndex >= 0 ? DragDimOpacity : 1;
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

		// Set the Fluent card background as a LOCAL dependency-property value so that:
		//   1. The drag ghost (captured by the compositor BEFORE DragStarting fires)
		//      always carries a visible card background regardless of DataTemplate content.
		//   2. The local value takes precedence over the transparent ThemeResource override
		//      set in the constructor (fix #13197) without changing that global default.
		// RemoveDragGhostAppearance calls ClearValue(BackgroundProperty) to undo this,
		// letting the transparent ThemeResource resume when drag-reorder is disabled.
		if (!isHeaderOrFooter
			&& WApp.Current?.Resources?.TryGetValue("CardBackgroundFillColorDefaultBrush", out var cardBg) == true
			&& cardBg is Microsoft.UI.Xaml.Media.Brush cardBrush)
		{
			itemContainer.Background = cardBrush;
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
			// Unsubscribe DropCompleted — it is a one-shot handler wired during
			// DragStarting and must be removed here so that a recycled container
			// doesn't carry a stale subscription into its next use.
			itemContainer.DropCompleted -= ItemContainer_DropCompleted;
			itemContainer.Tag = null;

			// If the container being cleared is the one currently hidden as the drag
			// source (e.g. recycled mid-drag), restore its opacity and hit-testability
			// so it doesn't get reused while invisible.
			itemContainer.Opacity = 1;
			itemContainer.IsHitTestVisible = true;
			RemoveDragGhostAppearance(itemContainer);
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

		// Check whether this container is bound to a source slot at all.
		// A container that has an ElementWrapper with a View IS bound — the item
		// itself may be null (valid null data row), so we must not treat null as
		// "no binding". We store the result so the null-item path shares the same
		// cancel logic as the "container not yet set up" path.
		bool hasBinding = itemContainer.Child is ElementWrapper _ew && _ew.VirtualView is View;

		// Use the container's currently bound item first. The Tag/index can become
		// stale after a reorder because the element is reused without being recreated.
		object? item = GetContainerItem(itemContainer);

		// Fallback: look up by index from the source (works for IList and IEnumerable).
		// Only run when there is no ElementWrapper binding (container not yet set up),
		// NOT when item is null — a null BindingContext is a valid null data row.
		if (!hasBinding && itemContainer.Tag is int index && index >= 0)
		{
			var sourceList = GetSourceList();
			if (sourceList is not null && index < sourceList.Count)
			{
				item = GetItemAtIndex(index, sourceList);
				hasBinding = true;
			}
		}

		if (!hasBinding)
		{
			args.Cancel = true;
			return;
		}

		_draggedItem = item;
		_draggedSourceIndex = GetContainerIndex(itemContainer);
		_sourceContainer = itemContainer;

		args.Data.Properties.Add("DragSource", "MauiItemsView");
		args.Data.RequestedOperation = WDataTransfer.DataPackageOperation.Move;

		// Make sure the drop-completed handler is wired exactly once so the source
		// container's opacity is restored on success, cancel, or escape.
		itemContainer.DropCompleted -= ItemContainer_DropCompleted;
		itemContainer.DropCompleted += ItemContainer_DropCompleted;

		// WinUI captures the drag ghost from the compositor tree BEFORE DragStarting
		// fires — no DragStarting-based approach (sync, deferral, RenderTargetBitmap)
		// can modify that snapshot.  The default ghost shows the item content on a
		// transparent background, which is the correct CV2 behaviour.
		// Hide the source slot and dim others on the next dispatcher frame so the
		// compositor snapshot has been committed before we change visual state.
		DispatcherQueue.TryEnqueue(
			Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
			() =>
			{
				if (_sourceContainer is not null)
				{
					_sourceContainer.Opacity = 0;
					_sourceContainer.IsHitTestVisible = false;
				}

				DimNonSourceContainers();
			});
	}

	void ItemContainer_DropCompleted(UIElement sender, UI.Xaml.DropCompletedEventArgs args)
	{
		if (sender is ItemContainer itemContainer)
		{
			itemContainer.DropCompleted -= ItemContainer_DropCompleted;
		}

		// DropCompleted fires on ALL drag-end paths: success (drop inside list),
		// cancel/ESC, and drop outside the window. On the success path,
		// ScrollViewer_Drop already called CleanupDragState() and unsubscribed this
		// handler — so DropCompleted typically only fires here for cancel/ESC/outside.
		// CleanupDragState() is idempotent, so calling it again on the success path
		// is harmless. This guarantees the auto-scroll timer is always stopped.
		CleanupDragState();
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

	bool IsContainerBoundToDraggedItem(ItemContainer container)
	{
		if (_draggedSourceIndex < 0)
			return false;

		// For null items, value-equality (Equals(null, null)) would match every blank
		// container. Use the authoritative index instead so only the source slot matches.
		if (_draggedItem is null)
			return container.Tag is int t && t == _draggedSourceIndex;

		return IsContainerBoundToItem(container, _draggedItem);
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
	/// on a grouped source collection. Used only for same-group drag-drop reorder in grouped lists.
	///
	/// <b>Not used for flat lists.</b> For flat lists, <see cref="PerformReorder"/> uses
	/// <see cref="ObservableItemTemplateCollection2.MoveItemAndSyncSource"/> (when a template
	/// collection is active) or plain <c>RemoveAt</c> + <c>Insert</c> (no-template case).
	/// Calling <c>Move</c> on a source that is directly bound to <see cref="ItemsRepeater"/>
	/// (no <see cref="ObservableItemTemplateCollection2"/> wrapping) fires
	/// <c>CollectionChanged(Move)</c> which CsWinRT maps to <c>VectorChanged(Reset)</c>,
	/// causing ItemsRepeater to clear all containers and scroll to the top.
	///
	/// Only <c>ObservableCollection&lt;object&gt;</c> is handled directly; all other types
	/// return <c>false</c> and the caller falls back to <c>RemoveAt</c> + <c>Insert</c>.
	/// </summary>
	static bool TryMoveObservableCollection(IList list, int oldIndex, int newIndex)
	{
		// Fast path: no reflection needed for the common MAUI binding source type.
		if (list is System.Collections.ObjectModel.ObservableCollection<object> oc)
		{
			oc.Move(oldIndex, newIndex);
			return true;
		}

		// General path: ObservableCollection<T> for any T.
		// Reflection is used intentionally here — ObservableCollection<T>.Move is a
		// public, stable API and this code runs only during interactive drag/drop on Windows.
#pragma warning disable IL2070 // 't' parameter doesn't need trimmer annotation — Move is always preserved on ObservableCollection<T>
		var moveMethod = s_moveMethodCache.GetOrAdd(list.GetType(), t => t.GetMethod(
			"Move",
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
			null,
			new[] { typeof(int), typeof(int) },
			null));
#pragma warning restore IL2070

		if (moveMethod is not null)
		{
			moveMethod.Invoke(list, new object[] { oldIndex, newIndex });
			return true;
		}

		return false;
	}

	void ScrollViewer_DragLeave(object sender, UI.Xaml.DragEventArgs e)
	{
		// Do NOT stop auto-scroll here. Two cases where this fires during a valid drag:
		//   1. Horizontal right edge — pointer exits the ScrollViewer bounds; we want
		//      scrolling to continue until the drag ends or velocity decays.
		//   2. Vertical scroll — ChangeView() triggers a brief DragLeave/DragEnter
		//      cycle as WinUI re-evaluates hit-targets after content shifts; stopping
		//      here would cause stuttering (scroll→stop→scroll→stop).
		// The timer decelerates naturally when _targetScrollVelocity is reset to 0 in
		// HandleAutoScroll (pointer back in neutral zone) or when the scroll boundary
		// is reached. Full cleanup happens in CleanupDragState when the drag ends.
		HideInsertionIndicator();
	}

	void ScrollViewer_Drop(object sender, UI.Xaml.DragEventArgs e)
	{
		// _draggedSourceIndex < 0 means no active drag. _draggedItem may be null for
		// null data rows, so we cannot use _draggedItem is null as the guard here.
		if (!_canReorderItems || _draggedSourceIndex < 0 || _insertionIndex < 0 || _mauiVirtualView is null)
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
		// _draggedSourceIndex < 0 means no active drag; _draggedItem may be null for null data rows.
		if (_draggedSourceIndex < 0)
		{
			return false;
		}

		// Prefer the source index captured at DragStarting so value-equal duplicates
		// (record structs, boxed primitives, two equal entries) resolve to the row the
		// user actually dragged. Validate it still points at an equal item — if a
		// concurrent insert/remove shifted the row, fall back to a linear search.
		int oldIndex = -1;
		if (_draggedSourceIndex >= 0 && _draggedSourceIndex < itemsList.Count)
		{
			var candidate = GetItemAtIndex(_draggedSourceIndex, itemsList);
			if (ReferenceEquals(candidate, _draggedItem) || Equals(candidate, _draggedItem))
			{
				oldIndex = _draggedSourceIndex;
			}
		}

		if (oldIndex < 0)
		{
			oldIndex = IndexOfItem(_draggedItem, itemsList);
		}

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

		// Prefer Move over RemoveAt + Insert.
		//
		// When FlatTemplateCollection is set (ItemTemplate is active), MoveItemAndSyncSource:
		//   1. Mutates the source silently (_observeChanges=false suppresses InnerCollectionChanged).
		//   2. Calls Move on the template collection; MoveItem fires Remove+Add (not Move) so
		//      CsWinRT emits VectorChanged(ItemRemoved + ItemInserted) — not VectorChanged(Reset).
		//      ItemsRepeater repositions the existing container without recycling it.
		//
		// When there is no ItemTemplate the raw source is the ItemsRepeater's data directly.
		// In that case we must use RemoveAt+Insert, NOT ObservableCollection.Move:
		//   Move → CollectionChanged(Move) → CsWinRT → VectorChanged(Reset)
		//       → ItemsRepeater clears all realized containers → scroll to top.
		//   RemoveAt+Insert → CollectionChanged(Remove+Add) → VectorChanged(ItemRemoved+ItemInserted)
		//       → ItemsRepeater repositions without resetting the scroll position.
		//
		// IsReordering guards ItemsChanged in ItemsViewHandler2 so ApplyItemsUpdatingScrollMode
		// does not call StartBringItemIntoView(0) (KeepItemsInView default) during the mutation.
		IsReordering = true;
		try
		{
			if (FlatTemplateCollection is not null)
			{
				// Template-collection path: atomically moves source item and repositions
				// the existing ItemTemplateContext2 wrapper — no new wrapper, no BringIntoView.
				FlatTemplateCollection.MoveItemAndSyncSource(oldIndex, adjustedInsertionIndex);
			}
			else
			{
				// No-template path: source IS the ItemsRepeater's data. Must NOT use Move
				// (would cause VectorChanged(Reset) via CsWinRT). RemoveAt+Insert is safe.
				var itemToMove = itemsList[oldIndex];
				itemsList.RemoveAt(oldIndex);
				adjustedInsertionIndex = Math.Clamp(adjustedInsertionIndex, 0, itemsList.Count);
				itemsList.Insert(adjustedInsertionIndex, itemToMove);
			}
		}
		finally
		{
			IsReordering = false;
		}

		DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
		{
			ItemsRepeaterControl?.UpdateLayout();
			UpdateAllContainerIndices();
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
		// _draggedSourceIndex < 0 means no active drag; _draggedItem may be null for null data rows.
		if (_draggedSourceIndex < 0 || _mauiVirtualView is not GroupableItemsView groupableView)
		{
			return false;
		}

		bool hasHeaders = groupableView.GroupHeaderTemplate is not null;
		bool hasFooters = groupableView.GroupFooterTemplate is not null;

		// Find which group the dragged item belongs to using _draggedSourceIndex with
		// flat-index arithmetic. This is more reliable than value-equality search for
		// all items: null items would match the first null in any group, and duplicate
		// non-null items would match the wrong occurrence. _draggedSourceIndex is
		// captured at DragStarting via GetContainerIndex and is always accurate.
		int sourceGroupIndex = -1;
		int sourceItemIndex = -1;
		IList? sourceGroup = null;

		int flatPos = 0;
		for (int g = 0; g < groupsList.Count; g++)
		{
			if (groupsList[g] is not IEnumerable groupItems)
				continue;

			int groupItemCount = groupsList[g] is ICollection coll
				? coll.Count
				: groupItems.Cast<object>().Count();

			if (hasHeaders) flatPos++; // skip header
			int itemsStart = flatPos;
			flatPos += groupItemCount;
			if (hasFooters) flatPos++; // skip footer

			if (_draggedSourceIndex >= itemsStart && _draggedSourceIndex < itemsStart + groupItemCount)
			{
				sourceGroupIndex = g;
				sourceItemIndex = _draggedSourceIndex - itemsStart;
				sourceGroup = groupsList[g] as IList;
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
		flatPos = 0;

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

			IsReordering = true;
			try
			{
				// Use Move when possible to fire a single CollectionChanged(Move) event,
				// preventing ItemsRepeater from recycling containers and resetting scroll.
				if (!TryMoveObservableCollection(sourceGroup, sourceItemIndex, adjustedTargetIndex))
				{
					var item = sourceGroup[sourceItemIndex];
					sourceGroup.RemoveAt(sourceItemIndex);
					adjustedTargetIndex = Math.Clamp(adjustedTargetIndex, 0, sourceGroup.Count);
					sourceGroup.Insert(adjustedTargetIndex, item);
				}
			}
			finally
			{
				IsReordering = false;
			}
		}
		else
		{
			IsReordering = true;
			try
			{
				var item = sourceGroup[sourceItemIndex];
				sourceGroup.RemoveAt(sourceItemIndex);
				targetItemIndex = Math.Clamp(targetItemIndex, 0, targetGroup.Count);
				targetGroup.Insert(targetItemIndex, item);
			}
			finally
			{
				IsReordering = false;
			}
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
		var repeater = ItemsRepeaterControl;
		int flatCount = repeater?.ItemsSourceView?.Count ?? 0;
		if (index < 0 || index >= flatCount)
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
		if (repeater is null)
			yield break;

		// Walk the ItemsRepeater's visual children directly: only realized containers
		// exist in the visual tree, so this is O(realized) rather than O(total items).
		// The previous approach (TryGetElement(i) for i in 0..ItemsSourceView.Count)
		// was O(N) over ALL items even though only ~20 are realized at any time.
		int childCount = VisualTreeHelper.GetChildrenCount(repeater);
		for (int i = 0; i < childCount; i++)
		{
			if (VisualTreeHelper.GetChild(repeater, i) is FrameworkElement fe)
				yield return fe;
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

		// For null-item containers, Equals(null, null) makes tag validation and
		// IndexOfItem both return the first null row — wrong when multiple nulls exist.
		// GetElementIndex is O(1) and always accurate after a layout pass, so use it first.
		if (containerItem is null && container is ItemContainer nullIc && ItemsRepeaterControl is not null)
		{
			int repeaterIdx = ItemsRepeaterControl.GetElementIndex(nullIc);
			if (repeaterIdx >= 0)
				return repeaterIdx;
		}

		// Prefer the Tag set during ElementPrepared — it is the authoritative flat
		// index and avoids the ambiguity where group headers and footers share the
		// same underlying Item (the group object). Validate the tag by checking that
		// the item at that index still matches the container's current item.
		// The containerItem is not null guard was intentionally removed: null-item
		// containers (valid null data rows) need tag validation too, and
		// Equals(null, null) correctly returns true for them.
		if (container.Tag is int tagIndex && sourceList is not null &&
			tagIndex >= 0 && tagIndex < sourceList.Count)
		{
			var tagItem = GetItemAtIndex(tagIndex, sourceList);
			if (Equals(tagItem, containerItem))
			{
				return tagIndex;
			}
		}

		// Tag is stale — fall back to a linear search.
		// The containerItem is not null guard was removed: null-item containers
		// need linear search fallback just like any other item.
		if (sourceList is not null)
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

	int IndexOfItem(object? item, IList itemsList)
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

		var repeater = ItemsRepeaterControl;

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
			else
			{
				// For null-item containers, IndexOfItem returns the first null which
				// may be a different row. Use the ItemsRepeater's authoritative element
				// index instead — it is always accurate after a layout pass.
				if (repeater is not null && container is ItemContainer ic)
				{
					int repeaterIndex = repeater.GetElementIndex(ic);
					if (repeaterIndex >= 0)
					{
						container.Tag = repeaterIndex;
					}
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
			// Restore only the drag-source-specific overrides (opacity + hit-testing).
			// Do NOT call RemoveDragGhostAppearance here — that would clear the card
			// Background set by ApplyDragAffordance.  A same-location drop leaves the
			// container in place (not recycled), so ApplyDragAffordance won't run again;
			// clearing the Background here would expose the transparent ThemeResource
			// and leave the item visually broken for subsequent drags.
			// Background is cleared in ElementClearing (recycle) and UnwireDragDropEvents
			// (drag-reorder disabled) — the two paths that actually require the cleanup.
			_sourceContainer.Opacity = 1;
			_sourceContainer.IsHitTestVisible = true;
			_sourceContainer.DropCompleted -= ItemContainer_DropCompleted;
			_sourceContainer = null;
		}

		// Restore all dimmed containers.
		RestoreAllContainerOpacity();

		_draggedItem = null;
		_draggedSourceIndex = -1;
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
			// Stop accelerating when the boundary is reached so the timer
			// decelerates and stops instead of spinning at max offset.
			if (newOffset <= 0 || newOffset >= scrollViewer.ScrollableWidth)
				_targetScrollVelocity = 0;
			newOffset = Math.Clamp(newOffset, 0, scrollViewer.ScrollableWidth);
			scrollViewer.ChangeView(newOffset, scrollViewer.VerticalOffset, null, disableAnimation: true);
		}
		else
		{
			newOffset = scrollViewer.VerticalOffset + _currentScrollVelocity;
			// Stop accelerating when the boundary is reached.
			if (newOffset <= 0 || newOffset >= scrollViewer.ScrollableHeight)
				_targetScrollVelocity = 0;
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
	/// Builds and caches the Storyboard + DoubleAnimations used to fade in the
	/// insertion indicator. Called once from <see cref="MauiItemsView.OnApplyTemplate"/>
	/// after the template parts are resolved. Caching avoids allocating new animation
	/// objects on every DragOver event that transitions Collapsed → Visible.
	/// </summary>
	void InitInsertionFadeStoryboard()
	{
		if (_dropIndicatorHead is null || _dropIndicatorLine is null)
			return;

		var ease = new Microsoft.UI.Xaml.Media.Animation.CubicEase
		{
			EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut
		};
		var duration = new Duration(TimeSpan.FromMilliseconds(80));

		_insertionHeadFadeIn = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
		{
			To = 1.0,
			Duration = duration,
			EasingFunction = ease,
		};
		_insertionLineFadeIn = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
		{
			To = 1.0,
			Duration = duration,
			EasingFunction = ease,
		};

		_insertionFadeStoryboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
		_insertionFadeStoryboard.Children.Add(_insertionHeadFadeIn);
		_insertionFadeStoryboard.Children.Add(_insertionLineFadeIn);

		Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(_insertionHeadFadeIn, _dropIndicatorHead);
		Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(_insertionHeadFadeIn, "Opacity");
		Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(_insertionLineFadeIn, _dropIndicatorLine);
		Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(_insertionLineFadeIn, "Opacity");
	}

	/// <summary>
	/// Shows a 2 px accent-coloured line on <see cref="_dropIndicatorCanvas"/> at the
	/// boundary between items — "insert before <paramref name="target"/>" or "insert
	/// after <paramref name="target"/>".  The line is positioned in the canvas coordinate
	/// space so it is always correct regardless of scroll position.
	/// </summary>
	void UpdateInsertionIndicator(ItemContainer target, bool insertAfter)
	{
		if (_dropIndicatorHead is null || _dropIndicatorLine is null || _dropIndicatorCanvas is null)
			return;

		// Skip the source slot itself.
		if (ReferenceEquals(target, _sourceContainer))
		{
			HideInsertionIndicator();
			return;
		}

		// ── Calculate position in canvas coordinates ──────────────────────────
		var origin = target.TransformToVisual(_dropIndicatorCanvas)
			.TransformPoint(new global::Windows.Foundation.Point(0, 0));

		if (_isHorizontalLayout)
		{
			// Vertical indicator: hollow circle at top-center, line extending downward.
			double lineX = insertAfter
				? origin.X + target.ActualWidth
				: origin.X;

			double lineHeight = target.ActualHeight - IndicatorHeadSize - IndicatorHeadGap;
			if (lineHeight < 0)
				lineHeight = 0;

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
			if (lineWidth < 0)
				lineWidth = 0;

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
		bool wasCollapsed = _dropIndicatorHead.Visibility == WVisibility.Collapsed;

		if (wasCollapsed)
		{
			_dropIndicatorHead.Opacity = 0;
			_dropIndicatorLine.Opacity = 0;
		}

		_dropIndicatorHead.Visibility = WVisibility.Visible;
		_dropIndicatorLine.Visibility = WVisibility.Visible;

		if (wasCollapsed)
		{
			// One-shot fade-in only on first show. Use the pre-built cached storyboard
			// so we don't allocate a new Storyboard + 2 DoubleAnimations on every DragOver.
			if (_insertionFadeStoryboard is not null)
			{
				_insertionFadeStoryboard.Begin();
			}
			else
			{
				// Fallback if the storyboard couldn't be built in OnApplyTemplate
				// (e.g. template parts missing). Snap to full opacity immediately.
				_dropIndicatorHead.Opacity = 1;
				_dropIndicatorLine.Opacity = 1;
			}
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
		// Type-pattern variables let us assign the field to a local before setting
		// the property — null-conditional (?.) cannot appear on the left side of an
		// assignment, so a local capture is the idiomatic null-safe setter pattern.
		if (_dropIndicatorHead is WBorder head)
			head.Visibility = WVisibility.Collapsed;
		if (_dropIndicatorLine is Rectangle line)
			line.Visibility = WVisibility.Collapsed;
	}

	// Indicator geometry constants.
	const double IndicatorHeadSize = 12.0;    // hollow circle outer diameter in px
	const double IndicatorHeadGap = 2.0;      // gap between circle and line
	const double IndicatorLineThickness = 2.0;

	#endregion

	#region Dim / Restore During Drag

	/// <summary>
	/// Removes any locally-set Background on <paramref name="container"/> so the
	/// DP falls back to the Style-set ThemeResource (transparent by default in
	/// MauiItemsView).  Called defensively in ElementClearing and CleanupDragState
	/// to guard against stale local values on recycled containers.
	/// </summary>
	static void RemoveDragGhostAppearance(ItemContainer container)
	{
		container.ClearValue(Microsoft.UI.Xaml.Controls.Control.BackgroundProperty);
	}

	/// <summary>
	/// Dims all realized containers except the source container to the
	/// <see cref="DragDimOpacity"/> level, visually signalling reorder mode.
	/// Called after the source container is hidden so it doesn't accidentally
	/// receive DragDimOpacity on top of Opacity=0.
	/// </summary>
	void DimNonSourceContainers()
	{
		// _draggedSourceIndex < 0 means no drag is in progress.
		// _draggedItem may be null for null data rows, so we cannot use that as the guard.
		if (_draggedSourceIndex < 0)
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
