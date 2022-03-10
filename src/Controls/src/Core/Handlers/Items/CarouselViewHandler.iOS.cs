using System;

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

		public static void MapIsSwipeEnabled(ICarouselViewHandler handler, CarouselView carouselView)
		{
			if(handler is CarouselViewHandler carouselViewHandler)
				carouselViewHandler.Controller.CollectionView.ScrollEnabled = carouselView.IsSwipeEnabled;
		}

		public static void MapIsBounceEnabled(ICarouselViewHandler handler, CarouselView carouselView)
		{
			if (handler is CarouselViewHandler carouselViewHandler)
				carouselViewHandler.Controller.CollectionView.Bounces = carouselView.IsBounceEnabled;
		}

		public static void MapPeekAreaInsets(ICarouselViewHandler handler, CarouselView carouselView)
		{
			((handler as CarouselViewHandler)?.Controller.Layout as CarouselViewLayout)?.UpdateConstraints(handler.PlatformView.Frame.Size);
			(handler as CarouselViewHandler)?.Controller.Layout.InvalidateLayout();
		}

		public static void MapCurrentItem(ICarouselViewHandler handler, CarouselView carouselView)
		{
			if (handler is CarouselViewHandler carouselViewHandler)
				(carouselViewHandler.Controller as CarouselViewController)?.UpdateFromCurrentItem();
		}

		public static void MapPosition(ICarouselViewHandler handler, CarouselView carouselView)
		{
			if (handler is CarouselViewHandler carouselViewHandler)
				(carouselViewHandler.Controller as CarouselViewController)?.UpdateFromPosition();
		}

		public static void MapLoop(ICarouselViewHandler handler, CarouselView carouselView)
		{
			if (handler is CarouselViewHandler carouselViewHandler)
				(carouselViewHandler.Controller as CarouselViewController)?.UpdateLoop();
		}
	}
}