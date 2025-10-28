#nullable disable
using System;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler
	{
		public CarouselViewHandler() : base(Mapper,CommandMapper)
		{

		}
		public CarouselViewHandler(PropertyMapper mapper = null) : base(mapper ?? Mapper, CommandMapper)
		{

		}

		public static PropertyMapper<CarouselView, CarouselViewHandler> Mapper = new(ItemsViewMapper)
		{
#if TIZEN || ANDROID
			[Controls.CarouselView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
#endif
			[Controls.CarouselView.IsSwipeEnabledProperty.PropertyName] = MapIsSwipeEnabled,
			[Controls.CarouselView.PeekAreaInsetsProperty.PropertyName] = MapPeekAreaInsets,
			[Controls.CarouselView.IsBounceEnabledProperty.PropertyName] = MapIsBounceEnabled,
			[Controls.CarouselView.PositionProperty.PropertyName] = MapPosition,
			[Controls.CarouselView.CurrentItemProperty.PropertyName] = MapCurrentItem
		};

		//TODO Make this public in .NET10
		internal static CommandMapper<CarouselView, CarouselViewHandler> CommandMapper = new(ViewCommandMapper)
		{
			["ItemsLayoutProperties"] = MapItemsLayoutPropertyChanged
		};
	}
}