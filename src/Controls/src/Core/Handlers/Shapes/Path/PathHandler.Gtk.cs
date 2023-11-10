using System;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PathHandler
	{

		public static void MapShape(IShapeViewHandler handler, Path path)
		{
			handler.PlatformView?.UpdateShape(path);
		}

		public static void MapData(IShapeViewHandler handler, Path path)
		{
			handler.PlatformView?.UpdateShape(path);
		}
		
		[MissingMapper]
		public static void MapRenderTransform(IShapeViewHandler handler, Path path) { }
	}
}