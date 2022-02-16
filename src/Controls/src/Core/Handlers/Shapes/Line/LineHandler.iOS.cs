using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LineHandler 
	{
		public static void MapX1(LineHandler handler, Line line)
		{
			handler.PlatformView?.InvalidateShape(line);
		}

		public static void MapY1(LineHandler handler, Line line)
		{
			handler.PlatformView?.InvalidateShape(line);
		}

		public static void MapX2(LineHandler handler, Line line)
		{
			handler.PlatformView?.InvalidateShape(line);
		}

		public static void MapY2(LineHandler handler, Line line)
		{
			handler.PlatformView?.InvalidateShape(line);
		}
	}
}