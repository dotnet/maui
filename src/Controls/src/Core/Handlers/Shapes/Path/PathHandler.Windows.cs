using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PathHandler
	{
		public static void MapData(PathHandler handler, Path path)
		{
			handler.PlatformView?.InvalidateShape(path);
		}

		public static void MapRenderTransform(PathHandler handler, Path path)
		{
			handler.PlatformView?.InvalidateShape(path);
		}
	}
}