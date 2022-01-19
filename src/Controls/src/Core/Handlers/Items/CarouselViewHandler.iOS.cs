using System;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		protected override ItemsViewController<CarouselView> CreateController(CarouselView newElement, ItemsViewLayout layout)
		{
			throw new NotImplementedException();
		}

		protected override ItemsViewLayout SelectLayout()
		{
			throw new NotImplementedException();
		}

		[MissingMapper]
		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView) { }

		[MissingMapper]
		public static void MapLoop(CarouselViewHandler handler, CarouselView carouselView) { }
	}
}