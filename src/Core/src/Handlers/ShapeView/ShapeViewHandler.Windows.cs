using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Handlers
{
	public partial class ShapeViewHandler : ViewHandler<IShapeView, W2DGraphicsView>
	{
		protected override W2DGraphicsView CreateNativeView()
		{
			return new W2DGraphicsView();
		}

		public static void MapShape(ShapeViewHandler handler, IShapeView shapeView)
		{
			handler.NativeView?.UpdateShape(shapeView);
		}

		public static void MapFill(ShapeViewHandler handler, IShapeView shapeView)
		{
			handler.NativeView?.InvalidateShape(shapeView);
		}

		public static void MapStroke(ShapeViewHandler handler, IShapeView shapeView)
		{
			handler.NativeView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeThickness(ShapeViewHandler handler, IShapeView shapeView)
		{
			handler.NativeView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeDashPattern(ShapeViewHandler handler, IShapeView shapeView)
		{
			handler.NativeView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeLineCap(ShapeViewHandler handler, IShapeView shapeView)
		{
			handler.NativeView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeLineJoin(ShapeViewHandler handler, IShapeView shapeView)
		{
			handler.NativeView?.InvalidateShape(shapeView);
		}

		public static void MapStrokeMiterLimit(ShapeViewHandler handler, IShapeView shapeView)
		{
			handler.NativeView?.InvalidateShape(shapeView);
		}
	}
}