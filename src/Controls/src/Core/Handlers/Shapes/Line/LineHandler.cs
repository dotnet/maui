#nullable disable
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LineHandler : ShapeViewHandler
	{
		public static new IPropertyMapper<Line, IShapeViewHandler> Mapper = new PropertyMapper<Line, IShapeViewHandler>(ShapeViewHandler.Mapper)
		{
			[nameof(Line.X1)] = MapX1,
			[nameof(Line.Y1)] = MapY1,
			[nameof(Line.X2)] = MapX2,
			[nameof(Line.Y2)] = MapY2,
		};

		public LineHandler() : base(Mapper)
		{

		}

		public LineHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{

		}
	}
}