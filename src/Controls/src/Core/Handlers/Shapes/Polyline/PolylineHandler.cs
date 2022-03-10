using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolylineHandler : ShapeViewHandler
	{
		public static IPropertyMapper<Polyline, IShapeViewHandler> PolylineMapper = new PropertyMapper<Polyline, IShapeViewHandler>(Mapper)
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