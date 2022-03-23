using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LineHandler : ShapeViewHandler
	{
		public static IPropertyMapper<Line, IShapeViewHandler> LineMapper = new PropertyMapper<Line, IShapeViewHandler>(Mapper)
		{
			[nameof(Line.X1)] = MapX1,
			[nameof(Line.Y1)] = MapY1,
			[nameof(Line.X2)] = MapX2,
			[nameof(Line.Y2)] = MapY2,
		};

		public LineHandler() : base(LineMapper)
		{

		}

		public LineHandler(IPropertyMapper mapper) : base(mapper ?? LineMapper)
		{

		}
	}
}