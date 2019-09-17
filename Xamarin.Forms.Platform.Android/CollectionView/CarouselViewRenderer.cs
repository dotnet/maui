using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using FormsCollectionView = Xamarin.Forms.CollectionView;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselViewRenderer : ItemsViewRenderer<ItemsView, ItemsViewAdapter<ItemsView, IItemsViewSource>, IItemsViewSource>
	{
		protected CarouselView Carousel;
		ItemDecoration _itemDecoration;
		bool _isSwipeEnabled;
		bool _isUpdatingPositionFromForms;
		int _oldPosition;
		int _initialPosition;

		public CarouselViewRenderer(Context context) : base(context)
		{
			FormsCollectionView.VerifyCollectionViewFlagEnabled(nameof(CarouselViewRenderer));
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
			{
				return;
			}

			UpdateIsSwipeEnabled();
			UpdateInitialPosition();
			UpdateItemSpacing();
		}

		protected override void UpdateItemsSource()
		{
			UpdateAdapter();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
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
		}

		public override void OnScrolled(int dx, int dy)
		{
			base.OnScrolled(dx, dy);

			UpdatePositionFromScroll();
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

		void UpdatePosition(int position)
		{
			if (position == -1 || _isUpdatingPositionFromForms)
				return;

			var item = ItemsViewAdapter?.ItemsSource.GetItem(position);

			if (item == null)
				throw new InvalidOperationException("Visible item not found");

			Carousel.SetCurrentItem(item);
		}

		void UpdateIsBounceEnabled()
		{
			OverScrollMode = Carousel.IsBounceEnabled ? OverScrollMode.Always : OverScrollMode.Never;
		}

		void UpdatePeekAreaInsets()
		{
			UpdateAdapter();
		}

		void UpdateAdapter()
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

		void UpdatePositionFromScroll()
		{
			var snapHelper = GetSnapManager()?.GetCurrentSnapHelper();

			if (snapHelper == null)
				return;

			var layoutManager = GetLayoutManager() as LayoutManager;

			var snapView = snapHelper.FindSnapView(layoutManager);

			if (snapView != null)
			{
				int middleCenterPosition = layoutManager.GetPosition(snapView);
	
				if (_oldPosition != middleCenterPosition)
				{
					_oldPosition = middleCenterPosition;
					UpdatePosition(middleCenterPosition);
				}
			}
		}

		void UpdateInitialPosition()
		{
			_isUpdatingPositionFromForms = true;
			// Goto to the Correct Position
			_initialPosition = Carousel.Position;
			_oldPosition = _initialPosition;
			Carousel.ScrollTo(_initialPosition, position: Xamarin.Forms.ScrollToPosition.Center);
			_isUpdatingPositionFromForms = false;
		}
	}
}