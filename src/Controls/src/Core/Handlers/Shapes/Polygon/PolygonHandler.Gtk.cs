using System;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{

	public partial class PolygonHandler
	{

		public static void MapShape(IShapeViewHandler handler, Polygon polygon)
		{
			handler.PlatformView?.UpdateShape(polygon);
		}

		public static void MapPoints(IShapeViewHandler handler, Polygon polygon)
		{
			handler.PlatformView?.UpdateShape(polygon);
		}

		public static void MapFillRule(IShapeViewHandler handler, Polygon polygon)
		{
			handler.PlatformView?.UpdateShape(polygon);
		}

	}

}