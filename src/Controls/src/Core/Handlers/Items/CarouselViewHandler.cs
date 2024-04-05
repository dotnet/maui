#nullable disable
namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler
	{
		public CarouselViewHandler() : this(Mapper, CarouselViewCommandMapper)
		{

		}
		public CarouselViewHandler(PropertyMapper mapper = null) : this(mapper ?? Mapper, CarouselViewCommandMapper)
		{

		}

		private protected CarouselViewHandler(PropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper ?? CarouselViewCommandMapper)
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

		// Make Public for Net9 possibly rename this
		static CommandMapper<CarouselView, CarouselViewHandler> CarouselViewCommandMapper = new(ItemsViewCommandMapper)
		{
			[nameof(IView.Frame)] = MapFrame
		};
	}
}