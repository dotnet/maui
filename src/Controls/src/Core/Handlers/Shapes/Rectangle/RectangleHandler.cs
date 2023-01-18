#nullable disable
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class RectangleHandler : ShapeViewHandler
	{
		public static new IPropertyMapper<Rectangle, IShapeViewHandler> Mapper = new PropertyMapper<Rectangle, IShapeViewHandler>(ShapeViewHandler.Mapper)
		{
			[nameof(Rectangle.RadiusX)] = MapRadiusX,
			[nameof(Rectangle.RadiusY)] = MapRadiusY,
		};

		public RectangleHandler() : base(Mapper)
		{

		}

		public RectangleHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{

		}
	}
}