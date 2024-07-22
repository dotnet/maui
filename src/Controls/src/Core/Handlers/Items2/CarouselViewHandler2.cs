#nullable disable
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class CarouselViewHandler2
	{
		public CarouselViewHandler2() : base(CarouselViewHandler.Mapper)
		{
		}
		
		public CarouselViewHandler2(PropertyMapper mapper = null) : base(mapper ?? CarouselViewHandler.Mapper)
		{
		}
	}
}