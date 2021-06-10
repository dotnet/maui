using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class ShapeViewHandler : ViewHandler<IShapeView, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapShape(IViewHandler handler, IShapeView shapeView) { }
		public static void MapAspect(IViewHandler handler, IShapeView shapeView) { }
		public static void MapFill(IViewHandler handler, IShapeView shapeView) { }
		public static void MapStroke(IViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeThickness(IViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeDashPattern(IViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeLineCap(IViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeLineJoin(IViewHandler handler, IShapeView shapeView) { }
		public static void MapStrokeMiterLimit(IViewHandler handler, IShapeView shapeView) { }
	}
}