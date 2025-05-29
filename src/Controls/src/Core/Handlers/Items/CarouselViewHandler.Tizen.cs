#nullable disable
using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		protected override TCollectionView CreatePlatformView()
		{
			return new MauiCarouselView();
		}

		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.PlatformView as MauiCarouselView)?.UpdateCurrentItem();
		}

		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.PlatformView as MauiCarouselView)?.UpdatePosition();
		}

		[MissingMapper]
		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView) { }

		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			(handler.PlatformView as MauiCarouselView)?.UpdateIsSwipeEnabled();
		}

		[MissingMapper]
		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapLoop(CarouselViewHandler handler, CarouselView carouselView) { }

		public static void MapItemsLayout(CarouselViewHandler handler, CarouselView itemsView)
		{
			(handler.PlatformView as MauiCarouselView)?.UpdateLayoutManager();
		}

		//TODO Make this public in .NET10
		internal static void MapItemsLayoutPropertyChanged(CarouselViewHandler handler, CarouselView view, object args)
		{
			(handler.PlatformView as MauiCarouselView)?.UpdateLayoutManager();
		}
	}
}
