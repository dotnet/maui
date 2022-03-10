using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolylineHandler : ShapeViewHandler
	{
		public static IPropertyMapper<Polyline, PolylineHandler> PolylineMapper = new PropertyMapper<Polyline, PolylineHandler>(Mapper)
		{
			[nameof(IShapeView.Shape)] = MapShape,
			[nameof(Polyline.Points)] = MapPoints,
			[nameof(Polyline.FillRule)] = MapFillRule,
		};

		public PolylineHandler() : base(PolylineMapper)
		{

		}

		public PolylineHandler(IPropertyMapper mapper) : base(mapper ?? PolylineMapper)
		{

		}
	}
}