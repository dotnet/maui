using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
#if __ANDROID_29__
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using Android.Views;
using Java.Interop;
using FormsCarouselView = Xamarin.Forms.CarouselView;
using Xamarin.Forms.Platform.Android.CollectionView;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselViewRenderer : ItemsViewRenderer<ItemsView, ItemsViewAdapter<ItemsView, IItemsViewSource>, IItemsViewSource>
	{
		protected FormsCarouselView Carousel;
		RecyclerView.ItemDecoration _itemDecoration;
		bool _isSwipeEnabled;
		int _oldPosition;
		int _gotoPosition = -1;
		bool _noNeedForScroll;
		bool _initialized;
		bool _isVisible;

		List<View> _oldViews;
		CarouselViewwOnGlobalLayoutListener _carouselViewLayoutListener;

		public CarouselViewRenderer(Context context) : base(context)
		{
			FormsCarouselView.VerifyCarouselViewFlagEnabled(nameof(CarouselViewRenderer));
			_oldViews = new List<View>();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
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
			Carousel = newElement as FormsCarouselView;

			base.SetUpNewElement(newElement);

			if (newElement == null)
				return;

			AddLayoutListener();
			UpdateIsSwipeEnabled();
			UpdateIsBounceEnabled();
			UpdateItemSpacing();
		}

		protected override RecyclerViewScrollListener<ItemsView, IItemsViewSource> CreateScrollListener()
		{
			return new CarouselViewOnScrollListener(ItemsView, ItemsViewAdapter);
		}

		protected override void TearDownOldElement(ItemsView oldElement)
		{
			if (Carousel != null)
				Carousel.Scrolled -= CarouselViewScrolled;

			ClearLayoutListener();
			base.TearDownOldElement(oldElement);
		}

		protected override void UpdateItemsSource()
		{
			UpdateAdapter();
			UpdateEmptyView();
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
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (!_isSwipeEnabled)
				return false;

			return base.OnInterceptTouchEvent(ev);
		}

		protected override RecyclerView.ItemDecoration CreateSpacingDecoration(IItemsLayout itemsLayout)
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

		protected override IItemsLayout GetItemsLayout()
		{
			return Carousel.ItemsLayout;
		}

		int GetItemWidth()
		{
			var itemWidth = Width;

			if (ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				itemWidth = (int)(Width - Context?.ToPixels(Carousel.PeekAreaInsets.Left) - Context?.ToPixels(Carousel.PeekAreaInsets.Right) - Context?.ToPixels(listItemsLayout.ItemSpacing));
			}

			return itemWidth;
		}

		int GetItemHeight()
		{
			var itemHeight = Height;

			if (ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			{
				itemHeight = (int)(Height - Context?.ToPixels(Carousel.PeekAreaInsets.Top) - Context?.ToPixels(Carousel.PeekAreaInsets.Bottom) - Context?.ToPixels(listItemsLayout.ItemSpacing));
			}

			return itemHeight;
		}

		void UpdateIsSwipeEnabled()
		{
			_isSwipeEnabled = Carousel.IsSwipeEnabled;
		}

		void UpdateIsBounceEnabled()
		{
			OverScrollMode = Carousel.IsBounceEnabled ? OverScrollMode.Always : OverScrollMode.Never;
		}

		void UpdatePeekAreaInsets()
		{
			UpdateAdapter();
		}

		protected override void UpdateAdapter()
		{
			// By default the CollectionViewAdapter creates the items at whatever size the template calls for
			// But for the Carousel, we want it to create the items to fit the width/height of the viewport
			// So we give it an alternate delegate for creating the views

			var oldItemViewAdapter = ItemsViewAdapter;
			UnsubscribeCollectionItemsSourceChanged(oldItemViewAdapter);

			ItemsViewAdapter = new ItemsViewAdapter<ItemsView, IItemsViewSource>(ItemsView, 
				(view, context) => new SizedItemContentView(Context, GetItemWidth, GetItemHeight));

			_gotoPosition = -1;


			SwapAdapter(ItemsViewAdapter, false);

			if (_oldPosition > 0)
				UpdateInitialPosition();

			if (ItemsViewAdapter?.ItemsSource is ObservableItemsSource observableItemsSource)
				observableItemsSource.CollectionItemsSourceChanged += CollectionItemsSourceChanged;

			oldItemViewAdapter?.Dispose();
		}

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
			bool removingCurrentElementButNotFirst = removingCurrentElement && removingLastElement && Carousel.Position > 0;

			if (removingCurrentElementButNotFirst)
			{
				carouselPosition = Carousel.Position - 1;

			}
			else if (removingFirstElement && !removingCurrentElement)
			{
				carouselPosition = currentItemPosition;
				_noNeedForScroll = true;
			}

			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
			{
				carouselPosition = 0;
			}

			//If we are adding a new item make sure to maintain the CurrentItemPosition
			else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
				&& currentItemPosition != -1)
			{
				carouselPosition = currentItemPosition;
				//if we are adding a item and we want to stay on the same position
				//we don't need to scroll
				_noNeedForScroll = true;
			}

			_gotoPosition = -1;

			SetCurrentItem(carouselPosition);
			UpdatePosition(carouselPosition);

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

			if (Carousel.CurrentItem != null)
			{
				var items = Carousel.ItemsSource as IList;

				for (int n = 0; n < items?.Count; n++)
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

			if (_oldPosition > 0)
				_gotoPosition = _oldPosition;

			SetCurrentItem(_oldPosition);
			Carousel.ScrollTo(_oldPosition, position: Xamarin.Forms.ScrollToPosition.Center, animate: Carousel.AnimatePositionChanges);
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
			var carouselPosition = Carousel.Position;
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
			_noNeedForScroll = false;
			UpdatePosition(e.CenterItemIndex);
			UpdateVisualStates();
		}

		void UpdatePosition(int position)
		{
			var carouselPosition = Carousel.Position;

			//we arrived center
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
			var itemCount = ItemsViewAdapter?.ItemsSource.Count;
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

			if (_gotoPosition == -1 && !Carousel.IsDragging && !Carousel.IsScrolling)
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
				UpdateInitialPosition();
				Carousel.Scrolled += CarouselViewScrolled;
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
			public CarouselViewOnScrollListener(ItemsView itemsView, ItemsViewAdapter<ItemsView, IItemsViewSource> itemsViewAdapter) : base(itemsView, itemsViewAdapter, true)
			{
			}

			public override void OnScrollStateChanged(RecyclerView recyclerView, int state)
			{
				base.OnScrollStateChanged(recyclerView, state);
				CarouselViewRenderer carouselViewRenderer = (CarouselViewRenderer)recyclerView;

				if (carouselViewRenderer._isSwipeEnabled)
				{
					if (state == ScrollStateDragging)
						carouselViewRenderer.Carousel.SetIsDragging(true);
					else
						carouselViewRenderer.Carousel.SetIsDragging(false);
				}

				carouselViewRenderer.Carousel.IsScrolling = state != ScrollStateIdle;
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
	}
}
