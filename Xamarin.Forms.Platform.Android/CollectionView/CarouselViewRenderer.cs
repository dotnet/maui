using System;
using System.ComponentModel;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselViewRenderer : ItemsViewRenderer<ItemsView, ItemsViewAdapter<ItemsView, IItemsViewSource>, IItemsViewSource>
	{
		readonly Context _context;
		protected CarouselView Carousel;
		bool _isSwipeEnabled;
		bool _isUpdatingPositionFromForms;
		int _oldPosition;
		int _initialPosition;
		bool _scrollingToInitialPosition = true;

		public CarouselViewRenderer(Context context) : base(context)
		{
			_context = context;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{

			}

			base.Dispose(disposing);
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			base.SetUpNewElement(newElement);

			if (newElement == null)
			{
				Carousel = null;
				return;
			}

			Carousel = newElement as CarouselView;

			UpdateIsSwipeEnabled();
			UpdateInitialPosition();
		}

		protected override void UpdateItemsSource()
		{
			// By default the CollectionViewAdapter creates the items at whatever size the template calls for
			// But for the Carousel, we want it to create the items to fit the width/height of the viewport
			// So we give it an alternate delegate for creating the views

			ItemsViewAdapter = new ItemsViewAdapter<ItemsView, IItemsViewSource>(ItemsView, 
				(view, context) => new SizedItemContentView(context, () => Width, () => Height));

			SwapAdapter(ItemsViewAdapter, false);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(CarouselView.IsSwipeEnabledProperty))
				UpdateIsSwipeEnabled();
			else if (e.Is(CarouselView.IsBounceEnabledProperty))
				UpdateIsBounceEnabled();
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			//TODO: this doesn't work because we need to interact with the Views
			if (!_isSwipeEnabled)
			{
				return false;
			}
			return base.OnTouchEvent(e);
		}

		public override void OnScrollStateChanged(int state)
		{
			base.OnScrollStateChanged(state);

			System.Diagnostics.Debug.WriteLine($"State {state}");

			if (_isSwipeEnabled)
			{
				if (state == ScrollStateDragging)
				{
					Carousel.SetIsDragging(true);
				}
				else
				{
					Carousel.SetIsDragging(false);
				}
			}
		}

		public override void OnScrolled(int dx, int dy)
		{
			base.OnScrolled(dx, dy);

			UpdatePositionFromScroll();
		}

		protected override ItemDecoration CreateSpacingDecoration(IItemsLayout itemsLayout)
		{
			return new CarouselSpacingItemDecoration(itemsLayout, GetItemWidth, GetItemHeight);
		}

		void UpdateIsSwipeEnabled()
		{
			_isSwipeEnabled = Carousel.IsSwipeEnabled;
		}

		void UpdatePosition(int position)
		{
			if (position == -1 || _isUpdatingPositionFromForms)
				return;

			var context = ItemsViewAdapter?.ItemsSource.GetItem(position);

			if (context == null)
				throw new InvalidOperationException("Visible item not found");

			Carousel.SetCurrentItem(context);
		}

		void UpdateIsBounceEnabled()
		{
			OverScrollMode = Carousel.IsBounceEnabled ? OverScrollMode.Always : OverScrollMode.Never;
		}

		int GetItemWidth()
		{
			var numberofSideItems = (Element as CarouselView).NumberOfSideItems;
			var numberOfItems = numberofSideItems * 2 + 1;
			int spacingWidth = 0;

			if (Carousel.ItemsLayout is ListItemsLayout listItemsLayout && numberofSideItems > 0)
				spacingWidth = (int)listItemsLayout.ItemSpacing * (numberOfItems - 1);

			var itemWidth = (Width - (int)Context.ToPixels(spacingWidth)) / numberOfItems;

			return itemWidth;
		}

		int GetItemHeight()
		{ 	 
			//TODO: Calculate item height.
			return Height;
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
				if (_scrollingToInitialPosition)
				{
					_scrollingToInitialPosition = !(_initialPosition == middleCenterPosition);
					return;
				}

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
			//Goto to the Correct Position
			_initialPosition = Carousel.Position;
			Carousel.ScrollTo(_initialPosition, position: Xamarin.Forms.ScrollToPosition.Center);
			_isUpdatingPositionFromForms = false;
		}
	}
}