#nullable disable
using System;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class CarouselViewHandler2 : ItemsViewHandler2<CarouselView>
	{
		UICollectionViewLayout _layout;

		protected override CarouselViewController2 CreateController(CarouselView newElement, UICollectionViewLayout layout)
				=> new CarouselViewController2(newElement, layout);

		protected override UICollectionViewLayout SelectLayout() =>
				_layout ??= LayoutFactory.CreateCarousel(VirtualView.ItemsLayout, new LayoutGroupingInfo());

		protected override void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			if (VirtualView?.Loop == true)
			{
				var goToIndexPath = (Controller as CarouselViewController2).GetScrollToIndexPath(args.Index);

				if (!IsIndexPathValid(goToIndexPath))
				{
					return;
				}

				Controller.CollectionView.ScrollToItem(goToIndexPath,
					args.ScrollToPosition.ToCollectionViewScrollPosition( UICollectionViewScrollDirection.Vertical), // TODO: Fix _layout.ScrollDirection),
					args.IsAnimated);
			}
			else
			{
				base.ScrollToRequested(sender, args);
			}
		}

		public static void MapIsSwipeEnabled(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			handler.Controller.CollectionView.ScrollEnabled = carouselView.IsSwipeEnabled;
		}

		public static void MapIsBounceEnabled(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			handler.Controller.CollectionView.Bounces = carouselView.IsBounceEnabled;
		}

		public static void MapPeekAreaInsets(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			// TODO: Fix (handler.Controller.Layout as CarouselViewLayout)?.UpdateConstraints(handler.PlatformView.Frame.Size);
			handler.Controller.Layout.InvalidateLayout();
		}

		public static void MapCurrentItem(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			(handler.Controller as CarouselViewController2)?.UpdateFromCurrentItem();
		}

		public static void MapPosition(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			// If the initial position hasn't been set, we have a UpdateInitialPosition call on CarouselViewController
			// that will handle this so we want to skip this mapper call. We need to wait for the CollectionView to be ready
			if(handler.Controller is CarouselViewController2 carouselViewController && carouselViewController.InitialPositionSet)
			{
				carouselViewController.UpdateFromPosition();
			}
		}

		public static void MapLoop(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			(handler.Controller as CarouselViewController2)?.UpdateLoop();
		}

		// public override Size GetDesiredSize(double widthConstraint, double heightConstraint) => 
		// 	this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);
	}
}