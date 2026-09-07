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

	public class MauiRecyclerView<TItemsView, TAdapter, TItemsViewSource> : RecyclerView, IMauiRecyclerView<TItemsView>, IMauiRecyclerView
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

		~MauiRecyclerView() => _layoutPropertyChangedProxy?.Unsubscribe();

		public MauiRecyclerView(Context context, Func<IItemsLayout> getItemsLayout, Func<TAdapter> getAdapter) : base(new ContextThemeWrapper(context, Resource.Style.collectionViewTheme))
		{
			_getItemsLayout = getItemsLayout ?? throw new ArgumentNullException(nameof(getItemsLayout));
			CreateAdapter = getAdapter ?? throw new ArgumentNullException(nameof(getAdapter));

			_emptyCollectionObserver = new DataChangeObserver(UpdateEmptyViewVisibility);
			_itemsUpdateScrollObserver = new DataChangeObserver(AdjustScrollForItemUpdate);
			_dispatchTouchEventToRecyclerView = DispatchTouchEventToRecyclerView;
			_parentScrollGestureDispatcher = new ParentScrollGestureDispatcher(this);
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

			RemoveScrollListener();

			if (ItemsViewAdapter != null)
			{
				// Stop watching for empty items or scroll adjustments
				_emptyCollectionObserver.Stop(ItemsViewAdapter);
				_itemsUpdateScrollObserver.Stop(ItemsViewAdapter);

				// Unhook whichever adapter is active
				SetAdapter(null);

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

			this.UpdateFlowDirection(ItemsView);

			ReconcileFlowDirectionAndLayout();
		}

		public virtual void UpdateAdapter()
		{
			var oldItemViewAdapter = ItemsViewAdapter;

			_emptyCollectionObserver.Stop(oldItemViewAdapter);

			ItemsViewAdapter = CreateAdapter();

			(RecyclerViewScrollListener as RecyclerViewScrollListener<TItemsView, TItemsViewSource>)?.UpdateAdapter(ItemsViewAdapter);

			if (GetAdapter() != _emptyViewAdapter)
			{
				_itemsUpdateScrollObserver.Stop(oldItemViewAdapter);

				SetAdapter(null);

				SwapAdapter(ItemsViewAdapter, true);
			}

			UpdateEmptyView();

			_itemTouchHelperCallback?.SetAdapter(ItemsViewAdapter as IItemTouchHelperAdapter);

			oldItemViewAdapter?.Dispose();
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

			if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView)
			{
				// Keeping the current items in view is the default, so we don't need to watch for data changes
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

			if (args.IsAnimated)
			{
				ScrollHelper.AnimateScrollToPosition(position, args.ScrollToPosition);
			}
			else
			{
				ScrollHelper.JumpScrollToPosition(position, args.ScrollToPosition);
			}
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

					item = FindBoundItemInGroup(args, groupItemSource);

					if (item is null)
					{
						return InvalidPosition;
					}
				}
			}

			return ItemsViewAdapter.GetPositionForItem(item);
		}

		private static object FindBoundItemInGroup(ScrollToRequestEventArgs args, IGroupableItemsViewSource groupItemSource)
		{
			var group = groupItemSource.GetGroupItemsViewSource(args.GroupIndex);

			// GetItem calls AdjustIndexRequest, which subtracts 1 if we have a header (UngroupedItemsSource does not do this)
			return group?.GetItem(args.Index + 1);
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
			base.OnLayout(changed, l, t, r, b);
#pragma warning disable CS0618 // Obsolete
			AViewCompat.SetClipBounds(this, new ARect(0, 0, Width, Height));
#pragma warning restore CS0618 // Obsolete

			// After a direct (non-animated) scroll operation, we may need to make adjustments
			// to align the target item; if an adjustment is pending, execute it here.
			// (Deliberately checking the private member here rather than the property accessor; the accessor will
			// create a new ScrollHelper if needed, and there's no reason to do that until a Scroll is requested.)
			_scrollHelper?.AdjustScroll();
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

			var showEmptyView = (ItemsView?.EmptyView is not null || ItemsView?.EmptyViewTemplate is not null) && ItemsViewAdapter.ItemCount == itemCount;

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

		internal void AdjustScrollForItemUpdate()
		{
			if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
			{
				ScrollTo(new ScrollToRequestEventArgs(GetLayoutManager().ItemCount, 0,
					Microsoft.Maui.Controls.ScrollToPosition.MakeVisible, true));
			}
			else if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepScrollOffset)
			{
				ScrollHelper.UndoNextScrollAdjustment();
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
