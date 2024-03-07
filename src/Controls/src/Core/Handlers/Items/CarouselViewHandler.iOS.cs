#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		ItemsViewLayout _layout;

		protected override CarouselViewController CreateController(CarouselView newElement, ItemsViewLayout layout)
				=> new CarouselViewController(newElement, layout);

		protected override ItemsViewLayout SelectLayout() =>
				_layout ??= new CarouselViewLayout(VirtualView.ItemsLayout, VirtualView);

		protected override void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			if (VirtualView?.Loop == true)
			{
				var goToIndexPath = (Controller as CarouselViewController).GetScrollToIndexPath(args.Index);

				if (!IsIndexPathValid(goToIndexPath))
				{
					return;
				}

				Controller.CollectionView.ScrollToItem(goToIndexPath,
					args.ScrollToPosition.ToCollectionViewScrollPosition(_layout.ScrollDirection),
					args.IsAnimated);
			}
			else
			{
				base.ScrollToRequested(sender, args);
			}
		}

		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.Controller.CollectionView.ScrollEnabled = carouselView.IsSwipeEnabled;
		}

		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.Controller.CollectionView.Bounces = carouselView.IsBounceEnabled;
		}

		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.Controller.Layout as CarouselViewLayout)?.UpdateConstraints(handler.PlatformView.Frame.Size);
			handler.Controller.Layout.InvalidateLayout();
		}

		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.Controller as CarouselViewController)?.UpdateFromCurrentItem();
		}

		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.Controller as CarouselViewController)?.UpdateFromPosition();
		}

		public static void MapLoop(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.Controller as CarouselViewController)?.UpdateLoop();
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);
	}
}