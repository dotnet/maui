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

		public CarouselViewHandler2() : base(Mapper, CommandMapper)
		{


		}
		public CarouselViewHandler2(PropertyMapper mapper = null) : base(mapper ?? Mapper, CommandMapper)
		{

		}

		public static PropertyMapper<CarouselView, CarouselViewHandler2> Mapper = new(ItemsViewMapper)
		{

			[Controls.CarouselView.IsSwipeEnabledProperty.PropertyName] = MapIsSwipeEnabled,
			[Controls.CarouselView.PeekAreaInsetsProperty.PropertyName] = MapPeekAreaInsets,
			[Controls.CarouselView.IsBounceEnabledProperty.PropertyName] = MapIsBounceEnabled,
			[Controls.CarouselView.PositionProperty.PropertyName] = MapPosition,
			[Controls.CarouselView.CurrentItemProperty.PropertyName] = MapCurrentItem,
			[Controls.CarouselView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
		};

		//TODO Make this public in .NET10
		internal static CommandMapper<CarouselView, CarouselViewHandler2> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(Controls.CarouselView.ItemsLayout.PropertyChanged)] = MapItemsLayoutPropertyChanged
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

		//TODO Make this public in .NET10
		internal static void MapItemsLayoutPropertyChanged(CarouselViewHandler2 handler, CarouselView view, object args)
		{
			handler.UpdateLayout();
		}

	}
}