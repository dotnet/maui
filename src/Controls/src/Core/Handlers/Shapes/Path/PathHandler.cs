using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PathHandler : ShapeViewHandler
	{
		public static IPropertyMapper<Path, PathHandler> PathMapper = new PropertyMapper<Path, PathHandler>(ShapeViewMapper)
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