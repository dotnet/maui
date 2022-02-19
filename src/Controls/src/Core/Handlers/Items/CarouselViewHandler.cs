namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler
	{
		public CarouselViewHandler() : base(CarouselViewMapper)
		{

		}
		public CarouselViewHandler(PropertyMapper mapper = null) : base(mapper ?? CarouselViewMapper)
		{

		}

		public static PropertyMapper<CarouselView, CarouselViewHandler> CarouselViewMapper = new PropertyMapper<CarouselView, CarouselViewHandler>(ViewMapper)
		{
			[CarouselView.ItemsSourceProperty.PropertyName] = MapItemsSource,
			[CarouselView.ItemTemplateProperty.PropertyName] = MapItemTemplate,
			[CarouselView.CurrentItemProperty.PropertyName] = MapCurrentItem,
			[CarouselView.PositionProperty.PropertyName] = MapPosition,
			[CarouselView.IsBounceEnabledProperty.PropertyName] = MapIsBounceEnabled,
			[CarouselView.IsSwipeEnabledProperty.PropertyName] = MapIsSwipeEnabled,
			[CarouselView.PeekAreaInsetsProperty.PropertyName] = MapPeekAreaInsets,
			[CarouselView.LoopProperty.PropertyName] = MapLoop,
		};
	}
}