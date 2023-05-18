#nullable disable
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolylineHandler : ShapeViewHandler
	{
		public static new IPropertyMapper<Polyline, IShapeViewHandler> Mapper = new PropertyMapper<Polyline, IShapeViewHandler>(ShapeViewHandler.Mapper)
		{
			[nameof(IShapeView.Shape)] = MapShape,
			[nameof(Polyline.Points)] = MapPoints,
			[nameof(Polyline.FillRule)] = MapFillRule,
		};

		public PolylineHandler() : base(Mapper)
		{

		}

		public PolylineHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{

		}
	}
}