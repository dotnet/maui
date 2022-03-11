using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolygonHandler : ShapeViewHandler
	{
		public static IPropertyMapper<Polygon, IShapeViewHandler> PolygonMapper = new PropertyMapper<Polygon, IShapeViewHandler>(Mapper)
		{
			[nameof(IShapeView.Shape)] = MapShape,
			[nameof(Polygon.Points)] = MapPoints,
			[nameof(Polygon.FillRule)] = MapFillRule,
		};

		public PolygonHandler() : base(PolygonMapper)
		{

		}

		public PolygonHandler(IPropertyMapper mapper) : base(mapper ?? PolygonMapper)
		{

		}
	}
}