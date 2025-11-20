#nullable disable
using System;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class CarouselViewHandler2
	{

		public CarouselViewHandler2() : base(Mapper)
		{


		}
		public CarouselViewHandler2(PropertyMapper mapper = null) : base(mapper ?? Mapper)
		{

		}

		public static PropertyMapper<CarouselView, CarouselViewHandler2> Mapper = new(ItemsViewMapper)
		{
			[Controls.VisualElement.IsEnabledProperty.PropertyName] = MapIsEnabled,
			[Controls.CarouselView.IsSwipeEnabledProperty.PropertyName] = MapIsSwipeEnabled,
			[Controls.CarouselView.PeekAreaInsetsProperty.PropertyName] = MapPeekAreaInsets,
			[Controls.CarouselView.IsBounceEnabledProperty.PropertyName] = MapIsBounceEnabled,
			[Controls.CarouselView.PositionProperty.PropertyName] = MapPosition,
			[Controls.CarouselView.CurrentItemProperty.PropertyName] = MapCurrentItem,
			[Controls.CarouselView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
		};
	}

	public partial class CarouselViewHandler2 : ItemsViewHandler2<CarouselView>
	{
		protected override CarouselViewController2 CreateController(CarouselView newElement, UICollectionViewLayout layout)
				=> new(newElement, layout);

		protected override UICollectionViewLayout SelectLayout()
		{
			var weakItemsView = new WeakReference<CarouselView>(ItemsView);
			var weakController = new WeakReference<CarouselViewController2>((CarouselViewController2)Controller);

			return LayoutFactory2.CreateCarouselLayout(weakItemsView, weakController);
		}

		protected override void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			if (VirtualView?.Loop == true)
			{
				var goToIndexPath = (Controller as CarouselViewController2).GetScrollToIndexPath(args.Index);

				if (!IsIndexPathValid(goToIndexPath))
				{
					return;
				}

				bool IsHorizontal = VirtualView.ItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal;
				UICollectionViewScrollDirection scrollDirection = IsHorizontal ? UICollectionViewScrollDirection.Horizontal : UICollectionViewScrollDirection.Vertical;

				Controller.CollectionView.ScrollToItem(goToIndexPath,
					args.ScrollToPosition.ToCollectionViewScrollPosition(scrollDirection), // TODO: Fix _layout.ScrollDirection),
					args.IsAnimated);
			}
			else
			{
				base.ScrollToRequested(sender, args);
			}
		}

		// TODO: Change the modifier to public in .NET 11.
		internal static void MapIsEnabled(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			handler.Controller?.CollectionView?.UpdateIsEnabled(carouselView);
		}

		public static void MapIsSwipeEnabled(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			handler.Controller.CollectionView.ScrollEnabled = carouselView.IsSwipeEnabled;
		}

		public static void MapIsBounceEnabled(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			handler.Controller.CollectionView.Bounces = carouselView.IsBounceEnabled;
		}

		// TODO: Change the modifier to public in .NET 10.
		internal static void MapItemsLayout(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			handler?.UpdateLayout();
			(handler.Controller as CarouselViewController2)?.UpdateScrollingConstraints();
		}

		public static void MapPeekAreaInsets(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			handler.UpdateLayout();
		}

		public static void MapCurrentItem(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			(handler.Controller as CarouselViewController2)?.UpdateFromCurrentItem();
		}

		public static void MapPosition(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			// If the initial position hasn't been set, we have a UpdateInitialPosition call on CarouselViewController2
			// that will handle this so we want to skip this mapper call. We need to wait for the CollectionView to be ready
			if (handler.Controller is CarouselViewController2 CarouselViewController2 && CarouselViewController2.InitialPositionSet)
			{
				CarouselViewController2.UpdateFromPosition();
			}
		}

		public static void MapLoop(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			(handler.Controller as CarouselViewController2)?.UpdateLoop();
		}

	}
}