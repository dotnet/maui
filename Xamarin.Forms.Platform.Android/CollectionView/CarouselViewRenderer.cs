using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using FormsCarouselView = Xamarin.Forms.CarouselView;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselViewRenderer : ItemsViewRenderer<ItemsView, ItemsViewAdapter<ItemsView, IItemsViewSource>, IItemsViewSource>
	{
		protected CarouselView Carousel;
		ItemDecoration _itemDecoration;
		bool _isSwipeEnabled;
		int _oldPosition;
		int _initialPosition;

		public CarouselViewRenderer(Context context) : base(context)
		{
			FormsCarouselView.VerifyCarouselViewFlagEnabled(nameof(CarouselViewRenderer));
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
			}

			base.Dispose(disposing);
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			Carousel = newElement as CarouselView;

			base.SetUpNewElement(newElement);

			if (newElement == null)
				return;
			
			UpdateIsSwipeEnabled();
			UpdateInitialPosition();
			UpdateItemSpacing();
		}

		protected override void UpdateItemsSource()
		{
			UpdateAdapter();
			UpdateEmptyView();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);
   
			if (changedProperty.Is(CarouselView.PeekAreaInsetsProperty))
				UpdatePeekAreaInsets();
			else if (changedProperty.Is(CarouselView.IsSwipeEnabledProperty))
				UpdateIsSwipeEnabled();
			else if (changedProperty.Is(CarouselView.IsBounceEnabledProperty))
				UpdateIsBounceEnabled();
			else if (changedProperty.Is(LinearItemsLayout.ItemSpacingProperty))
				UpdateItemSpacing();
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (!_isSwipeEnabled)
				return false;

			return base.OnInterceptTouchEvent(ev);
		}

		public override void OnScrollStateChanged(int state)
		{
			base.OnScrollStateChanged(state);

			if (_isSwipeEnabled)
			{
				if (state == ScrollStateDragging)
					Carousel.SetIsDragging(true);
				else
					Carousel.SetIsDragging(false);
			}

			Carousel.IsScrolling = state != ScrollStateIdle;
		}

		protected override ItemDecoration CreateSpacingDecoration(IItemsLayout itemsLayout)
		{
			return new CarouselSpacingItemDecoration(itemsLayout);
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

		int GetItemWidth()
		{
			var itemWidth = Width;

			if (ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				var numberOfVisibleItems = Carousel.NumberOfSideItems * 2 + 1;
				itemWidth = (int)(Width - Carousel.PeekAreaInsets.Left - Carousel.PeekAreaInsets.Right - Context?.ToPixels(listItemsLayout.ItemSpacing)) / numberOfVisibleItems;
			}

			return itemWidth;
		}

		int GetItemHeight()
		{
			var itemHeight = Height;

			if (ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			{
				var numberOfVisibleItems = Carousel.NumberOfSideItems * 2 + 1;
				itemHeight = (int)(Height - Carousel.PeekAreaInsets.Top - Carousel.PeekAreaInsets.Bottom - Context?.ToPixels(listItemsLayout.ItemSpacing)) / numberOfVisibleItems;
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

			ItemsViewAdapter = new ItemsViewAdapter<ItemsView, IItemsViewSource>(ItemsView,
				(view, context) => new SizedItemContentView(Context, GetItemWidth, GetItemHeight));

			SwapAdapter(ItemsViewAdapter, false);

			oldItemViewAdapter?.Dispose();
		}

		void UpdateInitialPosition()
		{
			_initialPosition = Carousel.Position;
			_oldPosition = _initialPosition;
			Carousel.ScrollTo(_initialPosition, position: Xamarin.Forms.ScrollToPosition.Center, animate: false);
		}
	}
}