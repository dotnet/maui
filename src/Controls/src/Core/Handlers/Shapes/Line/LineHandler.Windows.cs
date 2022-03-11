using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LineHandler 
	{
		public static void MapX1(IShapeViewHandler handler, Line line)
		{
			handler.PlatformView?.InvalidateShape(line);
		}

		public static void MapY1(IShapeViewHandler handler, Line line)
		{
			handler.PlatformView?.InvalidateShape(line);
		}

		public static void MapX2(IShapeViewHandler handler, Line line)
		{
			handler.PlatformView?.InvalidateShape(line);
		}

		public static void MapY2(IShapeViewHandler handler, Line line)
		{
			handler.PlatformView?.InvalidateShape(line);
		}
	}
}