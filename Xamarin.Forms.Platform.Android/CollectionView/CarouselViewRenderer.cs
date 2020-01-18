using System.Collections;
using System.ComponentModel;
using Android.Content;
#if __ANDROID_29__
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using Android.Views;
using FormsCarouselView = Xamarin.Forms.CarouselView;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselViewRenderer : ItemsViewRenderer<ItemsView, ItemsViewAdapter<ItemsView, IItemsViewSource>, IItemsViewSource>
	{
		protected FormsCarouselView Carousel;
		RecyclerView.ItemDecoration _itemDecoration;
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
			Carousel = newElement as FormsCarouselView;

			base.SetUpNewElement(newElement);

			if (newElement == null)
				return;
			
			UpdateIsSwipeEnabled();
			UpdateIsBounceEnabled();
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
   
			if (changedProperty.Is(FormsCarouselView.PeekAreaInsetsProperty))
				UpdatePeekAreaInsets();
			else if (changedProperty.Is(FormsCarouselView.IsSwipeEnabledProperty))
				UpdateIsSwipeEnabled();
			else if (changedProperty.Is(FormsCarouselView.IsBounceEnabledProperty))
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

			ItemsViewAdapter = new ItemsViewAdapter<ItemsView, IItemsViewSource>(ItemsView,
				(view, context) => new SizedItemContentView(Context, GetItemWidth, GetItemHeight));

			SwapAdapter(ItemsViewAdapter, false);

			oldItemViewAdapter?.Dispose();
		}

		void UpdateInitialPosition()
		{
			_initialPosition = Carousel.Position;

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
			Carousel.ScrollTo(_initialPosition, position: Xamarin.Forms.ScrollToPosition.Center, animate: false);
		}
	}
}