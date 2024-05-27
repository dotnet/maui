#nullable disable
using System;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

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
			NSIndexPath goToIndexPath = NSIndexPath.FromIndex((nuint)args.Index);
			if (VirtualView?.Loop == true)
			{
				goToIndexPath = (Controller as CarouselViewController).GetScrollToIndexPath(args.Index);
			}

			if (!IsIndexPathValid(goToIndexPath))
			{
				return;
			}
			var scrollDirection = args.ScrollToPosition.ToCollectionViewScrollPosition(_layout.ScrollDirection);

			void scrollToItemAction()
			{
				Controller.CollectionView.ScrollToItem(goToIndexPath, scrollDirection, false);
			}

			if (args.IsAnimated)
			{
				UIView.Animate(AnimationDuration, scrollToItemAction,
					() => Controller.CollectionView.Delegate?.DecelerationEnded(Controller.CollectionView));
			}
			else
			{
				scrollToItemAction();
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
			// If the initial position hasn't been set, we have a UpdateInitialPosition call on CarouselViewController
			// that will handle this so we want to skip this mapper call. We need to wait for the CollectionView to be ready
			if (handler.Controller is CarouselViewController carouselViewController && carouselViewController.InitialPositionSet)
			{
				carouselViewController.UpdateFromPosition();
			}
		}

		public static void MapLoop(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.Controller as CarouselViewController)?.UpdateLoop();
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

		protected virtual double AnimationDuration => 0.5;
	}
}