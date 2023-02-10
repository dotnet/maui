#nullable disable
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PathHandler : ShapeViewHandler
	{
		public static new IPropertyMapper<Path, IShapeViewHandler> Mapper = new PropertyMapper<Path, IShapeViewHandler>(ShapeViewHandler.Mapper)
		{
			[nameof(IShapeView.Shape)] = MapShape,
			[nameof(Path.Data)] = MapData,
			[nameof(Path.RenderTransform)] = MapRenderTransform,
		};

		public PathHandler() : base(Mapper)
		{

		}

		public PathHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{

		}
	}
}