#nullable disable
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolygonHandler : ShapeViewHandler
	{
		public static new IPropertyMapper<Polygon, IShapeViewHandler> Mapper = new PropertyMapper<Polygon, IShapeViewHandler>(ShapeViewHandler.Mapper)
		{
			[nameof(IShapeView.Shape)] = MapShape,
			[nameof(Polygon.Points)] = MapPoints,
			[nameof(Polygon.FillRule)] = MapFillRule,
		};

		public PolygonHandler() : base(Mapper)
		{

		}

		public PolygonHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{

		}
	}
}