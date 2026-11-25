#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Graphics;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;
using AViewCompat = AndroidX.Core.View.ViewCompat;

namespace Microsoft.Maui.Controls.Handlers.Items
{

	public class MauiRecyclerView<TItemsView, TAdapter, TItemsViewSource> : RecyclerView, IMauiRecyclerView<TItemsView>, IMauiRecyclerView, IScrollAnchorRecyclerView
		where TItemsView : ItemsView
		where TAdapter : ItemsViewAdapter<TItemsView, TItemsViewSource>
		where TItemsViewSource : IItemsViewSource
	{
		const int InvalidPosition = -1;

		protected TAdapter ItemsViewAdapter;

		protected TItemsView ItemsView;
		public IItemsLayout ItemsLayout { get; private set; }

		readonly Func<IItemsLayout> _getItemsLayout;
		protected Func<TAdapter> CreateAdapter;

		SnapManager _snapManager;
		ScrollHelper _scrollHelper;
		protected RecyclerView.OnScrollListener RecyclerViewScrollListener;

		EmptyViewAdapter _emptyViewAdapter;
		readonly DataChangeObserver _emptyCollectionObserver;
		readonly DataChangeObserver _itemsUpdateScrollObserver;
		readonly DataChangeObserver _scrollAnchorObserver;
		readonly Func<MotionEvent, bool> _dispatchTouchEventToRecyclerView;
		ParentScrollGestureDispatcher _parentScrollGestureDispatcher;

		ScrollBarVisibility _defaultHorizontalScrollVisibility = ScrollBarVisibility.Default;
		ScrollBarVisibility _defaultVerticalScrollVisibility = ScrollBarVisibility.Default;

		ItemDecoration _itemDecoration;

		ItemTouchHelper _itemTouchHelper;
		SimpleItemTouchHelperCallback _itemTouchHelperCallback;
		WeakNotifyPropertyChangedProxy _layoutPropertyChangedProxy;
		PropertyChangedEventHandler _layoutPropertyChanged;
		Java.Lang.IRunnable _setAppBarLiftTargetRunnable;
		RecyclerViewScrollAnchor _scrollAnchor;

		// Set as the app requests an animated scroll, and cleared once the view stops or the user drags it: the anchor stops a
		// scroll the app requested, which an edge it starts to follow supersedes, and lets a fling of the user's settle.
		bool _animatingRequestedScroll;

		// Counts the scrolls the app requests: ScrollTo an item, ScrollToStart, ScrollToEnd, and an edge AnchorMode has the view
		// start to follow. Stopping a scroll raises a pending RemainingItemsThresholdReached as the view goes idle: a scroll
		// requested from it is newer than the one that stopped the view.
		int _scrollRequests;

		CollectionViewAccessibilityMetadata _accessibilityMetadata;

		~MauiRecyclerView() => _layoutPropertyChangedProxy?.Unsubscribe();

		public MauiRecyclerView(Context context, Func<IItemsLayout> getItemsLayout, Func<TAdapter> getAdapter) : base(new ContextThemeWrapper(context, Resource.Style.collectionViewTheme))
		{
			_getItemsLayout = getItemsLayout ?? throw new ArgumentNullException(nameof(getItemsLayout));
			CreateAdapter = getAdapter ?? throw new ArgumentNullException(nameof(getAdapter));

			_emptyCollectionObserver = new DataChangeObserver(_ => UpdateEmptyViewVisibility());
			_itemsUpdateScrollObserver = new DataChangeObserver(AdjustScrollForItemUpdate);
			_scrollAnchorObserver = new DataChangeObserver(_ => ScrollAnchorItemsChanged());
			_dispatchTouchEventToRecyclerView = DispatchTouchEventToRecyclerView;
			_parentScrollGestureDispatcher = new ParentScrollGestureDispatcher(this);
		}

		public override void SetAdapter(Adapter adapter)
		{
			base.SetAdapter(adapter);
			_accessibilityMetadata?.InvalidateCache();
		}

		public override void SwapAdapter(Adapter adapter, bool removeAndRecycleExistingViews)
		{
			base.SwapAdapter(adapter, removeAndRecycleExistingViews);
			_accessibilityMetadata?.InvalidateCache();
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			if (RuntimeFeature.IsMaterial3Enabled)
			{
				PostTrySetAppBarLiftTargetIfOnScreen();
			}
		}

		protected override void OnDetachedFromWindow()
		{
			// Clean up AppBar listener while the ViewTreeObserver is still valid.
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				ClearAppBarLiftTargetAndPendingPost();
			}

			base.OnDetachedFromWindow();
		}

		protected override void OnVisibilityChanged(AView changedView, ViewStates visibility)
		{
			base.OnVisibilityChanged(changedView, visibility);

			if (changedView != this)
			{
				return;
			}

			if (!RuntimeFeature.IsMaterial3Enabled)
			{
				return;
			}

			if (visibility == ViewStates.Visible)
			{
				PostTrySetAppBarLiftTargetIfOnScreen();
			}
			else
			{
				ClearAppBarLiftTargetAndPendingPost();
			}
		}

		void PostTrySetAppBarLiftTargetIfOnScreen()
		{
			var runnable = GetOrCreateSetAppBarLiftTargetRunnable();
			RemoveCallbacks(runnable);
			Post(runnable);
		}

		void ClearAppBarLiftTargetAndPendingPost()
		{
			if (_setAppBarLiftTargetRunnable is not null)
			{
				RemoveCallbacks(_setAppBarLiftTargetRunnable);
			}

			this.ClearAppBarLiftTarget();
		}

		Java.Lang.IRunnable GetOrCreateSetAppBarLiftTargetRunnable()
		{
			return _setAppBarLiftTargetRunnable ??= new Java.Lang.Runnable(() => this.TrySetAppBarLiftTargetIfOnScreen());
		}

		public virtual void TearDownOldElement(TItemsView oldElement)
		{
			// Stop listening for layout property changes
			if (_layoutPropertyChangedProxy is not null)
			{
				_layoutPropertyChangedProxy.Unsubscribe();
				_layoutPropertyChanged = null;
			}

			// Stop listening for ScrollTo requests
			oldElement.ScrollToRequested -= ScrollToRequested;

			DetachScrollAnchor();

			RemoveScrollListener();

			if (ItemsViewAdapter != null)
			{
				// Stop watching for empty items or scroll adjustments
				_emptyCollectionObserver.Stop(ItemsViewAdapter);
				_itemsUpdateScrollObserver.Stop(ItemsViewAdapter);

				// Unhook whichever adapter is active
				SetAdapter(null);

				_accessibilityMetadata?.UnobserveAdapter(_emptyViewAdapter);
				_accessibilityMetadata?.UnobserveAdapter(ItemsViewAdapter);
				_emptyViewAdapter?.Dispose();
				ItemsViewAdapter?.Dispose();
			}

			_snapManager?.Dispose();
			_snapManager = null;

			if (_itemDecoration != null)
			{
				RemoveItemDecoration(_itemDecoration);
			}

			if (_itemTouchHelper != null)
			{
				_itemTouchHelper.AttachToRecyclerView(null);
				_itemTouchHelper.Dispose();
				_itemTouchHelper = null;
			}

			_itemTouchHelperCallback?.Dispose();
			_itemTouchHelperCallback = null;
		}

		public virtual void SetUpNewElement(TItemsView newElement)
		{
			if (newElement == null)
			{
				ItemsView = null;
				return;
			}

			ItemsView = newElement;

			// Check the control instance: CollectionViewHandler uses ReorderableItemsView as TItemsView.
			if (newElement is CollectionView && _accessibilityMetadata is null)
			{
				_accessibilityMetadata = new CollectionViewAccessibilityMetadata(this);
				SetAccessibilityDelegateCompat(new CollectionViewAccessibilityDelegate(this, _accessibilityMetadata));
				_accessibilityMetadata.ObserveAdapter(ItemsViewAdapter);
				_accessibilityMetadata.ObserveAdapter(_emptyViewAdapter);
			}

			UpdateLayoutManager();

			UpdateBackgroundColor();
			UpdateBackground();

			// Listen for ScrollTo requests
			ItemsView.ScrollToRequested += ScrollToRequested;

			// Listen for ScrollTo requests
			AddOrUpdateScrollListener();

			// Update the snap behavior after add the scroll listener
			UpdateSnapBehavior();
		}

		public void UpdateItemTemplate()
		{
			GetRecycledViewPool().Clear();
			UpdateAdapter();
		}

		public void UpdateScrollingMode()
		{
			UpdateItemsUpdatingScrollMode();
		}

		public virtual void UpdateVerticalScrollBarVisibility()
		{
			if (_defaultVerticalScrollVisibility == ScrollBarVisibility.Default)
				_defaultVerticalScrollVisibility = VerticalScrollBarEnabled ? ScrollBarVisibility.Always : ScrollBarVisibility.Never;

			var newVerticalScrollVisibility = ItemsView.VerticalScrollBarVisibility;

			if (newVerticalScrollVisibility == ScrollBarVisibility.Default)
				newVerticalScrollVisibility = _defaultVerticalScrollVisibility;

			VerticalScrollBarEnabled = newVerticalScrollVisibility == ScrollBarVisibility.Always;
		}

		public virtual void UpdateHorizontalScrollBarVisibility()
		{
			if (_defaultHorizontalScrollVisibility == ScrollBarVisibility.Default)
				_defaultHorizontalScrollVisibility =
					HorizontalScrollBarEnabled ? ScrollBarVisibility.Always : ScrollBarVisibility.Never;

			var newHorizontalScrollVisiblility = ItemsView.HorizontalScrollBarVisibility;

			if (newHorizontalScrollVisiblility == ScrollBarVisibility.Default)
				newHorizontalScrollVisiblility = _defaultHorizontalScrollVisibility;

			HorizontalScrollBarEnabled = newHorizontalScrollVisiblility == ScrollBarVisibility.Always;
		}

		public virtual void UpdateEmptyView()
		{
			if (ItemsViewAdapter == null || ItemsView == null)
			{
				return;
			}

			var emptyView = ItemsView?.EmptyView;
			var emptyViewTemplate = ItemsView?.EmptyViewTemplate;

			if (emptyView != null || emptyViewTemplate != null)
			{
				if (_emptyViewAdapter == null)
				{
					_emptyViewAdapter = new EmptyViewAdapter(ItemsView);
					_accessibilityMetadata?.ObserveAdapter(_emptyViewAdapter);
				}

				if (ItemsView is StructuredItemsView structuredItemsView)
				{
					_emptyViewAdapter.Header = structuredItemsView.Header;
					_emptyViewAdapter.HeaderTemplate = structuredItemsView.HeaderTemplate;

					_emptyViewAdapter.Footer = structuredItemsView.Footer;
					_emptyViewAdapter.FooterTemplate = structuredItemsView.FooterTemplate;
				}

				_emptyViewAdapter.EmptyView = emptyView;
				_emptyViewAdapter.EmptyViewTemplate = emptyViewTemplate;

				_emptyCollectionObserver.Start(ItemsViewAdapter);

				// When the EmptyView swaps while _emptyViewAdapter is active, RecyclerView can
				// reuse a stale item ViewHolder from the shared pool at the empty position.
				// Clear the pool before refreshing the EmptyView adapter to prevent that reuse.
				if (GetAdapter() == _emptyViewAdapter)
				{
					GetRecycledViewPool().Clear();
				}

				_emptyViewAdapter.NotifyDataSetChanged();
			}
			else
			{
				_emptyCollectionObserver.Stop(ItemsViewAdapter);
			}

			UpdateEmptyViewVisibility();
		}

		public virtual void UpdateFlowDirection()
		{
			if (ItemsView == null)
			{
				return;
			}

			var previousLayoutDirection = LayoutDirection;
			this.UpdateFlowDirection(ItemsView);

			ReconcileFlowDirectionAndLayout();

			if (LayoutDirection != previousLayoutDirection)
			{
				// Span offsets depend on direction and are cached on realized children.
				InvalidateItemDecorations();
			}
		}

		public virtual void UpdateAdapter()
		{
			var oldItemViewAdapter = ItemsViewAdapter;

			_emptyCollectionObserver.Stop(oldItemViewAdapter);
			_itemsUpdateScrollObserver.Stop(oldItemViewAdapter);
			_scrollAnchorObserver.Stop(oldItemViewAdapter);

			ItemsViewAdapter = CreateAdapter();
			_accessibilityMetadata?.ObserveAdapter(ItemsViewAdapter);

			if (_scrollAnchor is not null && ItemsViewAdapter is not null)
			{
				_scrollAnchorObserver.Start(ItemsViewAdapter);
			}

			(RecyclerViewScrollListener as RecyclerViewScrollListener<TItemsView, TItemsViewSource>)?.UpdateAdapter(ItemsViewAdapter);

			if (GetAdapter() != _emptyViewAdapter)
			{
				SetAdapter(null);

				SwapAdapter(ItemsViewAdapter, true);
			}

			UpdateEmptyView();

			UpdateItemsUpdatingScrollMode();

			_itemTouchHelperCallback?.SetAdapter(ItemsViewAdapter as IItemTouchHelperAdapter);

			_accessibilityMetadata?.UnobserveAdapter(oldItemViewAdapter);
			oldItemViewAdapter?.Dispose();

			// The new adapter lays the view out from its start: the anchor decides where it goes.
			_scrollAnchor?.AdapterReplaced();
		}

		public virtual void UpdateCanReorderItems()
		{
			var canReorderItems = (ItemsView as ReorderableItemsView)?.CanReorderItems == true;

			if (canReorderItems)
			{
				if (_itemTouchHelperCallback == null)
				{
					_itemTouchHelperCallback = new SimpleItemTouchHelperCallback();
				}
				if (_itemTouchHelper == null)
				{
					_itemTouchHelper = new ItemTouchHelper(_itemTouchHelperCallback);
					_itemTouchHelper.AttachToRecyclerView(this);
				}
				_itemTouchHelperCallback.SetAdapter(ItemsViewAdapter as IItemTouchHelperAdapter);
			}
			else
			{
				if (_itemTouchHelper != null)
				{
					_itemTouchHelper.AttachToRecyclerView(null);
					_itemTouchHelper.Dispose();
					_itemTouchHelper = null;
				}

				_itemTouchHelperCallback?.Dispose();
				_itemTouchHelperCallback = null;
			}
		}

		public virtual void UpdateLayoutManager()
		{
			var itemsLayout = _getItemsLayout();

			if (itemsLayout == ItemsLayout)
			{
				return;
			}

			_layoutPropertyChangedProxy?.Unsubscribe();
			ItemsLayout = itemsLayout;

			// Keep track of the ItemsLayout's property changes
			if (ItemsLayout is not null)
			{
				_layoutPropertyChanged ??= LayoutPropertyChanged;
				_layoutPropertyChangedProxy = new WeakNotifyPropertyChangedProxy(ItemsLayout, _layoutPropertyChanged);
			}

			SetLayoutManager(SelectLayoutManager(ItemsLayout));

			UpdateFlowDirection();
			UpdateItemSpacing();

			// A layout that scrolls vertically again starts the anchor over now, not at its next look: a scroll the app
			// requests next, in the same update, is newer.
			_scrollAnchor?.LayoutManagerReplaced();
		}

		protected virtual RecyclerViewScrollListener<TItemsView, TItemsViewSource> CreateScrollListener() => new(ItemsView, ItemsViewAdapter);


		protected virtual void UpdateSnapBehavior()
		{
			_snapManager = GetSnapManager();

			_snapManager.UpdateSnapBehavior(ItemsLayout);
		}

		protected virtual SnapManager GetSnapManager()
		{
			if (_snapManager == null)
				_snapManager = new SnapManager(this);
			return _snapManager;
		}

		// TODO hartez 2018/08/09 09:30:17 Package up background color and flow direction providers so we don't have to re-implement them here	
		protected virtual void UpdateBackgroundColor(Color color = null)
		{
			if (ItemsView == null)
				return;

			var backgroundColor = color ?? ItemsView.BackgroundColor;

			if (backgroundColor == null)
				return;

			SetBackgroundColor(backgroundColor.ToPlatform());
		}

		protected virtual void UpdateBackground(Brush brush = null)
		{
			if (ItemsView == null)
				return;

			if (!(this is RecyclerView recyclerView))
				return;

			Brush background = ItemsView.Background;

			recyclerView.UpdateBackground(background);
		}

		public virtual void UpdateItemsSource()
		{
			if (ItemsView == null)
			{
				return;
			}

			// Stop watching the old adapter 
			var adapter = ItemsViewAdapter ?? GetAdapter();
			_emptyCollectionObserver.Stop(adapter);
			_itemsUpdateScrollObserver.Stop(adapter);

			UpdateAdapter();

			// Set up any properties which require observing data changes in the adapter
			UpdateEmptyView();
			AddOrUpdateScrollListener();
			UpdateItemsUpdatingScrollMode();
			UpdateSnapBehavior();
		}

		protected virtual void UpdateItemsUpdatingScrollMode()
		{
			if (ItemsViewAdapter == null || ItemsView == null)
				return;

			if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepScrollOffset)
			{
				ScrollHelper.AddScrollListener();
			}
			else
			{
				ScrollHelper.RemoveScrollListener();
			}

			if (ItemsView is CarouselView && ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView)
			{
				// CarouselView manages its position in response to collection changes.
				_itemsUpdateScrollObserver.Stop(ItemsViewAdapter);
			}
			else
			{
				_itemsUpdateScrollObserver.Start(ItemsViewAdapter);
			}
		}

		public virtual void ScrollTo(ScrollToRequestEventArgs args)
		{
			if (ItemsView == null)
				return;

			var position = DetermineTargetPosition(args);

			if (position < 0)
			{
				System.Diagnostics.Debug.WriteLine($"Invalid scroll request: position = {position}");
				return;
			}

			// An index the items don't have: a position past the adapter's last one, or the header's or footer's, where an index
			// before the first item or past the last one puts it. Without animating, it would only replace a scroll requested
			// earlier that hasn't been laid out yet, which iOS doesn't do: RecyclerView ignores a position past the last one once
			// it lays out, and scrolls to the header or footer. Animated, RecyclerView scrolls to the position, or to the last
			// item for one past it (ItemsUpdatingScrollMode.KeepLastItemInView relies on that), which the anchor judges as a fling.
			var itemsSource = ItemsViewAdapter.ItemsSource;
			var isItem = position < ItemsViewAdapter.ItemCount && !itemsSource.IsHeader(position) && !itemsSource.IsFooter(position);
			if (!isItem && !args.IsAnimated)
			{
				return;
			}

			// A scroll the app requests is judged by where it lands: the anchor doesn't scroll it back to the end.
			var request = _scrollRequests;
			if (isItem)
			{
				request = ++_scrollRequests;
				_scrollAnchor?.ScrollRequested(args.IsAnimated);
			}

			if (args.IsAnimated)
			{
				_animatingRequestedScroll = true;

				// A jump an earlier request made scrolls in the next layout. RecyclerView's smooth scroller takes its first step
				// before that layout, and stops if it finds the item in place: the jump would then take the view away from it. So
				// the scroll starts once the view is laid out, as an animated ScrollToStart or ScrollToEnd does.
				if (isItem && (ScrollHelper.IsJumpPending || _scrollAnchor?.IsJumpPending == true))
				{
					ScrollHelper.AnimateScrollOnceLaidOut(() => AnimateScrollToItem(position, args.ScrollToPosition));
				}
				else
				{
					ScrollHelper.AnimateScrollToPosition(position, args.ScrollToPosition);
					if (isItem)
					{
						_scrollAnchor?.RequestedScrollStarted();
					}
				}
			}
			else
			{
				// A jump stops a scroll under way, and that raises a pending RemainingItemsThresholdReached as the view goes idle,
				// before RecyclerView sets the position it jumps to: a scroll the app requests from there is newer, and wins. So
				// the view stops first, as for ScrollToStart and ScrollToEnd (see StartScrollToEdge).
				StopScroll();
				if (request != _scrollRequests)
				{
					return;
				}

				ScrollHelper.JumpScrollToPosition(position, args.ScrollToPosition);
			}
		}

		// The animated scroll to an item that a ScrollTo starts once the view is laid out.
		void AnimateScrollToItem(int position, ScrollToPosition scrollToPosition)
		{
			ScrollHelper.AnimateScrollToPosition(position, scrollToPosition);
			_scrollAnchor?.RequestedScrollStarted();
		}

		protected virtual LayoutManager SelectLayoutManager(IItemsLayout layoutSpecification)
		{
			switch (layoutSpecification)
			{
				case GridItemsLayout gridItemsLayout:
					return CreateGridLayout(gridItemsLayout);
				case LinearItemsLayout listItemsLayout:
					var orientation = listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
						? LinearLayoutManager.Horizontal
						: LinearLayoutManager.Vertical;

					return new LinearLayoutManager(Context, orientation, false);
			}

			// Fall back to plain old vertical list
			// TODO hartez 2018/08/30 19:34:36 Log a warning when we have to fall back because of an unknown layout	
			return new LinearLayoutManager(Context);
		}

		protected virtual int DetermineTargetPosition(ScrollToRequestEventArgs args)
		{
			var item = args.Item;
			if (args.Mode == ScrollToMode.Position)
			{
				// Do not use `IGroupableItemsViewSource` since `UngroupedItemsSource` also implements that interface
				if (ItemsViewAdapter.ItemsSource is UngroupedItemsSource ungroupedSource)
				{
					return ungroupedSource.HasHeader ? args.Index + 1 : args.Index;
				}
				else if (ItemsViewAdapter.ItemsSource is IGroupableItemsViewSource groupItemSource)
				{
					// Flat-index request: compute the adapter position directly from the data
					// index. This avoids an item-roundtrip (Find item → GetPositionForItem walks
					// the groups again via .Equals) and the resulting ambiguity when two groups
					// contain equal-by-Equals items.
					if (args.GroupIndex < 0)
					{
						return GetAdapterPositionFromFlatDataIndex(args.Index, groupItemSource);
					}

					// The same for an index within a group, which also counts only the group's items.
					return GetAdapterPositionFromGroupedIndex(args.GroupIndex, args.Index, groupItemSource);
				}
			}

			return ItemsViewAdapter.GetPositionForItem(item);
		}

		/// <summary>
		/// Converts the index of an item within a group (excluding the group's header and footer, as on iOS/Mac/Windows) into
		/// a RecyclerView adapter position, which counts the headers and footers before it. Computed from the groups' sizes:
		/// no item round trip that could find an equal item in an earlier group.
		/// </summary>
		private static int GetAdapterPositionFromGroupedIndex(int groupIndex, int itemIndex, IGroupableItemsViewSource source)
		{
			if (itemIndex < 0)
			{
				return InvalidPosition;
			}

			// The outer ItemsView header (if any) sits at adapter position 0.
			int adapterPosition = source.HasHeader ? 1 : 0;
			for (int groupIdx = 0; groupIdx < groupIndex; groupIdx++)
			{
				if (source.GetGroupItemsViewSource(groupIdx) is not { } before)
				{
					return InvalidPosition;
				}

				// Skip this whole group (header + items + footer).
				adapterPosition += before.Count;
			}

			if (source.GetGroupItemsViewSource(groupIndex) is not { } group)
			{
				return InvalidPosition;
			}

			int groupHeader = group.HasHeader ? 1 : 0;
			int dataItemsInGroup = group.Count - groupHeader - (group.HasFooter ? 1 : 0);
			return itemIndex < dataItemsInGroup ? adapterPosition + groupHeader + itemIndex : InvalidPosition;
		}

		/// <summary>
		/// Converts a flat data item index (excluding group headers/footers — the cross-platform
		/// contract used by iOS/Mac/Windows) into a RecyclerView adapter position (which on Android
		/// does include group headers/footers). Computes the position in one pass, without
		/// materializing the item, so the caller can skip the second walk that
		/// <see cref="ItemsViewAdapter{TItemsView, TItemsViewSource}"/>'s
		/// GetPositionForItem would perform via .Equals.
		/// </summary>
		private static int GetAdapterPositionFromFlatDataIndex(int flatIndex, IGroupableItemsViewSource source)
		{
			if (flatIndex < 0)
			{
				return InvalidPosition;
			}

			// The outer ItemsView header (if any) sits at adapter position 0.
			int adapterPosition = source.HasHeader ? 1 : 0;
			int dataRemaining = flatIndex;

			for (int groupIdx = 0; ; groupIdx++)
			{
				var group = source.GetGroupItemsViewSource(groupIdx);
				if (group is null)
				{
					// Index is past the end of the data.
					return InvalidPosition;
				}

				int groupHeader = group.HasHeader ? 1 : 0;
				int dataItemsInGroup = group.Count - groupHeader - (group.HasFooter ? 1 : 0);

				if (dataRemaining < dataItemsInGroup)
				{
					// Item is in this group: skip the group header, then offset by the data index.
					return adapterPosition + groupHeader + dataRemaining;
				}

				// Skip this whole group (header + items + footer) and keep walking.
				adapterPosition += group.Count;
				dataRemaining -= dataItemsInGroup;
			}
		}

		protected virtual void UpdateItemSpacing()
		{
			if (ItemsLayout == null)
			{
				return;
			}

			if (_itemDecoration != null)
			{
				RemoveItemDecoration(_itemDecoration);
			}

			_itemDecoration = CreateSpacingDecoration(ItemsLayout);
			AddItemDecoration(_itemDecoration);

			if (_itemDecoration is SpacingItemDecoration spacingDecoration)
			{
				// Outer-edge spacing handled by SpacingItemDecoration.GetItemOffsets for all layout types.
				SetPadding(0, 0, 0, 0);
			}
		}

		protected virtual ItemDecoration CreateSpacingDecoration(IItemsLayout itemsLayout)
		{
			return new SpacingItemDecoration(Context, itemsLayout);
		}

		protected virtual void ReconcileFlowDirectionAndLayout()
		{
			if (!(GetLayoutManager() is LinearLayoutManager linearLayoutManager))
			{
				return;
			}

			if (linearLayoutManager.CanScrollVertically())
			{
				return;
			}
		}

		protected virtual void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			(GetSnapManager()?.GetCurrentSnapHelper() as SingleSnapHelper)?.ResetCurrentTargetPosition();
			ScrollTo(args);
		}

		protected virtual void LayoutPropertyChanged(object sender, PropertyChangedEventArgs propertyChanged)
		{
			if (propertyChanged.Is(GridItemsLayout.SpanProperty))
			{
				if (GetLayoutManager() is GridLayoutManager gridLayoutManager)
				{
					gridLayoutManager.SpanCount = ((GridItemsLayout)ItemsLayout).Span;
					UpdateItemSpacing();
				}
			}
			else if (propertyChanged.IsOneOf(Microsoft.Maui.Controls.ItemsLayout.SnapPointsTypeProperty, Microsoft.Maui.Controls.ItemsLayout.SnapPointsAlignmentProperty))
			{
				UpdateSnapBehavior();
			}
			else if (propertyChanged.IsOneOf(LinearItemsLayout.ItemSpacingProperty,
				GridItemsLayout.HorizontalItemSpacingProperty, GridItemsLayout.VerticalItemSpacingProperty))
			{
				UpdateItemSpacing();
			}
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			// If ItemsView is disabled, don't handle touch events
			if (ItemsView?.IsEnabled == false)
			{
				return false;
			}

			return base.OnTouchEvent(e);
		}

		bool DispatchTouchEventToRecyclerView(MotionEvent e) => base.DispatchTouchEvent(e);

		public override bool DispatchTouchEvent(MotionEvent e)
		{
			if (ItemsView?.IsEnabled == false)
			{
				return base.DispatchTouchEvent(e);
			}

			if (_parentScrollGestureDispatcher?.TryDispatchToParent(e, _dispatchTouchEventToRecyclerView, out var handled) == true)
			{
				return handled;
			}

			return base.DispatchTouchEvent(e);
		}

		public override bool OnInterceptTouchEvent(MotionEvent e)
		{
			// If ItemsView is disabled, intercept all touch events to prevent interactions
			if (ItemsView?.IsEnabled == false)
			{
				return true;
			}

			return base.OnInterceptTouchEvent(e);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			_scrollAnchor?.LayoutStarting();
			var jumps = _scrollHelper?.Jumps ?? 0;
			base.OnLayout(changed, l, t, r, b);
			ClipBounds = new ARect(0, 0, Width, Height);
			// After a direct (non-animated) scroll operation, we may need to make adjustments
			// to align the target item; if an adjustment is pending, execute it here.
			// (Deliberately checking the private member here rather than the property accessor; the accessor will
			// create a new ScrollHelper if needed, and there's no reason to do that until a Scroll is requested.)
			// A jump asked for as this layout ran, from the Scrolled event it raises, scrolls only in the layout after it, which
			// the jump asks for: so does its adjustment. An animated ScrollToStart or ScrollToEnd that waits for the view to be
			// laid out starts here too (see ScrollHelper.AnimateScrollOnceLaidOut).
			if (_scrollHelper is { } scrollHelper && scrollHelper.Jumps == jumps)
			{
				scrollHelper.AdjustScroll();
			}

			_scrollAnchor?.LayoutFinished();
		}

		public override void OnScrolled(int dx, int dy)
		{
			base.OnScrolled(dx, dy);
			_scrollAnchor?.Scrolled(dx, dy);
		}

		public override void OnScrollStateChanged(int state)
		{
			base.OnScrollStateChanged(state);
			if (state != ScrollStateSettling)
			{
				_animatingRequestedScroll = false;
			}

			_scrollAnchor?.ScrollStateChanged(state);
		}

		void IScrollAnchorRecyclerView.StopRequestedScroll()
		{
			// What a request left for the next layout: an adjustment, or an animated ScrollToStart or ScrollToEnd waiting for the
			// view to be laid out.
			_scrollHelper?.CancelPendingAdjustment();
			if (GetLayoutManager()?.IsSmoothScrolling == true || (_animatingRequestedScroll && ScrollState == ScrollStateSettling))
			{
				StopScroll();
			}
		}

		void IScrollAnchorRecyclerView.UpdateAnchorMode()
		{
			var itemsView = ItemsView as StructuredItemsView;
			var mode = itemsView is not null
				? itemsView.AnchorMode & (ScrollAnchorMode.Start | ScrollAnchorMode.End)
				: ScrollAnchorMode.None;

			// A layout that doesn't scroll vertically isn't anchored (see RecyclerViewScrollAnchor): there, an edge the view starts
			// to follow isn't a scroll, and a scroll the app requested carries on to where it was going.
			var scrollsToEdge = GetLayoutManager()?.CanScrollVertically() != false;

			if (mode == ScrollAnchorMode.None)
			{
				// Turning anchoring off only stops following. It isn't a scroll: a scroll the app requested carries on to where
				// it was going. An edge the view starts to follow is a scroll to that edge, and stops it (see
				// RecyclerViewScrollAnchor.UpdateMode).
				DetachScrollAnchor();
			}
			else if (_scrollAnchor is null)
			{
				// It scrolls to the edge it selects: it's newer than any scroll requested before.
				if (scrollsToEdge)
				{
					_scrollRequests++;
				}

				var scrollAnchor = new RecyclerViewScrollAnchor(this, itemsView, mode);
				_scrollAnchor = scrollAnchor;
				if (ItemsViewAdapter is not null)
				{
					_scrollAnchorObserver.Start(ItemsViewAdapter);
				}

				scrollAnchor.Attach();
			}
			else if (_scrollAnchor.Edges != mode)
			{
				if (scrollsToEdge)
				{
					_scrollRequests++;
				}

				_scrollAnchor.UpdateMode(mode);
			}
		}

		void DetachScrollAnchor()
		{
			_scrollAnchorObserver.Stop(ItemsViewAdapter);
			_scrollAnchor?.Detach();
			_scrollAnchor = null;
		}

		// RecyclerView keeps the rows in view where they are as it applies changes to the items: the anchor counts the offset
		// change that comes with them as the layout's.
		void ScrollAnchorItemsChanged() => _scrollAnchor?.ItemsChanged();

		bool IScrollAnchorRecyclerView.ScrollToStart(bool animate)
		{
			if (GetLayoutManager() is null)
			{
				return false;
			}

			// A scroll the app requested as this one stopped the view is newer, and wins.
			if (!StartScrollToEdge(animate) || GetLayoutManager() is not { } layoutManager)
			{
				return true;
			}

			if (!layoutManager.CanScrollVertically())
			{
				ScrollToEdgeItem(end: false, animate);
				return true;
			}

			if (_scrollAnchor is not null && _scrollAnchor.CanFollow(ScrollAnchorMode.Start))
			{
				_scrollAnchor.ScrollToStart(animate);
			}
			else if (layoutManager.ItemCount > 0)
			{
				// An anchor that follows the end judges where it lands, as for ScrollTo. The header is the adapter's first
				// position: its start lined up with the start of the viewport starts the content.
				_scrollAnchor?.ScrollRequested(animate);
				if (animate)
				{
					AnimateScrollToEdgeOnceLaidOut(end: false);
				}
				else
				{
					ScrollHelper.JumpScrollToStart();
				}
			}

			return true;
		}

		bool IScrollAnchorRecyclerView.ScrollToEnd(bool animate)
		{
			if (GetLayoutManager() is null)
			{
				return false;
			}

			// A scroll the app requested as this one stopped the view is newer, and wins.
			if (!StartScrollToEdge(animate) || GetLayoutManager() is not { } layoutManager)
			{
				return true;
			}

			if (!layoutManager.CanScrollVertically())
			{
				ScrollToEdgeItem(end: true, animate);
				return true;
			}

			if (_scrollAnchor is not null && _scrollAnchor.CanFollow(ScrollAnchorMode.End))
			{
				_scrollAnchor.ScrollToEnd(animate);
			}
			else if (layoutManager.ItemCount > 0)
			{
				// An anchor that follows the start judges where it lands, as for ScrollTo. The footer is the adapter's last
				// position: its end lined up with the end of the viewport ends the content.
				_scrollAnchor?.ScrollRequested(animate);
				if (animate)
				{
					AnimateScrollToEdgeOnceLaidOut(end: true);
				}
				else
				{
					ScrollHelper.JumpScrollToEnd();
				}
			}

			return true;
		}

		// An animated ScrollToStart or ScrollToEnd starts once the view is laid out (see ScrollHelper.AnimateScrollOnceLaidOut):
		// a jump an earlier request left for that layout lands first, and nothing follows an end that a row the app grew with
		// the request moves after the scroll stopped.
		void AnimateScrollToEdgeOnceLaidOut(bool end)
		{
			if (end)
			{
				ScrollHelper.AnimateScrollOnceLaidOut(AnimateScrollToEnd);
			}
			else
			{
				ScrollHelper.AnimateScrollOnceLaidOut(AnimateScrollToStart);
			}
		}

		void IScrollAnchorRecyclerView.AnimateScrollToEdgeOnceLaidOut(bool end) => AnimateScrollToEdgeOnceLaidOut(end);

		// The animated scrolls to an edge that ScrollToStart and ScrollToEnd start once the view is laid out.
		void AnimateScrollToStart()
		{
			if (GetLayoutManager() is { ItemCount: > 0 } layoutManager && layoutManager.CanScrollVertically())
			{
				_animatingRequestedScroll = true;
				RecyclerViewScrollAnchor.SmoothScrollToStart(this, layoutManager);
			}
		}

		void AnimateScrollToEnd()
		{
			if (GetLayoutManager() is { ItemCount: > 0 } layoutManager && layoutManager.CanScrollVertically())
			{
				_animatingRequestedScroll = true;
				RecyclerViewScrollAnchor.SmoothScrollToEnd(this, layoutManager);
			}
		}

		// Starts a ScrollToStart or ScrollToEnd, which is newer than any scroll requested before. Returns false when the app
		// requested a scroll as it stopped the view, which is newer still: a jump stops a scroll under way, and that raises a
		// pending RemainingItemsThresholdReached as the view goes idle, before RecyclerView sets the position it jumps to. So
		// the view stops first, and the jump checks for a newer request before it sets its position.
		bool StartScrollToEdge(bool animate)
		{
			var request = ++_scrollRequests;

			// A scroll the app requests, as for ScrollTo: a fling's snap target would snap the view back away from the edge.
			(GetSnapManager()?.GetCurrentSnapHelper() as SingleSnapHelper)?.ResetCurrentTargetPosition();

			// And an adjustment an earlier jump left for its layout would scroll the view on after it.
			_scrollHelper?.CancelPendingAdjustment();

			if (!animate)
			{
				StopScroll();
				if (request != _scrollRequests)
				{
					return false;
				}
			}

			_animatingRequestedScroll |= animate;
			return true;
		}

		// Where the view doesn't scroll to an edge of its content, as in a horizontal layout, the first or last item is as close as
		// it gets: the first or last the adapter shows, which an items source that can be read only once doesn't give again. An
		// animated scroll there starts once the view is laid out, as one to an edge does (see AnimateScrollToEdgeOnceLaidOut).
		void ScrollToEdgeItem(bool end, bool animate)
		{
			var position = FindEdgeItemPosition(end);
			if (position < 0)
			{
				return;
			}

			if (animate)
			{
				ScrollHelper.AnimateScrollOnceLaidOut(() => AnimateScrollToEdgeItem(end));
			}
			else
			{
				ScrollHelper.JumpScrollToPosition(position, end ? Microsoft.Maui.Controls.ScrollToPosition.End : Microsoft.Maui.Controls.ScrollToPosition.Start);
			}
		}

		// The animated scroll to the first or last item that ScrollToStart or ScrollToEnd starts once the view is laid out.
		void AnimateScrollToEdgeItem(bool end)
		{
			var position = FindEdgeItemPosition(end);
			if (position >= 0)
			{
				_animatingRequestedScroll = true;
				ScrollHelper.AnimateScrollToPosition(position, end ? Microsoft.Maui.Controls.ScrollToPosition.End : Microsoft.Maui.Controls.ScrollToPosition.Start);
			}
		}

		// The adapter position of the first or last item, past the header and footer and the headers and footers of groups.
		int FindEdgeItemPosition(bool end)
		{
			if ((ItemsViewAdapter as IItemsViewAdapter)?.ItemsSource is not { } source)
			{
				return -1;
			}

			var groupableSource = source as IGroupableItemsViewSource;
			var count = ItemsViewAdapter.ItemCount;
			for (var index = 0; index < count; index++)
			{
				var position = end ? count - 1 - index : index;
				if (!source.IsHeader(position) && !source.IsFooter(position)
					&& groupableSource?.IsGroupHeader(position) != true && groupableSource?.IsGroupFooter(position) != true)
				{
					return position;
				}
			}

			return -1;
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);

			// When the RecyclerView's size changes (e.g., after an orientation change while
			// the CollectionView was not visible), we need to invalidate all visible item views
			// to ensure they are re-measured with the new dimensions.
			if (oldw > 0 && oldh > 0 && (Math.Abs(w - oldw) > 1 || Math.Abs(h - oldh) > 1))
			{
				InvalidateItemMeasures();
			}
		}

		void InvalidateItemMeasures()
		{
			// Clear the adapter's static size cache (used by MeasureFirstItem strategy)
			ItemsViewAdapter?.ClearMeasureCache();

			// Force all visible children to invalidate their cached sizes and re-measure
			for (int i = 0; i < ChildCount; i++)
			{
				if (GetChildAt(i) is ItemContentView itemContentView)
				{
					itemContentView.InvalidateCachedSize();
					itemContentView.ForceLayout();
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_parentScrollGestureDispatcher?.Dispose();
				_parentScrollGestureDispatcher = null;
			}

			base.Dispose(disposing);
			if (disposing)
			{
				TearDownOldElement(ItemsView);
			}
		}

		internal ScrollHelper ScrollHelper => _scrollHelper ??= new ScrollHelper(this);

		bool CanHandleOwnScrollDirection => this is not MauiCarouselRecyclerView carouselRecyclerView || carouselRecyclerView.IsSwipeEnabled;

		class ParentScrollGestureDispatcher : IDisposable
		{
			readonly MauiRecyclerView<TItemsView, TAdapter, TItemsViewSource> _owner;
			readonly DescendantDisallowInterceptListener _disallowInterceptListener;
			readonly int[] _targetLocation = new int[2];
			MotionEvent _downEvent;
			AView _parentScrollTarget;
			float _touchStartX;
			float _touchStartY;
			int? _scaledTouchSlop;
			bool _descendantDisallowedIntercept;
			GestureOwner _gestureOwner;

			public ParentScrollGestureDispatcher(MauiRecyclerView<TItemsView, TAdapter, TItemsViewSource> owner)
			{
				_owner = owner;
				_disallowInterceptListener = new DescendantDisallowInterceptListener(this);
				_owner.AddOnItemTouchListener(_disallowInterceptListener);
			}

			public bool TryDispatchToParent(MotionEvent e, Func<MotionEvent, bool> dispatchToRecyclerView, out bool handled)
			{
				handled = false;

				if (_gestureOwner == GestureOwner.Parent)
				{
					ForwardToParent(e);

					if (IsTouchEnd(e))
					{
						Reset();
					}

					handled = true;
					return true;
				}

				if (_gestureOwner == GestureOwner.RecyclerView)
				{
					if (IsTouchEnd(e))
					{
						Reset();
					}

					return false;
				}

				switch (e.ActionMasked)
				{
					case MotionEventActions.Down:
						TrackDown(e);
						return false;
					case MotionEventActions.Move:
						return TryStartForwardingToParent(e, dispatchToRecyclerView, out handled);
					case MotionEventActions.Up:
					case MotionEventActions.Cancel:
						Reset();
						return false;
				}

				return false;
			}

			public void Dispose()
			{
				_owner.RemoveOnItemTouchListener(_disallowInterceptListener);
				_disallowInterceptListener.Dispose();
				Reset();
			}

			public void RequestDisallowInterceptTouchEvent(bool disallowIntercept)
			{
				if (_gestureOwner != GestureOwner.Parent)
				{
					_descendantDisallowedIntercept = disallowIntercept;
				}
			}

			void TrackDown(MotionEvent e)
			{
				Reset();
				_touchStartX = e.RawX;
				_touchStartY = e.RawY;
				_downEvent = MotionEvent.Obtain(e);
				_owner.Parent?.RequestDisallowInterceptTouchEvent(false);
			}

			bool TryStartForwardingToParent(MotionEvent e, Func<MotionEvent, bool> dispatchToRecyclerView, out bool handled)
			{
				handled = false;

				var layoutManager = _owner.GetLayoutManager();

				if (layoutManager is null)
				{
					return false;
				}

				var canScrollHorizontally = layoutManager.CanScrollHorizontally();
				var canScrollVertically = layoutManager.CanScrollVertically();

				if (canScrollHorizontally == canScrollVertically)
				{
					return false;
				}

				var deltaX = Math.Abs(e.RawX - _touchStartX);
				var deltaY = Math.Abs(e.RawY - _touchStartY);

				if (deltaX < ScaledTouchSlop && deltaY < ScaledTouchSlop)
				{
					return false;
				}

				var movesInOwnScrollDirection = canScrollHorizontally
					? deltaX >= deltaY
					: deltaY >= deltaX;

				if (movesInOwnScrollDirection)
				{
					_gestureOwner = GestureOwner.RecyclerView;
					_owner.Parent?.RequestDisallowInterceptTouchEvent(_owner.CanHandleOwnScrollDirection);
					return false;
				}

				var target = FindParentScrollTarget(e, canScrollHorizontally);

				if (target is null)
				{
					return false;
				}

				var recyclerViewHandled = dispatchToRecyclerView(e);

				if (_descendantDisallowedIntercept)
				{
					handled = recyclerViewHandled;
					return true;
				}

				_parentScrollTarget = target;
				_gestureOwner = GestureOwner.Parent;
				_owner.Parent?.RequestDisallowInterceptTouchEvent(false);
				CancelRecyclerViewGesture(e, dispatchToRecyclerView);

				if (_downEvent is not null)
				{
					ForwardToParent(_downEvent);
				}

				ForwardToParent(e);
				handled = true;
				return true;
			}

			AView FindParentScrollTarget(MotionEvent e, bool recyclerViewScrollsHorizontally)
			{
				var scrollDirection = recyclerViewScrollsHorizontally
					? Math.Sign(_touchStartY - e.RawY)
					: Math.Sign(_touchStartX - e.RawX);

				if (scrollDirection == 0)
				{
					return null;
				}

				var parent = _owner.Parent;

				while (parent is not null)
				{
					if (parent is AView view)
					{
						var canScroll = recyclerViewScrollsHorizontally
							? view.CanScrollVertically(scrollDirection)
							: view.CanScrollHorizontally(scrollDirection);

						if (canScroll)
						{
							return view;
						}
					}

					parent = parent.GetParent();
				}

				return null;
			}

			void CancelRecyclerViewGesture(MotionEvent e, Func<MotionEvent, bool> dispatchToRecyclerView)
			{
				var cancelEvent = MotionEvent.Obtain(e);
				cancelEvent.Action = MotionEventActions.Cancel;

				try
				{
					dispatchToRecyclerView(cancelEvent);
				}
				finally
				{
					cancelEvent.Recycle();
				}
			}

			void ForwardToParent(MotionEvent source)
			{
				if (_parentScrollTarget is null)
				{
					return;
				}

				var targetEvent = MotionEvent.Obtain(source);
				_parentScrollTarget.GetLocationOnScreen(_targetLocation);
				targetEvent.SetLocation(source.RawX - _targetLocation[0], source.RawY - _targetLocation[1]);

				try
				{
					_parentScrollTarget.OnTouchEvent(targetEvent);
				}
				finally
				{
					targetEvent.Recycle();
				}
			}

			void Reset()
			{
				_owner.Parent?.RequestDisallowInterceptTouchEvent(false);
				_parentScrollTarget = null;
				_descendantDisallowedIntercept = false;
				_gestureOwner = GestureOwner.Undecided;

				if (_downEvent is not null)
				{
					_downEvent.Recycle();
					_downEvent = null;
				}
			}

			int ScaledTouchSlop => _scaledTouchSlop ??= ViewConfiguration.Get(_owner.Context).ScaledTouchSlop;

			static bool IsTouchEnd(MotionEvent e) =>
				e.ActionMasked == MotionEventActions.Up || e.ActionMasked == MotionEventActions.Cancel;

			enum GestureOwner
			{
				Undecided,
				RecyclerView,
				Parent
			}

			sealed class DescendantDisallowInterceptListener : Java.Lang.Object, RecyclerView.IOnItemTouchListener
			{
				readonly ParentScrollGestureDispatcher _dispatcher;

				public DescendantDisallowInterceptListener(ParentScrollGestureDispatcher dispatcher)
				{
					_dispatcher = dispatcher;
				}

				public bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
				{
					return false;
				}

				public void OnTouchEvent(RecyclerView rv, MotionEvent e)
				{
				}

				public void OnRequestDisallowInterceptTouchEvent(bool disallowIntercept)
				{
					_dispatcher.RequestDisallowInterceptTouchEvent(disallowIntercept);
				}
			}
		}

		internal void UpdateEmptyViewVisibility()
		{
			if (ItemsViewAdapter == null)
			{
				return;
			}

			int itemCount = 0;
			if (ItemsView is StructuredItemsView itemsView)
			{
				if (itemsView.Header != null || itemsView.HeaderTemplate != null)
					itemCount++;
				if (itemsView.Footer != null || itemsView.FooterTemplate != null)
					itemCount++;
			}

			var sourceIsEmpty = ItemsView is GroupableItemsView { IsGrouped: true }
				&& ItemsViewAdapter.ItemsSource is ObservableGroupedSource groupedSource
					? groupedSource.GroupCount == 0
					: ItemsViewAdapter.ItemCount == itemCount;
			var showEmptyView = (ItemsView?.EmptyView is not null || ItemsView?.EmptyViewTemplate is not null) && sourceIsEmpty;

			var currentAdapter = GetAdapter();
			if (showEmptyView && currentAdapter != _emptyViewAdapter)
			{
				GetRecycledViewPool().Clear();
				SwapAdapter(_emptyViewAdapter, true);

				// TODO hartez 2018/10/24 17:34:36 If this works, cache this layout manager as _emptyLayoutManager	
				SetLayoutManager(SelectLayoutManager(ItemsLayout));
				UpdateEmptyView();
			}
			else if (!showEmptyView && currentAdapter != ItemsViewAdapter)
			{
				GetRecycledViewPool().Clear();
				SwapAdapter(ItemsViewAdapter, true);
				UpdateLayoutManager();
			}
			else if (showEmptyView && currentAdapter == _emptyViewAdapter)
			{
				if (ShouldUpdateEmptyView())
				{
					// Header/footer properties changed - detach and reattach adapter to force RecyclerView to recalculate the positions.
					SetAdapter(null);
					SwapAdapter(_emptyViewAdapter, true);
					UpdateEmptyView();
				}
			}
		}

		bool ShouldUpdateEmptyView()
		{
			if (ItemsView is StructuredItemsView structuredItemsView)
			{
				if (_emptyViewAdapter.Header != structuredItemsView.Header ||
					_emptyViewAdapter.HeaderTemplate != structuredItemsView.HeaderTemplate ||
					_emptyViewAdapter.Footer != structuredItemsView.Footer ||
					_emptyViewAdapter.FooterTemplate != structuredItemsView.FooterTemplate ||
					_emptyViewAdapter.EmptyView != ItemsView.EmptyView ||
					_emptyViewAdapter.EmptyViewTemplate != ItemsView.EmptyViewTemplate)
				{
					return true;
				}
			}

			return false;
		}

		internal void AdjustScrollForItemUpdate(bool itemsInserted)
		{
			// While the view follows an edge (ScrollAnchorMode), the anchor keeps it there instead.
			if (_scrollAnchor?.IsFollowingEdge == true)
			{
				return;
			}

			if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
			{
				var source = ItemsViewAdapter.ItemsSource;
				int itemCount = source.Count - (source.HasHeader ? 1 : 0) - (source.HasFooter ? 1 : 0);

				if (source is ObservableGroupedSource groupedSource)
				{
					itemCount = 0;
					for (int groupIndex = 0; groupIndex < groupedSource.GroupCount; groupIndex++)
					{
						var group = groupedSource.GetGroupItemsViewSource(groupIndex);
						itemCount += group.Count - (group.HasHeader ? 1 : 0) - (group.HasFooter ? 1 : 0);
					}
				}

				if (itemCount > 0)
				{
					// A negative group index selects the flat data-index path without an item lookup.
					ScrollTo(new ScrollToRequestEventArgs(itemCount - 1, -1,
						Microsoft.Maui.Controls.ScrollToPosition.MakeVisible, true));
				}
			}
			else if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepScrollOffset)
			{
				ScrollHelper.UndoNextScrollAdjustment();
			}
			else if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView
				&& itemsInserted && ItemsView is not CarouselView)
			{
				if (GetLayoutManager().ItemCount > 0)
				{
					ScrollTo(new ScrollToRequestEventArgs(0, -1,
						Microsoft.Maui.Controls.ScrollToPosition.Start, false));
				}
			}
		}

		GridLayoutManager CreateGridLayout(GridItemsLayout gridItemsLayout)
		{
			var gridLayoutManager = new GridLayoutManager(Context, gridItemsLayout.Span,
				gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? LinearLayoutManager.Horizontal
					: LinearLayoutManager.Vertical,
				false);

			// Give the layout a way to determine that headers/footers span multiple rows/columns
			gridLayoutManager.SetSpanSizeLookup(new GridLayoutSpanSizeLookup(gridItemsLayout, this));

			return gridLayoutManager;
		}

		void AddOrUpdateScrollListener()
		{
			RemoveScrollListener();

			RecyclerViewScrollListener = CreateScrollListener();
			AddOnScrollListener(RecyclerViewScrollListener);
		}

		void RemoveScrollListener()
		{
			if (RecyclerViewScrollListener == null)
				return;

			RemoveOnScrollListener(RecyclerViewScrollListener);
			RecyclerViewScrollListener.Dispose();
			RecyclerViewScrollListener = null;
		}
	}
}
