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
	class CarouselViewwOnGlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
	{
		public EventHandler<EventArgs> LayoutReady;
		public void OnGlobalLayout()
		{
			LayoutReady?.Invoke(this, new EventArgs());
		}
	}

	public class CarouselViewRenderer : ItemsViewRenderer<ItemsView, ItemsViewAdapter<ItemsView, IItemsViewSource>, IItemsViewSource>
	{
		protected FormsCarouselView Carousel;
		RecyclerView.ItemDecoration _itemDecoration;
		bool _isSwipeEnabled;
		int _oldPosition;
		int _initialPosition;
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

			Carousel.Scrolled += CarouselViewScrolled;
			AddLayoutListener();
			UpdateIsSwipeEnabled();
			UpdateIsBounceEnabled();
			UpdateInitialPosition();
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
				UpdateVisualStates();
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

			if (_itemDecoration != null)
			{
				RemoveItemDecoration(_itemDecoration);
			}

			_itemDecoration = CreateSpacingDecoration(ItemsLayout);
			AddItemDecoration(_itemDecoration);

			var adapter = GetAdapter();

			if (adapter != null)
			{
				adapter.NotifyItemChanged(_oldPosition);
				Carousel.ScrollTo(_oldPosition, position: Xamarin.Forms.ScrollToPosition.Center);
			}

			base.UpdateItemSpacing();
		}

		protected override IItemsLayout GetItemsLayout()
		{
			return Carousel.ItemsLayout;
		}

		protected override void UpdateAdapter()
		{
			// By default the CollectionViewAdapter creates the items at whatever size the template calls for
			// But for the Carousel, we want it to create the items to fit the width/height of the viewport
			// So we give it an alternate delegate for creating the views

			var oldItemViewAdapter = ItemsViewAdapter;

			ItemsViewAdapter = new ItemsViewAdapter<ItemsView, IItemsViewSource>(ItemsView,
				(view, context) => new SizedItemContentView(Context, GetItemWidth, GetItemHeight));

			SwapAdapter(ItemsViewAdapter, false);

			oldItemViewAdapter?.Dispose();
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

		void UpdateInitialPosition()
		{
			if (Carousel.CurrentItem != null)
			{
				int position = 0;

				var items = Carousel.ItemsSource as IList;

				for (int n = 0; n < items?.Count; n++)
				{
					if (items[n] == Carousel.CurrentItem)
					{
						position = n;
						break;
					}
				}

				_initialPosition = position;
				Carousel.Position = _initialPosition;
			}
			else
				_initialPosition = Carousel.Position;

			_oldPosition = _initialPosition;
		}

		void UpdateVisualStates()
		{
			var layoutManager = GetLayoutManager() as LinearLayoutManager;

			if (layoutManager == null)
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
			UpdateVisualStates();
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
			while (Carousel.ScrollToActions.Count > 0)
			{
				var action = Carousel.ScrollToActions.Dequeue();
				action();
			}

			Carousel.PlatformInitialized();
			UpdateVisualStates();
			ClearLayoutListener();
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
			public CarouselViewOnScrollListener(ItemsView itemsView, ItemsViewAdapter<ItemsView, IItemsViewSource> itemsViewAdapter) : base(itemsView, itemsViewAdapter)
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

			public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
			{
				base.OnScrolled(recyclerView, dx, dy);
				CarouselViewRenderer carouselViewRenderer = (CarouselViewRenderer)recyclerView;
				carouselViewRenderer.UpdateVisualStates();
			}
		}
	}
}