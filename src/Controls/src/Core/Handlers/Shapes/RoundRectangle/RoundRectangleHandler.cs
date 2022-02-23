using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class RoundRectangleHandler : ShapeViewHandler
	{
		public static IPropertyMapper<RoundRectangle, RoundRectangleHandler> RoundRectangleMapper = new PropertyMapper<RoundRectangle, RoundRectangleHandler>(ShapeViewMapper)
		{
			[nameof(RoundRectangle.CornerRadius)] = MapCornerRadius
		};

		public RoundRectangleHandler() : base(RoundRectangleMapper)
		{

		}

		public RoundRectangleHandler(IPropertyMapper mapper) : base(mapper ?? RoundRectangleMapper)
		{

		}
	}
}