using System;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();
		public static void MapCurrentItem(ICarouselViewHandler handler, CarouselView carouselView) { }
		public static void MapPosition(ICarouselViewHandler handler, CarouselView carouselView) { }
		public static void MapIsBounceEnabled(ICarouselViewHandler handler, CarouselView carouselView) { }
		public static void MapIsSwipeEnabled(ICarouselViewHandler handler, CarouselView carouselView) { }
		public static void MapPeekAreaInsets(ICarouselViewHandler handler, CarouselView carouselView) { }
		public static void MapLoop(ICarouselViewHandler handler, CarouselView carouselView) { }
	}
}