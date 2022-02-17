using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolygonHandler : ShapeViewHandler
	{
		public static IPropertyMapper<Polygon, PolygonHandler> PolygonMapper = new PropertyMapper<Polygon, PolygonHandler>(ShapeViewMapper)
		{
			[nameof(Polygon.Points)] = MapPoints,
		};

		public PolygonHandler() : base(PolygonMapper)
		{

		}

		public PolygonHandler(IPropertyMapper mapper) : base(mapper ?? PolygonMapper)
		{

		}
	}
}