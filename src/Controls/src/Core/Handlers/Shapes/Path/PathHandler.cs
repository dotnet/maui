using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PathHandler : ShapeViewHandler
	{
		public static IPropertyMapper<Path, IShapeViewHandler> PathMapper = new PropertyMapper<Path, IShapeViewHandler>(Mapper)
		{
			[nameof(Path.Data)] = MapData,
			[nameof(Path.RenderTransform)] = MapRenderTransform,
		};

		public PathHandler() : base(PathMapper)
		{

		}

		public PathHandler(IPropertyMapper mapper) : base(mapper ?? PathMapper)
		{

		}
	}
}