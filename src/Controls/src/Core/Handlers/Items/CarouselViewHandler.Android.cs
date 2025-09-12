#nullable disable
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		double _widthConstraint;
		double _heightConstraint;

		protected override IItemsLayout GetItemsLayout() => VirtualView.ItemsLayout;

		protected override ItemsViewAdapter<CarouselView, IItemsViewSource> CreateAdapter()
		{
			return new CarouselViewAdapter<CarouselView, IItemsViewSource>(VirtualView, (view, context) =>
			{
				return new SizedItemContentView(Context, GetItemWidth, GetItemHeight)
				{
					LayoutParameters = new RecyclerView.LayoutParams(RecyclerView.LayoutParams.MatchParent, RecyclerView.LayoutParams.MatchParent)
				};
			});
		}

		protected override RecyclerView CreatePlatformView()
		{
			return new MauiCarouselRecyclerView(Context, GetItemsLayout, CreateAdapter);
		}

		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.PlatformView as IMauiCarouselRecyclerView).IsSwipeEnabled = carouselView.IsSwipeEnabled;
		}

		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.PlatformView.OverScrollMode = carouselView?.IsBounceEnabled == true ? OverScrollMode.Always : OverScrollMode.Never;
		}

		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.PlatformView as IMauiRecyclerView<CarouselView>).UpdateAdapter();
		}

		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.PlatformView as IMauiCarouselRecyclerView).UpdateFromPosition();
		}

		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.PlatformView as IMauiCarouselRecyclerView).UpdateFromCurrentItem();
		}

		// TODO: Change the modifier to public in .NET 10.
		internal static void MapItemsLayout(CarouselViewHandler handler, CarouselView carouselView)
		{
			if (handler.PlatformView is IMauiRecyclerView<CarouselView> recyclerView)
			{
				recyclerView.UpdateLayoutManager();
			}
		}

		//TODO Make this public in .NET10
		internal static void MapItemsLayoutPropertyChanged(CarouselViewHandler handler, CarouselView view, object args)
		{
			if (handler.PlatformView is IMauiRecyclerView<CarouselView> recyclerView)
			{
				recyclerView.UpdateItemsLayoutProperties(args);
			}
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			_widthConstraint = widthConstraint;
			_heightConstraint = heightConstraint;

			if (!double.IsInfinity(_widthConstraint))
				_widthConstraint = Context.ToPixels(_widthConstraint);

			if (!double.IsInfinity(_heightConstraint))
				_heightConstraint = Context.ToPixels(_heightConstraint);

			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public override void PlatformArrange(Rect frame)
		{
			_widthConstraint = Context.ToPixels(frame.Width);
			_heightConstraint = Context.ToPixels(frame.Height);

			base.PlatformArrange(frame);
		}

		double GetItemWidth()
		{
			var itemWidth = _widthConstraint;

			if ((PlatformView as IMauiRecyclerView<CarouselView>)?.ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				var width = (PlatformView.MeasuredWidth == 0) ? _widthConstraint : PlatformView.MeasuredWidth;

				if (double.IsInfinity(width))
					return width;

				itemWidth = (int)(width - Context?.ToPixels(VirtualView.PeekAreaInsets.Left) - Context?.ToPixels(VirtualView.PeekAreaInsets.Right));
			}

			return itemWidth;
		}

		double GetItemHeight()
		{
			var itemHeight = _heightConstraint;

			if ((PlatformView as IMauiRecyclerView<CarouselView>)?.ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			{
				var height = (PlatformView.MeasuredHeight == 0) ? _heightConstraint : PlatformView.MeasuredHeight;

				if (double.IsInfinity(height))
					return height;

				itemHeight = (int)(height - Context?.ToPixels(VirtualView.PeekAreaInsets.Top) - Context?.ToPixels(VirtualView.PeekAreaInsets.Bottom));
			}

			return itemHeight;
		}
	}
}