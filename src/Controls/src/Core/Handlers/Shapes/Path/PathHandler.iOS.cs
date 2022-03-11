using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PathHandler 
	{
		public static void MapData(IShapeViewHandler handler, Path path)
		{
			handler.PlatformView?.InvalidateShape(path);
		}

		public static void MapRenderTransform(IShapeViewHandler handler, Path path)
		{
			handler.PlatformView?.InvalidateShape(path);
		}
	}
}