using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class RoundRectangleHandler : ShapeViewHandler
	{
		public static IPropertyMapper<RoundRectangle, IShapeViewHandler> RoundRectangleMapper = new PropertyMapper<RoundRectangle, IShapeViewHandler>(Mapper)
		{
			[nameof(RoundRectangle.CornerRadius)] = MapCornerRadius
		};

		public RoundRectangleHandler() : base(RoundRectangleMapper)
		{

		}

		public RoundRectangleHandler(IPropertyMapper mapper) : base(mapper ?? RoundRectangleMapper)
		{

		}
	}
}