﻿#nullable disable
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

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			// I'm not sure if this solution is fully correct or if it properly accounts
			// for all the constraints checks that  GetDesiredSizeFromHandler takes into account 
			if (Primitives.Dimension.IsExplicitSet(widthConstraint) && Primitives.Dimension.IsExplicitSet(heightConstraint))
			{
				// If both width and height are explicitly set, we can use the base implementation
				return base.GetDesiredSize(widthConstraint, heightConstraint);
			}

			var result = this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

			if (Primitives.Dimension.IsExplicitSet(widthConstraint))
			{
				// If width is explicitly set, we can use the width from the result
				result = new Size(widthConstraint, result.Height);
			}
			else if (Primitives.Dimension.IsExplicitSet(heightConstraint))
			{
				// If height is explicitly set, we can use the height from the result
				result = new Size(result.Width, heightConstraint);
			}

			return result;
		}

		public override void PlatformArrange(Rect rect)
		{
			(Controller.Layout as CarouselViewLayout)?.UpdateConstraints(rect.Size);
			base.PlatformArrange(rect);
		}
	}
}