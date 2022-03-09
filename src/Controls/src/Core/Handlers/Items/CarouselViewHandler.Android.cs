using Android.Views;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		double _widthConstraint;
		double _heightConstraint;

		protected override IItemsLayout GetItemsLayout() => VirtualView.ItemsLayout;

		protected override ItemsViewAdapter<CarouselView, IItemsViewSource> CreateAdapter()
		{
			return new CarouselViewAdapter<CarouselView, IItemsViewSource>(VirtualView, (view, context) => new SizedItemContentView(Context, GetItemWidth, GetItemHeight));
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

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			_widthConstraint = widthConstraint;
			_heightConstraint = heightConstraint;
			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		int GetItemWidth()
		{
			var itemWidth = (int)Context?.ToPixels(_widthConstraint);

			if ((PlatformView as IMauiRecyclerView<CarouselView>)?.ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				itemWidth = (int)(PlatformView.Width - Context?.ToPixels(VirtualView.PeekAreaInsets.Left) - Context?.ToPixels(VirtualView.PeekAreaInsets.Right) - Context?.ToPixels(listItemsLayout.ItemSpacing));

			return itemWidth;
		}

		int GetItemHeight()
		{
			var itemHeight = (int)Context?.ToPixels(_heightConstraint);

			if ((PlatformView as IMauiRecyclerView<CarouselView>)?.ItemsLayout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				itemHeight = (int)(PlatformView.Height - Context?.ToPixels(VirtualView.PeekAreaInsets.Top) - Context?.ToPixels(VirtualView.PeekAreaInsets.Bottom) - Context?.ToPixels(listItemsLayout.ItemSpacing));

			return itemHeight;
		}
	}
}