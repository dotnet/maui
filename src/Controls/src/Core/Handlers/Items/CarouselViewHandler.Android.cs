namespace Microsoft.Maui.Controls.Handlers.Items
{
    public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		protected override IItemsLayout GetItemsLayout() => VirtualView.ItemsLayout;
		
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