using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class ShapeViewHandler : ViewHandler<IShapeView, MauiShapeView>
	{
		protected override MauiShapeView CreateNativeView()
		{
			return new MauiShapeView();
		}

		[MissingMapper]
		public static void MapShape(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapFill(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapStroke(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapStrokeThickness(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapStrokeDashPattern(ShapeViewHandler handler, IShapeView shapeView) { }
	
		[MissingMapper]
		public static void MapStrokeLineCap(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapStrokeLineJoin(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapStrokeMiterLimit(ShapeViewHandler handler, IShapeView shapeView) { }
	}
}