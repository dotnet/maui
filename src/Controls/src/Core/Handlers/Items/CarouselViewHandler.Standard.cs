#nullable disable
using System;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		protected override object CreatePlatformView()
		{
			throw new NotImplementedException();
		}

		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView) { }
		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView) { }
		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView) { }
		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView) { }
		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView) { }
		public static void MapLoop(CarouselViewHandler handler, CarouselView carouselView) { }
	}
}