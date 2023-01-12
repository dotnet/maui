#nullable disable
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class RoundRectangleHandler : ShapeViewHandler
	{
		public static new IPropertyMapper<RoundRectangle, IShapeViewHandler> Mapper = new PropertyMapper<RoundRectangle, IShapeViewHandler>(ShapeViewHandler.Mapper)
		{
			[nameof(RoundRectangle.CornerRadius)] = MapCornerRadius
		};

		public RoundRectangleHandler() : base(Mapper)
		{

		}

		public RoundRectangleHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{

		}
	}
}