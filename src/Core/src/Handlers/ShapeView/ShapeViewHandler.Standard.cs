using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class ShapeViewHandler : ViewHandler<IShapeView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapBackground(IShapeViewHandler handler, IShapeView shapeView) { }
		public static void MapShape(IShapeViewHandler handler, IShapeView shapeView) { }
		public static void MapAspect(IShapeViewHandler handler, IShapeView shapeView) { }
		public static void MapFill(IShapeViewHandler handler, IShapeView shapeView) { }
		public static void MapStroke(IShapeViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeThickness(IShapeViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeDashPattern(IShapeViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeDashOffset(IShapeViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeLineCap(IShapeViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeLineJoin(IShapeViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeMiterLimit(IShapeViewHandler handler, IShapeView shapeView) { }
	}
}