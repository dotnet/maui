using System;
using AndroidX.RecyclerView.Widget;
using Android.Views;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		CarouselViewwOnGlobalLayoutListener _carouselViewLayoutListener;
		CarouselViewLoopManager _carouselViewLoopManager;

		CarouselView CarouselView => VirtualView;

		protected override void ConnectHandler(RecyclerView nativeView)
		{
			_carouselViewLoopManager = new CarouselViewLoopManager();

			AddLayoutListener();

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(RecyclerView nativeView)
		{
			if (_carouselViewLoopManager != null)
			{
				_carouselViewLoopManager?.SetItemsSource(null);
				_carouselViewLoopManager = null;
			}

			ClearLayoutListener();

			base.DisconnectHandler(nativeView);
		}

		protected override IItemsLayout GetItemsLayout() => VirtualView.ItemsLayout;

		protected override ItemsViewAdapter<CarouselView, IItemsViewSource> CreateAdapter()
		{
			return new CarouselViewAdapter<CarouselView, IItemsViewSource>(CarouselView,
				(view, context) => new SizedItemContentView(Context, GetItemWidth, GetItemHeight));
		}

		[MissingMapper]
		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapLoop(CarouselViewHandler handler, CarouselView carouselView) { }

		void AddLayoutListener()
		{
			if (_carouselViewLayoutListener != null)
				return;

			_carouselViewLayoutListener = new CarouselViewwOnGlobalLayoutListener();
			_carouselViewLayoutListener.LayoutReady += LayoutReady;

			NativeView?.ViewTreeObserver?.AddOnGlobalLayoutListener(_carouselViewLayoutListener);
		}

		void ClearLayoutListener()
		{
			if (_carouselViewLayoutListener == null)
				return;

			NativeView?.ViewTreeObserver?.RemoveOnGlobalLayoutListener(_carouselViewLayoutListener);
			_carouselViewLayoutListener.LayoutReady -= LayoutReady;
			_carouselViewLayoutListener = null;
		}

		int GetItemWidth()
		{
			var itemWidth = NativeView.Width;

			if (CarouselView.ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				itemWidth = (int)(NativeView.Width - Context?.ToPixels(CarouselView.PeekAreaInsets.Left) - Context?.ToPixels(CarouselView.PeekAreaInsets.Right) - Context?.ToPixels(listItemsLayout.ItemSpacing));

			return itemWidth;
		}

		int GetItemHeight()
		{
			var itemHeight = NativeView.Height;

			if (CarouselView.ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				itemHeight = (int)(NativeView.Height - Context?.ToPixels(CarouselView.PeekAreaInsets.Top) - Context?.ToPixels(CarouselView.PeekAreaInsets.Bottom) - Context?.ToPixels(listItemsLayout.ItemSpacing));

			return itemHeight;
		}

		void LayoutReady(object sender, EventArgs e)
		{

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