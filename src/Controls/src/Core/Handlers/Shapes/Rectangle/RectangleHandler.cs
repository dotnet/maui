using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class RectangleHandler : ShapeViewHandler
	{
		public static IPropertyMapper<Rectangle, IShapeViewHandler> RectangleMapper = new PropertyMapper<Rectangle, IShapeViewHandler>(Mapper)
		{
			[nameof(Rectangle.RadiusX)] = MapRadiusX,
			[nameof(Rectangle.RadiusY)] = MapRadiusY,
		};

		public RectangleHandler() : base(RectangleMapper)
		{

		}

		public RectangleHandler(IPropertyMapper mapper) : base(mapper ?? RectangleMapper)
		{

		}
	}
}