using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Xamarin.Forms.Platform.Android.CollectionView;
using FormsCarouselView = Xamarin.Forms.CarouselView;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselViewRenderer : ItemsViewRenderer<ItemsView, ItemsViewAdapter<ItemsView, IItemsViewSource>, IItemsViewSource>
	{
		protected FormsCarouselView Carousel => ItemsView as FormsCarouselView;
		ItemDecoration _itemDecoration;
		CarouselViewLoopManager _carouselViewLoopManager;
		bool _isSwipeEnabled;
		int _oldPosition;
		int _gotoPosition = -1;
		bool _noNeedForScroll;
		bool _initialized;
		bool _isVisible;
		bool _disposed;

		List<View> _oldViews;
		CarouselViewwOnGlobalLayoutListener _carouselViewLayoutListener;

		public CarouselViewRenderer(Context context) : base(context)
		{
			_oldViews = new List<View>();
			_carouselViewLoopManager = new CarouselViewLoopManager();
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (!_isSwipeEnabled)
				return false;

			return base.OnInterceptTouchEvent(ev);
		}

		protected virtual bool IsHorizontal => (Carousel?.ItemsLayout)?.Orientation == ItemsLayoutOrientation.Horizontal;

		protected override int DetermineTargetPosition(ScrollToRequestEventArgs args)
		{
			if (args.Mode == ScrollToMode.Element)
				return ItemsViewAdapter.GetPositionForItem(args.Item);

			if (!Carousel.Loop)
				return args.Index;

			if (_carouselViewLoopManager == null)
				return -1;

			var carouselPosition = GetCarouselViewCurrentIndex(Carousel.Position);
			var getGoIndex = _carouselViewLoopManager.GetGoToIndex(this, carouselPosition, args.Index);

			return getGoIndex;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (Carousel.Loop)
				_carouselViewLoopManager.CenterIfNeeded(this, IsHorizontal);

			return base.OnTouchEvent(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				_carouselViewLoopManager?.SetItemsSource(null);
				_carouselViewLoopManager = null;

				if (_itemDecoration != null)
				{
					_itemDecoration.Dispose();
					_itemDecoration = null;
				}

				ClearLayoutListener();
			}

			base.Dispose(disposing);
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			base.SetUpNewElement(newElement);

			if (newElement == null)
				return;

			AddLayoutListener();
			UpdateIsSwipeEnabled();
			UpdateIsBounceEnabled();
			UpdateItemSpacing();
			UpdateInitialPosition();
		}

		protected override RecyclerViewScrollListener<ItemsView, IItemsViewSource> CreateScrollListener()
		{
			return new CarouselViewOnScrollListener(ItemsView, ItemsViewAdapter, _carouselViewLoopManager);
		}

		protected override void TearDownOldElement(ItemsView oldElement)
		{
			if (Carousel != null)
				Carousel.Scrolled -= CarouselViewScrolled;

			ClearLayoutListener();
			base.TearDownOldElement(oldElement);
		}

		protected override void UpdateAdapter()
		{
			// By default the CollectionViewAdapter creates the items at whatever size the template calls for
			// But for the Carousel, we want it to create the items to fit the width/height of the viewport
			// So we give it an alternate delegate for creating the views

			var oldItemViewAdapter = ItemsViewAdapter;
			UnsubscribeCollectionItemsSourceChanged(oldItemViewAdapter);
			if (oldItemViewAdapter != null)
			{
				Carousel.SetValueFromRenderer(FormsCarouselView.PositionProperty, 0);
				Carousel.SetValueFromRenderer(FormsCarouselView.CurrentItemProperty, null);
			}

			ItemsViewAdapter = new CarouselViewAdapter<ItemsView, IItemsViewSource>(Carousel,
				(view, context) => new SizedItemContentView(Context, GetItemWidth, GetItemHeight));

			_gotoPosition = -1;

			SwapAdapter(ItemsViewAdapter, false);

			UpdateInitialPosition();

			if (ItemsViewAdapter?.ItemsSource is ObservableItemsSource observableItemsSource)
				observableItemsSource.CollectionItemsSourceChanged += CollectionItemsSourceChanged;

			oldItemViewAdapter?.Dispose();
		}

		protected override void UpdateItemsSource()
		{
			UpdateAdapter();
			UpdateEmptyView();
			_carouselViewLoopManager.SetItemsSource(ItemsViewAdapter.ItemsSource);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(FormsCarouselView.PeekAreaInsetsProperty))
				UpdatePeekAreaInsets();
			else if (changedProperty.Is(FormsCarouselView.IsSwipeEnabledProperty))
				UpdateIsSwipeEnabled();
			else if (changedProperty.Is(FormsCarouselView.IsBounceEnabledProperty))
				UpdateIsBounceEnabled();
			else if (changedProperty.Is(LinearItemsLayout.ItemSpacingProperty))
				UpdateItemSpacing();
			else if (changedProperty.Is(FormsCarouselView.PositionProperty))
				UpdateFromPosition();
			else if (changedProperty.Is(FormsCarouselView.CurrentItemProperty))
				UpdateFromCurrentItem();
			else if (changedProperty.Is(FormsCarouselView.LoopProperty))
				UpdateAdapter();
		}

		protected override ItemDecoration CreateSpacingDecoration(IItemsLayout itemsLayout)
		{
			return new CarouselSpacingItemDecoration(itemsLayout, Carousel);
		}

		protected override void UpdateItemSpacing()
		{
			if (ItemsLayout == null)
			{
				return;
			}

			UpdateItemDecoration();

			var adapter = GetAdapter();

			if (adapter != null)
			{
				adapter.NotifyItemChanged(_oldPosition);
			}

			base.UpdateItemSpacing();
		}

		protected override IItemsLayout GetItemsLayout() => Carousel.ItemsLayout;

		protected override void ScrollTo(ScrollToRequestEventArgs args)
		{
			var position = DetermineTargetPosition(args);

			if (_carouselViewLoopManager == null)
				return;
			//Special case here
			//We could have a race condition where we are scrolling our collection to center the first item
			//And at the same time the user is requesting we go to a particular item
			if (position == -1 && Carousel.Loop)
			{
				_carouselViewLoopManager.AddPendingScrollTo(args);
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

		int GetItemWidth()
		{
			var itemWidth = Width;

			if (ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				itemWidth = (int)(Width - Context?.ToPixels(Carousel.PeekAreaInsets.Left) - Context?.ToPixels(Carousel.PeekAreaInsets.Right) - Context?.ToPixels(listItemsLayout.ItemSpacing));

			return itemWidth;
		}

		int GetItemHeight()
		{
			var itemHeight = Height;

			if (ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				itemHeight = (int)(Height - Context?.ToPixels(Carousel.PeekAreaInsets.Top) - Context?.ToPixels(Carousel.PeekAreaInsets.Bottom) - Context?.ToPixels(listItemsLayout.ItemSpacing));

			return itemHeight;
		}

		void UpdateIsSwipeEnabled() => _isSwipeEnabled = Carousel?.IsSwipeEnabled ?? false;

		void UpdateIsBounceEnabled() => OverScrollMode = Carousel?.IsBounceEnabled == true ? OverScrollMode.Always : OverScrollMode.Never;

		void UpdatePeekAreaInsets() => UpdateAdapter();

		void UnsubscribeCollectionItemsSourceChanged(ItemsViewAdapter<ItemsView, IItemsViewSource> oldItemViewAdapter)
		{
			if (oldItemViewAdapter?.ItemsSource is ObservableItemsSource oldObservableItemsSource)
				oldObservableItemsSource.CollectionItemsSourceChanged -= CollectionItemsSourceChanged;
		}

		void CollectionItemsSourceChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (!(ItemsViewAdapter?.ItemsSource is IItemsViewSource observableItemsSource))
				return;

			var carouselPosition = Carousel.Position;
			var currentItemPosition = observableItemsSource.GetPosition(Carousel.CurrentItem);
			var count = observableItemsSource.Count;

			bool removingCurrentElement = currentItemPosition == -1;
			bool removingLastElement = e.OldStartingIndex == count;
			bool removingFirstElement = e.OldStartingIndex == 0;

			_noNeedForScroll = true;
			_gotoPosition = -1;

			if (removingCurrentElement)
			{
				if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
				{
					carouselPosition = 0;
				}

				if (removingFirstElement)
					carouselPosition = 0;
				else if (removingLastElement)
					carouselPosition = Carousel.Position - 1;

				if (Carousel.Loop)
				{
					UpdateAdapter();
					ScrollToPosition(carouselPosition);
				}
			}
			//If we are adding a new item make sure to maintain the CurrentItemPosition
			else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
				&& currentItemPosition != -1)
			{
				carouselPosition = currentItemPosition;
			}

			if (!Carousel.Loop)
			{
				SetCurrentItem(carouselPosition);
				UpdatePosition(carouselPosition);
			}

			//If we are adding or removing the last item we need to update
			//the inset that we give to items so they are centered
			if (e.NewStartingIndex == count - 1 || removingLastElement)
			{
				UpdateItemDecoration();
			}

			UpdateVisualStates();
		}

		void UpdateItemDecoration()
		{
			if (_itemDecoration != null)
				RemoveItemDecoration(_itemDecoration);
			_itemDecoration = CreateSpacingDecoration(ItemsLayout);
			AddItemDecoration(_itemDecoration);
		}

		void UpdateInitialPosition()
		{
			int position = 0;
			var items = Carousel.ItemsSource as IList;
			var itemCount = items?.Count ?? 0;

			if (Carousel.CurrentItem != null || items == null)
			{
				for (int n = 0; n < itemCount; n++)
				{
					if (items[n] == Carousel.CurrentItem)
					{
						position = n;
						break;
					}
				}

				Carousel.Position = position;
			}
			else
				position = Carousel.Position;

			_oldPosition = position;

			SetCurrentItem(_oldPosition);

			var index = Carousel.Loop ? itemCount * 5000 + _oldPosition : _oldPosition;
			ScrollHelper.JumpScrollToPosition(index, Xamarin.Forms.ScrollToPosition.Center);
		}

		void UpdatePositionFromVisibilityChanges()
		{
			if (_isVisible != Carousel.IsVisible)
				UpdateInitialPosition();

			_isVisible = Carousel.IsVisible;
		}

		void UpdateVisualStates()
		{
			if (!(GetLayoutManager() is LinearLayoutManager layoutManager))
				return;

			var first = layoutManager.FindFirstVisibleItemPosition();
			var last = layoutManager.FindLastVisibleItemPosition();


			if (first == -1)
				return;

			var newViews = new List<View>();
			var carouselPosition = this.CalculateCenterItemIndex(first, layoutManager, false);
			var previousPosition = carouselPosition - 1;
			var nextPosition = carouselPosition + 1;

			for (int i = first; i <= last; i++)
			{
				var cell = layoutManager.FindViewByPosition(i);
				if (!((cell as ItemContentView)?.VisualElementRenderer?.Element is View itemView))
					return;

				if (i == carouselPosition)
				{
					VisualStateManager.GoToState(itemView, FormsCarouselView.CurrentItemVisualState);
				}
				else if (i == previousPosition)
				{
					VisualStateManager.GoToState(itemView, FormsCarouselView.PreviousItemVisualState);
				}
				else if (i == nextPosition)
				{
					VisualStateManager.GoToState(itemView, FormsCarouselView.NextItemVisualState);
				}
				else
				{
					VisualStateManager.GoToState(itemView, FormsCarouselView.DefaultItemVisualState);
				}

				newViews.Add(itemView);

				if (!Carousel.VisibleViews.Contains(itemView))
				{
					Carousel.VisibleViews.Add(itemView);
				}
			}

			foreach (var itemView in _oldViews)
			{
				if (!newViews.Contains(itemView))
				{
					VisualStateManager.GoToState(itemView, FormsCarouselView.DefaultItemVisualState);
					if (Carousel.VisibleViews.Contains(itemView))
					{
						Carousel.VisibleViews.Remove(itemView);
					}
				}
			}

			_oldViews = newViews;
		}

		void CarouselViewScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			if (!_initialized || !_isVisible)
				return;

			_noNeedForScroll = false;
			var index = e.CenterItemIndex;
			if (Carousel?.Loop == true)
			{
				index = GetCarouselViewCurrentIndex(index);
			}

			if (index == -1)
				return;

			UpdatePosition(index);
			UpdateVisualStates();
		}

		int GetCarouselViewCurrentIndex(int index)
		{
			var centeredView = this.GetCenteredView();

			if (centeredView is ItemContentView templatedCell)
			{
				var bContext = templatedCell?.Element?.BindingContext;
				index = ItemsViewAdapter.GetPositionForItem(bContext);
			}
			else
			{
				return -1;
			}

			return index;
		}

		void UpdatePosition(int position)
		{
			var carouselPosition = Carousel.Position;

			// We arrived center
			if (position == _gotoPosition)
				_gotoPosition = -1;

			if (_gotoPosition == -1 && carouselPosition != position)
				Carousel.SetValueFromRenderer(FormsCarouselView.PositionProperty, position);
		}

		void SetCurrentItem(int carouselPosition)
		{
			if (ItemsViewAdapter?.ItemsSource?.Count == 0)
				return;

			var item = ItemsViewAdapter.ItemsSource.GetItem(carouselPosition);
			Carousel.SetValueFromRenderer(FormsCarouselView.CurrentItemProperty, item);
		}

		void UpdateFromCurrentItem()
		{
			var currentItemPosition = ItemsViewAdapter.ItemsSource.GetPosition(Carousel.CurrentItem);
			var carouselPosition = Carousel.Position;

			if (_gotoPosition == -1 && currentItemPosition != carouselPosition)
			{
				_gotoPosition = currentItemPosition;
				Carousel.ScrollTo(currentItemPosition, position: Xamarin.Forms.ScrollToPosition.Center, animate: Carousel.AnimateCurrentItemChanges);
			}
		}

		void UpdateFromPosition()
		{
			if (!_initialized)
			{
				_carouselViewLoopManager.AddPendingScrollTo(new ScrollToRequestEventArgs(Carousel.Position, -1, Xamarin.Forms.ScrollToPosition.Center, false));
			}

			var itemCount = ItemsViewAdapter?.ItemsSource.Count ?? 0;
			var carouselPosition = Carousel.Position;

			if (itemCount == 0)
			{
				//we are trying to set a position but our Collection doesn't have items still
				_oldPosition = carouselPosition;
				return;
			}


			if (carouselPosition >= itemCount || carouselPosition < 0)
				throw new IndexOutOfRangeException($"Can't set CarouselView to position {carouselPosition}. ItemsSource has {itemCount} items.");

			if (carouselPosition == _gotoPosition)
				_gotoPosition = -1;

			if (_noNeedForScroll)
			{
				_noNeedForScroll = false;
				return;
			}

			var centerPosition = GetCarouselViewCurrentIndex(carouselPosition);
			if (_gotoPosition == -1 && !Carousel.IsDragging && !Carousel.IsScrolling && centerPosition != carouselPosition)
			{
				_gotoPosition = carouselPosition;

				Carousel.ScrollTo(carouselPosition, position: Xamarin.Forms.ScrollToPosition.Center, animate: Carousel.AnimatePositionChanges);
			}
			SetCurrentItem(carouselPosition);
		}

		void AddLayoutListener()
		{
			if (_carouselViewLayoutListener != null)
				return;

			_carouselViewLayoutListener = new CarouselViewwOnGlobalLayoutListener();
			_carouselViewLayoutListener.LayoutReady += LayoutReady;

			ViewTreeObserver.AddOnGlobalLayoutListener(_carouselViewLayoutListener);
		}

		void LayoutReady(object sender, EventArgs e)
		{
			if (!_initialized)
			{
				Carousel.Scrolled += CarouselViewScrolled;
				if (Carousel.Loop)
				{
					_carouselViewLoopManager.CenterIfNeeded(this, IsHorizontal);
					_carouselViewLoopManager.CheckPendingScrollToEvents(this);
				}
				_initialized = true;
				_isVisible = Carousel.IsVisible;
			}

			UpdatePositionFromVisibilityChanges();
			UpdateVisualStates();
		}

		void ClearLayoutListener()
		{
			if (_carouselViewLayoutListener == null)
				return;

			ViewTreeObserver?.RemoveOnGlobalLayoutListener(_carouselViewLayoutListener);
			_carouselViewLayoutListener.LayoutReady -= LayoutReady;
			_carouselViewLayoutListener = null;
		}

		class CarouselViewOnScrollListener : RecyclerViewScrollListener<ItemsView, IItemsViewSource>
		{
			readonly FormsCarouselView _carouselView;
			readonly CarouselViewLoopManager _carouselViewLoopManager;

			public CarouselViewOnScrollListener(ItemsView itemsView, ItemsViewAdapter<ItemsView, IItemsViewSource> itemsViewAdapter, CarouselViewLoopManager carouselViewLoopManager) : base(itemsView, itemsViewAdapter, true)
			{
				_carouselView = itemsView as FormsCarouselView;
				_carouselViewLoopManager = carouselViewLoopManager;
			}

			public override void OnScrollStateChanged(RecyclerView recyclerView, int state)
			{
				base.OnScrollStateChanged(recyclerView, state);

				if (_carouselView.IsSwipeEnabled)
				{
					if (state == ScrollStateDragging)
						_carouselView.SetIsDragging(true);
					else
						_carouselView.SetIsDragging(false);
				}

				_carouselView.IsScrolling = state != ScrollStateIdle;
			}

			public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
			{
				base.OnScrolled(recyclerView, dx, dy);

				if (_carouselView.Loop)
				{
					//We could have a race condition where we are scrolling our collection to center the first item
					//We save that ScrollToEventARgs and call it again
					_carouselViewLoopManager.CheckPendingScrollToEvents(recyclerView);
				}
			}

			protected override (int First, int Center, int Last) GetVisibleItemsIndex(RecyclerView recyclerView)
			{
				var firstVisibleItemIndex = -1;
				var lastVisibleItemIndex = -1;
				var centerItemIndex = -1;

				if (recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager)
				{
					var firstView = recyclerView.FindViewHolderForAdapterPosition(linearLayoutManager.FindFirstVisibleItemPosition());
					var lastView = recyclerView.FindViewHolderForAdapterPosition(linearLayoutManager.FindLastVisibleItemPosition());
					var centerView = recyclerView.GetCenteredView();
					firstVisibleItemIndex = GetIndexFromTemplatedCell(firstView.ItemView);
					lastVisibleItemIndex = GetIndexFromTemplatedCell(lastView.ItemView);
					centerItemIndex = GetIndexFromTemplatedCell(centerView);
				}

				return (firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
			}

			int GetIndexFromTemplatedCell(global::Android.Views.View view)
			{
				int itemIndex = -1;

				if (view is ItemContentView templatedCell)
				{
					var bContext = templatedCell?.Element?.BindingContext;
					itemIndex = ItemsViewAdapter.GetPositionForItem(bContext);
				}

				return itemIndex;
			}
		}

		class CarouselViewwOnGlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
		{
			public EventHandler<EventArgs> LayoutReady;
			public void OnGlobalLayout()
			{
				LayoutReady?.Invoke(this, new EventArgs());
			}
		}

		class CarouselViewLoopManager
		{
			IItemsViewSource _itemsSource;
			readonly Queue<ScrollToRequestEventArgs> _pendingScrollTo = new Queue<ScrollToRequestEventArgs>();

			public void CenterIfNeeded(RecyclerView recyclerView, bool isHorizontal)
			{
				if (!(recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager))
					return;

				var itemSourceCount = _itemsSource.Count;

				var firstCompletelyItemVisible = linearLayoutManager.FindFirstCompletelyVisibleItemPosition();

				var offSet = recyclerView.ComputeHorizontalScrollOffset();

				if (!isHorizontal)
					offSet = recyclerView.ComputeVerticalScrollOffset();

				if (firstCompletelyItemVisible == 0)
					linearLayoutManager.ScrollToPositionWithOffset(itemSourceCount, -offSet);
			}

			public void CheckPendingScrollToEvents(RecyclerView recyclerView)
			{
				if (!(recyclerView is CarouselViewRenderer carouselViewRenderer))
					return;

				if (_pendingScrollTo.TryDequeue(out ScrollToRequestEventArgs scrollToRequestEventArgs))
					carouselViewRenderer.ScrollTo(scrollToRequestEventArgs);
			}

			public void AddPendingScrollTo(ScrollToRequestEventArgs args) => _pendingScrollTo.Enqueue(args);

			public int GetGoToIndex(RecyclerView recyclerView, int carouselPosition, int newPosition)
			{
				if (!(recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager))
					return -1;

				var currentCarouselPosition = carouselPosition;
				var itemSourceCount = _itemsSource.Count;

				var diffToStart = currentCarouselPosition + (itemSourceCount - newPosition);
				var diffToEnd = itemSourceCount - currentCarouselPosition + newPosition;
				var centerView = recyclerView.GetCenteredView();

				if (centerView == null)
					return -1;

				var centerPosition = linearLayoutManager.GetPosition(centerView);
				var increment = currentCarouselPosition - newPosition;
				var incrementAbs = System.Math.Abs(increment);

				int goToPosition;
				if (diffToStart < incrementAbs)
					goToPosition = centerPosition - diffToStart;
				else if (diffToEnd < incrementAbs)
					goToPosition = centerPosition + diffToEnd;
				else
					goToPosition = centerPosition - increment;

				return goToPosition;
			}

			public void SetItemsSource(IItemsViewSource itemsSource) => _itemsSource = itemsSource;
		}
	}
}