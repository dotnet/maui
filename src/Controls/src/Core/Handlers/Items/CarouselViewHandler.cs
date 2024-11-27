#nullable disable
namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler
	{
		public CarouselViewHandler() : base(Mapper)
		{

		}
		public CarouselViewHandler(PropertyMapper mapper = null) : base(mapper ?? Mapper)
		{

		}

		public static PropertyMapper<CarouselView, CarouselViewHandler> Mapper = new(ItemsViewMapper)
		{
#if TIZEN
			[Controls.CarouselView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
#endif
			[Controls.CarouselView.IsSwipeEnabledProperty.PropertyName] = MapIsSwipeEnabled,
			[Controls.CarouselView.PeekAreaInsetsProperty.PropertyName] = MapPeekAreaInsets,
			[Controls.CarouselView.IsBounceEnabledProperty.PropertyName] = MapIsBounceEnabled,
			[Controls.CarouselView.PositionProperty.PropertyName] = MapPosition,
			[Controls.CarouselView.CurrentItemProperty.PropertyName] = MapCurrentItem
		};
	}
}